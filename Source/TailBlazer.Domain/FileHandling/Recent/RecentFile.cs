namespace TailBlazer.Domain.FileHandling.Recent
{
    using System;
    using System.IO;
    public class RecentFile : IEquatable<RecentFile>
    {
        #region Constructors
        public RecentFile(FileInfo fileInfo)
        {
            this.Name = fileInfo.FullName;
            this.Timestamp = DateTime.UtcNow;
        }
        public RecentFile(DateTime timestamp, string name)
        {
            this.Timestamp = timestamp;
            this.Name = name;
        }
        #endregion
        #region Properties
        public string Name { get; }
        public DateTime Timestamp { get; }
        #endregion
        #region Methods
        public static bool operator ==(RecentFile left, RecentFile right) => object.Equals(left, right);
        public static bool operator !=(RecentFile left, RecentFile right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RecentFile)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Timestamp.GetHashCode() * 397) ^ (this.Name?.GetHashCode() ?? 0);
            }
        }
        public override string ToString() => $"{this.Name} ({this.Timestamp})";
        public bool Equals(RecentFile other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Timestamp.Equals(other.Timestamp) && string.Equals(this.Name, other.Name);
        }
        #endregion
    }
}