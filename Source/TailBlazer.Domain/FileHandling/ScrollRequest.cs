namespace TailBlazer.Domain.FileHandling
{
    using System;
    public sealed class ScrollRequest : IEquatable<ScrollRequest>
    {
        #region Fields
        public static readonly ScrollRequest None = new ScrollRequest();
        #endregion
        #region Constructors
        public ScrollRequest(int pageSize)
        {
            this.PageSize = pageSize;
            this.Mode = ScrollReason.Tail;
        }
        public ScrollRequest(int pageSize, long position)
        {
            this.PageSize = pageSize;
            this.Position = position;
            this.SpecifiedByPosition = true;
            this.Mode = ScrollReason.User;
        }
        public ScrollRequest(int pageSize, int firstIndex, bool specifiedByPosition = false)
        {
            this.PageSize = pageSize;
            this.FirstIndex = firstIndex;
            this.SpecifiedByPosition = specifiedByPosition;
            this.Mode = ScrollReason.User;
        }
        public ScrollRequest(ScrollReason mode, int pageSize, long position)
        {
            this.PageSize = pageSize;
            this.Mode = mode;
            this.Position = position;
            this.SpecifiedByPosition = true;
        }
        public ScrollRequest(ScrollReason mode, int pageSize, int firstIndex)
        {
            this.PageSize = pageSize;
            this.FirstIndex = firstIndex;
            this.Mode = mode;
            this.SpecifiedByPosition = false;
        }
        private ScrollRequest() { }
        #endregion
        #region Properties
        public int FirstIndex { get; }
        public ScrollReason Mode { get; }
        public int PageSize { get; }
        public long Position { get; }
        public bool SpecifiedByPosition { get; }
        #endregion
        #region Methods
        public static bool operator ==(ScrollRequest left, ScrollRequest right) => object.Equals(left, right);
        public static bool operator !=(ScrollRequest left, ScrollRequest right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ScrollRequest)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.PageSize;
                hashCode = (hashCode * 397) ^ this.FirstIndex;
                hashCode = (hashCode * 397) ^ this.Position.GetHashCode();
                hashCode = (hashCode * 397) ^ this.SpecifiedByPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.Mode;
                return hashCode;
            }
        }
        public bool Equals(ScrollRequest other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            if (this.Mode == ScrollReason.Tail)
                return this.PageSize == other.PageSize && this.Mode == other.Mode;
            return this.PageSize == other.PageSize && this.FirstIndex == other.FirstIndex && this.Position == other.Position && this.Mode == other.Mode;
        }
        #endregion
    }
}