namespace TailBlazer.Views.Searching
{
    using DynamicData.Kernel;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Formatting;
    public class SearchStateToMetadataMapper : ISearchStateToMetadataMapper
    {
        #region Fields
        private readonly IColourProvider _colourProvider;
        #endregion
        #region Constructors
        public SearchStateToMetadataMapper(IColourProvider colourProvider) { this._colourProvider = colourProvider; }
        #endregion
        #region Methods
        public SearchMetadata Map(SearchState state, bool isGlobal = false)
        {
            var hue = this._colourProvider.Lookup(new HueKey(state.Swatch, state.Hue)).ValueOr(() => this._colourProvider.DefaultAccent);
            return new SearchMetadata(state.Position, state.Text, state.Filter, state.Highlight, state.UseRegEx, state.IgnoreCase, hue, state.Icon, isGlobal, state.IsExclusion);
        }
        public SearchState Map(SearchMetadata search) => new SearchState(search.SearchText, search.Position, search.UseRegex, search.Highlight, search.Filter, false, search.IgnoreCase, search.HighlightHue.Swatch, search.IconKind, search.HighlightHue.Name, search.IsExclusion);
        #endregion
    }
}