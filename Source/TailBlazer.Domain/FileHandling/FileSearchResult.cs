namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DynamicData.Kernel;
    public class FileSearchResult : ILineProvider, IEquatable<FileSearchResult>, IHasLimitationOfLines, IProgressInfo
    {
        #region Fields
        public static readonly FileSearchResult None = new FileSearchResult();
        private readonly IDictionary<FileSegmentKey, FileSegmentSearch> _allSearches;
        #endregion
        #region Constructors
        public FileSearchResult(FileSegmentSearch initial, FileInfo info, Encoding encoding, int limit)
        {
            this.Info = info;
            this.Encoding = encoding;
            this.LastSearch = initial;
            this._allSearches = new Dictionary<FileSegmentKey, FileSegmentSearch> { [initial.Key] = initial };
            this.IsSearching = initial.Status != FileSegmentSearchStatus.Complete;
            this.Segments = 1;
            this.SegmentsCompleted = this.IsSearching ? 0 : 1;
            this.Matches = initial.Lines.ToArray();
            this.TailInfo = TailInfo.None;
            this.Size = 0;
            this.Maximum = limit;
            this.HasReachedLimit = false;
        }
        public FileSearchResult(FileSearchResult previous, FileSegmentSearch current, FileInfo info, Encoding encoding, int limit)
        {
            this.Maximum = limit;
            this.LastSearch = current;
            this.Info = info;
            this.Encoding = encoding;
            this._allSearches = previous._allSearches.Values.ToDictionary(fss => fss.Key);
            var lastTail = this._allSearches.Lookup(FileSegmentKey.Tail);
            if (current.Segment.Type == FileSegmentType.Tail)
                this.TailInfo = lastTail.HasValue ? new TailInfo(lastTail.Value.Segment.End) : new TailInfo(current.Segment.End);
            else
                this.TailInfo = lastTail.HasValue ? previous.TailInfo : TailInfo.None;
            this._allSearches[current.Key] = current;
            var all = this._allSearches.Values.ToArray();
            this.IsSearching = all.Any(s => s.Segment.Type == FileSegmentType.Head && s.Status != FileSegmentSearchStatus.Complete);
            this.Segments = all.Length;
            this.SegmentsCompleted = all.Count(s => s.Segment.Type == FileSegmentType.Head && s.Status == FileSegmentSearchStatus.Complete);
            this.Size = all.Last().Segment.End;

            //For large sets this could be very inefficient
            this.Matches = all.SelectMany(s => s.Lines).OrderBy(l => l).ToArray();
            this.HasReachedLimit = this.Matches.Length >= limit;
        }
        private FileSearchResult()
        {
            this.Matches = new long[0];
            this.HasReachedLimit = false;
        }
        #endregion
        #region Properties
        public int Count => this.Matches.Length;
        public bool HasReachedLimit { get; }
        public bool IsEmpty => this == None;
        public bool IsSearching { get; }
        public long[] Matches { get; }
        public int Maximum { get; }
        public int Segments { get; }
        public int SegmentsCompleted { get; }
        private Encoding Encoding { get; }
        private FileInfo Info { get; }
        private FileSegmentSearch LastSearch { get; }
        private long Size { get; }
        private TailInfo TailInfo { get; }
        #endregion
        #region Methods
        public static bool operator ==(FileSearchResult left, FileSearchResult right) => object.Equals(left, right);
        public static bool operator !=(FileSearchResult left, FileSearchResult right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileSearchResult)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Matches?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.SegmentsCompleted;
                hashCode = (hashCode * 397) ^ this.Segments;
                hashCode = (hashCode * 397) ^ this.IsSearching.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString() => this == None ? "<None>" : $"Count: {this.Count}, Segments: {this.Segments}, Size: {this.Size}";
        public bool Equals(FileSearchResult other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.Matches, other.Matches) && this.SegmentsCompleted == other.SegmentsCompleted && this.Segments == other.Segments && this.IsSearching == other.IsSearching;
        }
        public IEnumerable<Line> ReadLines(ScrollRequest scroll)
        {
            var page = this.GetPage(scroll);
            if (page.Size == 0) yield break;
            using (var stream = File.Open(this.Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, this.Encoding, false))
                {
                    if (page.Size == 0) yield break;
                    foreach (var i in Enumerable.Range(page.Start, page.Size))
                    {
                        if (i > this.Count - 1) continue;
                        var start = this.Matches[i];
                        var startPosition = reader.AbsolutePosition();
                        if (startPosition != start)
                        {
                            reader.DiscardBufferedData();
                            reader.BaseStream.Seek(start, SeekOrigin.Begin);
                        }
                        startPosition = reader.AbsolutePosition();
                        var line = reader.ReadLine();
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(i + 1, i, startPosition, endPosition);
                        var ontail = endPosition >= this.TailInfo.TailStartsAt && DateTime.UtcNow.Subtract(this.TailInfo.LastTail).TotalSeconds < 1 ? DateTime.UtcNow : (DateTime?)null;
                        yield return new Line(info, line, ontail);
                    }
                }
            }
        }
        private Page GetPage(ScrollRequest scroll)
        {
            int first;
            if (scroll.SpecifiedByPosition)
                first = this.IndexOf(scroll.Position);
            else
                first = scroll.FirstIndex;
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
        private int IndexOf(long value)
        {
            for (var i = 0; i < this.Matches.Length; ++i)
                if (object.Equals(this.Matches[i], value))
                    return i;
            return -1;
        }
        #endregion
    }
}