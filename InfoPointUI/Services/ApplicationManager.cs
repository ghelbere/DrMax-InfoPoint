using InfoPointUI.Views;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Threading.Tasks;
using InfoPointUI.Services.Interfaces;

namespace InfoPointUI.Services
{
    public class ApplicationManager : IApplicationManager
    {
        private readonly IStandbyService _standbyService;
        private readonly IStandbyManager _standbyManager;
        private readonly MainWindow _mainWindow;
        private readonly ILogger<ApplicationManager> _logger;
        private bool _isShuttingDown;

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
            _isShuttingDown = false;

            _standbyService.PropertyChanged += OnStandbyStateChanged;
            _mainWindow.Closing += OnMainWindowClosing;

            _logger.LogInformation("ApplicationManager initialized");
        }

        public void StartApplication()
        {
            _logger.LogInformation("=== STARTING APPLICATION ===");
            ConfigureMainWindow();


            _logger.LogInformation("Calling ForceStandbyMode to start in standby");
            _standbyService.ForceStandbyMode();

            _logger.LogInformation("Application started successfully");
        }

        public void ShowMainWindow()
        {
            if (_isShuttingDown) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _logger.LogInformation(">>> Showing MainWindow <<<");

                if (!_mainWindow.IsVisible)
                {
                    _mainWindow.Show();
                    _logger.LogInformation("MainWindow shown");
                }

#if !DEBUG
                _mainWindow.WindowState = WindowState.Maximized;
                _mainWindow.WindowStyle = WindowStyle.None;
#endif
                _mainWindow.Activate();
                _mainWindow.Focus();

                _logger.LogInformation("MainWindow activated and focused");
            });
        }

        public void ShowStandbyWindow()
        {
            if (_isShuttingDown) return;

            _logger.LogInformation(">>> Calling StandbyManager.ShowStandbyWindow <<<");
            _standbyManager.ShowStandbyWindow();
        }

        public void ShutdownApplication()
        {
            _logger.LogInformation("Shutting down application gracefully");
            _isShuttingDown = true;
            _standbyService.PropertyChanged -= OnStandbyStateChanged;
            _mainWindow.Closing -= OnMainWindowClosing;
            TouchKeyboardManager.HideTouchKeyboard();
            Application.Current.Shutdown();
        }

        public void ForceShutdown()
        {
            _logger.LogInformation("Force shutting down application");
            _isShuttingDown = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                TouchKeyboardManager.HideTouchKeyboard();
                Application.Current.Shutdown();
            });
        }

        private void ConfigureMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
#if !DEBUG
                _mainWindow.WindowState = WindowState.Maximized;
                _mainWindow.WindowStyle = WindowStyle.None;
                _mainWindow.WindowStyle = WindowStyle.None;
                _mainWindow.ResizeMode = ResizeMode.NoResize;
                _mainWindow.Topmost = true;
#endif
                _logger.LogDebug("Main window configured for kiosk mode");
            });
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isShuttingDown)
            {
                _logger.LogInformation("Main window closing - application shutdown");
                return;
            }

            _logger.LogInformation("Main window closing blocked - entering standby mode");
            e.Cancel = true;
            _standbyService.ForceStandbyMode();
        }

        private void OnStandbyStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isShuttingDown) return;

            if (e.PropertyName == nameof(IStandbyService.IsInStandbyMode))
            {
                _logger.LogInformation("=== STANDBY STATE CHANGED ===");
                _logger.LogInformation("New IsInStandbyMode value: {IsInStandbyMode}", _standbyService.IsInStandbyMode);

                if (_standbyService.IsInStandbyMode)
                {
                    _logger.LogInformation(">>> ENTERING STANDBY FLOW <<<");

                    _standbyService.ForceStandbyMode();

                    _logger.LogInformation("Hiding MainWindow...");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.Hide();
                        _logger.LogInformation("MainWindow hidden successfully");
                    });

                    _logger.LogInformation("Calling ShowStandbyWindow...");
                    ShowStandbyWindow();
                    _logger.LogInformation(">>> STANDBY FLOW COMPLETED <<<");
                }
                else
                {
                    _logger.LogInformation(">>> EXITING STANDBY FLOW <<<");

                    _standbyService.ForceActiveMode();

                    _logger.LogInformation("Hiding standby window...");
                    _standbyManager.HideStandbyWindow();

                    _logger.LogInformation("Showing MainWindow...");
                    ShowMainWindow();
                    _logger.LogInformation(">>> ACTIVE FLOW COMPLETED <<<");
                }
            }
        }

        public void Dispose()
        {
            _isShuttingDown = true;
            _standbyService.PropertyChanged -= OnStandbyStateChanged;
            _mainWindow.Closing -= OnMainWindowClosing;
            _logger.LogInformation("ApplicationManager disposed");
        }
    }
}