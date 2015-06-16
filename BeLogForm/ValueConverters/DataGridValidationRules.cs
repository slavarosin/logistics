using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace BeLogForm.ValueConverters
{
    public class DataGridValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value,
            System.Globalization.CultureInfo cultureInfo)
        {
            
            BindingGroup bg = (BindingGroup)value;
            DataAccessLayer.OrderItem ot = (DataAccessLayer.OrderItem)bg.Items[0];
            //System.Diagnostics.Debug.Print(ot.);

            if (bg.Items.Count > 3)
            {
                return new ValidationResult(false,
                    "Start Date must be earlier than End Date.");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
