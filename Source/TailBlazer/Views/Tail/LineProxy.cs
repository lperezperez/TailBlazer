namespace TailBlazer.Views.Tail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Media;
    using DynamicData.Binding;
    using DynamicData.Kernel;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture.Virtualisation;
    public class LineProxy : AbstractNotifyPropertyChanged, IComparable<LineProxy>, IComparable, IEquatable<LineProxy>, IDisposable
    {
        #region Fields
        private static readonly IComparer<LineProxy> DefaultSort = SortExpressionComparer<LineProxy>.Ascending(p => p.Line.LineInfo.Start).ThenByAscending(p => p.Line.LineInfo.Offset);
        private readonly IDisposable _cleanUp;
        private bool _isRecent;
        #endregion
        #region Constructors
        public LineProxy([NotNull] Line line, [NotNull] IObservable<IEnumerable<DisplayText>> formattedText, [NotNull] IObservable<LineMatchCollection> lineMatches, [NotNull] IObservable<TextScrollInfo> textScroll, [NotNull] IThemeProvider themeProvider)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (formattedText == null) throw new ArgumentNullException(nameof(formattedText));
            if (lineMatches == null) throw new ArgumentNullException(nameof(lineMatches));
            if (textScroll == null) throw new ArgumentNullException(nameof(textScroll));
            if (themeProvider == null) throw new ArgumentNullException(nameof(themeProvider));
            this.Start = line.LineInfo.Start;
            this.Index = line.LineInfo.Index;
            this.Line = line;
            this.Key = this.Line.Key;
            var lineMatchesShared = lineMatches.Publish();
            var textScrollShared = textScroll.Publish();
            this.PlainText = textScrollShared.Select(ts => line.Text.Virtualise(ts)).ForBinding();
            this.LineNumber = textScrollShared.Select(ts => line.Number.ToString()).ForBinding();
            this.FormattedText = formattedText.CombineLatest(textScrollShared, (fmt, scroll) => fmt.Virtualise(scroll)).ForBinding();
            this.ShowIndicator = lineMatchesShared.Select(lmc => lmc.HasMatches ? Visibility.Visible : Visibility.Collapsed).ForBinding();
            this.IndicatorColour = lineMatchesShared.Select(lmc => lmc.FirstMatch?.Hue).CombineLatest
                (
                 themeProvider.Accent,
                 (user, system) =>
                     {
                         if (user == null) return null;
                         return user == Hue.NotSpecified ? system.BackgroundBrush : user.BackgroundBrush;
                     }).ForBinding();
            this.IndicatorIcon = lineMatchesShared.Select
                (
                 lmc =>
                     {
                         var icon = lmc.FirstMatch?.Icon;
                         return icon.ParseEnum<PackIconKind>().ValueOr(() => PackIconKind.ArrowRightBold);
                     }).StartWith(PackIconKind.ArrowRightBold).ForBinding();
            this.IndicatorMatches = lineMatchesShared.Select(lmc => { return lmc.Matches.Select(m => new LineMatchProxy(m, themeProvider)).ToList(); }).ForBinding();
            if (this.Line.Timestamp.HasValue && DateTime.UtcNow.Subtract(this.Line.Timestamp.Value).TotalSeconds < 0.25)
            {
                this.IsRecent = true;
                Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ => this.IsRecent = false);
            }
            this._cleanUp = new CompositeDisposable(this.FormattedText, this.IndicatorColour, this.IndicatorMatches, this.IndicatorIcon, this.ShowIndicator, this.FormattedText, this.PlainText, this.LineNumber, lineMatchesShared.Connect(), textScrollShared.Connect());
        }
        #endregion
        #region Properties
        public IProperty<IEnumerable<DisplayText>> FormattedText { get; }
        public int Index { get; }
        public IProperty<Brush> IndicatorColour { get; }
        public IProperty<PackIconKind> IndicatorIcon { get; }
        public IProperty<IEnumerable<LineMatchProxy>> IndicatorMatches { get; }
        public bool IsRecent { get => this._isRecent; set => this.SetAndRaise(ref this._isRecent, value); }
        public LineKey Key { get; }
        public Line Line { get; }
        public IProperty<string> LineNumber { get; }
        public IProperty<string> PlainText { get; }
        public IProperty<Visibility> ShowIndicator { get; }
        public long Start { get; }
        #endregion
        #region Methods
        public static bool operator ==(LineProxy left, LineProxy right) => object.Equals(left, right);
        public static bool operator !=(LineProxy left, LineProxy right) => !object.Equals(left, right);
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((LineProxy)obj);
        }
        public override int GetHashCode() => this.Key.GetHashCode();
        public override string ToString() => $"{this.Line}";
        public int CompareTo(LineProxy other) => DefaultSort.Compare(this, other);
        public int CompareTo(object obj) => this.CompareTo((LineProxy)obj);
        public void Dispose() { this._cleanUp.Dispose(); }
        public bool Equals(LineProxy other)
        {
            if (other is null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            return this.Key.Equals(other.Key);
        }
        #endregion
    }
}