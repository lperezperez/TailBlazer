namespace TailBlazer.Views.DialogServices
{
    using System.ComponentModel;
    public interface IDialogViewModel : INotifyPropertyChanged
    {
        #region Properties
        object DialogContent { get; set; }
        bool IsDialogOpen { get; set; }
        #endregion
    }
}