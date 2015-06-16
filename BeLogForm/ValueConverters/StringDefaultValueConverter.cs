using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BeLogForm.ValueConverters
{
    [ValueConversion(typeof(String), typeof(String))]
    public class StringDefaultValueConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            String selValue = (String)value;
            if (selValue == null) return String.Empty;
            return selValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String selValue = (String)value;
            if (selValue == null) return String.Empty;
            return selValue;
        }
    }
}
