namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.IO;
    public sealed class FileNotification : IEquatable<FileNotification>
    {
        #region Constructors
        public FileNotification(FileInfo fileInfo)
        {
            fileInfo.Refresh();
            this.Info = fileInfo;
            this.Exists = fileInfo.Exists;
            if (this.Exists)
            {
                this.NotificationType = FileNotificationType.CreatedOrOpened;
                this.Size = this.Info.Length;
            }
            else
            {
                this.NotificationType = FileNotificationType.Missing;
            }
        }
        public FileNotification(FileInfo fileInfo, Exception error)
        {
            this.Info = fileInfo;
            this.Error = error;
            this.Exists = false;
            this.NotificationType = FileNotificationType.Error;
        }
        public FileNotification(FileNotification previous)
        {
            previous.Info.Refresh();
            this.Info = previous.Info;
            this.Exists = this.Info.Exists;
            if (this.Exists)
            {
                this.Size = this.Info.Length;
                if (!previous.Exists)
                    this.NotificationType = FileNotificationType.CreatedOrOpened;
                else if (this.Size > previous.Size)
                    this.NotificationType = FileNotificationType.Changed;
                else if (this.Size < previous.Size)
                    this.NotificationType = FileNotificationType.CreatedOrOpened;
                else
                    this.NotificationType = FileNotificationType.None;
            }
            else
            {
                this.NotificationType = FileNotificationType.Missing;
            }
        }
        #endregion
        #region Properties
        public Exception Error { get; }
        public bool Exists { get; }
        public string Folder => this.Info.DirectoryName;
        public string FullName => this.Info.FullName;
        public string Name => this.Info.Name;
        public FileNotificationType NotificationType { get; }
        public long Size { get; }
        private FileInfo Info { get; }
        #endregion
        #region Methods
        public static bool operator ==(FileNotification left, FileNotification right) => object.Equals(left, right);
        public static explicit operator FileInfo(FileNotification source) => source.Info;
        public static bool operator !=(FileNotification left, FileNotification right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((FileNotification)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.FullName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ this.Exists.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Size.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.NotificationType;
                return hashCode;
            }
        }
        public override string ToString() => $"{this.Name}  Size: {this.Size}, Type: {this.NotificationType}";
        public bool Equals(FileNotification other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this.FullName, other.FullName) && this.Exists == other.Exists && this.Size == other.Size && this.NotificationType == other.NotificationType;
        }
        #endregion
    }
}