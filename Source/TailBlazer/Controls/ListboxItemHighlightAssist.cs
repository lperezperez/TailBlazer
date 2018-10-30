namespace TailBlazer.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    public static class ListboxItemHighlightAssist
    {
        #region Fields
        public static readonly DependencyProperty HighlightBackgroundBrushProperty = DependencyProperty.RegisterAttached("HighlightBackgroundBrush", typeof(Brush), typeof(ListboxItemHighlightAssist), new PropertyMetadata(default(Brush), ListboxItemHighlightAssist.OnPropertyChanged));
        public static readonly DependencyProperty HighlightForegroundBrushProperty = DependencyProperty.RegisterAttached("HighlightForegroundBrush", typeof(Brush), typeof(ListboxItemHighlightAssist), new PropertyMetadata(default(Brush), ListboxItemHighlightAssist.OnPropertyChanged));
        public static readonly DependencyProperty IsRecentProperty = DependencyProperty.RegisterAttached("IsRecent", typeof(bool), typeof(ListboxItemHighlightAssist), new PropertyMetadata(default(bool), ListboxItemHighlightAssist.OnPropertyChanged));
        public static readonly DependencyProperty BaseStyleProperty = DependencyProperty.RegisterAttached("BaseStyle", typeof(Style), typeof(ListboxItemHighlightAssist), new PropertyMetadata(default(Style)));
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ListboxItemHighlightAssist), new PropertyMetadata(default(bool)));
        #endregion
        #region Methods
        public static Style GetBaseStyle(ListBoxItem element) => (Style)element.GetValue(BaseStyleProperty);
        public static Brush GetHighlightBackgroundBrush(ListBoxItem element) => (Brush)element.GetValue(HighlightBackgroundBrushProperty);
        public static Brush GetHighlightForegroundBrush(ListBoxItem element) => (Brush)element.GetValue(HighlightForegroundBrushProperty);
        public static bool GetIsEnabled(ListBoxItem element) => (bool)element.GetValue(IsEnabledProperty);
        public static bool GetIsRecent(ListBoxItem element) => (bool)element.GetValue(IsRecentProperty);
        public static void SetBaseStyle(ListBoxItem element, Style value) { element.SetValue(BaseStyleProperty, value); }
        public static void SetHighlightBackgroundBrush(ListBoxItem element, Brush value) { element.SetValue(HighlightBackgroundBrushProperty, value); }
        public static void SetHighlightForegroundBrush(ListBoxItem element, Brush value) { element.SetValue(HighlightForegroundBrushProperty, value); }
        public static void SetIsEnabled(ListBoxItem element, bool value) { element.SetValue(IsEnabledProperty, value); }
        public static void SetIsRecent(ListBoxItem element, bool value) { element.SetValue(IsRecentProperty, value); }
        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (!(dependencyObject is ListBoxItem sender)) return;
            var enabled = ListboxItemHighlightAssist.GetIsEnabled(sender);
            var isRecent = ListboxItemHighlightAssist.GetIsRecent(sender);
            if (!enabled || !isRecent)
                return;
            var foreground = ListboxItemHighlightAssist.GetHighlightForegroundBrush(sender);
            var background = ListboxItemHighlightAssist.GetHighlightBackgroundBrush(sender);
            if (foreground == null || background == null)
                return;

            //Foreground
            var foregroundAnimation = new ColorAnimation { From = ((SolidColorBrush)background).Color, Duration = new Duration(TimeSpan.FromSeconds(5)) };
            Storyboard.SetTarget(foregroundAnimation, sender);
            Storyboard.SetTargetProperty(foregroundAnimation, new PropertyPath("(Control.Foreground).Color"));
            var sb = new Storyboard();
            //  sb.Children.Add(backgroundAnimation);
            sb.Children.Add(foregroundAnimation);
            sb.Begin();
        }
        #endregion
    }
}