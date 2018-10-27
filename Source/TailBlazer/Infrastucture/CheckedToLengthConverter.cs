namespace TailBlazer.Infrastucture
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    public class CheckedToLengthConverter : MarkupExtension, IValueConverter
    {
        #region Properties
        public GridLength FalseValue { get; set; }
        public GridLength TrueValue { get; set; }
        #endregion
        #region Methods
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToBoolean(value) ? this.TrueValue : this.FalseValue;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
        #endregion
    }
}