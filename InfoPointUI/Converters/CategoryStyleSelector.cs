using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows;

namespace InfoPointUI.Converters
{
    [MarkupExtensionReturnType(typeof(CategoryStyleSelector))]
    public class CategoryStyleSelector : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string selected = value?.ToString() ?? "";
            string current = parameter?.ToString() ?? "";
            string key = selected == current ? "CategoryButtonSelectedStyle" : "CategoryButtonStyle";
            return Application.Current.TryFindResource(key) ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
