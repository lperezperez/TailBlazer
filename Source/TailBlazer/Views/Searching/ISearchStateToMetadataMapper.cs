namespace TailBlazer.Views.Searching
{
    using TailBlazer.Domain.FileHandling.Search;
    public interface ISearchStateToMetadataMapper
    {
        #region Methods
        SearchMetadata Map(SearchState state, bool isGlobal = false);
        SearchState Map(SearchMetadata search);
        #endregion
    }
}