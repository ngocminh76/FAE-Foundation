using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.UI.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selectedValue && parameter is string expectedValue)
            {
                return selectedValue == expectedValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string expectedValue)
            {
                return expectedValue;
            }
            return "";
        }
    }
}
