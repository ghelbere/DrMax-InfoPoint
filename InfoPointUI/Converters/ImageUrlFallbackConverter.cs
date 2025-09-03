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
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = imageUri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bitmap.EndInit();
                    if (!bitmap.IsFrozen && bitmap.CanFreeze)
                        bitmap.Freeze();
                    return bitmap;
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