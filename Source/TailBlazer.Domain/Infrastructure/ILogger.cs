namespace TailBlazer.Domain.Infrastructure
{
    using System;
    public interface ILogger
    {
        #region Methods
        void Debug(string message, params object[] values);
        void Error(Exception ex, string message, params object[] values);
        void Fatal(string message, params object[] values);
        void Info(string message, params object[] values);
        void Warn(string message, params object[] values);
        #endregion
    }
}