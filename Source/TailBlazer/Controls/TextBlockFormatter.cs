namespace TailBlazer.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using TailBlazer.Domain.Formatting;
    public static class TextBlockFormatter
    {
        #region Fields
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(TextBlockFormatter), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty HighlightBackgroundProperty = DependencyProperty.RegisterAttached("HighlightBackground", typeof(Brush), typeof(TextBlockFormatter), new PropertyMetadata(default(Brush), TextBlockFormatter.OnBackgroundChanged));
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached("FormattedText", typeof(IEnumerable<DisplayText>), typeof(TextBlockFormatter), new PropertyMetadata(default(IEnumerable<DisplayText>), TextBlockFormatter.OnFormattedTextChanged));
        #endregion
        #region Methods
        public static Brush GetForeground(UIElement element) => (Brush)element.GetValue(ForegroundProperty);
        public static IEnumerable<DisplayText> GetFormattedText(UIElement element) => (IEnumerable<DisplayText>)element.GetValue(FormattedTextProperty);
        public static Brush GetHighlightBackground(UIElement element) => (Brush)element.GetValue(HighlightBackgroundProperty);
        public static void OnBackgroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //if (e)
        }
        public static void OnFormattedTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var textBlock = (TextBlock)sender;
            var textBlocks = (IEnumerable<DisplayText>)args.NewValue;

            //Run run = new Run(searchedString)
            //{
            //Background = isHighlight ? this.HighlightBackground : this.Background,
            //Foreground = isHighlight ? this.HighlightForeground : this.Foreground,

            // Set the source text with the style which is Italic.
            //   FontStyle = isHighlight ? FontStyles.Italic : FontStyles.Normal,

            // Set the source text with the style which is Bold.
            //FontWeight = isHighlight ? FontWeights.Bold : FontWeights.Normal
            //};
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange
                (
                 textBlocks.Select
                     (
                      ft =>
                          {
                              var run = new Run(ft.Text);
                              var background = TextBlockFormatter.GetHighlightBackground(textBlock);
                              if (background != null) run.Background = background;
                              return new Run(ft.Text);
                          }));
        }
        public static void SetForeground(UIElement element, Brush value) { element.SetValue(ForegroundProperty, value); }
        public static void SetFormattedText(UIElement element, IEnumerable<DisplayText> value) { element.SetValue(FormattedTextProperty, value); }
        public static void SetHighlightBackground(UIElement element, Brush value) { element.SetValue(HighlightBackgroundProperty, value); }
        #endregion
    }
}