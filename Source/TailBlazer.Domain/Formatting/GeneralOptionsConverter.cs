namespace TailBlazer.Domain.Formatting
{
    using System.Xml.Linq;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    public class GeneralOptionsConverter : IConverter<GeneralOptions>
    {
        #region Methods
        public GeneralOptions Convert(State state)
        {
            var defaults = this.GetDefaultValue();
            if (state == State.Empty) return defaults;
            var doc = XDocument.Parse(state.Value);
            var root = doc.ElementOrThrow(Structure.Root);
            var theme = root.ElementOrThrow(Structure.Theme).ParseEnum<Theme>().ValueOr(() => defaults.Theme);
            var logFont = root.ElementOrThrow(Structure.LogFont) ?? defaults.LogFont;
            var highlight = root.ElementOrThrow(Structure.HighlightTail).ParseBool().ValueOr(() => defaults.HighlightTail);
            var duration = root.ElementOrThrow(Structure.Duration).ParseDouble().ValueOr(() => defaults.HighlightDuration);
            var scale = root.ElementOrThrow(Structure.Scale).ParseDouble().ValueOr(() => defaults.Scale);
            var frameRate = root.OptionalElement(Structure.Rating).ConvertOr(rate => rate.ParseInt().Value, () => defaults.Rating);
            var openRecent = root.ElementOrThrow(Structure.OpenRecentOnStartup).ParseBool().ValueOr(() => defaults.OpenRecentOnStartup);
            var showLineNumbers = root.OptionalElement(Structure.ShowLineNumbers).ConvertOr(x => x.ParseBool().Value, () => defaults.ShowLineNumbers);
            return new GeneralOptions(theme, logFont, highlight, duration, scale, frameRate, openRecent, showLineNumbers);
        }
        public State Convert(GeneralOptions options)
        {
            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.Theme, options.Theme));
            root.Add(new XElement(Structure.LogFont, options.LogFont));
            root.Add(new XElement(Structure.HighlightTail, options.HighlightTail));
            root.Add(new XElement(Structure.Duration, options.HighlightDuration));
            root.Add(new XElement(Structure.Scale, options.Scale));
            root.Add(new XElement(Structure.Rating, options.Rating));
            root.Add(new XElement(Structure.OpenRecentOnStartup, options.OpenRecentOnStartup));
            root.Add(new XElement(Structure.ShowLineNumbers, options.ShowLineNumbers));
            var doc = new XDocument(root);
            var value = doc.ToString();
            return new State(1, value);
        }
        public GeneralOptions GetDefaultValue() => new GeneralOptions(Theme.Light, "Consolas", true, 5, 100, 5, true, false);
        #endregion
        #region Classes
        private static class Structure
        {
            #region Constants
            public const string Duration = "Duration";
            public const string LogFont = "LogFont";
            public const string HighlightTail = "HighlightTail";
            public const string OpenRecentOnStartup = "OpenRecentOnStartup";
            public const string Rating = "FrameRate";
            public const string Root = "Root";
            public const string Scale = "Scale";
            public const string ShowLineNumbers = "ShowLineNumbers";
            public const string Theme = "Theme";
            #endregion
        }
        #endregion
    }
}