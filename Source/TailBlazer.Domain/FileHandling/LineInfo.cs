namespace TailBlazer.Domain.FileHandling
{
    using System;
    public struct LineInfo : IEquatable<LineInfo>
    {
        #region Constructors
        public LineInfo(int line, int index, long startPosition, long endPosition)
        {
            this.Line = line;
            this.Index = index;
            this.Start = startPosition;
            this.End = endPosition;
            this.Offset = 0;
            this.Type = LineIndexType.Absolute;
        }
        #endregion
        #region Properties
        public int Index { get; }
        public int Line { get; }
        public int Offset { get; }
        public long Start { get; }
        private long End { get; }
        private long Size => this.End - this.Start;
        private LineIndexType Type { get; }
        #endregion
        #region Methods
        public static bool operator ==(LineInfo left, LineInfo right) => left.Equals(right);
        public static bool operator !=(LineInfo left, LineInfo right) => !left.Equals(right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return obj is LineInfo info && this.Equals(info);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Line.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Index;
                hashCode = (hashCode * 397) ^ this.Start.GetHashCode();
                hashCode = (hashCode * 397) ^ this.End.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Offset;
                hashCode = (hashCode * 397) ^ (int)this.Type;
                return hashCode;
            }
        }
        public override string ToString()
        {
            if (this.Type == LineIndexType.Relative)
                return $"{this.Index} ({this.Line}) {this.Start}+{this.Offset}";
            return $"{this.Index} ({this.Line}) {this.Start}->{this.End}, {this.Size}b";
        }
        public bool Equals(LineInfo other) => this.Line == other.Line && this.Index == other.Index && this.Start == other.Start && this.End == other.End && this.Offset == other.Offset && this.Type == other.Type;
        #endregion
    }
}