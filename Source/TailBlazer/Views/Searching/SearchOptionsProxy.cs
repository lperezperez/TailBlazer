namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;
    using DynamicData.Binding;
    using DynamicData.Kernel;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    public class SearchOptionsProxy : AbstractNotifyPropertyChanged, IDisposable, IEquatable<SearchOptionsProxy>
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly IDefaultIconSelector _defaultIconSelector;
        private readonly SearchMetadata _searchMetadata;
        private bool _caseSensitive;
        private bool _filter;
        private bool _highlight;
        private Hue _highlightHue;
        private PackIconKind _iconKind;
        private bool _isExclusion;
        private int _position;
        private bool _useRegex;
        #endregion
        #region Constructors
        public SearchOptionsProxy([NotNull] SearchMetadata searchMetadata, [NotNull] Action<SearchMetadata> changeScopeAction, [NotNull] IColourProvider colourProvider, [NotNull] IThemeProvider themeProvider, [NotNull] IconSelector iconSelector, [NotNull] Action<SearchMetadata> removeAction, [NotNull] IDefaultIconSelector defaultIconSelector, Guid parentId)
        {
            if (changeScopeAction == null) throw new ArgumentNullException(nameof(changeScopeAction));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            if (removeAction == null) throw new ArgumentNullException(nameof(removeAction));
            this._searchMetadata = searchMetadata ?? throw new ArgumentNullException(nameof(searchMetadata));
            this._defaultIconSelector = defaultIconSelector ?? throw new ArgumentNullException(nameof(defaultIconSelector));
            this.IconSelector = iconSelector ?? throw new ArgumentNullException(nameof(iconSelector));
            this.ParentId = parentId;
            this.Highlight = this._searchMetadata.Highlight;
            this.Filter = this._searchMetadata.Filter;
            this.UseRegex = searchMetadata.UseRegex;
            this.CaseSensitive = !searchMetadata.IgnoreCase;
            this.Position = searchMetadata.Position;
            this.Hues = colourProvider.Hues;
            this.HighlightHue = searchMetadata.HighlightHue;
            this.IsGlobal = searchMetadata.IsGlobal;
            this.IsExclusion = searchMetadata.IsExclusion;
            this.ShowIconSelectorCommand = new Command(async () => await this.ShowIconSelector());
            this.RemoveCommand = new Command(() => removeAction(searchMetadata));
            this.ChangeScopeCommand = new Command(() => changeScopeAction((SearchMetadata)this));
            this.HighlightCommand = new Command<Hue>(newHue => { this.HighlightHue = newHue; });
            this.IconKind = this._searchMetadata.IconKind.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);

            //combine system with user choice.
            var defaultHue = this.WhenValueChanged(vm => vm.HighlightHue).CombineLatest(themeProvider.Accent, (user, system) => user == Hue.NotSpecified ? system : user).Publish();
            this.Foreground = defaultHue.Select(h => h.ForegroundBrush).ForBinding();
            this.Background = defaultHue.Select(h => h.BackgroundBrush).ForBinding();
            this._cleanUp = new CompositeDisposable(this.IconSelector, this.Foreground, this.Background, defaultHue.Connect());
        }
        #endregion
        #region Properties
        public IProperty<Brush> Background { get; }
        public bool CaseSensitive { get => this._caseSensitive; set => this.SetAndRaise(ref this._caseSensitive, value); }
        public Command ChangeScopeCommand { get; }
        public string ChangeScopeToolTip => this.IsGlobal ? "Change to local scope" : "Change to global scope";
        public bool Filter { get => this._filter; set => this.SetAndRaise(ref this._filter, value); }
        public IProperty<Brush> Foreground { get; }
        public bool Highlight { get => this._highlight; set => this.SetAndRaise(ref this._highlight, value); }
        public ICommand HighlightCommand { get; }
        public Hue HighlightHue { get => this._highlightHue; set => this.SetAndRaise(ref this._highlightHue, value); }
        public IEnumerable<Hue> Hues { get; }
        public PackIconKind IconKind { get => this._iconKind; set => this.SetAndRaise(ref this._iconKind, value); }
        public bool IsExclusion { get => this._isExclusion; private set => this.SetAndRaise(ref this._isExclusion, value); }
        public bool IsGlobal { get; }
        public Guid ParentId { get; }
        public int Position { get => this._position; set => this.SetAndRaise(ref this._position, value); }
        public ICommand RemoveCommand { get; }
        public string RemoveTooltip => $"Get rid of {this.Text}?";
        public ICommand ShowIconSelectorCommand { get; }
        public string Text => this._searchMetadata.SearchText;
        public bool UseRegex { get => this._useRegex; private set => this.SetAndRaise(ref this._useRegex, value); }
        private IconSelector IconSelector { get; }
        #endregion
        #region Methods
        public static bool operator ==(SearchOptionsProxy left, SearchOptionsProxy right) => object.Equals(left, right);
        public static explicit operator SearchMetadata(SearchOptionsProxy proxy) => new SearchMetadata(proxy.Position, proxy.Text, proxy.Filter, proxy.Highlight, proxy.UseRegex, !proxy.CaseSensitive, proxy.HighlightHue, proxy.IconKind.ToString(), proxy.IsGlobal, proxy.IsExclusion);
        public static bool operator !=(SearchOptionsProxy left, SearchOptionsProxy right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(null, obj)) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((SearchOptionsProxy)obj);
        }
        public override int GetHashCode() => this._searchMetadata?.GetHashCode() ?? 0;
        public override string ToString() => $"SearchOptionsProxy: {this._searchMetadata}";
        public void Dispose() { this._cleanUp.Dispose(); }
        public bool Equals(SearchOptionsProxy other)
        {
            if (object.ReferenceEquals(null, other)) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return object.Equals(this._searchMetadata, other._searchMetadata);
        }
        private async Task ShowIconSelector()
        {
            var dialogResult = await DialogHost.Show(this.IconSelector, this.ParentId);
            var result = (IconSelectorResult)dialogResult;
            if (result == IconSelectorResult.UseDefault)
            {
                //Use default
                var icon = this._defaultIconSelector.GetIconFor(this.Text, this.UseRegex);
                this.IconKind = icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);
            }
            else if (result == IconSelectorResult.UseSelected)
            {
                this.IconKind = this.IconSelector.Selected.Type;
            }
        }
        #endregion
    }
}