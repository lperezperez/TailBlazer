namespace TailBlazer.Domain.Formatting
{
    using TailBlazer.Domain.FileHandling.Search;
    public class LineMatch
    {
        #region Fields
        private readonly SearchMetadata _searchMetadata;
        #endregion
        #region Constructors
        public LineMatch(SearchMetadata searchMetadata) { this._searchMetadata = searchMetadata; }
        #endregion
        #region Properties
        public Hue Hue => this._searchMetadata.HighlightHue;
        public string Icon => this._searchMetadata.IconKind;
        public string Text => this._searchMetadata.SearchText;
        public bool UseRegex => this._searchMetadata.UseRegex;
        #endregion
    }
}