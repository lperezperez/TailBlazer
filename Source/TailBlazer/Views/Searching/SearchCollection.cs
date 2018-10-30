namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using DynamicData.Binding;
    using DynamicData.Kernel;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Infrastructure;
    public class SearchCollection : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ReadOnlyObservableCollection<SearchViewModel> _items;
        private readonly IObservableCache<SearchViewModel, string> _viewModels;
        private int _count;
        private SearchViewModel _selected;
        #endregion
        #region Constructors
        public SearchCollection(ISearchInfoCollection searchInfoCollection, ISchedulerProvider schedulerProvider)
        {
            this._viewModels = searchInfoCollection.Searches.Connect().Transform(tail => new SearchViewModel(tail, vm => { searchInfoCollection.Remove(vm.Text); })).DisposeMany().AsObservableCache();
            var shared = this._viewModels.Connect(); //.Publish();
            var binderLoader = shared.Sort(SortExpressionComparer<SearchViewModel>.Ascending(tvm => tvm.SearchType == SearchType.All ? 1 : 2).ThenByAscending(tvm => tvm.Text)).ObserveOn(schedulerProvider.MainThread).Bind(out this._items).Subscribe();
            var autoSelector = shared.WhereReasonsAre(ChangeReason.Add).Flatten().Select(change => change.Current).Subscribe(latest => this.Selected = latest);
            var removed = shared.WhereReasonsAre(ChangeReason.Remove).Subscribe(_ => this.Selected = this._viewModels.Items.First());
            var counter = shared.ToCollection().Subscribe(count => this.Count = count.Count);
            var nullDodger = this.WhenValueChanged(sc => sc.Selected).Where(x => x == null).Subscribe(x => this.Selected = this._viewModels.Items.First());
            this.Latest = this.WhenValueChanged(sc => sc.Selected).Where(x => x != null).Select(svm => svm.Latest).Switch().Replay(1).RefCount();
            this._cleanUp = new CompositeDisposable(this._viewModels, binderLoader, counter, removed, autoSelector, nullDodger);
        }
        #endregion
        #region Properties
        public int Count { get => this._count; set => this.SetAndRaise(ref this._count, value); }
        public ReadOnlyObservableCollection<SearchViewModel> Items => this._items;
        public IObservable<ILineProvider> Latest { get; }
        public SearchViewModel Selected { get => this._selected; set => this.SetAndRaise(ref this._selected, value); }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public void Select(string item) { this._viewModels.Lookup(item).IfHasValue(selected => this.Selected = selected); }
        #endregion
    }
}