namespace TailBlazer.Infrastucture
{
    using System;
    using System.Collections;
    using System.Linq;
    using log4net;
    using TailBlazer.Domain.Infrastructure;
    public class Log4NetLogger : ILogger
    {
        #region Fields
        private readonly ILog _log;
        #endregion
        #region Constructors
        public Log4NetLogger(Type type)
        {
            var name = type.Name;
            var genericArgs = type.GenericTypeArguments;
            if (!genericArgs.Any())
            {
                this._log = LogManager.GetLogger(name);
            }
            else
            {
                var startOfGeneric = name.IndexOf("`", StringComparison.Ordinal);
                name = name.Substring(0, startOfGeneric);
                var generics = genericArgs.Select(t => t.Name).ToDelimited();
                this._log = LogManager.GetLogger($"{name}<{generics}>");
            }
        }
        public Log4NetLogger(string name) { this._log = LogManager.GetLogger(name); }
        #endregion
        #region Properties
        public string Name => this._log.Logger.Name;
        #endregion
        #region Methods
        public void Debug(string message, params object[] values)
        {
            if (!this._log.IsDebugEnabled) return;
            this._log.DebugFormat(message, values);
        }
        public void Error(Exception ex, string message, params object[] values)
        {
            if (!this._log.IsErrorEnabled) return;
            this._log.Error(string.Format(message, values), ex);
        }
        public void Fatal(string message, params object[] values)
        {
            if (!this._log.IsFatalEnabled) return;
            this._log.FatalFormat(message, values);
        }
        public void Info(string message, params object[] values)
        {
            if (!this._log.IsInfoEnabled) return;
            if (values.Length == 0)
                this._log.Info(message);
            else
                this._log.InfoFormat(message, values);
        }
        public void Warn(string message, params object[] values)
        {
            if (!this._log.IsWarnEnabled) return;
            this._log.WarnFormat(message, values);
        }
        #endregion
    }
}