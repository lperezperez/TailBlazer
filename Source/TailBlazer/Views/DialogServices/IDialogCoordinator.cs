namespace TailBlazer.Views.DialogServices
{
    using System;
    public interface IDialogCoordinator
    {
        #region Methods
        void Close();
        void Show(IDialogViewModel view, object content, Action<object> onClosed);
        #endregion
    }
}