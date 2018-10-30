namespace TailBlazer.Domain.FileHandling
{
    using System;
    public struct LineKey : IEquatable<LineKey>
    {
        #region Fields
        private readonly long _start;
        private readonly string _text;
        #endregion
        #region Constructors
        public LineKey(string text, long start)
        {
            this._text = text;
            this._start = start;
        }
        #endregion
        #region Methods
        public static bool operator ==(LineKey left, LineKey right) => left.Equals(right);
        public static bool operator !=(LineKey left, LineKey right) => !left.Equals(right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is LineKey && this.Equals((LineKey)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this._text?.GetHashCode() ?? 0) * 397) ^ this._start.GetHashCode();
            }
        }
        public override string ToString() => $"{this._text} (@{this._start})";
        public bool Equals(LineKey other) => string.Equals(this._text, other._text) && this._start == other._start;
        #endregion
    }
}