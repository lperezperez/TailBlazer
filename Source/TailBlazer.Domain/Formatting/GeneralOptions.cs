namespace TailBlazer.Domain.Formatting
{
    public class GeneralOptions
    {
        #region Constructors
        public GeneralOptions(Theme theme, bool highlightTail, double highlightTailDuration, double scale, int rating, bool openRecentOnStartup, bool showLineNumbers)
        {
            this.Theme = theme;
            this.HighlightTail = highlightTail;
            this.HighlightDuration = highlightTailDuration;
            this.Scale = scale;
            this.Rating = rating;
            this.OpenRecentOnStartup = openRecentOnStartup;
            this.ShowLineNumbers = showLineNumbers;
        }
        #endregion
        #region Properties
        public double HighlightDuration { get; }
        public bool HighlightTail { get; }
        public bool OpenRecentOnStartup { get; }
        public int Rating { get; }
        public double Scale { get; }
        public bool ShowLineNumbers { get; }
        public Theme Theme { get; }
        #endregion
    }
}