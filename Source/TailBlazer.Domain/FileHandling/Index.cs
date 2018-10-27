namespace TailBlazer.Domain.FileHandling
{
    using System;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    public class Index : IEquatable<Index>
    {
        #region Constructors
        public Index(long start, long end, long[] indicies, int compression, int lineCount, IndexType type)
        {
            this.Start = start;
            this.End = end;
            this.Indicies = new ImmutableList<long>(indicies);
            this.Compression = compression;
            this.LineCount = lineCount;
            this.Type = type;
        }
        public Index(long start, long end, int compression, int lineCount, IndexType type)
        {
            this.Start = start;
            this.End = end;
            this.Indicies = new ImmutableList<long>();
            this.Compression = compression;
            this.LineCount = lineCount;
            this.Type = type;
        }
        public Index([NotNull] Index latest, Index previous)
        {
            if (latest == null) throw new ArgumentNullException(nameof(latest));
            if (previous == null) throw new ArgumentNullException(nameof(previous));
            this.Start = previous.Start;
            this.End = latest.End;
            this.Compression = latest.Compression;
            this.LineCount = latest.LineCount + previous.LineCount;
            this.Type = latest.Type;

            //combine latest arrays
            this.Indicies = previous.Indicies.Add(latest.Indicies);
        }
        #endregion
        #region Properties
        public int Compression { get; }
        public long End { get; }
        public int IndexCount => this.Indicies.Count;
        public ImmutableList<long> Indicies { get; }
        public int LineCount { get; }
        public long Size => this.End - this.Start;
        public long Start { get; }
        public DateTime TimeStamp { get; } = DateTime.UtcNow;
        public IndexType Type { get; }
        #endregion
        #region Methods
        public static bool operator ==(Index left, Index right) => object.Equals(left, right);
        public static bool operator !=(Index left, Index right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Index)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Start.GetHashCode();
                hashCode = (hashCode * 397) ^ this.End.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Compression;
                hashCode = (hashCode * 397) ^ this.LineCount;
                hashCode = (hashCode * 397) ^ (int)this.Type;
                hashCode = (hashCode * 397) ^ this.TimeStamp.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString() => $"{this.Type} {this.Start}->{this.End}  x{this.Compression} Compression. Lines: {this.LineCount}, Indicies: {this.IndexCount}, @ {this.TimeStamp}";
        public bool Equals(Index other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Start == other.Start && this.End == other.End && this.Compression == other.Compression && this.LineCount == other.LineCount && this.Type == other.Type && this.TimeStamp.Equals(other.TimeStamp);
        }
        #endregion
    }
}