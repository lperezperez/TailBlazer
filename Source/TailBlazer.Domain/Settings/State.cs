namespace TailBlazer.Domain.Settings
{
    using System;
    public class State : IEquatable<State>
    {
        #region Fields
        public static readonly State Empty = new State(0, string.Empty);
        #endregion
        #region Constructors
        public State(int version, string value)
        {
            this.Version = version;
            this.Value = value;
        }
        #endregion
        #region Properties
        public string Value { get; }
        public int Version { get; }
        #endregion
        #region Methods
        public static bool operator ==(State left, State right) => object.Equals(left, right);
        public static bool operator !=(State left, State right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((State)obj);
        }
        public override int GetHashCode() => this.Value?.GetHashCode() ?? 0;
        public override string ToString() => $"Version: {this.Version}, Value: {this.Value}";
        public bool Equals(State other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this.Value, other.Value);
        }
        #endregion
    }
}