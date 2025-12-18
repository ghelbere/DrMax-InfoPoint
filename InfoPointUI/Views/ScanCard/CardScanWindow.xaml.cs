using InfoPointUI.Helpers;
using InfoPointUI.Services.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InfoPointUI.Views
{
    public partial class CardScanWindow : Window
    {
        private readonly ICardService _cardService;
        public CardScanWindow(ICardService cardService)
        {
            InitializeComponent();
            _cardService = cardService;

            // Abonează-te la evenimente
            _cardService.CardValidated += OnCardValidated;
            _cardService.CardValidationFailed += OnCardValidationFailed;

            txtCardCode.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Grid)
                Close();
        }

        private void CenterPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.Enter)
                ValidateCard(txtCardCode.Text);
        }

        private async void ValidateCard(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                Close();
                return;
            }

            // Folosește serviciul injectat
            var result = await _cardService.ValidateAndStoreCardAsync(code);

            // UI-ul se va actualiza prin evenimente (OnCardValidated/OnCardValidationFailed)
            // Fereastra se va închide automat dacă cardul e valid
        }

        private void OnCardValidated(object? sender, EventArgs e)
        {
            // Card validat cu succes
            // Închide fereastra sau navighează mai departe
            Dispatcher.Invoke(() =>
            {
                // Poți afișa un mesaj mai frumos decât MessageBox
                // statusText.Text = "✓ Card validat!";
                Close();
            });
        }

        private void OnCardValidationFailed(object? sender, string errorMessage)
        {
            // Card invalid
            Dispatcher.Invoke(() =>
            {
                // Poți afișa eroarea într-un control UI, nu MessageBox
                // errorText.Text = errorMessage;
                txtCardCode.Text = string.Empty;
                txtCardCode.Focus();
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtCardCode.Focus();
        }

        private void txtCardCode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Acceptă doar cifre
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void txtCardCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Permite ESC și ENTER
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                e.Handled = false; // acceptă
                return;
            }

            // Permite taste de control (Backspace, Delete, săgeți)
            if (e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Left || e.Key == Key.Right  ||
                e.Key == Key.Home || e.Key == Key.End)
            {
                e.Handled = false;
                return;
            }

            // Blochează restul tastelor
            //if (Keyboard.Modifiers == ModifierKeys.Control)
            //{
            //    // ex: Ctrl+C, Ctrl+V – le blocam
            //    e.Handled = true;
            //}
        }

        // Event handler pentru buton/Enter key
        private void OnCardCodeSubmitted(object sender, EventArgs e)
        {
            ValidateCard(txtCardCode.Text);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup
            _cardService.CardValidated -= OnCardValidated;
            _cardService.CardValidationFailed -= OnCardValidationFailed;
            base.OnClosed(e);
        }
    }
}
