namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    using TailBlazer.Views.Recent;
    public class SearchHints : AbstractNotifyPropertyChanged, IDisposable, IDataErrorInfo
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<string> _hints;
        private readonly RegexInspector _regexInspector = new RegexInspector();
        private string _searchText;
        private bool _useRegex;
        #endregion
        #region Constructors
        public SearchHints(IRecentSearchCollection recentSearchCollection, ISchedulerProvider schedulerProvider)
        {
            //User feedback to guide them whilst typing
            var searchText = this.WhenValueChanged(vm => vm.SearchText);
            var useRegEx = this.WhenValueChanged(vm => vm.UseRegex);

            //if regex then validate
            var combined = searchText.CombineLatest(useRegEx, (text, regex) => new SearchRequest(text, regex)).Throttle(TimeSpan.FromMilliseconds(250)).Select(searchRequest => searchRequest.BuildMessage()).Publish();
            this.IsValid = combined.Select(shm => shm.IsValid).DistinctUntilChanged().ForBinding();
            this.Message = combined.Select(shm => shm.Message).DistinctUntilChanged().ForBinding();
            var forceRefreshOfError = combined.Select(shm => shm.IsValid).DistinctUntilChanged().Subscribe(_ => { this.OnPropertyChanged(nameof(SearchHints.SearchText)); });
            var predictRegex = this.WhenValueChanged(vm => vm.SearchText).Select(text => this._regexInspector.DoesThisLookLikeRegEx(text)).Subscribe(likeRegex => this.UseRegex = likeRegex);

            //Handle adding new search
            var searchRequested = new Subject<SearchRequest>();
            this.SearchRequested = searchRequested.AsObservable();
            this.AddSearchCommand = new Command
                (
                 async () =>
                     {
                         await Task.Run
                             (
                              () =>
                                  {
                                      recentSearchCollection.Add(new RecentSearch(this.SearchText));
                                      searchRequested.OnNext(new SearchRequest(this.SearchText, this.UseRegex));
                                      this.SearchText = string.Empty;
                                      this.UseRegex = false;
                                  });
                     },
                 () => this.IsValid.Value && this.SearchText.Length > 0);
            var dataLoader = recentSearchCollection.Items.Connect()
                                                   // .Filter(filter)
                                                   .Transform(recentSearch => recentSearch.Text).Sort(SortExpressionComparer<string>.Ascending(str => str)).ObserveOn(schedulerProvider.MainThread).Bind(out this._hints).Subscribe();
            this._cleanUp = new CompositeDisposable(this.IsValid, this.Message, predictRegex, dataLoader, searchRequested.SetAsComplete(), combined.Connect(), forceRefreshOfError);
        }
        #endregion
        #region Properties
        public ICommand AddSearchCommand { get; }
        public ReadOnlyObservableCollection<string> Hints => this._hints;
        public IProperty<bool> IsValid { get; }
        public IProperty<string> Message { get; }
        public IObservable<SearchRequest> SearchRequested { get; }
        public string SearchText { get => this._searchText; set => this.SetAndRaise(ref this._searchText, value); }
        public bool UseRegex { get => this._useRegex; set => this.SetAndRaise(ref this._useRegex, value); }
        string IDataErrorInfo.Error => null;
        #endregion
        #region Indexers
        string IDataErrorInfo.this[string columnName] => this.IsValid.Value ? null : this.Message.Value;
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private Func<RecentSearch, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) return trade => true;
            return recentSearch => recentSearch.Text.StartsWith(searchText, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}