namespace TailBlazer.Domain
{
    using System;
    public struct CaseInsensitiveString : IEquatable<CaseInsensitiveString>
    {
        #region Fields
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        private readonly string _original;
        #endregion
        #region Constructors
        public CaseInsensitiveString(string source) { this._original = source; }
        #endregion
        #region Methods
        public static bool operator ==(CaseInsensitiveString left, CaseInsensitiveString right) => Comparer.Equals(left, right);
        public static implicit operator string(CaseInsensitiveString source) => source._original;
        public static implicit operator CaseInsensitiveString(string source) => new CaseInsensitiveString(source);
        public static bool operator !=(CaseInsensitiveString left, CaseInsensitiveString right) => !Comparer.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is CaseInsensitiveString && this.Equals((CaseInsensitiveString)obj);
        }
        public override int GetHashCode() => Comparer.GetHashCode(this);
        public override string ToString() => this._original;
        public bool Equals(CaseInsensitiveString other) => Comparer.Equals(this._original, other);
        #endregion
    }
}