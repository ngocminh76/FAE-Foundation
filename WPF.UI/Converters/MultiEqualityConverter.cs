using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.UI.Converters
{
    public class MultiEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return false;
            }

            var left = values[0]?.ToString();
            var right = values[1]?.ToString();
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
