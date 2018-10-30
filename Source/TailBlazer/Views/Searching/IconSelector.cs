namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Views.Formatting;
    public enum IconSelectorResult
    {
        UseSelected,
        UseDefault,
        None
    }
    public class IconSelector : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private string _iconSearchText;
        private IconDescription _selected;
        #endregion
        #region Constructors
        public IconSelector([NotNull] IIconProvider iconsProvider, [NotNull] ISchedulerProvider schedulerProvider)
        {
            if (iconsProvider == null) throw new ArgumentNullException(nameof(iconsProvider));
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));

            //build a predicate when SearchText changes
            var filter = this.WhenValueChanged(t => t.SearchText).Throttle(TimeSpan.FromMilliseconds(250)).Select(this.BuildFilter);
            var userOptions = iconsProvider.Icons.Connect().Filter(filter).Sort(SortExpressionComparer<IconDescription>.Ascending(icon => icon.Name)).ObserveOn(schedulerProvider.MainThread).Bind(out var icons).Subscribe();
            this.HasSelection = this.WhenValueChanged(vm => vm.Selected).Select(selected => selected != null).ForBinding();
            this.Icons = icons;
            this._cleanUp = new CompositeDisposable(userOptions);
        }
        #endregion
        #region Properties
        public IProperty<bool> HasSelection { get; }
        public ReadOnlyObservableCollection<IconDescription> Icons { get; }
        public string SearchText { get => this._iconSearchText; set => this.SetAndRaise(ref this._iconSearchText, value); }
        public IconDescription Selected { get => this._selected; set => this.SetAndRaise(ref this._selected, value); }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private Func<IconDescription, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText)) return icon => true;
            return icon => icon.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
        #endregion
    }
}