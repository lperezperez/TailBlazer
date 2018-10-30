namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Text.RegularExpressions;
    using MaterialDesignThemes.Wpf;
    public class IconDescription : IEquatable<IconDescription>
    {
        #region Constructors
        public IconDescription(PackIconKind type, string name)
        {
            this.Type = type;
            this.Name = name;
            this.Description = Regex.Replace(name, "(\\B[A-Z])", " $1");
        }
        #endregion
        #region Properties
        public string Description { get; }
        public string Name { get; }
        public PackIconKind Type { get; }
        #endregion
        #region Methods
        public static bool operator ==(IconDescription left, IconDescription right) => object.Equals(left, right);
        public static bool operator !=(IconDescription left, IconDescription right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((IconDescription)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Name?.GetHashCode() ?? 0) * 397) ^ (int)this.Type;
            }
        }
        public override string ToString() => this.Name;
        public bool Equals(IconDescription other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return string.Equals(this.Name, other.Name) && this.Type == other.Type;
        }
        #endregion
    }
}