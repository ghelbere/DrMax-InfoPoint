using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace InfoPointUI.Services
{
    public class StandbyService : IStandbyService
    {
        private readonly DispatcherTimer _standbyTimer;
        private readonly ILogger<StandbyService> _logger;
        private Window? _registeredWindow;
        private HwndSource? _hwndSource;
        private bool _isUserActive;
        private bool _isTransitioning;
        private readonly SmartHumanDetectionService _humanDetectionService;
        private bool _humanDetectionEnabled = true;
        private const int STANDBY_SECONDS = 120; // 120 secunde

        public StandbyService(ILogger<StandbyService> logger, SmartHumanDetectionService humanDetectionService)
        {
            _logger = logger;

            _standbyTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(STANDBY_SECONDS)
            };
            _standbyTimer.Tick += OnStandbyTimerTick;

            _isUserActive = true;
            _isTransitioning = false;

            _humanDetectionService = humanDetectionService;
            _humanDetectionService.ConfirmedHumanPresenceChanged += OnHumanPresenceChanged;
            StartHumanDetection();

            _logger.LogInformation("StandbyService created with timeout: {Timeout}", _standbyTimer.Interval);
        }

        private void OnHumanPresenceChanged(object? sender, bool humanPresent)
        {
            if (!_humanDetectionEnabled) return;

            if (humanPresent && IsInStandbyMode)
            {
                _logger.LogInformation("Auto-exiting standby - human detected");
                ForceActiveMode(); 
            }
        }

        public bool IsInStandbyMode { get; private set; }

        public TimeSpan StandbyTimeout
        {
            get => _standbyTimer.Interval;
            set
            {
                _standbyTimer.Interval = value;
                _logger.LogInformation("Standby timeout changed to: {Timeout}", value);
                OnPropertyChanged(nameof(StandbyTimeout));
            }
        }

        public void StartTransition()
        {
            _logger.LogInformation("Starting transition - disabling input detection temporarily");
            _isTransitioning = true;

            DispatcherTimer transitionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            transitionTimer.Tick += (s, e) =>
            {
                _isTransitioning = false;
                transitionTimer.Stop();
                _logger.LogInformation("Transition completed - input detection enabled");
            };
            transitionTimer.Start();
        }

        public void RegisterActiveWindow(Window window)
        {
            _registeredWindow = window;
            _logger.LogInformation("StandbyService registered window: {Window}", window.GetType().Name);

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

            _logger.LogInformation("Hooking window events for standby detection");

            var helper = new WindowInteropHelper(_registeredWindow);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource?.AddHook(WndProc);

            ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;

            //_registeredWindow.MouseMove += OnUserActivity;
            _registeredWindow.KeyDown += OnUserActivity;
            _registeredWindow.TouchDown += OnUserActivity;
            _registeredWindow.GotFocus += OnUserActivity;

            _logger.LogInformation("Standby service successfully hooked to window events");
        }

        private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message >= 0x100 && msg.message < 0x200 && msg.message != 0x113)
            {
                _logger.LogDebug("Input detected via ThreadPreprocessMessage: {Message}", msg.message);
                OnUserActivity();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (IsInputMessage(msg))
            {
                _logger.LogDebug("Input detected via WndProc: {Message}", msg);
                OnUserActivity();
            }
            return IntPtr.Zero;
        }

        private bool IsInputMessage(int msg)
        {
            return (msg >= 0x100 && msg <= 0x109) ||
                   //(msg >= 0x200 && msg <= 0x20E) ||
                   (msg >= 0x240 && msg <= 0x241) ||
                   msg == 0x00FE || msg == 0x00FF;
        }

        private void OnUserActivity(object? sender, EventArgs? e)
        {
            _logger.LogDebug("User activity detected via event: {Sender}", sender?.GetType().Name);
            OnUserActivity();
        }

        private void OnUserActivity()
        {
            if (_isTransitioning)
            {
                _logger.LogDebug("Input detected during transition - ignoring");
                return;
            }

            _logger.LogDebug("User activity processed. Previous state: {IsUserActive}", _isUserActive);

            if (!_isUserActive)
            {
                _isUserActive = true;
                _logger.LogInformation("User became active after being inactive");
                ForceActiveMode();
            }

            ResetStandbyTimer();
        }

        public void ResetStandbyTimer()
        {
            _logger.LogDebug("Resetting standby timer");
            _standbyTimer.Stop();
            _standbyTimer.Start();
        }

        public void ForceStandbyMode()
        {
            if (IsInStandbyMode)
            {
                _logger.LogDebug("Already in standby mode");
                return;
            }

            _logger.LogInformation("=== FORCING STANDBY MODE ===");

            StartTransition();

            IsInStandbyMode = true;
            _isUserActive = false;
            _standbyTimer.Stop();

            _logger.LogInformation("Now in standby mode. Timer stopped.");
            OnPropertyChanged(nameof(IsInStandbyMode));
        }

        public void ForceActiveMode()
        {
            if (!IsInStandbyMode)
            {
                _logger.LogDebug("Already in active mode");
                return;
            }

            _logger.LogInformation("=== FORCING ACTIVE MODE ===");

            StartTransition();

            IsInStandbyMode = false;
            _standbyTimer.Start();

            _logger.LogInformation("Now in active mode. Timer started with interval: {Interval}", _standbyTimer.Interval);
            OnPropertyChanged(nameof(IsInStandbyMode));
        }

        private void OnStandbyTimerTick(object? sender, EventArgs e)
        {
            _logger.LogInformation("=== STANDBY TIMER TICK - Entering standby ===");
            ForceStandbyMode();
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            _logger.LogDebug("Property changed: {PropertyName}", propertyName);
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing StandbyService");
            _standbyTimer.Stop();
            _standbyTimer.Tick -= OnStandbyTimerTick;
            _hwndSource?.RemoveHook(WndProc);
            _humanDetectionService.StopDetection();
            ComponentDispatcher.ThreadPreprocessMessage -= OnThreadPreprocessMessage;
        }

        internal void StartHumanDetection()
        {
            _humanDetectionService.StartDetection();
        }
    }
}