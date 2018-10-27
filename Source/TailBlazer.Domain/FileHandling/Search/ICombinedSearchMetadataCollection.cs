namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using DynamicData;
    public interface ICombinedSearchMetadataCollection : IDisposable
    {
        #region Properties
        IObservableCache<SearchMetadata, string> Combined { get; }
        ISearchMetadataCollection Global { get; }
        ISearchMetadataCollection Local { get; }
        #endregion
    }
}