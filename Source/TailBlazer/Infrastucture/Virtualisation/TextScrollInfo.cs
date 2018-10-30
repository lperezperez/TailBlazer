namespace TailBlazer.Infrastucture.Virtualisation
{
    using System;
    public delegate void TextScrollDelegate(TextScrollInfo textScrollInfo);
    public class TextScrollInfo : IEquatable<TextScrollInfo>
    {
        #region Constructors
        public TextScrollInfo(int firstIndex, int totalChars)
        {
            this.FirstIndex = firstIndex;
            this.TotalChars = totalChars;
        }
        #endregion
        #region Properties
        public int FirstIndex { get; }
        public int TotalChars { get; }
        #endregion
        #region Methods
        public static bool operator ==(TextScrollInfo left, TextScrollInfo right) => object.Equals(left, right);
        public static bool operator !=(TextScrollInfo left, TextScrollInfo right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((TextScrollInfo)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.FirstIndex * 397) ^ this.TotalChars;
            }
        }
        public override string ToString() => $"{this.FirstIndex} Take {this.TotalChars}";
        public bool Equals(TextScrollInfo other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.FirstIndex == other.FirstIndex && this.TotalChars == other.TotalChars;
        }
        #endregion
    }
}