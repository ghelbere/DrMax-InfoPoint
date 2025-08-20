using System;
using System.Globalization;
using System.Windows.Data;

namespace InfoPointUI.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double price;

            switch (value)
            {
                case double d:
                    price = d;
                    break;
                case decimal dec:
                    price = (double)dec;
                    break;
                case int i:
                    price = i;
                    break;
                case string s when double.TryParse(s, out var parsed):
                    price = parsed;
                    break;
                case object o when double.TryParse(o.ToString(), out var parsedObj):
                    price = parsedObj;
                    break;
                default:
                    return value; // fallback, show raw value
            }

            var ci = CultureInfo.CurrentCulture;
            var symbol = ci.Name == "ro-RO" ? "lei" :
                         string.IsNullOrWhiteSpace(ci.NumberFormat.CurrencySymbol) ? "¤" :
                         ci.NumberFormat.CurrencySymbol;

            return string.Format(ci, "{0:N2} {1}", price, symbol);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
