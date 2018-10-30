namespace TailBlazer.Views.Layout
{
    using System;
    using System.Windows;
    public class ShellSettings : IEquatable<ShellSettings>
    {
        #region Constructors
        public ShellSettings(double top, double left, double width, double height, WindowState state)
        {
            this.Top = top;
            this.Left = left;
            this.Width = width;
            this.Height = height;
            this.State = state;
        }
        #endregion
        #region Properties
        public double Height { get; }
        public double Left { get; }
        public WindowState State { get; }
        public double Top { get; }
        public double Width { get; }
        #endregion
        #region Methods
        public static bool operator ==(ShellSettings left, ShellSettings right) => object.Equals(left, right);
        public static bool operator !=(ShellSettings left, ShellSettings right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((ShellSettings)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Top.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Left.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Width.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Height.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.State;
                return hashCode;
            }
        }
        public override string ToString() => $"Location: ({this.Top},{this.Left}).  Size({this.Width},{this.Height}). State: {this.State}";
        public bool Equals(ShellSettings other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Top.Equals(other.Top) && this.Left.Equals(other.Left) && this.Width.Equals(other.Width) && this.Height.Equals(other.Height) && this.State == other.State;
        }
        #endregion
    }
}