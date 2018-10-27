namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Reactive.Linq;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Settings;
    public class ThemeProvider : IThemeProvider
    {
        #region Constructors
        public ThemeProvider([NotNull] ISetting<GeneralOptions> setting, [NotNull] IColourProvider colourProvider)
        {
            if (setting == null) throw new ArgumentNullException(nameof(setting));
            if (colourProvider == null) throw new ArgumentNullException(nameof(colourProvider));
            this.Theme = setting.Value.Select(options => options.Theme).Replay(1).RefCount();
            this.Accent = setting.Value.Select(options => colourProvider.GetAccent(options.Theme)).Replay(1).RefCount();
        }
        #endregion
        #region Properties
        public IObservable<Hue> Accent { get; }
        public IObservable<Theme> Theme { get; }
        #endregion
    }
}