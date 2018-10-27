namespace TailBlazer.Domain.FileHandling
{
    using System.Collections.Generic;
    public sealed class EmptyLineProvider : ILineProvider
    {
        #region Fields
        public static readonly ILineProvider Instance = new EmptyLineProvider();
        #endregion
        #region Properties
        public int Count { get; } = 0;
        #endregion
        #region Methods
        public IEnumerable<Line> ReadLines(ScrollRequest scroll) { yield break; }
        #endregion
    }
}