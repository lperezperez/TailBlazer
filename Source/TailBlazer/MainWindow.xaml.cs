namespace TailBlazer
{
    using System.ComponentModel;
    using MahApps.Metro.Controls;
    using TailBlazer.Views.WindowManagement;
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : MetroWindow
    {
        #region Constructors
        public MainWindow()
        {
            this.InitializeComponent();
            this.Closing += this.MainWindow_Closing;
        }
        #endregion
        #region Methods
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var windowsModel = this.DataContext as WindowViewModel;
            windowsModel?.OnWindowClosing();
        }
        #endregion
    }
}