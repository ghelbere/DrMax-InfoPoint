using InfoPointUI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InfoPointUI.Views
{
    public partial class CardScanWindow : Window
    {
        public CardScanWindow(Window? owner = null)
        {
            InitializeComponent();
            if (owner != null)
                Owner = owner;

            this.Activated += (s, e) =>
            {
                txtCardCode.Focus();
            };

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

        private void ValidateCard(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                Close();
                return;
            }

            if (LoyaltyCardValidator.IsValid(code))
            {
                MessageBox.Show("Card valid");
                ((App)Application.Current).LoyaltyCardCode = $"{code}";
                // TODO: Acum verifica daca acest card este activ si in DrMax/Dataklas
                Close();
            }
            else
            {
                MessageBox.Show("Card invalid");
                ((App)Application.Current).LoyaltyCardCode = String.Empty;
                txtCardCode.Text = String.Empty;
                txtCardCode.Focus();
            }
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
    }
}
