namespace TailBlazer.Infrastucture
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    public class ClipboardHandler : IClipboardHandler
    {
        #region Fields
        private readonly ILogger _logger;
        #endregion
        #region Constructors
        public ClipboardHandler(ILogger logger) { this._logger = logger; }
        #endregion
        #region Methods
        public void WriteToClipboard(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            try
            {
                this._logger.Info($"Attempting to copy {Environment.NewLine}{text}{Environment.NewLine} to the clipboard");
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Problem copying items to the clipboard");
            }
        }
        public void WriteToClipboard([NotNull] IEnumerable<string> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            try
            {
                var array = items.AsArray();
                this._logger.Info($"Attempting to copy {array.Length} lines to the clipboard");
                Clipboard.SetText(array.ToDelimited(Environment.NewLine), TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Problem copying items to the clipboard");
            }
        }
        #endregion
    }
}