using System;
using System.Windows;
using System.Windows.Input;
using InfoPointUI.Commands;
using InfoPoint.Models;


namespace InfoPointUI.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductDto Product { get; }

        public ICommand LocalizeazaCommand { get; }
        public ICommand CallConsultantCommand { get; }
        public ICommand NavigateToDetailsCommand { get; }



        // Eveniment pentru navigare către pagina Localizare
        public event Action<ProductDto>? RequestNavigateToLocationPage;
        public event Action<ProductDto>? RequestNavigateToDetails;
        public event Action<ProductDto>? RequestNavigateToCallConsultant;


        public ProductDetailsViewModel(ProductDto product)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));

            LocalizeazaCommand = new RelayCommand<object>(_ => Localizeaza());
            CallConsultantCommand = new RelayCommand<object>(_ => CallConsultant());
            NavigateToDetailsCommand = new RelayCommand<object>(_ => NavigateToDetails());
        }


        private void NavigateToDetails()
        {
            // Emitem evenimentul, pagina va naviga
            RequestNavigateToDetails?.Invoke(Product);
        }
        private void Localizeaza()
        {
            // Emitem evenimentul, pagina va naviga
            RequestNavigateToLocationPage?.Invoke(Product);
        }

        private void CallConsultant()
        {
            RequestNavigateToCallConsultant?.Invoke(Product);
        }

    }
}
