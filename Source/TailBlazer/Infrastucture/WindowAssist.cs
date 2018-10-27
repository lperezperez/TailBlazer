namespace TailBlazer.Infrastucture
{
    using System.Linq;
    using System.Windows;
    public delegate void ApplicationExitingDelegate();
    public static class WindowAssist
    {
        #region Fields
        public static readonly DependencyProperty ApplicationClosingProperty = DependencyProperty.RegisterAttached("ApplicationClosing", typeof(ApplicationExitingDelegate), typeof(WindowAssist), new PropertyMetadata(default(ApplicationExitingDelegate), WindowAssist.OnClosingDelegateSet));
        #endregion
        #region Methods
        public static ApplicationExitingDelegate GetApplicationClosing(Window element) => (ApplicationExitingDelegate)element.GetValue(ApplicationClosingProperty);
        public static void OnClosingDelegateSet(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var window = (Window)sender;
            var closingDelegate = args.NewValue as ApplicationExitingDelegate;
            window.Closing += (s, e) =>
                {
                    var windows = Application.Current.Windows.OfType<MainWindow>().Count();
                    if (windows == 1)
                        closingDelegate?.Invoke();
                };
        }
        public static void SetApplicationClosing(Window element, ApplicationExitingDelegate value) { element.SetValue(ApplicationClosingProperty, value); }
        #endregion
    }
}