using InfoPointUI.Services.Interfaces;
using InfoPointUI.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace InfoPointUI.Services
{
    public class NotificationService : INotificationService
    {
        public async Task ShowInformationAsync(string message, string title = "Informație")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowNotificationWindow(message, title, NotificationType.Info);
            });
        }

        public async Task ShowErrorAsync(string message, string title = "Eroare")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowNotificationWindow(message, title, NotificationType.Error);
            });
        }

        public async Task ShowSuccessAsync(string message, string title = "Succes")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowNotificationWindow(message, title, NotificationType.Success);
            });
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "Confirmare")
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );
                return result == MessageBoxResult.Yes;
            });
        }

        public void ShowInformation(string message, string title = "Informație")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowNotificationWindow(message, title, NotificationType.Info);
            });
        }

        public void ShowError(string message, string title = "Eroare")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowNotificationWindow(message, title, NotificationType.Error);
            });
        }

        private void ShowNotificationWindow(string message, string title, NotificationType type)
        {
            var parentWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;

            if (parentWindow == null) 
                return;

            var notificationWindow = new NotificationOverlay(message, title, type)
            {
                Owner = parentWindow,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                ShowInTaskbar = false,
                Topmost = true
            };

            // Setăm dimensiunile pentru a acoperi fereastra principală
            notificationWindow.Left = parentWindow.Left;
            notificationWindow.Top = parentWindow.Top;
            notificationWindow.Width = parentWindow.ActualWidth;
            notificationWindow.Height = parentWindow.ActualHeight;

            notificationWindow.ShowDialog();
        }
    }

    // ENUM definit în același namespace
    public enum NotificationType
    {
        Info,
        Error,
        Success
    }
}