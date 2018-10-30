namespace TailBlazer.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    public class SearchIcon : Control
    {
        #region Constructors
        static SearchIcon() { DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchIcon), new FrameworkPropertyMetadata(typeof(SearchIcon))); }
        #endregion
    }
}