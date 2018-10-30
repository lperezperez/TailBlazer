namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Dragablz;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.FileHandling.TextAssociations;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Views.Formatting;
    public class SearchProxyCollection : ISearchProxyCollection
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public SearchProxyCollection(ISearchMetadataCollection metadataCollection, Guid id, Action<SearchMetadata> changeScopeAction, ISchedulerProvider schedulerProvider, IColourProvider colourProvider, IIconProvider iconsProvider, ITextAssociationCollection textAssociationCollection, IThemeProvider themeProvider)
        {
            var proxyItems = metadataCollection.Metadata.Connect().WhereReasonsAre(ChangeReason.Add, ChangeReason.Remove) //ignore updates because we update from here
                                               .Transform(meta => { return new SearchOptionsProxy(meta, changeScopeAction, colourProvider, themeProvider, new IconSelector(iconsProvider, schedulerProvider), m => metadataCollection.Remove(m.SearchText), iconsProvider.DefaultIconSelector, id); }).SubscribeMany
                                                   (
                                                    so =>
                                                        {
                                                            //when a value changes, write the original value back to the metadata collection
                                                            var anyPropertyHasChanged = so.WhenAnyPropertyChanged().Select(_ => (SearchMetadata)so).Subscribe(metadataCollection.AddorUpdate);

                                                            //when an icon or colour has changed we need to record user choice so 
                                                            //the same choice can be used again
                                                            var iconChanged = so.WhenValueChanged(proxy => proxy.IconKind, false).ToUnit();
                                                            var colourChanged = so.WhenValueChanged(proxy => proxy.HighlightHue, false).ToUnit();
                                                            var ignoreCaseChanged = so.WhenValueChanged(proxy => proxy.CaseSensitive, false).ToUnit();
                                                            var textAssociationChanged = iconChanged.Merge(colourChanged).Merge(ignoreCaseChanged).Throttle(TimeSpan.FromMilliseconds(250)).Select(_ => new TextAssociation(so.Text, so.CaseSensitive, so.UseRegex, so.HighlightHue.Swatch, so.IconKind.ToString(), so.HighlightHue.Name, DateTime.UtcNow)).Subscribe(textAssociationCollection.MarkAsChanged);
                                                            return new CompositeDisposable(anyPropertyHasChanged, textAssociationChanged);
                                                        }).AsObservableCache();
            this.Count = proxyItems.CountChanged.StartWith(0).ForBinding();
            var monitor = this.MonitorPositionalChanges().Subscribe(metadataCollection.Add);

            //load data onto grid
            var collection = new ObservableCollectionExtended<SearchOptionsProxy>();
            var includedLoader = proxyItems.Connect(proxy => !proxy.IsExclusion).Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Position)).ObserveOn(schedulerProvider.MainThread)
                                           //force reset for each new or removed item dues to a bug in the underlying dragablz control which inserts in an incorrect position
                                           .Bind(collection, new ObservableCollectionAdaptor<SearchOptionsProxy, string>(0)).DisposeMany().Subscribe();
            var excludedLoader = proxyItems.Connect(proxy => proxy.IsExclusion).Sort(SortExpressionComparer<SearchOptionsProxy>.Ascending(proxy => proxy.Text)).ObserveOn(schedulerProvider.MainThread)
                                           //force reset for each new or removed item dues to a bug in the underlying dragablz control which inserts in an incorrect position
                                           .Bind(out var excluded).DisposeMany().Subscribe();
            this.Excluded = excluded;
            this.Included = new ReadOnlyObservableCollection<SearchOptionsProxy>(collection);
            this._cleanUp = new CompositeDisposable(proxyItems, includedLoader, excludedLoader, monitor);
        }
        #endregion
        #region Properties
        public IProperty<int> Count { get; }
        public ReadOnlyObservableCollection<SearchOptionsProxy> Excluded { get; }
        public ReadOnlyObservableCollection<SearchOptionsProxy> Included { get; }
        public VerticalPositionMonitor PositionMonitor { get; } = new VerticalPositionMonitor();
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private IObservable<IEnumerable<SearchMetadata>> MonitorPositionalChanges()
        {
            return Observable.FromEventPattern<OrderChangedEventArgs>(h => this.PositionMonitor.OrderChanged += h, h => this.PositionMonitor.OrderChanged -= h).Throttle(TimeSpan.FromMilliseconds(125)).Select(evt => evt.EventArgs).Where(args => args.PreviousOrder != null && !args.PreviousOrder.SequenceEqual(args.NewOrder)).Select
                (
                 positionChangedArgs =>
                     {
                         var newOrder = positionChangedArgs.NewOrder.OfType<SearchOptionsProxy>().Select
                             (
                              (item, index) =>
                                  {
                                      item.Position = index;
                                      return new { Meta = (SearchMetadata)item, index };
                                  }).ToArray();

                         //reprioritise filters and highlights
                         return newOrder.Select(x => new SearchMetadata(x.Meta, x.index)).ToArray();
                     });
        }
        #endregion
    }
}