namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Collections.Generic;
    public interface ITextFormatter
    {
        #region Methods
        IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText);
        #endregion
    }
}