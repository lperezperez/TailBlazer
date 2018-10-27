namespace TailBlazer.Views.Tail
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;
    /// <summary>Interaction logic for TailView.xaml</summary>
    public partial class TailView : UserControl
    {
        #region Constructors
        public TailView()
        {
            this.InitializeComponent();
            this.IsVisibleChanged += (sender, e) => { this.FocusSearchTextBox(); };
        }
        #endregion
        #region Methods
        private void ApplicationCommandFind_Executed(object sender, ExecutedRoutedEventArgs e) { this.FocusSearchTextBox(); }
        private void FocusSearchTextBox()
        {
            this.Dispatcher.BeginInvoke
                (
                 new Action
                     (
                      () =>
                          {
                              this.SearchTextBox.Focus();
                              this.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                          }));
        }
        #endregion
    }
}