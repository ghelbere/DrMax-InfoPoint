using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InfoPointUI.Converters
{
    public class BottomBarTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int count && count == 0)
                return "Bine ați venit la farmaciile DrMax";

            if (values.Length >= 3 &&
                values[1] is int page &&
                values[2] is int total)
                return $"Pagina {page} din {total}";

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

    }

}
