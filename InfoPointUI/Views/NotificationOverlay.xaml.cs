using InfoPointUI.Helpers;
using InfoPointUI.Services;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace InfoPointUI.Views
{
    public partial class NotificationOverlay : Window
    {
        private DispatcherTimer _autoCloseTimer;

        public NotificationOverlay(string message, string title, NotificationType type)
        {
            InitializeComponent();
            InitializeNotification(message, title, type);
            InitializeAnimations();
            InitializeAutoClose();

            Loaded += (s, e) => WindowManager.BringToFront<NotificationOverlay>();
            this.Activate();
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape || e.Key == Key.Enter)
                {
                    CloseNotification();
                }
            };
        }

        private void InitializeNotification(string message, string title, NotificationType type)
        {
            MessageTextBlock.Text = message;
            TitleTextBlock.Text = title;

            // Setăm culoarea și iconița
            Color bandColor;
            switch (type)
            {
                case NotificationType.Info:
                    bandColor = Color.FromArgb(255, 33, 150, 243); // Albastru
                    IconTextBlock.Text = "ℹ";
                    break;

                case NotificationType.Error:
                    bandColor = Color.FromArgb(255, 244, 67, 54); // Roșu
                    IconTextBlock.Text = "⚠";
                    break;

                case NotificationType.Success:
                    bandColor = Color.FromArgb(255, 76, 175, 80); // Verde
                    IconTextBlock.Text = "✓";
                    break;

                default:
                    bandColor = Colors.Gray;
                    IconTextBlock.Text = "ℹ";
                    break;
            }

            // Aplicăm gradient pentru bandă
            var gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(1, 1);

            var lightColor = Color.FromArgb(255,
                (byte)Math.Min(bandColor.R * 1.1, 255),
                (byte)Math.Min(bandColor.G * 1.1, 255),
                (byte)Math.Min(bandColor.B * 1.1, 255));

            var darkColor = Color.FromArgb(255,
                (byte)(bandColor.R * 0.9),
                (byte)(bandColor.G * 0.9),
                (byte)(bandColor.B * 0.9));

            gradient.GradientStops.Add(new GradientStop(lightColor, 0));
            gradient.GradientStops.Add(new GradientStop(bandColor, 0.5));
            gradient.GradientStops.Add(new GradientStop(darkColor, 1));

            NotificationBand.Background = gradient;
        }

        private void InitializeAnimations()
        {
            // Facem fereastra invizibilă la început
            this.Opacity = 0;

            // Animație de fade-in
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            this.BeginAnimation(OpacityProperty, fadeInAnimation);

            // Animație de scalare pentru bandă
            var scaleTransform = new ScaleTransform(0.8, 0.8);
            NotificationBand.RenderTransform = scaleTransform;
            NotificationBand.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new ElasticEase
                {
                    Oscillations = 1,
                    Springiness = 3
                }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private void InitializeAutoClose()
        {
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
            _autoCloseTimer.Start();
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            CloseNotification();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseNotification();
        }

        private void CloseNotification()
        {
            if (_autoCloseTimer != null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer.Tick -= AutoCloseTimer_Tick;
            }

            // Animație de fade-out
            var fadeOutAnimation = new DoubleAnimation
            {
                From = this.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            fadeOutAnimation.Completed += (s, e) => this.Close();
            this.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_autoCloseTimer != null)
            {
                _autoCloseTimer.Stop();
                _autoCloseTimer = null;
            }
            base.OnClosed(e);
        }

        private void Window_TouchDown(object sender, TouchEventArgs e)
        {
            CloseNotification();
        }
    }
}