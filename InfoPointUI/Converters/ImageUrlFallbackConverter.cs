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
        public string FallBackImageName = @"pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png";

        public bool ImageExistsInResources(string imageName)
        {
            return true;
            var assembly = Assembly.GetExecutingAssembly();
            string resursaCautata = "InfoPointUI.Assets.Images." + imageName.ToLower(); 
            return assembly.GetManifestResourceNames().Contains(resursaCautata);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? url = value as string;

            if (string.IsNullOrWhiteSpace(url))
            {
                // Fallback image embedded in project
                return new BitmapImage(new Uri(FallBackImageName));
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !File.Exists(url))
            {
                if (!string.IsNullOrEmpty(url))
                {
                    if (ImageExistsInResources(url))
                    {
                        url = @$"pack://application:,,,/InfoPointUI;component/Assets/Images/{url.ToLower()}";
                        try
                        {
                            // try to return the image from Embedded Resources. Warning, not Resource but Embedded resource
                            return new BitmapImage(new Uri(url));
                        }
                        catch
                        {
                            // Fallback image embedded in project, if the resource image is not found
                            return new BitmapImage(new Uri(FallBackImageName));
                        }
                    }
                }
                // Fallback image embedded in project
                return new BitmapImage(new Uri(FallBackImageName));
            }

            try
            {
                return new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                // In case the URL is malformed
                return new BitmapImage(new Uri(FallBackImageName));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    }

}