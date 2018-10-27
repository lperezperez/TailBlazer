namespace TailBlazer.Domain.FileHandling
{
    using System;
    public sealed class FileSegment : IEquatable<FileSegment>
    {
        #region Constructors
        public FileSegment(int index, long start, long end, FileSegmentType type)
        {
            this.Index = index;
            this.Start = start;
            this.End = end;
            this.Type = type;
            this.Key = new FileSegmentKey(index, type);
        }
        public FileSegment(FileSegment previous, long end)
        {
            this.Index = previous.Index;
            this.Start = previous.Start;
            this.End = end;
            this.Type = previous.Type;
            this.Key = previous.Key;
        }
        #endregion
        #region Properties
        public long End { get; }
        public int Index { get; }
        public FileSegmentKey Key { get; }
        public long Size => this.End - this.Start;
        public long Start { get; }
        public FileSegmentType Type { get; }
        #endregion
        #region Methods
        public static bool operator ==(FileSegment left, FileSegment right) => object.Equals(left, right);
        public static bool operator !=(FileSegment left, FileSegment right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileSegment)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Index;
                hashCode = (hashCode * 397) ^ this.Start.GetHashCode();
                hashCode = (hashCode * 397) ^ this.End.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.Type;
                hashCode = (hashCode * 397) ^ this.Key.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString() => $"{this.Index} {this.Type}. {this.Start}->{this.End} [{this.Size.FormatWithAbbreviation()}] ";
        public bool Equals(FileSegment other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Index == other.Index && this.Start == other.Start && this.End == other.End && this.Type == other.Type && this.Key.Equals(other.Key);
        }
        #endregion
    }
}