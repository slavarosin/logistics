using System;
using System.Windows.Controls;

namespace BeLogForm
{
    public class EmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (((string)value) == String.Empty) return new ValidationResult(false, "Ei saa olla tühi");
            else return new ValidationResult(true, null);
        }
    }
}
