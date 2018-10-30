namespace TailBlazer.Domain.Settings
{
    using System;
    public class SettingsException : Exception
    {
        #region Constructors
        public SettingsException(string message)
            : base(message) { }
        public SettingsException(string message, Exception innerException)
            : base(message, innerException) { }
        #endregion
    }
}