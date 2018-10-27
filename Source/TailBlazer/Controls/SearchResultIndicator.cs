namespace TailBlazer.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using TailBlazer.Domain.Annotations;
    public enum SearchResultIndicatorStatus
    {
        None,
        Regex,
        Text
    }
    [TemplatePart(Name = TemplateParts.Regex, Type = typeof(RegexMatchedIcon)), TemplatePart(Name = TemplateParts.Text, Type = typeof(TextMatchedIcon)), TemplateVisualState(Name = SearchResultIndicatorStates.None, GroupName = "Indicator"), TemplateVisualState(Name = SearchResultIndicatorStates.Regex, GroupName = "Indicator"), TemplateVisualState(Name = SearchResultIndicatorStates.Text, GroupName = "Indicator")]
    public class SearchResultIndicator : Control
    {
        #region Fields
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(SearchResultIndicatorStatus), typeof(SearchResultIndicator), new PropertyMetadata(SearchResultIndicatorStatus.None, SearchResultIndicator.OnStatusPropertyChanged));
        #endregion
        #region Constructors
        //public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        //    "Foreground", typeof (Brush), typeof (SearchResultIndicator), new PropertyMetadata(default(Brush)));

        //public Brush Foreground
        //{
        //    get { return (Brush) GetValue(ForegroundProperty); }
        //    set { SetValue(ForegroundProperty, value); }
        //}
        static SearchResultIndicator() { DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchResultIndicator), new FrameworkPropertyMetadata(typeof(SearchResultIndicator))); }
        #endregion
        #region Properties
        public SearchResultIndicatorStatus Status { get => (SearchResultIndicatorStatus)this.GetValue(StatusProperty); set => this.SetValue(StatusProperty, value); }
        #endregion
        #region Methods
        private static void OnStatusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = (SearchResultIndicator)sender;
            var newStatus = (SearchResultIndicatorStatus)args.NewValue;
            var oldStatus = (SearchResultIndicatorStatus)args.OldValue;
            if (newStatus != oldStatus)
                indicator.UpdateVisualState(true);
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UpdateVisualState(false);
        }
        private void UpdateVisualState(bool useTransitions)
        {
            switch (this.Status)
            {
                case SearchResultIndicatorStatus.Regex:
                    var x = VisualStateManager.GoToState(this, SearchResultIndicatorStates.Regex, useTransitions);
                    break;
                case SearchResultIndicatorStatus.Text:
                    VisualStateManager.GoToState(this, SearchResultIndicatorStates.Text, useTransitions);
                    break;
                default:
                    VisualStateManager.GoToState(this, SearchResultIndicatorStates.None, useTransitions);
                    break;
            }
        }
        #endregion
        #region Classes
        [UsedImplicitly]
        private class SearchResultIndicatorStates
        {
            #region Constants
            public const string None = "None";
            public const string Regex = "Regex";
            public const string Text = "Text";
            #endregion
        }
        [UsedImplicitly]
        private class TemplateParts
        {
            #region Constants
            public const string Regex = "PART_RegexImage";
            public const string Text = "PART_TextImage";
            #endregion
        }
        #endregion

        //private void UpdateVisualState(SearchResultIndicatorStatus status)
        //{

        //}
    }
}