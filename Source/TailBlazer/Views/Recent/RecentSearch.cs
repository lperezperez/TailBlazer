namespace TailBlazer.Views.Recent
{
    using System;
    public class RecentSearch : IEquatable<RecentSearch>
    {
        #region Constructors
        public RecentSearch(string seaarchText)
        {
            this.Text = seaarchText;
            this.Timestamp = DateTime.UtcNow;
        }
        public RecentSearch(DateTime timestamp, string text)
        {
            this.Timestamp = timestamp;
            this.Text = text;
        }
        #endregion
        #region Properties
        public string Text { get; }
        public DateTime Timestamp { get; }
        #endregion
        #region Methods
        public static bool operator ==(RecentSearch left, RecentSearch right) => object.Equals(left, right);
        public static bool operator !=(RecentSearch left, RecentSearch right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RecentSearch)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Timestamp.GetHashCode() * 397) ^ (this.Text?.GetHashCode() ?? 0);
            }
        }
        public override string ToString() => $"{this.Text} ({this.Timestamp})";
        public bool Equals(RecentSearch other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Timestamp.Equals(other.Timestamp) && string.Equals(this.Text, other.Text);
        }
        #endregion
    }
}