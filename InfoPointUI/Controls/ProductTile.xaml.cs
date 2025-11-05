using InfoPointUI.Models;
using InfoPointUI.Views.ProductDetails;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace InfoPointUI.Controls
{
    public partial class ProductTile : UserControl
    {
        private const string FallBackImageName = @"pack://application:,,,/InfoPointUI;component/Assets/Images/empty_image.png";
        private Point _touchStart;
        private Point _mouseStart;
        private const double TapThreshold = 10;

        public ProductTile()
        {
            InitializeComponent();

            TouchDown += OnTouchDown;
            TouchUp += OnTouchUp;
            PreviewMouseLeftButtonDown += OnMouseDown;
            PreviewMouseLeftButtonUp += OnMouseUp;
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

        private void OnTouchDown(object? sender, TouchEventArgs e)
        {
            _touchStart = e.GetTouchPoint(this).Position;
        }

        private async void OnTouchUp(object? sender, TouchEventArgs e)
        {
            var end = e.GetTouchPoint(this).Position;
            var deltaX = Math.Abs(end.X - _touchStart.X);
            var deltaY = Math.Abs(end.Y - _touchStart.Y);

            if (deltaX < TapThreshold && deltaY < TapThreshold)
                await ShowProductDetailsAsync();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseStart = e.GetPosition(this);
        }

        private async void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var end = e.GetPosition(this);
            var deltaX = Math.Abs(end.X - _mouseStart.X);
            var deltaY = Math.Abs(end.Y - _mouseStart.Y);

            if (deltaX < TapThreshold && deltaY < TapThreshold)
                await ShowProductDetailsAsync();
        }

        private async Task ShowProductDetailsAsync()
        {
            if (Product is ProductDto p)
            {
                bool imgOk = await IsImageUrlValidAsync(p.ImageUrl);
               /* MessageBox.Show(
                    $"📦 {p.Name}\n💰 {p.Price:C}\n📍 {p.Location}\n🖼️ Imagine validă: {imgOk}",
                    "Detalii produs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );*/
                var detailsWindow = new ProductDetailsWindow(p);
                detailsWindow.ShowDialog();
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
