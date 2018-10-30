namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Windows.Media;
    using TailBlazer.Domain.Annotations;
    public class Hue : IEquatable<Hue>
    {
        #region Fields
        public static readonly Hue NotSpecified = new Hue();
        #endregion
        #region Constructors
        public Hue([NotNull] string swatch, [NotNull] string name, Color foreground, Color background)
        {
            if (swatch == null) throw new ArgumentNullException(nameof(swatch));
            if (name == null) throw new ArgumentNullException(nameof(name));
            this.Key = new HueKey(swatch, name);
            this.Swatch = swatch;
            this.Name = name;
            this.Foreground = foreground;
            this.Background = background;
            this.ForegroundBrush = new SolidColorBrush(foreground);
            this.ForegroundBrush.Freeze();
            this.BackgroundBrush = new SolidColorBrush(background);
            this.BackgroundBrush.Freeze();
        }
        private Hue()
        {
            this.Swatch = "<None>";
            this.Name = "<None>";
            this.Key = new HueKey(this.Swatch, this.Name);
        }
        #endregion
        #region Properties
        public Color Background { get; }
        public Brush BackgroundBrush { get; }
        public Color Foreground { get; }
        public Brush ForegroundBrush { get; }
        public HueKey Key { get; }
        public string Name { get; }
        public string Swatch { get; }
        #endregion
        #region Methods
        public static bool operator ==(Hue left, Hue right) => object.Equals(left, right);
        public static bool operator !=(Hue left, Hue right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Hue)obj);
        }
        public override int GetHashCode() => this.Key.GetHashCode();
        public override string ToString() => $"{this.Swatch} ({this.Name})";
        public bool Equals(Hue other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Key.Equals(other.Key);
        }
        #endregion
    }
}