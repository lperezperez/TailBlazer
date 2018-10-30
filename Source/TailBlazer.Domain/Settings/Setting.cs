namespace TailBlazer.Domain.Settings
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using TailBlazer.Domain.Infrastructure;
    public class Setting<T> : IEquatable<Setting<T>>, IDisposable, ISetting<T>
    {
        #region Fields
        private readonly ISubject<T> _changed = new ReplaySubject<T>(1);
        private readonly IConverter<T> _converter;
        private readonly string _key;
        private readonly ILogger _logger;
        private readonly ISettingsStore _settingsStore;
        private string _rawValue;
        private T _value;
        #endregion
        #region Constructors
        public Setting(ILogger logger, ISettingsStore settingsStore, IConverter<T> converter, string key)
        {
            this._logger = logger;
            this._settingsStore = settingsStore;
            this._converter = converter;
            this._key = key;
            try
            {
                //make this awaitable
                var state = this._settingsStore.Load(this._key);
                this._rawValue = state.Value;
                this._value = converter.Convert(state);
            }
            catch (Exception ex)
            {
                this._value = converter.GetDefaultValue();
                this._rawValue = converter.Convert(this._value).Value;
                this._logger.Error(ex, "Problem reading {0}", this._key);
            }
            this._changed.OnNext(this._value);
        }
        #endregion
        #region Properties
        public IObservable<T> Value => this._changed.AsObservable();
        #endregion
        #region Methods
        public static bool operator ==(Setting<T> left, Setting<T> right) => object.Equals(left, right);
        public static bool operator !=(Setting<T> left, Setting<T> right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Setting<T>)obj);
        }
        public override int GetHashCode() => this._key?.GetHashCode() ?? 0;
        public override string ToString() => this._key;
        public void Dispose() { this._changed.OnCompleted(); }
        public bool Equals(Setting<T> other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this._key, other._key);
        }
        public void Write(T value)
        {
            var converted = this._converter.Convert(value);
            if (this._rawValue == converted.Value) return;
            this._rawValue = converted.Value;
            this._value = value;
            try
            {
                //make this awaitable
                this._settingsStore.Save(this._key, converted);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Problem writing {0}", value);
            }
            this._changed.OnNext(value);
        }
        #endregion
    }
}