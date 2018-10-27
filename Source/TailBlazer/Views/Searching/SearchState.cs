namespace TailBlazer.Views.Searching
{
    public sealed class SearchState
    {
        #region Constructors
        public SearchState(string text, int position, bool useRegEx, bool highlight, bool filter, bool alert, bool ignoreCase, string swatch, string icon, string hue, bool isExclusion)
        {
            this.Text = text;
            this.Position = position;
            this.UseRegEx = useRegEx;
            this.Highlight = highlight;
            this.Filter = filter;
            this.Alert = alert;
            this.IgnoreCase = ignoreCase;
            this.Swatch = swatch;
            this.Icon = icon;
            this.Hue = hue;
            this.IsExclusion = isExclusion;
        }
        #endregion
        #region Properties
        public bool Alert { get; }
        public bool Filter { get; }
        public bool Highlight { get; }
        public string Hue { get; }
        public string Icon { get; }
        public bool IgnoreCase { get; }
        public bool IsExclusion { get; }
        public int Position { get; }
        public string Swatch { get; }
        public string Text { get; }
        public bool UseRegEx { get; }
        #endregion
    }
}