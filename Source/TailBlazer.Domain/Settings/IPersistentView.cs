namespace TailBlazer.Domain.Settings
{
    public interface IPersistentView
    {
        #region Methods
        ViewState CaptureState();
        void Restore(ViewState state);
        #endregion
    }
}