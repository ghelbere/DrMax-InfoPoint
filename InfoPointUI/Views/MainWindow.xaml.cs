using System.Windows;
using System.Windows.Input;
using InfoPointUI.Models;
using InfoPointUI.ViewModels;

namespace InfoPointUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bool isPortrait = SystemParameters.PrimaryScreenHeight > SystemParameters.PrimaryScreenWidth;
            if (isPortrait)
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
                MainGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            }

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedCategory = "";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Dacă folosești pe tabletă, poți activa tastatura virtuală:
            // Process.Start("C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe");
        }

        private void TileClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement tile && tile.DataContext is ProductDto product)
            {
                MessageBox.Show(
                    $"📦 {product.Name}\n💰 {product.Price:C}\n📍 {product.Location}",
                    "Detalii produs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
    }
}
