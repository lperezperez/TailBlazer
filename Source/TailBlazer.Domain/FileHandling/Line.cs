namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections.Generic;
    public class Line : IEquatable<Line>
    {
        #region Constructors
        [Obsolete("Only used for testing")]
        public Line(int number, string text, DateTime? timestamp)
        {
            this.Number = number;
            this.Start = number;
            this.Text = text ?? string.Empty;
            this.Timestamp = timestamp;
            this.Key = new LineKey(text, number);
        }
        public Line(LineInfo lineInfo, string text, DateTime? timestamp)
        {
            this.LineInfo = lineInfo;
            this.Text = text ?? string.Empty;
            this.Timestamp = timestamp;
            this.Number = this.LineInfo.Line;
            this.Start = this.LineInfo.Start;
            this.Index = this.LineInfo.Index;
            this.Key = new LineKey(text, lineInfo.Start);
        }
        #endregion
        #region Properties
        public static IEqualityComparer<Line> TextStartComparer { get; } = new TextStartEqualityComparer();
        public int Index { get; }
        public LineKey Key { get; }
        public LineInfo LineInfo { get; }
        public int Number { get; }
        public string Text { get; }
        public DateTime? Timestamp { get; }
        private long Start { get; }
        #endregion
        #region Methods
        public static bool operator ==(Line left, Line right) => object.Equals(left, right);
        public static bool operator !=(Line left, Line right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Line)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Text?.GetHashCode() ?? 0) * 397) ^ this.LineInfo.GetHashCode();
            }
        }
        public override string ToString() => $"{this.Number}: {this.Text}";
        public bool Equals(Line other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this.Text, other.Text) && this.LineInfo.Equals(other.LineInfo);
        }
        #endregion
        #region Classes
        private sealed class TextStartEqualityComparer : IEqualityComparer<Line>
        {
            #region Methods
            public bool Equals(Line x, Line y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (object.ReferenceEquals(x, null)) return false;
                if (object.ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Text, y.Text) && x.Start == y.Start;
            }
            public int GetHashCode(Line obj)
            {
                unchecked
                {
                    return ((obj.Text?.GetHashCode() ?? 0) * 397) ^ obj.Start.GetHashCode();
                }
            }
            #endregion
        }
        #endregion
    }
}