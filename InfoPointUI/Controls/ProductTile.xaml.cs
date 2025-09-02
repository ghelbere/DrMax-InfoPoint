using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using InfoPointUI.Models;

namespace InfoPointUI.Controls
{
    public partial class ProductTile : UserControl
    {
        private const string FallBackImageName = @"pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png";

        public ProductTile()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProductProperty =
            DependencyProperty.Register(nameof(Product), typeof(ProductDto), typeof(ProductTile),
                new PropertyMetadata(null, OnProductChanged));

        public ProductDto Product
        {
            get => (ProductDto)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        private static void OnProductChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProductTile tile && e.NewValue is ProductDto product)
            {
                tile.DataContext = product;
            }
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (sender is Image img)
            {
                img.Source = new BitmapImage(new Uri(FallBackImageName));
            }
        }

        private async void OnTileClicked(object sender, MouseButtonEventArgs e)
        {
            if (Product is ProductDto p)
            {
                bool imgOk = await IsImageUrlValidAsync(p.ImageUrl);
                MessageBox.Show(
                    $"📦 {p.Name}\n💰 {p.Price:C}\n📍 {p.Location}\n🖼️ Imagine validă: {imgOk}",
                    "Detalii produs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        public static async Task<bool> IsImageUrlValidAsync(string imageUrl)
        {
            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode &&
                       response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true;
            }
            catch
            {
                return false;
            }
        }
    }
}
