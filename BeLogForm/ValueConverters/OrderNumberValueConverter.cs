using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BeLogForm.ValueConverters
{
    [ValueConversion(typeof(int), typeof(String))]
    public class OrderNumberValueConverter:IValueConverter
    {
        const string OrderPrefix = "OM";
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //String selValue = (int)value;
            Nullable<int> setValue = value as Nullable<int>;
            if(setValue.HasValue)
            {
               return OrderPrefix + setValue.Value;   
            }
           return OrderPrefix+1000000;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string setValue = value as string;
            var digitStr = setValue.Select(c=>Char.IsDigit(c)).ToString();
            int digit  = 0;
            int.TryParse(digitStr, out digit);
            return digit;
        }
    }
}
