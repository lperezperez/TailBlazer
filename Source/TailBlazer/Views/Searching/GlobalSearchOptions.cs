namespace TailBlazer.Views.Searching
{
    using System;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Settings;
    public class GlobalSearchOptions : IGlobalSearchOptions, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public GlobalSearchOptions(ISearchMetadataCollection metadata, ISearchStateToMetadataMapper converter, ISetting<SearchState[]> searchStateSettings)
        {
            this.Metadata = metadata;
            var loader = searchStateSettings.Value.Take(1).Select(items => items.Select(state => converter.Map(state, true))).Subscribe(metadata.Add);
            var writer = metadata.Metadata.Connect().ToCollection().Select(metaData => metaData.ToArray()).Throttle(TimeSpan.FromMilliseconds(250)).Select(searchStateItems => searchStateItems.Select(converter.Map).ToArray()).Subscribe(searchStateSettings.Write);
            this._cleanUp = new CompositeDisposable(loader, writer);
        }
        #endregion
        #region Properties
        public ISearchMetadataCollection Metadata { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}