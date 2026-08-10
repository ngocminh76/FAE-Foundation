using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.UI.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool v && v;
            if (Invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v) return Invert ? v != Visibility.Visible : v == Visibility.Visible;
            return false;
        }
    }
}
