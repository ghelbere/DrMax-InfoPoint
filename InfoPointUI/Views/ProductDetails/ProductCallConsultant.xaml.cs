using InfoPoint.Models;
using InfoPointUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoPointUI.Views.ProductDetails
{
    /// <summary>
    /// Interaction logic for ProductCallConsultant.xaml
    /// </summary>
    public partial class ProductCallConsultant : Page
    {

        private ProductDetailsViewModel _vm;
        public ProductCallConsultant(ProductDto product)
        {
            InitializeComponent();
            _vm = new ProductDetailsViewModel(product);
            DataContext = _vm;

            // Abonare la evenimentul din ViewModel
            _vm.RequestNavigateToDetails += NavigateToDetails;
        }

        private void NavigateToDetails(ProductDto product)
        {
            this.NavigationService?.Navigate(new ProductDetailsPage(product));
        }
    }
}
