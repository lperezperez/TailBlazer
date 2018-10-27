namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Collections.Generic;
    using DynamicData;
    public interface ISearchMetadataCollection : IDisposable
    {
        #region Properties
        IObservableCache<SearchMetadata, string> Metadata { get; }
        #endregion
        #region Methods
        void Add(IEnumerable<SearchMetadata> metadata);
        void AddorUpdate(SearchMetadata metadata);
        int NextIndex();
        void Remove(string searchText);
        #endregion
    }
}