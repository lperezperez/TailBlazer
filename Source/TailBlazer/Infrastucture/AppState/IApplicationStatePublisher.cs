namespace TailBlazer.Infrastucture.AppState
{
    public interface IApplicationStatePublisher
    {
        #region Methods
        void Publish(ApplicationState state);
        #endregion
    }
}