namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Media.Animation;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Ratings;
    using TailBlazer.Domain.Settings;
    public sealed class SystemSetterJob : IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public SystemSetterJob(ISetting<GeneralOptions> setting, IRatingService ratingService, ISchedulerProvider schedulerProvider)
        {
            var themeSetter = setting.Value.Select(options => options.Theme).DistinctUntilChanged().ObserveOn(schedulerProvider.MainThread).Subscribe
                (
                 theme =>
                     {
                         var dark = theme == Theme.Dark;
                         var paletteHelper = new PaletteHelper();
                         paletteHelper.SetLightDark(dark);
                         paletteHelper.ReplaceAccentColor(theme.GetAccentColor());
                     });
            var frameRate = ratingService.Metrics.Take(1).Select(metrics => metrics.FrameRate).Wait();
            schedulerProvider.MainThread.Schedule(() => { Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = frameRate }); });
            this._cleanUp = new CompositeDisposable(themeSetter);
        }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}