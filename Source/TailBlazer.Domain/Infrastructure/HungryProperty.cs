namespace TailBlazer.Domain.Infrastructure
{
    using System;
    using System.Reactive.Disposables;
    using DynamicData.Binding;
    internal sealed class HungryProperty<T> : AbstractNotifyPropertyChanged, IProperty<T>
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private T _value;
        #endregion
        #region Constructors
        public HungryProperty(IObservable<T> source) { this._cleanUp = source.Subscribe(t => this.Value = t); }
        #endregion
        #region Properties
        public T Value { get => this._value; set => this.SetAndRaise(ref this._value, value); }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
    internal sealed class LazyProperty<T> : AbstractNotifyPropertyChanged, IProperty<T>, IDisposable
    {
        #region Fields
        private readonly SingleAssignmentDisposable _cleanUp = new SingleAssignmentDisposable();
        private readonly Lazy<IDisposable> _factory;
        private T _value;
        #endregion
        #region Constructors
        public LazyProperty(IObservable<T> source)
        {
            this._factory = new Lazy<IDisposable>(() => source.Subscribe(t => this.Value = t));

            //_cleanUp =
        }
        #endregion
        #region Properties
        public T Value
        {
            get
            {
                this.EnsureLoaded();
                return this._value;
            }
            private set => this.SetAndRaise(ref this._value, value);
        }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private void EnsureLoaded() { this._cleanUp.Disposable = this._factory.Value; }
        #endregion

        //}
    }
}