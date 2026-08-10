using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.UI.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public static readonly CountToBoolConverter Instance = new CountToBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                // Enable remove button only if there are more than 2 conditions
                return count > 2;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
