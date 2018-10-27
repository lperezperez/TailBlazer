namespace TailBlazer.Views.Searching
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Infrastructure;
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SearchOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private int _selectedIndex;
        #endregion
        #region Constructors
        public SearchOptionsViewModel(ICombinedSearchMetadataCollection combinedSearchMetadataCollection, ISearchProxyCollectionFactory searchProxyCollectionFactory, ISearchMetadataFactory searchMetadataFactory, ISchedulerProvider schedulerProvider, SearchHints searchHints)
        {
            this.SearchHints = searchHints;
            var global = combinedSearchMetadataCollection.Global;
            var local = combinedSearchMetadataCollection.Local;
            void ChangeScopeAction(SearchMetadata meta)
            {
                if (meta.IsGlobal)
                {
                    //make global
                    global.Remove(meta.SearchText);
                    var newValue = new SearchMetadata(meta, local.NextIndex(), false);
                    local.AddorUpdate(newValue);
                }
                else
                {
                    //make local
                    local.Remove(meta.SearchText);
                    var newValue = new SearchMetadata(meta, global.NextIndex(), true);
                    global.AddorUpdate(newValue);
                }
            }
            this.Local = searchProxyCollectionFactory.Create(local, this.Id, ChangeScopeAction);
            this.Global = searchProxyCollectionFactory.Create(global, this.Id, ChangeScopeAction);

            //command to add the current search to the tail collection
            var searchInvoker = this.SearchHints.SearchRequested.ObserveOn(schedulerProvider.Background).Subscribe
                (
                 request =>
                     {
                         var isGlobal = this.SelectedIndex == 1;
                         var nextIndex = isGlobal ? global.NextIndex() : local.NextIndex();
                         var meta = searchMetadataFactory.Create(request.Text, request.UseRegEx, nextIndex, false, isGlobal);
                         if (isGlobal)
                             global.AddorUpdate(meta);
                         else
                             local.AddorUpdate(meta);
                     });
            this._cleanUp = new CompositeDisposable(searchInvoker, searchInvoker, this.SearchHints, this.Global, this.Local);
        }
        #endregion
        #region Properties
        public ISearchProxyCollection Global { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public ISearchProxyCollection Local { get; }
        public SearchHints SearchHints { get; }
        public int SelectedIndex { get => this._selectedIndex; set => this.SetAndRaise(ref this._selectedIndex, value); }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}