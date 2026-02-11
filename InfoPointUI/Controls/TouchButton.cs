using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace InfoPointUI.Controls
{
    /// <summary>
    /// Button control optimizat pentru touch pe tablete
    /// FĂRĂ conversie touch->mouse
    /// </summary>
    public class TouchButton : ContentControl
    {
        #region Constants

        private const double MOVE_TOLERANCE = 20.0; // pixeli
        private const int MAX_TAP_DURATION = 400; // milisecunde

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TouchButton),
                new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(TouchButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(TouchButton),
                new PropertyMetadata(new CornerRadius(5)));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(TouchButton),
                new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(TouchButton),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(TouchButton),
                new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(TouchButton),
                new PropertyMetadata(new Thickness(1)));

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register("PressedBackground", typeof(Brush), typeof(TouchButton),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 200, 200, 200))));

        public static readonly DependencyProperty PressedForegroundProperty =
            DependencyProperty.Register("PressedForeground", typeof(Brush), typeof(TouchButton),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof(bool), typeof(TouchButton),
                new PropertyMetadata(false, OnIsPressedChanged));

        public static readonly DependencyProperty MoveToleranceProperty =
            DependencyProperty.Register("MoveTolerance", typeof(double), typeof(TouchButton),
                new PropertyMetadata(MOVE_TOLERANCE));

        #endregion

        #region Properties

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public Brush PressedForeground
        {
            get => (Brush)GetValue(PressedForegroundProperty);
            set => SetValue(PressedForegroundProperty, value);
        }

        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            private set => SetValue(IsPressedProperty, value);
        }

        public double MoveTolerance
        {
            get => (double)GetValue(MoveToleranceProperty);
            set => SetValue(MoveToleranceProperty, value);
        }

        #endregion

        #region Private Fields

        private Border _border;
        private ContentPresenter _contentPresenter;
        private TouchDevice _activeTouchDevice;
        private StylusDevice _activeStylusDevice;
        private Storyboard _pressAnimation;
        private Storyboard _releaseAnimation;

        // Toleranță tremur
        private Point _touchStartPoint;
        private DateTime _touchStartTime;
        private bool _isMoveCanceled;
        private bool _isTouchActive;

        #endregion

        #region Constructor

        static TouchButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TouchButton),
                new FrameworkPropertyMetadata(typeof(TouchButton)));
        }

        public TouchButton()
        {
            // Configurare de bază
            this.IsManipulationEnabled = true;
            this.IsHitTestVisible = true;
            this.Focusable = true;

            // IMPORTANT: Dezactivează conversia touch->mouse
            Stylus.SetIsPressAndHoldEnabled(this, false);
            Stylus.SetIsFlicksEnabled(this, false);
            Stylus.SetIsTapFeedbackEnabled(this, false);
            Stylus.SetIsTouchFeedbackEnabled(this, false);

            // PREVIEW EVENTS - DOAR touch și stylus, fără mouse!
            this.PreviewTouchDown += OnPreviewTouchDown;
            this.PreviewTouchUp += OnPreviewTouchUp;
            this.PreviewTouchMove += OnPreviewTouchMove;
            this.PreviewStylusDown += OnPreviewStylusDown;
            this.PreviewStylusUp += OnPreviewStylusUp;
            this.PreviewStylusMove += OnPreviewStylusMove;

            // MOUSE EVENTS - complet ignorate pentru touch
            this.PreviewMouseLeftButtonDown += OnPreviewMouseDownIgnore;
            this.PreviewMouseLeftButtonUp += OnPreviewMouseUpIgnore;
            this.PreviewMouseMove += OnPreviewMouseMoveIgnore;

            // BUBBLING EVENTS - doar leave events
            this.AddHandler(TouchLeaveEvent, new EventHandler<TouchEventArgs>(OnTouchLeave), true);
            this.AddHandler(StylusLeaveEvent, new StylusEventHandler(OnStylusLeave), true);
            this.AddHandler(MouseLeaveEvent, new MouseEventHandler(OnMouseLeaveIgnore), true);

            // BLOCHEAZĂ ORICE ALTCEVA
            this.AddHandler(TouchDownEvent, new EventHandler<TouchEventArgs>((s, e) => e.Handled = true), true);
            this.AddHandler(TouchUpEvent, new EventHandler<TouchEventArgs>((s, e) => e.Handled = true), true);
            this.AddHandler(TouchMoveEvent, new EventHandler<TouchEventArgs>((s, e) => e.Handled = true), true);
            this.AddHandler(StylusDownEvent, new StylusDownEventHandler((s, e) => e.Handled = true), true);
            this.AddHandler(StylusUpEvent, new StylusEventHandler((s, e) => e.Handled = true), true);
            this.AddHandler(StylusMoveEvent, new StylusEventHandler((s, e) => e.Handled = true), true);
            this.AddHandler(MouseDownEvent, new MouseButtonEventHandler((s, e) => e.Handled = true), true);
            this.AddHandler(MouseUpEvent, new MouseButtonEventHandler((s, e) => e.Handled = true), true);

            // Evenimente de viață
            this.Loaded += (s, e) => CreateAnimations();
            this.Unloaded += (s, e) => ResetState();
            this.IsVisibleChanged += (s, e) => { if (!this.IsVisible) ResetState(); };
        }

        #endregion

        #region Mouse Ignore Handlers

        private void OnPreviewMouseDownIgnore(object sender, MouseButtonEventArgs e)
        {
            // Ignoră complet evenimentele de mouse care vin de la touch
            if (e.StylusDevice != null)
            {
                e.Handled = true;
                return;
            }

            // Doar mouse real - tratăm normal
            if (!_isTouchActive)
            {
                e.Handled = true;
                _touchStartPoint = e.GetPosition(this);
                _touchStartTime = DateTime.Now;
                _isMoveCanceled = false;

                SetPressedState(true);
                ApplyPressEffect();
            }
        }

        private void OnPreviewMouseUpIgnore(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null)
            {
                e.Handled = true;
                return;
            }

            if (!_isTouchActive && IsPressed)
            {
                e.Handled = true;

                var duration = DateTime.Now - _touchStartTime;
                if (!_isMoveCanceled && duration.TotalMilliseconds < MAX_TAP_DURATION)
                {
                    ExecuteCommand();
                }

                SetPressedState(false);
                ApplyNormalEffect();
            }
        }

        private void OnPreviewMouseMoveIgnore(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice != null)
            {
                e.Handled = true;
                return;
            }

            if (!_isTouchActive && IsPressed && !_isMoveCanceled)
            {
                var currentPoint = e.GetPosition(this);
                var distance = Math.Sqrt(
                    Math.Pow(currentPoint.X - _touchStartPoint.X, 2) +
                    Math.Pow(currentPoint.Y - _touchStartPoint.Y, 2));

                if (distance > MoveTolerance)
                {
                    _isMoveCanceled = true;
                    SetPressedState(false);
                    ApplyNormalEffect();
                }
            }
        }

        private void OnMouseLeaveIgnore(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice != null)
            {
                e.Handled = true;
                return;
            }

            if (!_isTouchActive && IsPressed)
            {
                e.Handled = true;
                SetPressedState(false);
                ApplyNormalEffect();
            }
        }

        #endregion

        #region Touch Handlers

        private void OnPreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
            _isTouchActive = true;

            // Eliberează orice captură existentă
            if (_activeTouchDevice != null)
            {
                this.ReleaseTouchCapture(_activeTouchDevice);
                _activeTouchDevice = null;
            }

            _activeTouchDevice = e.TouchDevice;

            // FORȚEAZĂ captura pe TOATE nivelele
            _activeTouchDevice.Capture(this);
            this.CaptureTouch(_activeTouchDevice);
            Mouse.Capture(this, CaptureMode.SubTree);

            // Salvează poziția
            var touchPoint = e.GetTouchPoint(this);
            _touchStartPoint = touchPoint.Position;
            _touchStartTime = DateTime.Now;
            _isMoveCanceled = false;

            SetPressedState(true);
            ApplyPressEffect();
        }

        private void OnPreviewTouchMove(object sender, TouchEventArgs e)
        {
            e.Handled = true;

            if (_activeTouchDevice != null && _activeTouchDevice == e.TouchDevice && IsPressed && !_isMoveCanceled)
            {
                var touchPoint = e.GetTouchPoint(this);
                var currentPoint = touchPoint.Position;

                var distance = Math.Sqrt(
                    Math.Pow(currentPoint.X - _touchStartPoint.X, 2) +
                    Math.Pow(currentPoint.Y - _touchStartPoint.Y, 2));

                if (distance > MoveTolerance)
                {
                    _isMoveCanceled = true;
                    SetPressedState(false);
                    ApplyNormalEffect();
                }
            }
        }

        private void OnPreviewTouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = true;

            if (_activeTouchDevice != null && _activeTouchDevice == e.TouchDevice)
            {
                var duration = DateTime.Now - _touchStartTime;

                if (IsPressed && !_isMoveCanceled && duration.TotalMilliseconds < MAX_TAP_DURATION)
                {
                    ExecuteCommand();
                }

                // Eliberează captura
                this.ReleaseTouchCapture(_activeTouchDevice);
                _activeTouchDevice?.Capture(null);
                _activeTouchDevice = null;
                Mouse.Capture(null);

                SetPressedState(false);
                ApplyNormalEffect();
            }

            _isTouchActive = false;
        }

        private void OnTouchLeave(object sender, TouchEventArgs e)
        {
            e.Handled = true;

            if (_activeTouchDevice != null && _activeTouchDevice == e.TouchDevice)
            {
                this.ReleaseTouchCapture(_activeTouchDevice);
                _activeTouchDevice?.Capture(null);
                _activeTouchDevice = null;
                Mouse.Capture(null);

                SetPressedState(false);
                ApplyNormalEffect();
            }

            _isTouchActive = false;
        }

        #endregion

        #region Stylus Handlers

        private void OnPreviewStylusDown(object sender, StylusDownEventArgs e)
        {
            e.Handled = true;
            _isTouchActive = true;

            if (_activeStylusDevice != null)
            {
                this.ReleaseStylusCapture();
                _activeStylusDevice = null;
            }

            _activeStylusDevice = e.StylusDevice;
            this.CaptureStylus();
            Mouse.Capture(this, CaptureMode.SubTree);

            var stylusPoints = e.GetStylusPoints(this);
            if (stylusPoints.Count > 0)
            {
                _touchStartPoint = stylusPoints[0].ToPoint();
            }
            _touchStartTime = DateTime.Now;
            _isMoveCanceled = false;

            SetPressedState(true);
            ApplyPressEffect();
        }

        private void OnPreviewStylusMove(object sender, StylusEventArgs e)
        {
            e.Handled = true;

            if (_activeStylusDevice != null && _activeStylusDevice == e.StylusDevice && IsPressed && !_isMoveCanceled)
            {
                var stylusPoints = e.GetStylusPoints(this);
                if (stylusPoints.Count > 0)
                {
                    var currentPoint = stylusPoints[0].ToPoint();
                    var distance = Math.Sqrt(
                        Math.Pow(currentPoint.X - _touchStartPoint.X, 2) +
                        Math.Pow(currentPoint.Y - _touchStartPoint.Y, 2));

                    if (distance > MoveTolerance)
                    {
                        _isMoveCanceled = true;
                        SetPressedState(false);
                        ApplyNormalEffect();
                    }
                }
            }
        }

        private void OnPreviewStylusUp(object sender, StylusEventArgs e)
        {
            e.Handled = true;

            if (_activeStylusDevice != null && _activeStylusDevice == e.StylusDevice)
            {
                var duration = DateTime.Now - _touchStartTime;

                if (IsPressed && !_isMoveCanceled && duration.TotalMilliseconds < MAX_TAP_DURATION)
                {
                    ExecuteCommand();
                }

                this.ReleaseStylusCapture();
                _activeStylusDevice = null;
                Mouse.Capture(null);

                SetPressedState(false);
                ApplyNormalEffect();
            }

            _isTouchActive = false;
        }

        private void OnStylusLeave(object sender, StylusEventArgs e)
        {
            e.Handled = true;

            if (_activeStylusDevice != null && _activeStylusDevice == e.StylusDevice)
            {
                this.ReleaseStylusCapture();
                _activeStylusDevice = null;
                Mouse.Capture(null);

                SetPressedState(false);
                ApplyNormalEffect();
            }

            _isTouchActive = false;
        }

        #endregion

        #region Private Methods

        private void ResetState()
        {
            if (_activeTouchDevice != null)
            {
                this.ReleaseTouchCapture(_activeTouchDevice);
                _activeTouchDevice?.Capture(null);
                _activeTouchDevice = null;
            }

            if (_activeStylusDevice != null)
            {
                this.ReleaseStylusCapture();
                _activeStylusDevice = null;
            }

            Mouse.Capture(null);

            SetPressedState(false);
            ApplyNormalEffect();
            _isMoveCanceled = false;
            _isTouchActive = false;
        }

        private void SetPressedState(bool pressed)
        {
            if (IsPressed != pressed)
            {
                IsPressed = pressed;
            }
        }

        private void ExecuteCommand()
        {
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
            OnClick();
        }

        protected virtual void OnClick()
        {
            var args = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(args);
        }

        private void ApplyPressEffect()
        {
            if (_border != null)
            {
                _border.Background = PressedBackground;
                if (_contentPresenter != null)
                {
                    _contentPresenter.SetValue(TextElement.ForegroundProperty, PressedForeground);
                }
            }
            _pressAnimation?.Begin(this);
        }

        private void ApplyNormalEffect()
        {
            if (_border != null)
            {
                _border.Background = Background;
                if (_contentPresenter != null)
                {
                    _contentPresenter.SetValue(TextElement.ForegroundProperty, Foreground);
                }
            }
            _releaseAnimation?.Begin(this);
        }

        private void CreateAnimations()
        {
            this.RenderTransform = new ScaleTransform(1, 1);
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            _pressAnimation = new Storyboard();

            var pressScaleX = new DoubleAnimation
            {
                To = 0.95,
                Duration = TimeSpan.FromMilliseconds(80),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(pressScaleX, this);
            Storyboard.SetTargetProperty(pressScaleX, new PropertyPath("RenderTransform.ScaleX"));
            _pressAnimation.Children.Add(pressScaleX);

            var pressScaleY = new DoubleAnimation
            {
                To = 0.95,
                Duration = TimeSpan.FromMilliseconds(80),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(pressScaleY, this);
            Storyboard.SetTargetProperty(pressScaleY, new PropertyPath("RenderTransform.ScaleY"));
            _pressAnimation.Children.Add(pressScaleY);

            _releaseAnimation = new Storyboard();

            var releaseScaleX = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(80),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(releaseScaleX, this);
            Storyboard.SetTargetProperty(releaseScaleX, new PropertyPath("RenderTransform.ScaleX"));
            _releaseAnimation.Children.Add(releaseScaleX);

            var releaseScaleY = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(80),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(releaseScaleY, this);
            Storyboard.SetTargetProperty(releaseScaleY, new PropertyPath("RenderTransform.ScaleY"));
            _releaseAnimation.Children.Add(releaseScaleY);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TouchButton button)
            {
                if (e.OldValue is ICommand oldCommand)
                    oldCommand.CanExecuteChanged -= button.CanExecuteChanged;
                if (e.NewValue is ICommand newCommand)
                    newCommand.CanExecuteChanged += button.CanExecuteChanged;
            }
        }

        private static void OnIsPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TouchButton)?.OnIsPressedChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIsPressedChanged(bool oldValue, bool newValue) { }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command != null)
            {
                this.IsEnabled = Command.CanExecute(CommandParameter);
            }
        }

        #endregion

        #region Events

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TouchButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        #endregion
    }
}