namespace TailBlazer.Domain.Infrastructure
{
    using System;
    public interface IProperty<out T> : IDisposable
    {
        #region Properties
        T Value { get; }
        #endregion
    }
}