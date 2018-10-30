namespace TailBlazer.Infrastucture
{
    using System.Collections.Generic;
    using TailBlazer.Domain.Annotations;
    public interface IClipboardHandler
    {
        #region Methods
        void WriteToClipboard([NotNull] string text);
        void WriteToClipboard([NotNull] IEnumerable<string> items);
        #endregion
    }
}