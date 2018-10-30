namespace TailBlazer.Domain.Formatting
{
    public class DisplayText
    {
        #region Constructors
        public DisplayText(MatchedString matchedString)
        {
            this.Text = matchedString.Part;
            this.Highlight = matchedString.IsMatch;
            this.Hue = matchedString.Hue;
        }
        public DisplayText(DisplayText displayText, string text)
        {
            this.Text = text;
            this.Highlight = displayText.Highlight;
            this.Hue = displayText.Hue;
        }
        #endregion
        #region Properties
        public bool Highlight { get; }
        public Hue Hue { get; }
        public int Length => this.Text.Length;
        public string Text { get; }
        #endregion
    }
}