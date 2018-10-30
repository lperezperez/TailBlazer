namespace TailBlazer.Views.Tail
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows.Media;
    using DynamicData.Binding;
    using DynamicData.Kernel;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    public sealed class LineMatchProxy : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly LineMatch _match;
        #endregion
        #region Constructors
        public LineMatchProxy(LineMatch match, IThemeProvider themeProvider)
        {
            this._match = match;
            this.IconKind = this._match.Icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);
            var defaultHue = themeProvider.Accent.Select(hue => match.Hue == Hue.NotSpecified ? hue : this._match.Hue);
            this.Foreground = defaultHue.Select(h => h.ForegroundBrush).ForBinding();
            this.Background = defaultHue.Select(h => h.BackgroundBrush).ForBinding();
            this._cleanUp = new CompositeDisposable(this.Foreground, this.Background);
        }
        #endregion
        #region Properties
        public IProperty<Brush> Background { get; }
        public string Description => $"Matches '{this._match.Text}'";
        public IProperty<Brush> Foreground { get; }
        public PackIconKind IconKind { get; }
        public string Text => this._match.Text;
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}