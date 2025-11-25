using InfoPointUI.Views;
using Microsoft.Extensions.Logging;
using System;

namespace InfoPointUI.Services
{
    public class StandbyManager : IStandbyManager
    {
        private readonly IStandbyService _standbyService;
        private readonly ILogger<StandbyManager> _logger;
        private StandbyWindow? _currentStandbyWindow;
        private bool _isStandbyMode;

        public StandbyManager(IStandbyService standbyService, ILogger<StandbyManager> logger)
        {
            _standbyService = standbyService;
            _logger = logger;

            _standbyService.PropertyChanged += OnStandbyServicePropertyChanged;
        }

        public void Initialize()
        {
            _logger.LogInformation("Standby manager initialized");
        }

        public void ShowStandbyWindow()
        {
            if (_currentStandbyWindow != null)
            {
                _logger.LogDebug("Standby window already shown");
                return;
            }

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentStandbyWindow = new StandbyWindow(_standbyService);
                    _currentStandbyWindow.Closed += OnStandbyWindowClosed;
                    _currentStandbyWindow.Show();

                    _isStandbyMode = true;
                    _logger.LogInformation("Standby window shown");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to show standby window");
            }
        }

        public void HideStandbyWindow()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (_currentStandbyWindow != null)
                {
                    _currentStandbyWindow.Closed -= OnStandbyWindowClosed;
                    _currentStandbyWindow.Close();
                    _currentStandbyWindow = null;
                    _isStandbyMode = false;
                    _logger.LogInformation("Standby window hidden");
                }
            });
        }

        private void OnStandbyServicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStandbyService.IsInStandbyMode))
            {
                _logger.LogDebug("Standby mode changed to: {IsInStandbyMode}", _standbyService.IsInStandbyMode);

                if (_standbyService.IsInStandbyMode && !_isStandbyMode)
                {
                    _logger.LogInformation("Entering standby mode - showing standby window");
                    ShowStandbyWindow();
                }
                else if (!_standbyService.IsInStandbyMode && _isStandbyMode)
                {
                    _logger.LogInformation("Exiting standby mode - hiding standby window");
                    HideStandbyWindow();
                }
            }
        }

        private void OnStandbyWindowClosed(object? sender, EventArgs e)
        {
            _logger.LogDebug("Standby window closed event received");
            _currentStandbyWindow = null;
            _isStandbyMode = false;

            // Când fereastra de standby se închide (prin interacțiune utilizator),
            // forțăm modul activ pentru a preveni reapariția imediată
            if (_standbyService.IsInStandbyMode)
            {
                _logger.LogInformation("Standby window closed by user - forcing active mode");
                _standbyService.ForceActiveMode();
            }
        }

        public void Dispose()
        {
            _standbyService.PropertyChanged -= OnStandbyServicePropertyChanged;
            HideStandbyWindow();
        }
    }
}