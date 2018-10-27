namespace TailBlazer.Views.Tail
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows.Input;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    using TailBlazer.Infrastucture.Virtualisation;
    public class InlineViewer : AbstractNotifyPropertyChanged, ILinesVisualisation
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private int _firstIndex;
        private bool _isSettingScrollPosition;
        private int _pageSize;
        #endregion
        #region Constructors
        public InlineViewer([NotNull] IObservable<ILineProvider> lineProvider, [NotNull] IObservable<LineProxy> selectedChanged, [NotNull] IClipboardHandler clipboardHandler, [NotNull] ISchedulerProvider schedulerProvider, [NotNull] ISelectionMonitor selectionMonitor, [NotNull] ILogger logger, [NotNull] IThemeProvider themeProvider, [NotNull] ITextFormatter textFormatter, [NotNull] ILineMatches lineMatches)
        {
            if (lineProvider == null) throw new ArgumentNullException(nameof(lineProvider));
            if (selectedChanged == null) throw new ArgumentNullException(nameof(selectedChanged));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            this.SelectionMonitor = selectionMonitor ?? throw new ArgumentNullException(nameof(selectionMonitor));
            this.CopyToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            this._isSettingScrollPosition = false;
            var pageSize = this.WhenValueChanged(vm => vm.PageSize);

            //if use selection is null, tail the file
            var scrollSelected = selectedChanged.CombineLatest(lineProvider, pageSize, (proxy, lp, pge) => proxy == null ? new ScrollRequest(pge, 0) : new ScrollRequest(pge, proxy.Start)).DistinctUntilChanged();
            var horizonalScrollArgs = new ReplaySubject<TextScrollInfo>(1);
            this.HorizonalScrollChanged = hargs => { horizonalScrollArgs.OnNext(hargs); };
            var scrollUser = this._userScrollRequested.Where(x => !this._isSettingScrollPosition).Select(request => new ScrollRequest(ScrollReason.User, request.PageSize, request.FirstIndex));
            var scroller = scrollSelected.Merge(scrollUser).ObserveOn(schedulerProvider.Background).DistinctUntilChanged();
            var lineScroller = new LineScroller(lineProvider, scroller);
            this.Count = lineProvider.Select(lp => lp.Count).ForBinding();
            this.MaximumChars = lineScroller.MaximumLines().ObserveOn(schedulerProvider.MainThread).ForBinding();
            var proxyFactory = new LineProxyFactory(textFormatter, lineMatches, horizonalScrollArgs.DistinctUntilChanged(), themeProvider);

            //load lines into observable collection
            var loader = lineScroller.Lines.Connect().Transform(proxyFactory.Create).Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy)).ObserveOn(schedulerProvider.MainThread).Bind(out this._data).DisposeMany().LogErrors(logger).Subscribe();

            // track first visible index [required to set scroll extent]
            var firstIndexMonitor = lineScroller.Lines.Connect().Buffer(TimeSpan.FromMilliseconds(250)).FlattenBufferResult().ToCollection().Select(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Max() - lines.Count + 1).ObserveOn(schedulerProvider.MainThread).Subscribe
                (
                 first =>
                     {
                         try
                         {
                             this._isSettingScrollPosition = true;
                             this.FirstIndex = first;
                         }
                         finally
                         {
                             this._isSettingScrollPosition = false;
                         }
                     });
            this._cleanUp = new CompositeDisposable(lineScroller, loader, this.Count, firstIndexMonitor, this.SelectionMonitor, this.MaximumChars, horizonalScrollArgs.SetAsComplete(), this._userScrollRequested.SetAsComplete());
        }
        #endregion
        #region Properties
        public ICommand CopyToClipboardCommand { get; }
        public IProperty<int> Count { get; }
        public int FirstIndex { get => this._firstIndex; set => this.SetAndRaise(ref this._firstIndex, value); }
        public TextScrollDelegate HorizonalScrollChanged { get; }
        public ReadOnlyObservableCollection<LineProxy> Lines => this._data;
        public IProperty<int> MaximumChars { get; }
        public int PageSize { get => this._pageSize; set => this.SetAndRaise(ref this._pageSize, value); }
        public ISelectionMonitor SelectionMonitor { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public void ScrollDiff(int lineChanged) { this._userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, this.PageSize, this.FirstIndex + lineChanged)); }
        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            if (!this._isSettingScrollPosition)
                this._userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, boundsArgs.PageSize, boundsArgs.FirstIndex));
            this.PageSize = boundsArgs.PageSize;
            this.FirstIndex = boundsArgs.FirstIndex;
        }
        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs) { }
        #endregion
    }
}