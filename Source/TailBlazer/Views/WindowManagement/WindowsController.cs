namespace TailBlazer.Views.WindowManagement
{
    using System;
    using DynamicData;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    public class WindowsController : IWindowsController, IDisposable
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly ISourceCache<HeaderedView, Guid> _views = new SourceCache<HeaderedView, Guid>(vc => vc.Id);
        #endregion
        #region Constructors
        public WindowsController(ILogger logger)
        {
            this._logger = logger;
            this.Views = this._views.AsObservableCache();
        }
        #endregion
        #region Properties
        public IObservableCache<HeaderedView, Guid> Views { get; }
        #endregion
        #region Methods
        public void Dispose()
        {
            this._views.Dispose();
            this.Views.Dispose();
        }
        public void Register(HeaderedView item) { this._views.AddOrUpdate(item); }
        public void Remove(HeaderedView item) { this._views.Remove(item); }
        public void Remove(Guid id) { this._views.Remove(id); }
        #endregion
    }
}