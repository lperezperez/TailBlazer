namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    public class IndexCollection : ILineProvider
    {
        #region Constructors
        public IndexCollection(IReadOnlyCollection<Index> latest, IndexCollection previous, FileInfo info, Encoding encoding)
        {
            this.Info = info;
            this.Encoding = encoding;
            this.Count = latest.Select(idx => idx.LineCount).Sum();
            this.Indicies = latest.ToArray();
            this.Diff = this.Count - (previous?.Count ?? 0);

            //need to check whether
            if (previous == null)
                this.TailInfo = new TailInfo(latest.Max(idx => idx.End));
            else
                this.TailInfo = new TailInfo(previous.Indicies.Max(idx => idx.End));
        }
        #endregion
        #region Properties
        public int Count { get; }
        public int Diff { get; }
        public bool IsEmpty => this.Count != 0;
        private Encoding Encoding { get; }
        private Index[] Indicies { get; }
        private FileInfo Info { get; }
        private TailInfo TailInfo { get; }
        #endregion
        #region Methods
        /// <summary>Reads the lines.</summary>
        /// <param name="scroll">The scroll.</param>
        /// <returns></returns>
        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            if (scroll.SpecifiedByPosition)
                foreach (var line in this.ReadLinesByPosition(scroll))
                    yield return line;
            else
                foreach (var line in this.ReadLinesByIndex(scroll))
                    yield return line;
        }
        private long CalculateIndexByPositon(long position)
        {
            long firstLineInContainer = 0;
            long lastLineInContainer = 0;
            foreach (var sparseIndex in this.Indicies)
            {
                lastLineInContainer += sparseIndex.End;
                if (position < lastLineInContainer)
                {
                    if (sparseIndex.LineCount != 0 && sparseIndex.Indicies.Count == 0)
                    {
                        var lines = sparseIndex.LineCount;
                        var bytes = sparseIndex.End - sparseIndex.Start;
                        var bytesPerLine = bytes / lines;
                        return position / bytesPerLine;
                    }
                    if (sparseIndex.Compression == 1)
                        return firstLineInContainer + sparseIndex.Indicies.IndexOf(position);

                    //find nearest, then work out offset
                    var nearest = sparseIndex.Indicies.Data.Select((value, index) => new { value, index }).OrderByDescending(x => x.value).FirstOrDefault(i => i.value <= position);
                    if (nearest != null)
                    {
                        //index depends of how far in container
                        var relativeIndex = nearest.index * sparseIndex.Compression;

                        //remaining size
                        var size = sparseIndex.End - sparseIndex.Start;
                        var offset = position - nearest.value;
                        var estimateOffset = offset / size * sparseIndex.Compression;
                        return firstLineInContainer + relativeIndex + estimateOffset;
                    }
                    else
                    {
                        //index depends of how far in container
                        var relativeIndex = 0;

                        //remaining size
                        var size = sparseIndex.End - sparseIndex.Start;
                        var offset = position;
                        var estimateOffset = offset / size * sparseIndex.Compression;
                        return firstLineInContainer + relativeIndex + estimateOffset;
                    }
                }
                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return -1;
        }
        private RelativeIndex CalculateRelativeIndex(int index)
        {
            var firstLineInContainer = 0;
            var lastLineInContainer = 0;
            foreach (var sparseIndex in this.Indicies)
            {
                lastLineInContainer += sparseIndex.LineCount;
                if (index < lastLineInContainer)
                {
                    //It could be that the user is scrolling into a part of the file
                    //which is still being indexed [or will never be indexed]. 
                    //In this case we need to estimate where to scroll to
                    if (sparseIndex.LineCount != 0 && sparseIndex.Indicies.Count == 0)
                    {
                        //return estimate here!
                        var lines = sparseIndex.LineCount;
                        var bytes = sparseIndex.End - sparseIndex.Start;
                        var bytesPerLine = bytes / lines;
                        var estimate = index * bytesPerLine;
                        return new RelativeIndex(index, estimate, 0, true);
                    }
                    var relativePosition = index - firstLineInContainer;
                    var relativeIndex = relativePosition / sparseIndex.Compression;
                    var offset = relativePosition % sparseIndex.Compression;
                    if (relativeIndex >= sparseIndex.IndexCount)
                        relativeIndex = sparseIndex.IndexCount - 1;
                    var start = relativeIndex == 0 ? 0 : sparseIndex.Indicies[relativeIndex - 1];
                    return new RelativeIndex(index, start, offset, false);
                }
                firstLineInContainer = firstLineInContainer + sparseIndex.LineCount;
            }
            return null;
        }
        private Page GetPage(ScrollRequest scroll)
        {
            var first = scroll.FirstIndex;
            var size = scroll.PageSize;
            if (scroll.Mode == ScrollReason.Tail)
            {
                first = size > this.Count ? 0 : this.Count - size;
            }
            else
            {
                if (scroll.FirstIndex + size >= this.Count)
                    first = this.Count - size;
            }
            first = Math.Max(0, first);
            size = Math.Min(size, this.Count);
            return new Page(first, size);
        }
        private IEnumerable<Line> ReadLinesByIndex(ScrollRequest scroll)
        {
            var page = this.GetPage(scroll);
            var relativeIndex = this.CalculateRelativeIndex(page.Start);
            if (relativeIndex == null) yield break;
            var offset = relativeIndex.LinesOffset;
            using (var stream = File.Open(this.Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, this.Encoding, false))
                {
                    //go to starting point
                    stream.Seek(relativeIndex.Start, SeekOrigin.Begin);
                    if (offset > 0)
                        for (var i = 0; i < offset; i++)
                            reader.ReadLine();

                    //if estimate move to the next start of line
                    if (relativeIndex.IsEstimate && relativeIndex.Start != 0)
                        reader.ReadLine();
                    foreach (var i in Enumerable.Range(page.Start, page.Size))
                    {
                        var startPosition = reader.AbsolutePosition();
                        var line = reader.ReadLine();
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(i + 1, i, startPosition, endPosition);
                        var ontail = startPosition >= this.TailInfo.TailStartsAt && DateTime.UtcNow.Subtract(this.TailInfo.LastTail).TotalSeconds < 1 ? DateTime.UtcNow : (DateTime?)null;
                        yield return new Line(info, line, ontail);
                    }
                }
            }
        }
        private IEnumerable<Line> ReadLinesByPosition(ScrollRequest scroll)
        {
            //TODO: Calculate initial index of first item.

            //scroll from specified position
            using (var stream = File.Open(this.Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                var taken = 0;
                using (var reader = new StreamReaderExtended(stream, this.Encoding, false))
                {
                    var startPosition = scroll.Position;
                    var first = (int)this.CalculateIndexByPositon(startPosition);
                    reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
                    do
                    {
                        var line = reader.ReadLine();
                        if (line == null) yield break;
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(first + taken + 1, first + taken, startPosition, endPosition);
                        var ontail = endPosition >= this.TailInfo.TailStartsAt && DateTime.UtcNow.Subtract(this.TailInfo.LastTail).TotalSeconds < 1 ? DateTime.UtcNow : (DateTime?)null;
                        yield return new Line(info, line, ontail);
                        startPosition = endPosition;
                        taken++;
                    }
                    while (taken < scroll.PageSize);
                }
            }
        }
        #endregion
        #region Classes
        private class RelativeIndex
        {
            #region Constructors
            public RelativeIndex(long index, long start, int linesOffset, bool isEstimate)
            {
                this.Index = index;
                this.Start = start;
                this.LinesOffset = linesOffset;
                this.IsEstimate = isEstimate;
            }
            #endregion
            #region Properties
            public long Index { get; }
            public bool IsEstimate { get; }
            public int LinesOffset { get; }
            public long Start { get; }
            #endregion
        }
        #endregion
    }
}