using InfoPointUI.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace InfoPointUI.Views
{
    public partial class StandbyWindow : Window
    {
        private readonly IStandbyService _standbyService;

        public StandbyWindow(IStandbyService standbyService)
        {
            _standbyService = standbyService;
            InitializeComponent();

            Loaded += OnStandbyWindowLoaded;
            MouseDown += OnUserInteraction;
            TouchDown += OnUserInteraction;
            KeyDown += OnUserInteraction;
        }

        private void OnStandbyWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Start animations
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
            }
        }

        private void StartTextAnimation()
        {
            var textAnimation = (Storyboard)Resources["TextPulseAnimation"];
            if (textAnimation != null && InstructionText != null)
            {
                Storyboard.SetTarget(textAnimation, InstructionText);
                textAnimation.Begin();
            }
        }

        private void OnUserInteraction(object sender, RoutedEventArgs e)
        {
            _standbyService.ForceActiveMode();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Stop animations
            var logoAnimation = (Storyboard)Resources["LogoBounceAnimation"];
            var textAnimation = (Storyboard)Resources["TextPulseAnimation"];

            logoAnimation?.Stop(LogoBorder);
            textAnimation?.Stop(InstructionText);

            Loaded -= OnStandbyWindowLoaded;
            MouseDown -= OnUserInteraction;
            TouchDown -= OnUserInteraction;
            KeyDown -= OnUserInteraction;

            base.OnClosed(e);
        }
    }
}