namespace TailBlazer.Views.Options
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Input;
    using DynamicData.Binding;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture;
    public sealed class GeneralOptionsViewModel : AbstractNotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private double _highlightDuration;
        private bool _highlightTail;
        private bool _openRecentOnStartup;
        private int _rating;
        private double _scale;
        private bool _showLineNumbers;
        private bool _useDarkTheme;
        #endregion
        #region Constructors
        public GeneralOptionsViewModel(ISetting<GeneralOptions> setting)
        {
            var reader = setting.Value.Subscribe
                (
                 options =>
                     {
                         this.UseDarkTheme = options.Theme == Theme.Dark;
                         this.HighlightTail = options.HighlightTail;
                         this.HighlightDuration = options.HighlightDuration;
                         this.Scale = options.Scale;
                         this.Rating = options.Rating;
                         this.OpenRecentOnStartup = options.OpenRecentOnStartup;
                         this.ShowLineNumbers = options.ShowLineNumbers;
                     });
            this.RequiresRestart = setting.Value.Select(options => options.Rating).HasChanged().ForBinding();
            this.RestartCommand = new Command
                (
                 () =>
                     {
                         Process.Start(Application.ResourceAssembly.Location);
                         Application.Current.Shutdown();
                     });
            var writter = this.WhenAnyPropertyChanged().Subscribe(vm => { setting.Write(new GeneralOptions(this.UseDarkTheme ? Theme.Dark : Theme.Light, this.HighlightTail, this.HighlightDuration, this.Scale, this.Rating, this.OpenRecentOnStartup, this.ShowLineNumbers)); });
            this.HighlightDurationText = this.WhenValueChanged(vm => vm.HighlightDuration).DistinctUntilChanged().Select(value => value.ToString("0.00 Seconds")).ForBinding();
            this.ScaleText = this.WhenValueChanged(vm => vm.Scale).DistinctUntilChanged().Select(value => $"{value} %").ForBinding();
            this.ScaleRatio = this.WhenValueChanged(vm => vm.Scale).DistinctUntilChanged().Select(value => (decimal)value / (decimal)100).ForBinding();
            this._cleanUp = new CompositeDisposable(reader, writter, this.HighlightDurationText, this.ScaleText, this.ScaleRatio);
        }
        #endregion
        #region Properties
        public double HighlightDuration { get => this._highlightDuration; set => this.SetAndRaise(ref this._highlightDuration, value); }
        public IProperty<string> HighlightDurationText { get; }
        public bool HighlightTail { get => this._highlightTail; set => this.SetAndRaise(ref this._highlightTail, value); }
        public bool OpenRecentOnStartup { get => this._openRecentOnStartup; set => this.SetAndRaise(ref this._openRecentOnStartup, value); }
        public int Rating { get => this._rating; set => this.SetAndRaise(ref this._rating, value); }
        public IProperty<bool> RequiresRestart { get; }
        public ICommand RestartCommand { get; }
        public double Scale { get => this._scale; set => this.SetAndRaise(ref this._scale, value); }
        public IProperty<decimal> ScaleRatio { get; }
        public IProperty<string> ScaleText { get; }
        public bool ShowLineNumbers { get => this._showLineNumbers; set => this.SetAndRaise(ref this._showLineNumbers, value); }
        public bool UseDarkTheme { get => this._useDarkTheme; set => this.SetAndRaise(ref this._useDarkTheme, value); }
        #endregion
        #region Methods
        void IDisposable.Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}