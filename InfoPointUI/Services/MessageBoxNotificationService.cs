using InfoPointUI.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace InfoPointUI.Services
{
    /// <summary>
    /// Implementare temporară folosind MessageBox-urile standard WPF
    /// Va fi înlocuită cu OverlayNotificationService pentru interfață modernă
    /// </summary>
    public class MessageBoxNotificationService : INotificationService
    {
        public async Task ShowInformationAsync(string message, string title = "Informație")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public async Task ShowErrorAsync(string message, string title = "Eroare")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public async Task ShowSuccessAsync(string message, string title = "Succes")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirmare")
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }

        public void ShowInformation(string message, string title = "Informație")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void ShowError(string message, string title = "Eroare")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }
}