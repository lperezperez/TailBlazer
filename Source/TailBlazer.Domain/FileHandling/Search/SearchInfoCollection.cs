namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using TailBlazer.Domain.Annotations;
    public sealed class SearchInfoCollection : ISearchInfoCollection
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ICombinedSearchMetadataCollection _combinedSearchMetadataCollection;
        private readonly IFileWatcher _fileWatcher;
        private readonly ISearchMetadataCollection _localMetadataCollection;
        private readonly ISearchMetadataFactory _searchMetadataFactory;
        #endregion
        #region Constructors
        public SearchInfoCollection(ICombinedSearchMetadataCollection combinedSearchMetadataCollection, ISearchMetadataFactory searchMetadataFactory, IFileWatcher fileWatcher)
        {
            this._localMetadataCollection = combinedSearchMetadataCollection.Local;
            this._combinedSearchMetadataCollection = combinedSearchMetadataCollection;
            this._searchMetadataFactory = searchMetadataFactory;
            this._fileWatcher = fileWatcher;
            var exclusionPredicate = combinedSearchMetadataCollection.Combined.Connect().IncludeUpdateWhen((current, previous) => !SearchMetadata.EffectsFilterComparer.Equals(current, previous)).Filter(meta => meta.IsExclusion).ToCollection().Select
                (
                 searchMetadataItems =>
                     {
                         Func<string, bool> predicate = null;
                         if (searchMetadataItems.Count == 0)
                             return predicate;
                         var predicates = searchMetadataItems.Select(meta => meta.BuildPredicate()).ToArray();
                         predicate = str => { return !predicates.Any(item => item(str)); };
                         return predicate;
                     }).StartWith((Func<string, bool>)null).Replay(1).RefCount();
            this.All = exclusionPredicate.Select
                (
                 predicate =>
                     {
                         if (predicate == null)
                             return this._fileWatcher.Latest.Index();
                         return this._fileWatcher.Latest.Search(predicate);
                     }).Switch().Replay(1).RefCount();

            //create a collection with 1 item, which is used to show entire file
            var systemSearches = new SourceCache<SearchInfo, string>(t => t.SearchText);
            systemSearches.AddOrUpdate(new SearchInfo("<All>", false, this.All, SearchType.All));

            //create a collection of all possible user filters
            var userSearches = combinedSearchMetadataCollection.Combined.Connect(meta => meta.Filter).IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsFilterComparer.Equals(current, previous)).Transform
                (
                 meta =>
                     {
                         var latest = exclusionPredicate.Select
                             (
                              exclpredicate =>
                                  {
                                      Func<string, bool> resultingPredicate;
                                      if (exclpredicate == null)
                                      {
                                          resultingPredicate = meta.BuildPredicate();
                                      }
                                      else
                                      {
                                          var toMatch = meta.BuildPredicate();
                                          resultingPredicate = str => toMatch(str) && exclpredicate(str);
                                      }
                                      return this._fileWatcher.Latest.Search(resultingPredicate);
                                  }).Switch().Replay(1).RefCount();
                         return new SearchInfo(meta.SearchText, meta.IsGlobal, latest, SearchType.User);
                     });

            //combine the results into a single collection
            this.Searches = systemSearches.Connect().Or(userSearches).AsObservableCache();
            this._cleanUp = new CompositeDisposable(this.Searches, systemSearches);
        }
        #endregion
        #region Properties
        public IObservable<ILineProvider> All { get; }
        public IObservableCache<SearchInfo, string> Searches { get; }
        #endregion
        #region Methods
        public void Add([NotNull] string searchText, bool useRegex)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            var index = this._localMetadataCollection.NextIndex();
            var metatdata = this._searchMetadataFactory.Create(searchText, useRegex, index, true);
            this._localMetadataCollection.AddorUpdate(metatdata);
        }
        public void Dispose() { this._cleanUp.Dispose(); }
        public void Remove(string searchText)
        {
            var item = this.Searches.Lookup(searchText);
            if (!item.HasValue) return;
            if (!item.Value.IsGlobal)
                this._localMetadataCollection.Remove(searchText);
            else
                this._combinedSearchMetadataCollection.Global.Remove(searchText);
        }
        #endregion
    }
}