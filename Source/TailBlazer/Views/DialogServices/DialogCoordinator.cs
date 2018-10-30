namespace TailBlazer.Views.DialogServices
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData.Binding;
    public class DialogCoordinator : IDialogCoordinator
    {
        #region Fields
        private readonly SerialDisposable _disposer = new SerialDisposable();
        #endregion
        #region Methods
        public void Close() { this._disposer.Disposable = Disposable.Empty; }
        public void Show(IDialogViewModel view, object content, Action<object> onClosed = null)
        {
            this._disposer.Disposable = Disposable.Empty;
            view.DialogContent = content;
            view.IsDialogOpen = true;
            var closedCallback = view.WhenValueChanged(v => v.IsDialogOpen, false).Where(isOpen => !isOpen).Subscribe(_ => this._disposer.Disposable = Disposable.Empty);
            this._disposer.Disposable = Disposable.Create
                (
                 () =>
                     {
                         closedCallback.Dispose();
                         view.IsDialogOpen = false;
                         onClosed?.Invoke(view.DialogContent);
                     });
        }
        #endregion
    }
}