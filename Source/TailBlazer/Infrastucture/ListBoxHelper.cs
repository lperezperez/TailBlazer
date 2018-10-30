namespace TailBlazer.Infrastucture
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    public class ListBoxHelper
    {
        #region Fields
        public static readonly DependencyProperty SelectionMonitorProperty = DependencyProperty.RegisterAttached("SelectionMonitor", typeof(IAttachedListBox), typeof(ListBoxHelper), new PropertyMetadata(default(ISelectionMonitor), ListBoxHelper.PropertyChanged));
        #endregion
        #region Methods
        public static ISelectionMonitor GetSelectionMonitor(Selector element) => (ISelectionMonitor)element.GetValue(SelectionMonitorProperty);
        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var receiver = args.NewValue as IAttachedListBox;
            receiver?.Receive((ListBox)sender);
        }
        public static void SetSelectionMonitor(Selector element, ISelectionMonitor value) { element.SetValue(SelectionMonitorProperty, value); }
        #endregion
    }
}