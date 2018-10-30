namespace TailBlazer.Infrastucture
{
    using System;
    public class HeaderedView : IEquatable<HeaderedView>
    {
        #region Constructors
        public HeaderedView(object header, object content)
        {
            this.Header = header;
            this.Content = content;
        }
        #endregion
        #region Properties
        public object Content { get; }
        public object Header { get; }
        public Guid Id { get; } = Guid.NewGuid();
        #endregion
        #region Methods
        public static bool operator ==(HeaderedView left, HeaderedView right) => object.Equals(left, right);
        public static bool operator !=(HeaderedView left, HeaderedView right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((HeaderedView)obj);
        }
        public override int GetHashCode() => this.Id.GetHashCode();
        public bool Equals(HeaderedView other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Id.Equals(other.Id);
        }
        #endregion
    }
}