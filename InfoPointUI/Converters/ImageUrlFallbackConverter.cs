using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace InfoPointUI.Converters
{
    public class ImageUrlFallbackConverter : IValueConverter
    {
        private static readonly string FallbackImageUri = @"pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png";

        private BitmapImage LoadImageFromUrl(string url)
        {
            using var client = new HttpClient();
            var imageBytes = client.GetByteArrayAsync(url).Result;

            using var stream = new MemoryStream(imageBytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? imageName = value as string;

            if (string.IsNullOrWhiteSpace(imageName))
                return new BitmapImage(new Uri(FallbackImageUri));

            try
            {
                Uri imageUri;
                bool isImageFromUrl = false;

                if (Uri.IsWellFormedUriString(imageName, UriKind.Absolute))
                {
                    imageUri = new Uri(imageName, UriKind.Absolute);
                    isImageFromUrl = true;
                }
                else
                {
                    imageUri = new Uri($"pack://application:,,,/InfoPointUI;component/Assets/Images/{imageName.ToLower()}", UriKind.Absolute);
                }

                if (isImageFromUrl)
                {
                    return LoadImageFromUrl(imageName);
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