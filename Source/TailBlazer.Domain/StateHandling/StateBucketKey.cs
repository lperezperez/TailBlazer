namespace TailBlazer.Domain.StateHandling
{
    using System;
    public struct StateBucketKey : IEquatable<StateBucketKey>
    {
        #region Fields
        private readonly string _id;
        private readonly string _type;
        #endregion
        #region Constructors
        public StateBucketKey(string type, string id)
        {
            this._type = type;
            this._id = id;
        }
        #endregion
        #region Methods
        public static bool operator ==(StateBucketKey left, StateBucketKey right) => left.Equals(right);
        public static bool operator !=(StateBucketKey left, StateBucketKey right) => !left.Equals(right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            return obj is StateBucketKey && this.Equals((StateBucketKey)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this._type?.GetHashCode() ?? 0) * 397) ^ (this._id?.GetHashCode() ?? 0);
            }
        }
        public override string ToString() => $"{this._type}, id={this._id}";
        public bool Equals(StateBucketKey other) => string.Equals(this._type, other._type) && string.Equals(this._id, other._id);
        #endregion
    }
}