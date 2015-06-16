using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BeLogForm.ValueConverters
{
    [ValueConversion(typeof(DateTime),typeof(DateTime))]
    public class DefaultDateTimeValueConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
           
           DateTime selDateTime = (DateTime)value;
           if(selDateTime==DateTime.MinValue || selDateTime.Year<1970) return DateTime.Now;
            return selDateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime selDateTime = DateTime.Now;
                DateTime.TryParse((string)value, culture, System.Globalization.DateTimeStyles.None, out selDateTime);
           
            if(selDateTime==DateTime.MinValue || selDateTime.Year<1970) return DateTime.Now;
            return selDateTime;
        }
    }
}
