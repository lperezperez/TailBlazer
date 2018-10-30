namespace TailBlazer.Infrastucture
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using TailBlazer.Domain.Infrastructure;
    public class UhandledExceptionHandler
    {
        #region Fields
        private readonly ILogger _logger;
        #endregion
        #region Constructors
        public UhandledExceptionHandler(ILogger logger)
        {
            this._logger = logger;
            Application.Current.DispatcherUnhandledException += this.CurrentDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomainUnhandledException;
        }
        #endregion
        #region Methods
        private void CurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            this._logger.Error(ex, ex.Message);
            e.Handled = true;
        }
        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            this._logger.Error(ex, ex.Message);
        }
        #endregion
    }
}