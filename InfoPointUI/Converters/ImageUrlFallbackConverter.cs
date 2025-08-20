using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace InfoPointUI.Converters
{
    public class ImageUrlFallbackConverter : IValueConverter
    {
        private static readonly string FallbackImageUri = @"pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? imageName = value as string;

            if (string.IsNullOrWhiteSpace(imageName))
                return new BitmapImage(new Uri(FallbackImageUri));

            try
            {
                Uri imageUri;

                if (Uri.IsWellFormedUriString(imageName, UriKind.Absolute))
                {
                    imageUri = new Uri(imageName, UriKind.Absolute);
                }
                else
                {
                    imageUri = new Uri($"pack://application:,,,/InfoPointUI;component/Assets/Images/{imageName.ToLower()}", UriKind.Absolute);
                }

                return new BitmapImage(imageUri);
            }
            catch
            {
                return new BitmapImage(new Uri(FallbackImageUri));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

}