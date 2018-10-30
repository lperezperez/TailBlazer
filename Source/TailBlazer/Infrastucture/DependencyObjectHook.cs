namespace TailBlazer.Infrastucture
{
    using System.Windows;
    public interface IDependencyObjectReceiver
    {
        #region Methods
        void Receive(DependencyObject value);
        #endregion
    }
    public static class DependencyObjectHook
    {
        #region Fields
        public static readonly DependencyProperty ReceiverProperty = DependencyProperty.RegisterAttached("Receiver", typeof(IDependencyObjectReceiver), typeof(DependencyObjectHook), new PropertyMetadata(default(IDependencyObjectReceiver), DependencyObjectHook.PropertyChanged));
        #endregion
        #region Methods
        public static IDependencyObjectReceiver GetReceiver(UIElement element) => (IDependencyObjectReceiver)element.GetValue(ReceiverProperty);
        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var receiver = args.NewValue as IDependencyObjectReceiver;
            receiver?.Receive(sender);
        }
        public static void SetReceiver(UIElement element, IDependencyObjectReceiver value) { element.SetValue(ReceiverProperty, value); }
        #endregion
    }
}