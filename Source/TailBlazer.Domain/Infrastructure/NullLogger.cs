﻿namespace TailBlazer.Domain.Infrastructure
{
    using System;
    public class NullLogger : ILogger
    {
        #region Methods
        public void Debug(string message, params object[] values) { }
        public void Error(Exception ex, string message, params object[] values) { }
        public void Fatal(string message, params object[] values) { }
        public void Info(string message, params object[] values) { }
        public void Warn(string message, params object[] values) { }
        #endregion
    }
}