using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WPF.UI.Converters
{
    public class FirstItemColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFirstInImage)
            {
                return isFirstInImage ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Black);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
