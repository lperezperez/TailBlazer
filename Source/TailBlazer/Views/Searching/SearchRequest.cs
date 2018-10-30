namespace TailBlazer.Views.Searching
{
    public class SearchRequest
    {
        #region Constructors
        public SearchRequest(string text, bool useRegEx)
        {
            this.Text = text;
            this.UseRegEx = useRegEx;
        }
        #endregion
        #region Properties
        public bool IsExclusion => this.Text.Substring(0, 1) == "-";
        public string Text { get; }
        public string TextWithoutExclusion => this.IsExclusion ? this.Text.Substring(1, this.Text.Length - 1) : this.Text;
        public bool UseRegEx { get; }
        #endregion
    }
}