namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using TailBlazer.Domain.Annotations;
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CombinedSearchMetadataCollection : ICombinedSearchMetadataCollection
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public CombinedSearchMetadataCollection([NotNull] ISearchMetadataCollection metadataCollection, [NotNull] IGlobalSearchOptions globalSearchOptions)
        {
            if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));
            if (globalSearchOptions == null) throw new ArgumentNullException(nameof(globalSearchOptions));
            this.Local = metadataCollection;
            this.Global = globalSearchOptions.Metadata;
            var cache = new SourceCache<SearchMetadata, string>(t => t.SearchText);

            ////Prioritise local before global and renumber
            var localItems = metadataCollection.Metadata.Connect().ToCollection().Select(items => items.ToArray()).StartWith(Enumerable.Empty<SearchMetadata>());
            var globalItems = globalSearchOptions.Metadata.Metadata.Connect().ToCollection().Select(items => items.ToArray()).StartWith(Enumerable.Empty<SearchMetadata>());
            var combiner = localItems.CombineLatest(globalItems, (local, global) => new { local, global }).Select(x => this.Combine(x.local, x.global)).Subscribe
                (
                 uppdatedItems =>
                     {
                         cache.Edit
                             (
                              innerCache =>
                                  {
                                      var toRemove = innerCache.Items.Except(uppdatedItems, SearchMetadata.SearchTextComparer).ToArray();
                                      innerCache.Remove(toRemove);
                                      innerCache.AddOrUpdate(uppdatedItems);
                                  });
                     });
            this.Combined = cache.Connect().IgnoreUpdateWhen((current, previous) => current.Equals(previous)).AsObservableCache();
            this._cleanUp = new CompositeDisposable(this.Combined, cache, combiner);
        }
        #endregion
        #region Properties
        public IObservableCache<SearchMetadata, string> Combined { get; }
        public ISearchMetadataCollection Global { get; }
        public ISearchMetadataCollection Local { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private SearchMetadata[] Combine(IEnumerable<SearchMetadata> local, IEnumerable<SearchMetadata> global)
        {
            var i = 0;
            var dictionary = new Dictionary<string, SearchMetadata>();
            foreach (var meta in local.OrderBy(meta => meta.Position))
            {
                dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                i++;
            }
            foreach (var meta in global.OrderBy(meta => meta.Position))
            {
                if (dictionary.ContainsKey(meta.SearchText)) continue;
                dictionary[meta.SearchText] = new SearchMetadata(meta, i);
                i++;
            }
            return dictionary.Values.ToArray();
        }
        #endregion
    }
}