using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace EXOOAFloor.Helpers
{
    class OnlyDigitsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            Regex regex = new Regex("[^0-9]+");
            if (regex.IsMatch((string)value))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Sólo números");
        }
    }
}
