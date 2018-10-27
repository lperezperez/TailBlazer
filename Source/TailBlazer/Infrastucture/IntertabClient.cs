namespace TailBlazer.Infrastucture
{
    using System.Linq;
    using System.Windows;
    using Dragablz;
    using TailBlazer.Views.WindowManagement;
    public class InterTabClient : IInterTabClient
    {
        #region Fields
        private readonly IWindowFactory _factory;
        #endregion
        #region Constructors
        public InterTabClient(IWindowFactory tradeWindowFactory) { this._factory = tradeWindowFactory; }
        #endregion
        #region Methods
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var window = this._factory.Create();
            return new NewTabHost<Window>(window, window.InitialTabablzControl);
        }
        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window) => Application.Current.Windows.OfType<MainWindow>().Count() == 1 ? TabEmptiedResponse.DoNothing : TabEmptiedResponse.CloseWindowOrLayoutBranch;
        #endregion
    }
}