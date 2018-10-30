namespace TailBlazer.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Formatting;
    public class HighlightTextControl : Control
    {
        #region Fields
        public static readonly DependencyProperty HighlightForegroundBrushProperty = DependencyProperty.Register("HighlightForegroundBrush", typeof(Brush), typeof(HighlightTextControl), new PropertyMetadata(default(Brush), HighlightTextControl.UpdateControlCallBack));
        public static readonly DependencyProperty HighlightBackgroundBrushProperty = DependencyProperty.Register("HighlightBackgroundBrush", typeof(Brush), typeof(HighlightTextControl), new PropertyMetadata(default(Brush), HighlightTextControl.UpdateControlCallBack));
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register(nameof(HighlightTextControl.FormattedText), typeof(IEnumerable<DisplayText>), typeof(HighlightTextControl), new PropertyMetadata(default(IEnumerable<DisplayText>), HighlightTextControl.UpdateControlCallBack));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(HighlightTextControl.Text), typeof(string), typeof(HighlightTextControl), new PropertyMetadata(default(string), HighlightTextControl.UpdateControlCallBack));
        public static readonly DependencyProperty HighlightEnabledProperty = DependencyProperty.Register(nameof(HighlightTextControl.HighlightEnabled), typeof(bool), typeof(HighlightTextControl), new PropertyMetadata(true, HighlightTextControl.UpdateControlCallBack));
        private TextBlock _textBlock;
        #endregion
        #region Constructors
        static HighlightTextControl() { DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightTextControl), new FrameworkPropertyMetadata(typeof(HighlightTextControl))); }
        #endregion
        #region Properties
        public IEnumerable<DisplayText> FormattedText { get => (IEnumerable<DisplayText>)this.GetValue(FormattedTextProperty); set => this.SetValue(FormattedTextProperty, value); }
        public Brush HighlightBackgroundBrush { get => (Brush)this.GetValue(HighlightBackgroundBrushProperty); set => this.SetValue(HighlightBackgroundBrushProperty, value); }
        public bool HighlightEnabled { get => (bool)this.GetValue(HighlightEnabledProperty); set => this.SetValue(HighlightEnabledProperty, value); }
        public Brush HighlightForegroundBrush { get => (Brush)this.GetValue(HighlightForegroundBrushProperty); set => this.SetValue(HighlightForegroundBrushProperty, value); }
        public string Text { get => (string)this.GetValue(TextProperty); set => this.SetValue(TextProperty, value); }
        #endregion
        #region Methods
        private static void UpdateControlCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (HighlightTextControl)d;
            obj.InvalidateVisual();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._textBlock = (TextBlock)this.Template.FindName("PART_TEXT", this);

            //const string sample = "The quick brown fox jumps over the lazy dog";
            //var stringSize = this.MeasureString(sample);
            //var widthPerChar = stringSize.Width / sample.Length;

            ////6.5966
            //Console.WriteLine(widthPerChar);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            this._textBlock.Inlines.Clear();
            if (this.FormattedText == null || !this.FormattedText.Any())
            {
                if (this.Text != null)
                    this._textBlock.Inlines.Add(this.Text);
                base.OnRender(drawingContext);
                return;
            }
            var formattedText = this.FormattedText.AsArray();
            if (formattedText.Length == 1)
            {
                var line = formattedText[0];
                this._textBlock.Text = line.Text;
            }
            else
            {
                this._textBlock.Inlines.AddRange
                    (
                     formattedText.Select
                         (
                          ft =>
                              {
                                  var run = new Run(ft.Text);
                                  if (ft.Highlight && this.HighlightEnabled)
                                  {
                                      if (ft.Hue == Hue.NotSpecified)
                                      {
                                          run.Background = this.HighlightBackgroundBrush;
                                          run.Foreground = this.HighlightForegroundBrush;
                                      }
                                      else
                                      {
                                          run.Background = ft.Hue.BackgroundBrush;
                                          run.Foreground = ft.Hue.ForegroundBrush;
                                      }
                                  }
                                  return run;
                              }));
            }
            base.OnRender(drawingContext);
        }
        #endregion
    }
}