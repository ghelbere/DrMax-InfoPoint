using InfoPointUI.Views;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;

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
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_currentStandbyWindow != null)
                    {
                        _currentStandbyWindow.Show();
                        _currentStandbyWindow.WindowState = WindowState.Maximized;
                        _currentStandbyWindow.Activate();
                        _logger.LogDebug("Standby window shown (existing instance)");
                        return;
                    }

                    _currentStandbyWindow = new StandbyWindow(_standbyService);
                    _currentStandbyWindow.Show();

                    _isStandbyMode = true;
                    _logger.LogInformation("Standby window created and shown");
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
                if (_currentStandbyWindow != null && _currentStandbyWindow.IsVisible)
                {
                    _currentStandbyWindow.Hide();
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

        public void Dispose()
        {
            _standbyService.PropertyChanged -= OnStandbyServicePropertyChanged;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _currentStandbyWindow?.Close();
                _currentStandbyWindow = null;
            });
        }
    }
}