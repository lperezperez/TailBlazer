namespace TailBlazer.Views.Tail
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    public class GeneralOptionBindings : IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public GeneralOptionBindings([NotNull] ISetting<GeneralOptions> generalOptions, ISchedulerProvider schedulerProvider)
        {
            this.UsingDarkTheme = generalOptions.Value.ObserveOn(schedulerProvider.MainThread).Select(options => options.Theme == Theme.Dark).ForBinding();
            this.HighlightTail = generalOptions.Value.ObserveOn(schedulerProvider.MainThread).Select(options => options.HighlightTail).ForBinding();
            this.ShowLineNumbers = generalOptions.Value.ObserveOn(schedulerProvider.MainThread).Select(options => options.ShowLineNumbers).ForBinding();
            this._cleanUp = new CompositeDisposable(this.UsingDarkTheme, this.HighlightTail, this.ShowLineNumbers);
        }
        #endregion
        #region Properties
        public IProperty<bool> HighlightTail { get; }
        public IProperty<bool> ShowLineNumbers { get; }
        public IProperty<bool> UsingDarkTheme { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}