namespace TailBlazer.Infrastucture.AppState
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    internal class ApplicationStateBroker : IApplicationStateNotifier, IApplicationStatePublisher
    {
        #region Fields
        private readonly ISubject<ApplicationState> _stateChanged = new ReplaySubject<ApplicationState>(1);
        #endregion
        #region Properties
        public IObservable<ApplicationState> StateChanged => this._stateChanged.DistinctUntilChanged();
        #endregion
        #region Methods
        public void Publish(ApplicationState state) { this._stateChanged.OnNext(state); }
        #endregion
    }
}