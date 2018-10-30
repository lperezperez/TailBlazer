namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.IO;
    using System.Linq;
    public sealed class FileSegmentCollection : IEquatable<FileSegmentCollection>
    {
        #region Constructors
        public FileSegmentCollection(FileInfo fileInfo, FileSegment[] segments, long sizeDiff)
        {
            if (segments.Length == 0)
                throw new ArgumentException("Argument is empty collection", nameof(segments));
            this.Info = fileInfo;
            this.Segments = segments;
            this.TailStartsAt = segments.Max(fs => fs.End);
            this.Count = this.Segments.Length;
            this.FileSize = this.TailStartsAt;
            this.SizeDiff = sizeDiff;
            this.Reason = FileSegmentChangedReason.Loaded;
        }
        public FileSegmentCollection(long newLength, FileSegmentCollection previous)
        {
            this.SizeDiff = newLength - previous.FileLength;

            //All this assumes it is the tail which has changed, but that may not be so
            this.Reason = FileSegmentChangedReason.Tailed;
            this.Info = previous.Info;
            var last = previous.Tail;
            this.TailStartsAt = last.End;
            var segments = previous.Segments;
            segments[segments.Length - 1] = new FileSegment(last, newLength);
            this.Segments = segments;
            this.Count = this.Segments.Length;
            this.FileSize = newLength;
        }
        #endregion
        #region Properties
        public int Count { get; }
        public long FileLength => this.Tail.End;
        public long FileSize { get; }
        public FileInfo Info { get; }
        public FileSegmentChangedReason Reason { get; }
        public FileSegment[] Segments { get; }
        public long SizeDiff { get; }
        public FileSegment Tail => this.Segments[this.Count - 1];
        public long TailStartsAt { get; }
        #endregion
        #region Methods
        public static bool operator ==(FileSegmentCollection left, FileSegmentCollection right) => object.Equals(left, right);
        public static bool operator !=(FileSegmentCollection left, FileSegmentCollection right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileSegmentCollection)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Segments?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.TailStartsAt.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Count;
                hashCode = (hashCode * 397) ^ (int)this.Reason;
                return hashCode;
            }
        }
        public bool Equals(FileSegmentCollection other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.Segments, other.Segments) && this.TailStartsAt == other.TailStartsAt && this.Count == other.Count && this.Reason == other.Reason;
        }
        #endregion
    }
}