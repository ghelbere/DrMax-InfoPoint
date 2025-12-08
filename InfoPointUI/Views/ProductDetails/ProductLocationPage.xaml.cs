using InfoPoint.Models;
using InfoPointUI.ViewModels;
using System.Windows.Controls;


namespace InfoPointUI.Views.ProductDetails
{
    /// <summary>
    /// Interaction logic for ProductLocationPage.xaml
    /// </summary>
    public partial class ProductLocationPage : Page
    {
        private ProductDetailsViewModel _vm;

        public ProductLocationPage(ProductDto product)
        {
            InitializeComponent();
            _vm = new ProductDetailsViewModel(product);
            DataContext = _vm;

            // Abonare la evenimentul din ViewModel
            _vm.RequestNavigateToDetails += NavigateToDetails;
        }

        //Command="{Binding LocalizeazaCommand}"
        private void NavigateToDetails(ProductDto product)
        {

            this.NavigationService?.Navigate(new ProductDetailsPage(product));
        }

    }
}
