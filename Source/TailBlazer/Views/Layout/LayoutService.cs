namespace TailBlazer.Views.Layout
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Xml.Linq;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture.AppState;
    public class LayoutService : ILayoutService
    {
        #region Constants
        private const string LayoutName = "_LayoutName";
        #endregion
        #region Fields
        private readonly ILogger _logger;
        private readonly IObjectProvider _objectProvider;
        private readonly ISettingsStore _store;
        #endregion
        #region Constructors
        public LayoutService(ISettingsStore store, ILogger logger, ISchedulerProvider schedulerProvider, IObjectProvider objectProvider, IApplicationStateNotifier stateNotifier)
        {
            this._store = store;
            this._logger = logger;
            this._objectProvider = objectProvider;
            schedulerProvider.MainThread.Schedule(this.Restore);
            stateNotifier.StateChanged.Where(state => state == ApplicationState.ShuttingDown).Subscribe(_ => { this.Write(); });
        }
        #endregion
        #region Methods
        public void Restore()
        {
            try
            {
                var restored = this._store.Load(LayoutName);
                if (restored == State.Empty)
                    return;
                var element = XDocument.Parse(restored.Value);
                var converter = this._objectProvider.Get<ILayoutConverter>();
                converter.Restore(element.Root);
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Problem reading layout");
            }
        }
        public void Write()
        {
            try
            {
                var converter = this._objectProvider.Get<ILayoutConverter>();
                var xml = converter.CaptureState();
                this._store.Save(LayoutName, new State(1, xml.ToString()));
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "Problem saving layout");
            }
        }
        #endregion
    }
}