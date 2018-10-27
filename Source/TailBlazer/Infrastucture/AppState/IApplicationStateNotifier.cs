namespace TailBlazer.Infrastucture.AppState
{
    using System;
    public interface IApplicationStateNotifier
    {
        #region Properties
        IObservable<ApplicationState> StateChanged { get; }
        #endregion
    }
}