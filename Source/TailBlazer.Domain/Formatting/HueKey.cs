namespace TailBlazer.Domain.Formatting
{
    using System;
    public struct HueKey : IEquatable<HueKey>
    {
        #region Constructors
        public HueKey(string swatch, string name)
        {
            this.Swatch = swatch;
            this.Name = name;
        }
        #endregion
        #region Properties
        public CaseInsensitiveString Name { get; }
        public CaseInsensitiveString Swatch { get; }
        #endregion
        #region Methods
        public static bool operator ==(HueKey left, HueKey right) => left.Equals(right);
        public static bool operator !=(HueKey left, HueKey right) => !left.Equals(right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is HueKey && this.Equals((HueKey)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Swatch.GetHashCode() * 397) ^ this.Name.GetHashCode();
            }
        }
        public override string ToString() => $"{this.Swatch} ({this.Name})";
        public bool Equals(HueKey other) => string.Equals(this.Swatch, other.Swatch) && string.Equals(this.Name, other.Name);
        #endregion
    }
}