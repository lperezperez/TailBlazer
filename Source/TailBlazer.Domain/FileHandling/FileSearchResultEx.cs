namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class FileSearchResultEx
    {
        #region Methods
        public static IEnumerable<LineInfo> GetIndicies(this FileSearchResult source, ScrollRequest scroll)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (scroll == null) throw new ArgumentNullException(nameof(scroll));
            var first = scroll.FirstIndex;
            var size = scroll.PageSize;
            if (scroll.Mode == ScrollReason.Tail)
            {
                first = size > source.Count ? 0 : source.Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= source.Count)
                    first = source.Count - size;
            }
            first = Math.Max(0, first);
            size = Math.Min(size, source.Count);
            if (size == 0) yield break;
            foreach (var i in Enumerable.Range(first, size))
            {
                if (i > source.Count - 1) continue;
                var start = source.Matches[i];
                yield return new LineInfo(0, i, start, 0);
            }
        }
        #endregion
    }
}