namespace TailBlazer.Views.WindowManagement
{
    using System;
    using System.Collections.Generic;
    using Dragablz;
    using TailBlazer.Domain.Infrastructure;
    public class WindowFactory : IWindowFactory
    {
        #region Fields
        private readonly IObjectProvider _objectProvider;
        #endregion
        #region Constructors
        public WindowFactory(IObjectProvider objectProvider) { this._objectProvider = objectProvider; }
        #endregion
        #region Methods
        public MainWindow Create(IEnumerable<string> files = null)
        {
            var window = new MainWindow();
            var model = this._objectProvider.Get<WindowViewModel>();
            model.OpenFiles(files);
            window.DataContext = model;
            window.Closing += (sender, e) =>
                {
                    if (TabablzControl.GetIsClosingAsPartOfDragOperation(window)) return;
                    var todispose = ((MainWindow)sender).DataContext as IDisposable;
                    todispose?.Dispose();
                };
            return window;
        }
        #endregion
    }
}