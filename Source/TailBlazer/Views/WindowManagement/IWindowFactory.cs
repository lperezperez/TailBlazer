namespace TailBlazer.Views.WindowManagement
{
    using System.Collections.Generic;
    public interface IWindowFactory
    {
        #region Methods
        MainWindow Create(IEnumerable<string> files = null);
        #endregion
    }
}