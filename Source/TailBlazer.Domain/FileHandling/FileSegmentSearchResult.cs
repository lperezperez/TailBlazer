namespace TailBlazer.Domain.FileHandling
{
    using System;
    public class FileSegmentSearchResult
    {
        #region Constructors
        public FileSegmentSearchResult(long start, long end)
        {
            this.Start = start;
            this.End = end;
            this.Indicies = new long[0];
        }
        public FileSegmentSearchResult(long start, long end, long[] indicies)
        {
            if (indicies == null) throw new ArgumentNullException(nameof(indicies));
            this.Start = start;
            this.End = end;
            this.Indicies = indicies;
        }
        #endregion
        #region Properties
        public long End { get; }
        public long[] Indicies { get; }
        public long Start { get; }
        #endregion
    }
}