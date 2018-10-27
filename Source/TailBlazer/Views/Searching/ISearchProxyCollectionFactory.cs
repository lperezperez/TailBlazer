namespace TailBlazer.Views.Searching
{
    using System;
    using TailBlazer.Domain.FileHandling.Search;
    public interface ISearchProxyCollectionFactory
    {
        #region Methods
        ISearchProxyCollection Create(ISearchMetadataCollection metadataCollection, Guid id, Action<SearchMetadata> changeScopeAction);
        #endregion
    }
}