namespace TailBlazer.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    public class FileIcon : Control
    {
        #region Constructors
        static FileIcon() { DefaultStyleKeyProperty.OverrideMetadata(typeof(FileIcon), new FrameworkPropertyMetadata(typeof(FileIcon))); }
        #endregion
    }
}