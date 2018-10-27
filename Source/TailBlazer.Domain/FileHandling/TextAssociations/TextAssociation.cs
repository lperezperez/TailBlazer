namespace TailBlazer.Domain.FileHandling.TextAssociations
{
    using System;
    public sealed class TextAssociation
    {
        #region Constructors
        public TextAssociation(string text, bool ignoreCase, bool useRegEx, string swatch, string icon, string hue, DateTime dateTime)
        {
            this.Text = text;
            this.IgnoreCase = ignoreCase;
            this.UseRegEx = useRegEx;
            this.Swatch = swatch;
            this.Icon = icon;
            this.Hue = hue;
            this.DateTime = dateTime;
        }
        #endregion
        #region Properties
        public DateTime DateTime { get; }
        public string Hue { get; }
        public string Icon { get; }
        public bool IgnoreCase { get; }
        public string Swatch { get; }
        public string Text { get; }
        public bool UseRegEx { get; }
        #endregion
    }
}