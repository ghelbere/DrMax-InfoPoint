using InfoPointUI.Views;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace InfoPointUI.Services
{
    public class ApplicationManager : IApplicationManager
    {
        private readonly IStandbyService _standbyService;
        private readonly IStandbyManager _standbyManager;
        private readonly MainWindow _mainWindow;
        private readonly ILogger<ApplicationManager> _logger;

        public ApplicationManager(
            IStandbyService standbyService,
            IStandbyManager standbyManager,
            MainWindow mainWindow,
            ILogger<ApplicationManager> logger)
        {
            _standbyService = standbyService;
            _standbyManager = standbyManager;
            _mainWindow = mainWindow;
            _logger = logger;

            _standbyService.PropertyChanged += OnStandbyStateChanged;
        }

        public void StartApplication()
        {
            _logger.LogInformation("Starting DrMax InfoPoint application");

            // Start in standby mode initially
            _standbyService.ForceStandbyMode();
        }

        public void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_mainWindow.IsVisible)
                {
                    _mainWindow.Show();
                }

                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
                _mainWindow.Focus();
            });
        }

        public void ShowStandbyWindow()
        {
            _standbyManager.ShowStandbyWindow();
        }

        public void ShutdownApplication()
        {
            _logger.LogInformation("Shutting down application");
            Application.Current.Shutdown();
        }

        private void OnStandbyStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStandbyService.IsInStandbyMode))
            {
                if (_standbyService.IsInStandbyMode)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.Hide();
                    });
                    ShowStandbyWindow();
                }
                else
                {
                    _standbyManager.HideStandbyWindow();
                    ShowMainWindow();
                }
            }
        }
    }
}