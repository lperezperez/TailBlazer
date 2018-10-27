namespace TailBlazer.Views.Searching
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using MaterialDesignThemes.Wpf;
    public class IsGlobalToIconConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isGlobal = (bool)value;
            return isGlobal ? PackIconKind.Download : PackIconKind.Upload;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        #endregion
    }
}