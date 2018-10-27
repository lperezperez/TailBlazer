namespace TailBlazer.Views.Searching
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Infrastucture;
    public class SearchViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly SearchInfo _info;
        private int _count;
        private string _countText;
        private bool _searching;
        private int _segments;
        private int _segmentsSearched;
        #endregion
        #region Constructors
        public SearchViewModel(SearchInfo info, Action<SearchViewModel> removeAction)
        {
            this._info = info;
            this.RemoveCommand = new Command(() => removeAction(this));
            var counter = this._info.Latest.Select(lp => lp.Count).Subscribe(count => this.Count = count);
            var counterTextFormatter = this._info.Latest.Select
                (
                 lp =>
                     {
                         var limited = lp as IHasLimitationOfLines;
                         if (limited == null) return $"{lp.Count.ToString("#,###0")}";
                         return limited.HasReachedLimit ? $"{limited.Maximum.ToString("#,###0")}+" : $"{lp.Count.ToString("#,###0")}";
                     }).Subscribe(countText => this.CountText = countText);
            var progressMonitor = this._info.Latest.OfType<IProgressInfo>().Subscribe
                (
                 result =>
                     {
                         this.Searching = result.IsSearching;
                         this.Segments = result.Segments;
                         this.SegmentsSearched = result.SegmentsCompleted;
                     });
            this._cleanUp = new CompositeDisposable(progressMonitor, counter, counterTextFormatter);
        }
        #endregion
        #region Properties
        public int Count { get => this._count; set => this.SetAndRaise(ref this._count, value); }
        public string CountText { get => this._countText; set => this.SetAndRaise(ref this._countText, value); }
        public bool IsUserDefined => this._info.SearchType == SearchType.User;
        public IObservable<ILineProvider> Latest => this._info.Latest;
        public ICommand RemoveCommand { get; }
        public string RemoveTooltip => $"Get rid of {this.Text}?";
        public bool Searching { get => this._searching; set => this.SetAndRaise(ref this._searching, value); }
        public SearchType SearchType => this._info.SearchType;
        public int Segments { get => this._segments; set => this.SetAndRaise(ref this._segments, value); }
        public int SegmentsSearched { get => this._segmentsSearched; set => this.SetAndRaise(ref this._segmentsSearched, value); }
        public string Text => this._info.SearchText;
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}