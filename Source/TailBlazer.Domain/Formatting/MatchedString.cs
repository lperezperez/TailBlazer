namespace TailBlazer.Domain.Formatting
{
    using System;
    using TailBlazer.Domain.FileHandling.Search;
    public class MatchedString : IEquatable<MatchedString>
    {
        #region Fields
        private readonly SearchMetadata _metadata;
        #endregion
        #region Constructors
        public MatchedString(string part)
        {
            this.Part = part;
            this.IsMatch = false;
        }
        public MatchedString(string part, bool isMatch)
        {
            this.Part = part;
            this.IsMatch = isMatch;
        }
        public MatchedString(string part, SearchMetadata metadata)
        {
            this._metadata = metadata;
            this.Part = part;
            this.IsMatch = true;
        }
        #endregion
        #region Properties
        public Hue Hue => this._metadata?.HighlightHue;
        public bool IsMatch { get; }
        public string Part { get; }
        #endregion
        #region Methods
        public static bool operator ==(MatchedString left, MatchedString right) => object.Equals(left, right);
        public static bool operator !=(MatchedString left, MatchedString right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((MatchedString)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Part?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.IsMatch.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString() => $"{this.Part}, ({this.IsMatch})";
        public bool Equals(MatchedString other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this.Part, other.Part) && this.IsMatch == other.IsMatch;
        }
        #endregion
    }
}