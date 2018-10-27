namespace TailBlazer.Views.Tail
{
    using System;
    using System.Reactive.Linq;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Infrastucture.Virtualisation;
    public class LineProxyFactory : ILineProxyFactory
    {
        #region Fields
        private readonly ILineMatches _lineMatches;
        private readonly ITextFormatter _textFormatter;
        private readonly IObservable<TextScrollInfo> _textScroll;
        private readonly IThemeProvider _themeProvider;
        #endregion
        #region Constructors
        public LineProxyFactory([NotNull] ITextFormatter textFormatter, [NotNull] ILineMatches lineMatches, [NotNull] IObservable<TextScrollInfo> textScrollObservable, [NotNull] IThemeProvider themeProvider)
        {
            if (textScrollObservable == null) throw new ArgumentNullException(nameof(textScrollObservable));
            this._textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            this._lineMatches = lineMatches ?? throw new ArgumentNullException(nameof(lineMatches));
            this._themeProvider = themeProvider ?? throw new ArgumentNullException(nameof(themeProvider));
            this._textScroll = textScrollObservable.StartWith(new TextScrollInfo(0, 0));
        }
        #endregion
        #region Methods
        public LineProxy Create(Line line) => new LineProxy(line, this._textFormatter.GetFormatter(line.Text), this._lineMatches.GetMatches(line.Text), this._textScroll, this._themeProvider);
        #endregion
    }
}