namespace TailBlazer.Domain.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using TailBlazer.Domain.Annotations;
    public interface IArgument
    {
        #region Properties
        Type TargetType { get; }
        object Value { get; }
        #endregion
    }
    public interface IObjectProvider
    {
        #region Methods
        T Get<T>();
        T Get<T>(NamedArgument arg);
        T Get<T>(IEnumerable<NamedArgument> args);
        T Get<T>(IArgument arg);
        T Get<T>(IEnumerable<IArgument> args);
        #endregion
    }
    public interface IObjectRegister
    {
        #region Methods
        void Register<T>(T instance)
            where T : class;
        #endregion
    }
    public class Argument : IArgument
    {
        #region Constructors
        public Argument([NotNull] object value, Type registerAs = null)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            this.Value = value;
            this.TargetType = registerAs ?? value.GetType();
        }
        #endregion
        #region Properties
        public Type TargetType { get; }
        public object Value { get; }
        #endregion
    }
    public class Argument<T> : IArgument
    {
        #region Constructors
        public Argument([NotNull] T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            this.Value = value;
        }
        #endregion
        #region Properties
        public Type TargetType => typeof(T);
        private T Value { get; }
        object IArgument.Value => this.Value;
        #endregion
    }
    public class NamedArgument
    {
        #region Constructors
        public NamedArgument([NotNull] string key, [NotNull] object instance)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            this.Key = key;
            this.Instance = instance;
        }
        #endregion
        #region Properties
        public object Instance { get; }
        public string Key { get; }
        #endregion
    }
}