namespace TailBlazer.Views.Tail
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Domain.StateHandling;
    using TailBlazer.Infrastucture;
    using TailBlazer.Infrastucture.KeyboardNavigation;
    using TailBlazer.Infrastucture.Virtualisation;
    using TailBlazer.Views.DialogServices;
    using TailBlazer.Views.Searching;
    public class TailViewModel : AbstractNotifyPropertyChanged, ILinesVisualisation, IPersistentView, IDialogViewModel, IPageProvider, ISelectedAware
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<LineProxy> _data;
        private readonly IPersistentView _persister;
        private readonly SingleAssignmentDisposable _stateMonitor = new SingleAssignmentDisposable();
        private readonly ITailViewStateControllerFactory _tailViewStateControllerFactory;
        private readonly ISubject<ScrollRequest> _userScrollRequested = new ReplaySubject<ScrollRequest>(1);
        private bool _autoTail = true;
        private object _dialogContent;
        private int _firstIndex;
        private bool _isDialogOpen;
        private bool _isSelected;
        private int _pageSize;
        private LineProxy _selectedLine;
        private bool _showInline;
        #endregion
        #region Constructors
        public TailViewModel([NotNull] ILogger logger, [NotNull] ISchedulerProvider schedulerProvider, [NotNull] IFileWatcher fileWatcher, [NotNull] ISelectionMonitor selectionMonitor, [NotNull] IClipboardHandler clipboardHandler, [NotNull] ISearchInfoCollection searchInfoCollection, [NotNull] IInlineViewerFactory inlineViewerFactory, [NotNull] GeneralOptionBindings generalOptionBindings, [NotNull] ICombinedSearchMetadataCollection combinedSearchMetadataCollection, [NotNull] IStateBucketService stateBucketService, [NotNull] ITailViewStateRestorer restorer, [NotNull] SearchHints searchHints, [NotNull] ITailViewStateControllerFactory tailViewStateControllerFactory, [NotNull] IThemeProvider themeProvider, [NotNull] SearchCollection searchCollection, [NotNull] ITextFormatter textFormatter, [NotNull] ILineMatches lineMatches, [NotNull] IObjectProvider objectProvider, [NotNull] IDialogCoordinator dialogCoordinator)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));
            if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
            if (clipboardHandler == null) throw new ArgumentNullException(nameof(clipboardHandler));
            if (searchInfoCollection == null) throw new ArgumentNullException(nameof(searchInfoCollection));
            if (inlineViewerFactory == null) throw new ArgumentNullException(nameof(inlineViewerFactory));
            if (stateBucketService == null) throw new ArgumentNullException(nameof(stateBucketService));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            if (textFormatter == null) throw new ArgumentNullException(nameof(textFormatter));
            if (lineMatches == null) throw new ArgumentNullException(nameof(lineMatches));
            if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
            if (dialogCoordinator == null) throw new ArgumentNullException(nameof(dialogCoordinator));
            if (combinedSearchMetadataCollection == null) throw new ArgumentNullException(nameof(combinedSearchMetadataCollection));
            this.Name = fileWatcher.FullName;
            this.SelectionMonitor = selectionMonitor ?? throw new ArgumentNullException(nameof(selectionMonitor));
            this.GeneralOptionBindings = generalOptionBindings;
            this.SearchHints = searchHints ?? throw new ArgumentNullException(nameof(searchHints));
            this.CopyToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(selectionMonitor.GetSelectedText()));
            this.OpenFileCommand = new Command(() => Process.Start(fileWatcher.FullName));
            this.OpenFolderCommand = new Command(() => Process.Start(fileWatcher.Folder));
            this.CopyPathToClipboardCommand = new Command(() => clipboardHandler.WriteToClipboard(fileWatcher.FullName));
            this.UnClearCommand = new Command(fileWatcher.Reset);
            this.ClearCommand = new Command(fileWatcher.Clear);
            this.KeyAutoTail = new Command(() => { this.AutoTail = true; });
            this.OpenSearchOptionsCommand = new Command
                (
                 async () =>
                     {
                         await Task.Run
                             (
                              () =>
                                  {
                                      var content = objectProvider.Get<SearchOptionsViewModel>(new Argument<ICombinedSearchMetadataCollection>(combinedSearchMetadataCollection));
                                      dialogCoordinator.Show(this, content, x => content.Dispose());
                                  });
                     });
            var closeOnDeselect = this.WhenValueChanged(vm => vm.IsSelected, false).Where(selected => !selected).Subscribe(_ => dialogCoordinator.Close());
            this.SearchCollection = searchCollection ?? throw new ArgumentNullException(nameof(searchCollection));
            this.SearchMetadataCollection = combinedSearchMetadataCollection.Local;
            var horizonalScrollArgs = new ReplaySubject<TextScrollInfo>(1);
            this.HorizonalScrollChanged = args => horizonalScrollArgs.OnNext(args);
            this._tailViewStateControllerFactory = tailViewStateControllerFactory;

            //this deals with state when loading the system at start up and at shut-down
            this._persister = new TailViewPersister(this, restorer);
            this.FileStatus = fileWatcher.Status.ForBinding();

            //command to add the current search to the tail collection
            var searchInvoker = this.SearchHints.SearchRequested.Subscribe(request => searchInfoCollection.Add(request.Text, request.UseRegEx));

            //An observable which acts as a scroll command
            var autoChanged = this.WhenValueChanged(vm => vm.AutoTail);
            var scroller = this._userScrollRequested.CombineLatest
                (
                 autoChanged,
                 (user, auto) =>
                     {
                         var mode = this.AutoTail ? ScrollReason.Tail : ScrollReason.User;
                         return new ScrollRequest(mode, user.PageSize, user.FirstIndex);
                     }).Do(x => logger.Info("Scrolling to {0}/{1}", x.FirstIndex, x.PageSize)).DistinctUntilChanged();

            //User feedback to show file size
            this.FileSizeText = fileWatcher.Latest.Select(fn => fn.Size).Select(size => size.FormatWithAbbreviation()).DistinctUntilChanged().ForBinding();

            //tailer is the main object used to tail, scroll and filter in a file
            var selectedProvider = this.SearchCollection.Latest.ObserveOn(schedulerProvider.Background);
            var lineScroller = new LineScroller(selectedProvider, scroller);
            this.MaximumChars = lineScroller.MaximumLines().ObserveOn(schedulerProvider.MainThread).ForBinding();
            var lineProxyFactory = new LineProxyFactory(textFormatter, lineMatches, horizonalScrollArgs.DistinctUntilChanged(), themeProvider);
            var loader = lineScroller.Lines.Connect().LogChanges(logger, "Received").Transform(lineProxyFactory.Create).LogChanges(logger, "Sorting").Sort(SortExpressionComparer<LineProxy>.Ascending(proxy => proxy)).ObserveOn(schedulerProvider.MainThread).Bind(out this._data, 100).LogChanges(logger, "Bound").DisposeMany().LogErrors(logger).Subscribe();

            //monitor matching lines and start index,
            this.Count = searchInfoCollection.All.Select(latest => latest.Count).ForBinding();
            this.CountText = searchInfoCollection.All.Select(latest => $"{latest.Count:##,###} lines").ForBinding();
            this.LatestCount = this.SearchCollection.Latest.Select(latest => latest.Count).ForBinding();

            ////track first visible index
            var firstIndexMonitor = lineScroller.Lines.Connect().Buffer(TimeSpan.FromMilliseconds(25)).FlattenBufferResult().ToCollection().Select(lines => lines.Count == 0 ? 0 : lines.Select(l => l.Index).Max() - lines.Count + 1).ObserveOn(schedulerProvider.MainThread).Subscribe(first => { this.FirstIndex = first; });

            //Create objects required for inline viewing
            var isUserDefinedChanged = this.SearchCollection.WhenValueChanged(sc => sc.Selected).Where(selected => selected != null).Select(selected => selected.IsUserDefined).DistinctUntilChanged().Replay(1).RefCount();
            var showInline = this.WhenValueChanged(vm => vm.ShowInline);
            var inlineViewerVisible = isUserDefinedChanged.CombineLatest(showInline, (userDefined, showInlne) => userDefined && showInlne);
            this.CanViewInline = isUserDefinedChanged.ForBinding();
            this.InlineViewerVisible = inlineViewerVisible.ForBinding();

            //return an empty line provider unless user is viewing inline - this saves needless trips to the file
            var inline = searchInfoCollection.All.CombineLatest(inlineViewerVisible, (index, ud) => ud ? index : EmptyLineProvider.Instance);
            this.InlineViewer = inlineViewerFactory.Create(combinedSearchMetadataCollection, inline, this.WhenValueChanged(vm => vm.SelectedItem));
            this._cleanUp = new CompositeDisposable(lineScroller, loader, firstIndexMonitor, this.FileStatus, this.Count, this.CountText, this.LatestCount, this.FileSizeText, this.CanViewInline, this.InlineViewer, this.InlineViewerVisible, this.SearchCollection, searchInfoCollection, searchHints, this.SelectionMonitor, closeOnDeselect, Disposable.Create(dialogCoordinator.Close), searchInvoker, this.MaximumChars, this._stateMonitor, combinedSearchMetadataCollection, horizonalScrollArgs.SetAsComplete(), this._userScrollRequested.SetAsComplete());
        }
        #endregion
        #region Properties
        public bool AutoTail { get => this._autoTail; set => this.SetAndRaise(ref this._autoTail, value); }
        public IProperty<bool> CanViewInline { get; }
        public ICommand ClearCommand { get; }
        public ICommand CopyPathToClipboardCommand { get; }
        public ICommand CopyToClipboardCommand { get; }
        public IProperty<int> Count { get; }
        public IProperty<string> CountText { get; }
        public object DialogContent { get => this._dialogContent; set => this.SetAndRaise(ref this._dialogContent, value); }
        public IProperty<string> FileSizeText { get; }
        public IProperty<FileStatus> FileStatus { get; }
        public int FirstIndex { get => this._firstIndex; set => this.SetAndRaise(ref this._firstIndex, value); }
        public GeneralOptionBindings GeneralOptionBindings { get; }
        public TextScrollDelegate HorizonalScrollChanged { get; }
        public Guid Id { get; } = Guid.NewGuid();
        public InlineViewer InlineViewer { get; }
        public IProperty<bool> InlineViewerVisible { get; }
        public bool IsDialogOpen { get => this._isDialogOpen; set => this.SetAndRaise(ref this._isDialogOpen, value); }
        public bool IsSelected { get => this._isSelected; set => this.SetAndRaise(ref this._isSelected, value); }
        public ICommand KeyAutoTail { get; }
        public IProperty<int> LatestCount { get; }
        public ReadOnlyObservableCollection<LineProxy> Lines => this._data;
        public IProperty<int> MaximumChars { get; }
        public string Name { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenSearchOptionsCommand { get; }
        public int PageSize { get => this._pageSize; set => this.SetAndRaise(ref this._pageSize, value); }
        public SearchCollection SearchCollection { get; }
        public SearchHints SearchHints { get; }
        public LineProxy SelectedItem { get => this._selectedLine; set => this.SetAndRaise(ref this._selectedLine, value); }
        public ISelectionMonitor SelectionMonitor { get; }
        public bool ShowInline { get => this._showInline; set => this.SetAndRaise(ref this._showInline, value); }
        public ICommand UnClearCommand { get; }
        internal ISearchMetadataCollection SearchMetadataCollection { get; }
        #endregion
        #region Methods
        public void ApplySettings()
        {
            //this controller responsible for loading and persisting user search stuff as the user changes stuff
            this._stateMonitor.Disposable = this._tailViewStateControllerFactory.Create(this, true);
        }
        public void Dispose() { this._cleanUp.Dispose(); }
        ViewState IPersistentView.CaptureState() => this._persister.CaptureState();
        void IPersistentView.Restore(ViewState state)
        {
            //When this is called, we assume that FileInfo has not been set!
            this._persister.Restore(state);

            //this controller responsible for loading and persisting user search stuff as the user changes stuff
            this._stateMonitor.Disposable = this._tailViewStateControllerFactory.Create(this, false);
        }
        void IScrollReceiver.ScrollBoundsChanged(ScrollBoundsArgs boundsArgs)
        {
            if (boundsArgs == null) throw new ArgumentNullException(nameof(boundsArgs));
            var mode = this.AutoTail ? ScrollReason.Tail : ScrollReason.User;
            this.PageSize = boundsArgs.PageSize;
            this.FirstIndex = boundsArgs.FirstIndex;
            /*
                I need to get rid of this subject as I prefer functional over imperative. 
                However due to complexities int the interactions with the VirtualScrollPanel,
                each time I have tried to remove it all hell has broken loose
            */
            this._userScrollRequested.OnNext(new ScrollRequest(mode, boundsArgs.PageSize, boundsArgs.FirstIndex));
        }
        void IScrollReceiver.ScrollChanged(ScrollChangedArgs scrollChangedArgs)
        {
            if (scrollChangedArgs.Direction == ScrollDirection.Up)
                this.AutoTail = false;
        }
        void IScrollReceiver.ScrollDiff(int linesChanged) { this._userScrollRequested.OnNext(new ScrollRequest(ScrollReason.User, this.PageSize, this.FirstIndex + linesChanged)); }
        #endregion
    }
}