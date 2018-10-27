namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using DynamicData;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    public sealed class SearchMetadataCollection : ISearchMetadataCollection
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ILogger _logger;
        private readonly ISourceCache<SearchMetadata, string> _searches = new SourceCache<SearchMetadata, string>(t => t.SearchText);
        #endregion
        #region Constructors
        public SearchMetadataCollection([NotNull] ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            this._logger = logger;
            this.Metadata = this._searches.AsObservableCache();
            this._cleanUp = new CompositeDisposable(this._searches, this.Metadata);
        }
        #endregion
        #region Properties
        public IObservableCache<SearchMetadata, string> Metadata { get; }
        #endregion
        #region Methods
        public void Add([NotNull] IEnumerable<SearchMetadata> metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            var searchMetadatas = metadata.AsArray();
            this._searches.AddOrUpdate(searchMetadatas);
            this._logger.Info("{0} SearchMetadata has been loaded", searchMetadatas.Count());
        }
        public void AddorUpdate([NotNull] SearchMetadata metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            this._searches.AddOrUpdate(metadata);
            this._logger.Info("Search metadata has changed: {0}", metadata);
        }
        public void Dispose() { this._cleanUp.Dispose(); }
        public int NextIndex()
        {
            if (this._searches.Count == 0)
                return 0;
            return this._searches.Items.Select(m => m.Position).Max() + 1;
        }
        public void Remove(string searchText)
        {
            this._searches.Remove(searchText);
            this._logger.Info("Search metadata has been removed: {0}", searchText);
        }
        #endregion
    }
}