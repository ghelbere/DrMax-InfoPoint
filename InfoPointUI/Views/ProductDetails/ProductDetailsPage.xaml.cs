using InfoPoint.Models;
using InfoPointUI.Controls;
using InfoPointUI.ViewModels;
using System.Windows.Controls;


namespace InfoPointUI.Views.ProductDetails
{
    /// <summary>
    /// Interaction logic for ProductDetailsPage.xaml
    /// </summary>
    public partial class ProductDetailsPage : Page
    {
        private ProductDetailsViewModel _vm;

        public ProductDetailsPage(ProductDto product)
        {
            InitializeComponent();
            _vm = new ProductDetailsViewModel(product);
            DataContext = _vm;

            // Abonare la evenimentul din ViewModel
            _vm.RequestNavigateToLocationPage += NavigateToLocationPage;
            _vm.RequestNavigateToCallConsultant += NavigateToCallConsultant;
        }

        private void NavigateToLocationPage(ProductDto product)
        {
            this.NavigationService?.Navigate(new ProductLocationPage(product));
        }

        private void NavigateToCallConsultant(ProductDto product)
        {
            this.NavigationService?.Navigate(new ProductCallConsultant(product));
        }

        private void Button_PreviewTouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            if (sender is TouchButton button && button?.Command?.CanExecute(button.CommandParameter) == true)
            {
                button.Command.Execute(button.CommandParameter);
                e.Handled = true;
            }
        }

        private void Button_PreviewTouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            e.Handled = true; // Previne evenimentul de click după touch
        }
    }
}
