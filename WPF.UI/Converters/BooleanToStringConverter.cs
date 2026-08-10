using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPF.UI.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length == 2)
                {
                    string result = boolValue ? parts[0] : parts[1];

                    // Nếu targetType là double (cho Width/Height), convert sang số
                    if (targetType == typeof(double) && double.TryParse(result, out double numericResult))
                    {
                        return numericResult;
                    }

                    return result;
                }
            }
            return "AriCad"; // Default value khi không có parameter hoặc format không đúng
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
