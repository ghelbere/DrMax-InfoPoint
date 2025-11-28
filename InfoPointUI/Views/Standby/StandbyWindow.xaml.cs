using InfoPointUI.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using NLog;

namespace InfoPointUI.Views
{
    public partial class StandbyWindow : Window
    {
        private readonly IStandbyService _standbyService;
        private readonly ILogger<StandbyWindow> _logger;
        private bool _ignoreFirstInteraction;

        public StandbyWindow(IStandbyService standbyService)
        {
            _standbyService = standbyService;
            _logger = LogManager.GetCurrentClassLogger();
            _ignoreFirstInteraction = true;

            InitializeComponent();

            Loaded += OnStandbyWindowLoaded;
            MouseDown += OnUserInteraction;
            TouchDown += OnUserInteraction;
            KeyDown += OnUserInteraction;

            // Ignoră prima interacțiune (mouse move de la tranziție)
            DispatcherTimer ignoreTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            ignoreTimer.Tick += (s, e) =>
            {
                _ignoreFirstInteraction = false;
                ignoreTimer.Stop();
                _logger.LogInformation("StandbyWindow now accepting user interactions");
            };
            ignoreTimer.Start();
        }

        private void OnStandbyWindowLoaded(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("StandbyWindow loaded and visible");
            StartLogoAnimation();
            StartTextAnimation();
        }

        private void StartLogoAnimation()
        {
            var logoAnimation = (Storyboard)Resources["LogoBounceAnimation"];
            if (logoAnimation != null && LogoBorder != null)
            {
                Storyboard.SetTarget(logoAnimation, LogoBorder);
                logoAnimation.Begin();
                _logger.LogDebug("Logo bounce animation started");
            }
        }

        private void StartTextAnimation()
        {
            var textAnimation = (Storyboard)Resources["TextPulseAnimation"];
            if (textAnimation != null && InstructionText != null)
            {
                Storyboard.SetTarget(textAnimation, InstructionText);
                textAnimation.Begin();
                _logger.LogDebug("Text pulse animation started");
            }
        }

        private void OnUserInteraction(object sender, RoutedEventArgs e)
        {
            if (_ignoreFirstInteraction)
            {
                _logger.LogDebug("Ignoring first interaction (likely mouse move from transition)");
                return;
            }

            _logger.LogInformation("User interaction detected - exiting standby mode");
            _standbyService.ForceActiveMode();
            this.Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            var logoAnimation = (Storyboard)Resources["LogoBounceAnimation"];
            var textAnimation = (Storyboard)Resources["TextPulseAnimation"];

            logoAnimation?.Stop(LogoBorder);
            textAnimation?.Stop(InstructionText);

            Loaded -= OnStandbyWindowLoaded;
            MouseDown -= OnUserInteraction;
            TouchDown -= OnUserInteraction;
            KeyDown -= OnUserInteraction;

            _logger.LogInformation("StandbyWindow closed");
            base.OnClosed(e);
        }
    }
}