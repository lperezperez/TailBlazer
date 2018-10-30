namespace TailBlazer.Infrastucture
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    public class BooleanToHiddenConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo language) => value is bool && (bool)value ? Visibility.Visible : Visibility.Hidden;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language) => value is Visibility && (Visibility)value == Visibility.Hidden;
        #endregion
    }
    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo language) => value is bool && (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language) => value is Visibility && (Visibility)value == Visibility.Visible;
        #endregion
    }
    public class EqualityToBooleanConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value.Equals(parameter);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value.Equals(true) ? parameter : Binding.DoNothing;
        #endregion
    }
    public class EqualsToVisibilityConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
        #endregion
    }
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo language) => value is bool && (bool)value ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language) => value is Visibility && (Visibility)value == Visibility.Collapsed;
        #endregion
    }
    public class NotEqualsToVisibilityConverter : IValueConverter
    {
        #region Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !value.Equals(parameter) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
        #endregion
    }
}