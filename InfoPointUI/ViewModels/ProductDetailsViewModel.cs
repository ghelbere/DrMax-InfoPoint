using System;
using System.Windows;
using System.Windows.Input;
using InfoPointUI.Models;


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

            LocalizeazaCommand = new RelayCommand(_ => Localizeaza());
            CallConsultantCommand = new RelayCommand(_ => CallConsultant());
            NavigateToDetailsCommand = new RelayCommand(_ => NavigateToDetails());
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

    // Implementare RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
