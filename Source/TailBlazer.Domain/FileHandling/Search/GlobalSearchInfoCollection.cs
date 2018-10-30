namespace TailBlazer.Domain.FileHandling.Search
{
    using TailBlazer.Domain.Infrastructure;
    public sealed class GlobalSearchInfoCollection //: ISearchInfoCollection
    {
        #region Fields
        private readonly ISearchMetadataCollection _searchMetadataCollection;
        #endregion
        #region Constructors
        public GlobalSearchInfoCollection(ISearchMetadataCollection searchMetadataCollection, ILogger logger) { this._searchMetadataCollection = searchMetadataCollection; }
        #endregion
    }
}