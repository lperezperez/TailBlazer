namespace TailBlazer.Views.WindowManagement
{
    using TailBlazer.Infrastucture;
    public interface IViewOpener
    {
        #region Methods
        void OpenView(HeaderedView headeredView);
        #endregion
    }
}