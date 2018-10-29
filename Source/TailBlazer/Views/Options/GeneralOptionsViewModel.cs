namespace TailBlazer.Views.Options
{
    using DynamicData.Binding;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture;
    public sealed class GeneralOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable cleanUp;
        private string logFont;
        private double highlightDuration;
        private bool highlightTail;
        private bool openRecentOnStartup;
        private int rating;
        private double scale;
        private bool showLineNumbers;
        private bool useDarkTheme;
        #endregion
        #region Constructors
        public GeneralOptionsViewModel(ISetting<GeneralOptions> setting)
        {
            var reader = setting.Value.Subscribe(
                 options =>
                     {
                         this.UseDarkTheme = options.Theme == Theme.Dark;
                         this.LogFont = new FontFamily(options.LogFont);
                         this.HighlightTail = options.HighlightTail;
                         this.HighlightDuration = options.HighlightDuration;
                         this.Scale = options.Scale;
                         this.Rating = options.Rating;
                         this.OpenRecentOnStartup = options.OpenRecentOnStartup;
                         this.ShowLineNumbers = options.ShowLineNumbers;
                     });
            this.RequiresRestart = setting.Value.Select(options => options.Rating).HasChanged().ForBinding();
            this.RestartCommand = new Command(
                 () =>
                     {
                         Process.Start(Application.ResourceAssembly.Location);
                         Application.Current.Shutdown();
                     });
            var writer = this.WhenAnyPropertyChanged().Subscribe(vm => { setting.Write(new GeneralOptions(this.UseDarkTheme ? Theme.Dark : Theme.Light, this.LogFont.Source, this.HighlightTail, this.HighlightDuration, this.Scale, this.Rating, this.OpenRecentOnStartup, this.ShowLineNumbers)); });
            this.HighlightDurationText = this.WhenValueChanged(vm => vm.HighlightDuration).DistinctUntilChanged().Select(value => value.ToString("0.00 Seconds")).ForBinding();
            this.ScaleText = this.WhenValueChanged(vm => vm.Scale).DistinctUntilChanged().Select(value => $"{value} %").ForBinding();
            this.ScaleRatio = this.WhenValueChanged(vm => vm.Scale).DistinctUntilChanged().Select(value => (decimal)value / 100).ForBinding();
            this.cleanUp = new CompositeDisposable(reader, writer, this.HighlightDurationText, this.ScaleText, this.ScaleRatio);
        }
        #endregion
        #region Properties
        public double HighlightDuration { get => this.highlightDuration; set => this.SetAndRaise(ref this.highlightDuration, value); }
        public IProperty<string> HighlightDurationText { get; }
        public bool HighlightTail { get => this.highlightTail; set => this.SetAndRaise(ref this.highlightTail, value); }
        public bool OpenRecentOnStartup { get => this.openRecentOnStartup; set => this.SetAndRaise(ref this.openRecentOnStartup, value); }
        public int Rating { get => this.rating; set => this.SetAndRaise(ref this.rating, value); }
        public IProperty<bool> RequiresRestart { get; }
        public ICommand RestartCommand { get; }
        public double Scale { get => this.scale; set => this.SetAndRaise(ref this.scale, value); }
        public IProperty<decimal> ScaleRatio { get; }
        public IProperty<string> ScaleText { get; }
        public bool ShowLineNumbers { get => this.showLineNumbers; set => this.SetAndRaise(ref this.showLineNumbers, value); }
        public bool UseDarkTheme { get => this.useDarkTheme; set => this.SetAndRaise(ref this.useDarkTheme, value); }
        public FontFamily LogFont { get => new FontFamily(this.logFont); set => this.SetAndRaise(ref this.logFont, value.Source); }
        #endregion
        #region Methods
        void IDisposable.Dispose() => this.cleanUp.Dispose();
        #endregion
    }
}