namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DynamicData;
    using DynamicData.Kernel;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Formatting;
    public sealed class DefaultColourSelector : IDefaultColourSelector
    {
        #region Fields
        private readonly IColourProvider _colourProvider;
        private readonly DefaultHue[] _defaults;
        #endregion
        #region Constructors
        public DefaultColourSelector(IColourProvider colourProvider)
        {
            this._colourProvider = colourProvider;
            this._defaults = this.Load().ToArray();
        }
        #endregion
        #region Methods
        public Hue Lookup(HueKey key) { return this._colourProvider.Lookup(key).ValueOr(() => this._colourProvider.DefaultAccent); }
        public Hue Select(string text)
        {
            var match = this._defaults.FirstOrDefault(hue => hue.MatchTextOnCase ? hue.Text.Equals(text) : hue.Text.Equals(text, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.Hue : this._colourProvider.DefaultAccent;
        }
        private IEnumerable<DefaultHue> Load()
        {
            yield return new DefaultHue("DEBUG", this.Lookup("blue", "Accent400"));
            yield return new DefaultHue("INFO", this.Lookup("deeppurple", "Accent200"));
            yield return new DefaultHue("WARN", this.Lookup("orange", "Accent700"));
            yield return new DefaultHue("WARNING", this.Lookup("orange", "Accent700"));
            yield return new DefaultHue("ERROR", this.Lookup("red", "Accent400"));
            yield return new DefaultHue("FATAL", this.Lookup("red", "Accent700"));
        }
        private Hue Lookup(string swatch, string name) { return this._colourProvider.Lookup(new HueKey(swatch, name)).ValueOrThrow(() => new MissingKeyException(swatch + "." + name + " is invalid")); }
        #endregion
        #region Classes
        private class DefaultHue
        {
            #region Constructors
            public DefaultHue(string text, Hue hue, bool matchTextOnCase = false)
            {
                this.Text = text;
                this.Hue = hue;
                this.MatchTextOnCase = matchTextOnCase;
            }
            #endregion
            #region Properties
            public Hue Hue { get; }
            public bool MatchTextOnCase { get; }
            public string Text { get; }
            #endregion
        }
        #endregion
    }
}