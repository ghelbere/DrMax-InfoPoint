using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace InfoPointUI.Helpers
{
    public class SwipeGestureHandler
    {
        private readonly FrameworkElement _target;
        private readonly UIElement _gestureSurface;
        private readonly double _threshold;
        private double _cumulativeTranslation;
        private Point _mouseStart;
        private bool _isDragging;

        public Action OnSwipeLeft { get; set; }
        public Action OnSwipeRight { get; set; }

        public SwipeGestureHandler(UIElement gestureSurface, FrameworkElement targetToAnimate, double swipeThreshold = 100)
        {
            _gestureSurface = gestureSurface;
            _target = targetToAnimate;
            _threshold = swipeThreshold;

            gestureSurface.IsManipulationEnabled = true;
            gestureSurface.ManipulationDelta += OnManipulationDelta;
            gestureSurface.ManipulationCompleted += OnManipulationCompleted;

            gestureSurface.MouseDown += OnMouseDown;
            gestureSurface.MouseMove += OnMouseMove;
            gestureSurface.MouseUp += OnMouseUp;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _cumulativeTranslation += e.DeltaManipulation.Translation.X;
            _target.RenderTransform = new TranslateTransform(_cumulativeTranslation, 0);
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            HandleSwipe(_cumulativeTranslation);
            _cumulativeTranslation = 0;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseStart = e.GetPosition(_gestureSurface);
            _isDragging = true;
            _gestureSurface.CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var current = e.GetPosition(_gestureSurface);
                var deltaX = current.X - _mouseStart.X;
                _target.RenderTransform = new TranslateTransform(deltaX, 0);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                var end = e.GetPosition(_gestureSurface);
                var deltaX = end.X - _mouseStart.X;
                HandleSwipe(deltaX);
                _isDragging = false;
                _gestureSurface.ReleaseMouseCapture();
            }
        }

        private void HandleSwipe(double deltaX)
        {
            if (Math.Abs(deltaX) > _threshold)
            {
                if (deltaX < 0)
                    OnSwipeLeft?.Invoke();
                else
                    OnSwipeRight?.Invoke();
            }

            AnimateReset(deltaX);
        }

        private void AnimateReset(double from)
        {
            var transform = new TranslateTransform();
            _target.RenderTransform = transform;

            var animation = new DoubleAnimation
            {
                From = from,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
    }
}
