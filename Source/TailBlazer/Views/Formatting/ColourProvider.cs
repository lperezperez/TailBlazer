namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DynamicData.Kernel;
    using MaterialDesignColors;
    using TailBlazer.Domain.Formatting;
    using Hue = TailBlazer.Domain.Formatting.Hue;
    public class ColourProvider : IColourProvider
    {
        #region Constructors
        public ColourProvider()
        {
            var swatches = new SwatchesProvider().Swatches.AsArray();
            this.Swatches = swatches.ToDictionary(s => s.Name);
            var accents = swatches.Where(s => s.IsAccented).ToArray();
            var orders = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ThemeConstants.Themes.Select((str, idx) => new { str, idx }).ForEach(x => orders[x.str] = x.idx);
            var greys = swatches //.Where(s => !s.IsAccented)
                        .Where(s => s.Name == "bluegrey").SelectMany
                            (
                             swatch =>
                                 {
                                     var hues = swatch.PrimaryHues.Where(hue => hue.Name == "Primary200" || hue.Name == "Primary300" || hue.Name == "Primary400" || hue.Name == "Primary500").Select(hue => new Hue(swatch.Name, hue.Name, hue.Foreground, hue.Color));
                                     var withNumber = hues.Select
                                         (
                                          h =>
                                              {
                                                  var num = this.GetNumber(h.Name);
                                                  return new { hue = h, Num = num };
                                              }).OrderBy(h => h.Num).Take(8).Select(x => x.hue);
                                     return withNumber;
                                 });
            this.Hues = accents.OrderBy(x => orders.Lookup(x.Name).ValueOr(() => 100)).SelectMany(swatch => { return swatch.AccentHues.Select(hue => new Hue(swatch.Name, hue.Name, hue.Foreground, hue.Color)); }).Union(greys).ToArray();
            this.HueCache = this.Hues.ToDictionary(h => h.Key);
        }
        #endregion
        #region Properties
        public Hue DefaultAccent => Hue.NotSpecified;
        public IEnumerable<Hue> Hues { get; }
        private IDictionary<HueKey, Hue> HueCache { get; }
        private IDictionary<string, Swatch> Swatches { get; }
        #endregion
        #region Methods
        public Hue GetAccent(Theme theme)
        {
            var colour = theme.GetAccentColor();
            var swatch = this.Swatches.Lookup(colour);
            return swatch.Convert(s => new Hue(s.Name, s.AccentExemplarHue.Name, s.AccentExemplarHue.Foreground, s.AccentExemplarHue.Color)).ValueOrThrow(() => new ArgumentOutOfRangeException(colour));
        }
        public Optional<Hue> Lookup(HueKey key)
        {
            if (null == key.Name || null == key.Swatch)
                return new Optional<Hue>();
            return this.HueCache.Lookup(key);
        }
        private string GetNumber(string swatchName) => new string(swatchName.Where(char.IsDigit).ToArray());
        #endregion
    }
}