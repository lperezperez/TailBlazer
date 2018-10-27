namespace TailBlazer.Domain.Formatting
{
    using System;
    public interface ILineMatches
    {
        #region Methods
        IObservable<LineMatchCollection> GetMatches(string inputText);
        #endregion
    }
}