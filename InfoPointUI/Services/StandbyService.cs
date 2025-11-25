using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace InfoPointUI.Services
{
    public class StandbyService : IStandbyService
    {
        private readonly DispatcherTimer _standbyTimer;
        private readonly ILogger<StandbyService> _logger;
        private Window? _registeredWindow;
        private HwndSource? _hwndSource;
        private bool _isUserActive;

        public StandbyService(ILogger<StandbyService> logger)
        {
            _logger = logger;

            _standbyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(20) // Default 2 minute
            };
            _standbyTimer.Tick += OnStandbyTimerTick;

            _isUserActive = true;
        }

        public bool IsInStandbyMode { get; private set; }

        public TimeSpan StandbyTimeout
        {
            get => _standbyTimer.Interval;
            set
            {
                _standbyTimer.Interval = value;
                OnPropertyChanged(nameof(StandbyTimeout));
            }
        }

        public void RegisterActiveWindow(Window window)
        {
            _registeredWindow = window;

            if (window.IsLoaded)
            {
                HookWindowEvents();
            }
            else
            {
                window.Loaded += (s, e) => HookWindowEvents();
            }
        }

        private void HookWindowEvents()
        {
            if (_registeredWindow == null) return;

            var helper = new WindowInteropHelper(_registeredWindow);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource?.AddHook(WndProc);

            ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;

            _registeredWindow.MouseMove += OnUserActivity;
            _registeredWindow.KeyDown += OnUserActivity;
            _registeredWindow.TouchDown += OnUserActivity;
            _registeredWindow.GotFocus += OnUserActivity;

            _logger.LogInformation("Standby service hooked to window events");
        }

        private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message >= 0x100 && msg.message <= 0x209)
            {
                OnUserActivity();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (IsInputMessage(msg))
            {
                OnUserActivity();
            }
            return IntPtr.Zero;
        }

        private bool IsInputMessage(int msg)
        {
            return (msg >= 0x100 && msg <= 0x109) ||
                   (msg >= 0x200 && msg <= 0x20E) ||
                   (msg >= 0x240 && msg <= 0x241) ||
                   msg == 0x00FE || msg == 0x00FF;
        }

        private void OnUserActivity(object? sender = null, EventArgs? e = null)
        {
            OnUserActivity();
        }

        private void OnUserActivity()
        {
            if (!_isUserActive)
            {
                _isUserActive = true;
                ForceActiveMode();
            }

            ResetStandbyTimer();
        }

        public void ResetStandbyTimer()
        {
            _standbyTimer.Stop();
            _standbyTimer.Start();
        }

        public void ForceStandbyMode()
        {
            if (IsInStandbyMode) return;

            IsInStandbyMode = true;
            _isUserActive = false;
            _standbyTimer.Stop();

            _logger.LogInformation("Entering standby mode");
            OnPropertyChanged(nameof(IsInStandbyMode));
        }

        public void ForceActiveMode()
        {
            if (!IsInStandbyMode) return;

            IsInStandbyMode = false;
            _standbyTimer.Start();

            _logger.LogInformation("Exiting standby mode");
            OnPropertyChanged(nameof(IsInStandbyMode));
        }

        private void OnStandbyTimerTick(object? sender, EventArgs e)
        {
            _logger.LogInformation("Standby timeout reached");
            ForceStandbyMode();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _standbyTimer.Stop();
            _standbyTimer.Tick -= OnStandbyTimerTick;
            _hwndSource?.RemoveHook(WndProc);
            ComponentDispatcher.ThreadPreprocessMessage -= OnThreadPreprocessMessage;
        }
    }
}