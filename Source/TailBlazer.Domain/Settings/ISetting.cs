namespace TailBlazer.Domain.Settings
{
    using System;
    public interface ISetting<T>
    {
        #region Properties
        IObservable<T> Value { get; }
        #endregion
        #region Methods
        void Write(T item);
        #endregion
    }
}