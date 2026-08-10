using System;
using System.Windows.Data;
using WPF.UI.Interface;

namespace WPF.UI.Converters
{
    public class DynamicFieldValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 2)
                return "";

            var obj = values[0] as IDynamicFieldProvider;
            var key = values[1] as string;

            if (obj != null && !string.IsNullOrEmpty(key))
            {
                string val;
                return obj.DynamicFields.TryGetValue(key, out val) ? val : "";
            }

            return "";
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
