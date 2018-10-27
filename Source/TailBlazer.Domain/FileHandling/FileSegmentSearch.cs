namespace TailBlazer.Domain.FileHandling
{
    using System;
    using TailBlazer.Domain.Infrastructure;
    public class FileSegmentSearch : IEquatable<FileSegmentSearch>
    {
        #region Fields
        private readonly ImmutableArray<long> _matches;
        #endregion
        #region Constructors
        public FileSegmentSearch(FileSegment segment, FileSegmentSearchStatus status = FileSegmentSearchStatus.Pending)
        {
            this.Key = segment.Key;
            this.Segment = segment;
            this.Status = status;
            this._matches = new ImmutableArray<long>();
        }
        public FileSegmentSearch(FileSegment segment, FileSegmentSearchResult result)
        {
            this.Key = segment.Key;
            this.Segment = segment;
            this.Status = FileSegmentSearchStatus.Complete;
            this._matches = new ImmutableArray<long>(result.Indicies);
        }
        public FileSegmentSearch(FileSegmentSearch segmentSearch, FileSegmentSearchResult result)
        {
            //this can only be the tail as the tail will continue to grow
            this.Key = segmentSearch.Key;
            this.Segment = new FileSegment(segmentSearch.Segment, result.End);
            this.Status = FileSegmentSearchStatus.Complete;
            this._matches = segmentSearch._matches.Add(result.Indicies);
        }
        public FileSegmentSearch(FileSegmentSearch segmentSearch, FileSegmentSearchStatus complete)
        {
            this.Key = segmentSearch.Key;
            this.Segment = new FileSegment(segmentSearch.Segment, segmentSearch.Segment.End);
            this.Status = complete;
            this._matches = new ImmutableArray<long>();
        }
        #endregion
        #region Properties
        public FileSegmentKey Key { get; }
        public long[] Lines => this._matches.Data;
        public FileSegment Segment { get; }
        public FileSegmentSearchStatus Status { get; }
        public FileSegmentType Type { get; }
        #endregion
        #region Methods
        public static bool operator ==(FileSegmentSearch left, FileSegmentSearch right) => object.Equals(left, right);
        public static bool operator !=(FileSegmentSearch left, FileSegmentSearch right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileSegmentSearch)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Key.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Segment?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)this.Status;
                return hashCode;
            }
        }
        public override string ToString() => $"{this.Segment} ->{this.Status}. Items: {this.Lines.Length}";
        public bool Equals(FileSegmentSearch other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Key.Equals(other.Key) && object.Equals(this.Segment, other.Segment) && this.Status == other.Status;
        }
        #endregion
    }
}