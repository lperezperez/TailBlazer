namespace TailBlazer.Domain.FileHandling
{
    using System.Collections.Generic;
    public interface ILineProvider
    {
        #region Properties
        int Count { get; }
        #endregion
        #region Methods
        IEnumerable<Line> ReadLines(ScrollRequest scroll);
        #endregion
    }
}