using InfoPointUI.Helpers;
using InfoPointUI.Models;
using InfoPointUI.Sensors;
using InfoPointUI.ViewModels;
using InfoPointUI.Views.ProductDetails;
using InfoPointUI.Views.Standby;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace InfoPointUI.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? ViewModel => DataContext as MainViewModel;
        private SwipeGestureHandler? _swipeHandler;

        private const int INACTIVITY_THRESHOLD_SECONDS = 6;

        private DispatcherTimer _inactivityTimer = null!;
        private readonly TimeSpan _inactivityThreshold = TimeSpan.FromSeconds(INACTIVITY_THRESHOLD_SECONDS);
        private DateTime _lastActivityTime;

        private bool _isInStandby;
        private ProximitySensor _proximitySensor = null!;
        private StandbyWindow _standbyWindow = null!;


        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Application.Current.Shutdown();
                }

            };

            InitializeComponentsAndEvents();
            InitializeStandbySystem();

            _inactivityTimer.Start();
            _lastActivityTime = DateTime.Now;

            

            bool isPortrait = SystemParameters.PrimaryScreenHeight > SystemParameters.PrimaryScreenWidth;
            if (isPortrait)
            {
                MainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Auto);
                MainGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            }

            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedCategory = "";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focus();
            Keyboard.Focus(SearchTextBox);

            _swipeHandler = new SwipeGestureHandler(MainGrid, ProductItemsControl)
            {
                OnSwipeLeft = () => ViewModel?.NextPageCommand.Execute(null),
                OnSwipeRight = () => ViewModel?.PreviousPageCommand.Execute(null)
            };
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Dacă folosești pe tabletă, poți activa tastatura virtuală:
            // Process.Start("C:\\Program Files\\Common Files\\Microsoft Shared\\ink\\TabTip.exe");
        }


        /// <summary>
        /// Inițializează obiectele principale și evenimentele de activitate utilizator.
        /// </summary>
        private void InitializeComponentsAndEvents()
        {
            _proximitySensor = new ProximitySensor();
            _standbyWindow = new StandbyWindow();

            _inactivityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _inactivityTimer.Tick += InactivityTimer_Tick;

            // monitorizare activitate utilizator (mouse, tastatură, touch)
            this.MouseMove += OnUserActivity;
            this.MouseDown += OnUserActivity;
            this.KeyDown += OnUserActivity;
            this.TouchDown += OnUserActivity;
        }



        /// <summary>
        /// Configurează conexiunile logice dintre MainWindow, ProximitySensor și StandbyWindow.
        /// </summary>
        private void InitializeStandbySystem()
        {
            _standbyWindow.StandbyClicked += (_, _) =>
            {
                if (_isInStandby)
                    ExitStandbyMode();
            };

            // DE COMENTAT CAND AVEM SENZORI REALI (verificat cu timere in  clasa ProximitySensor)
            _standbyWindow.SensorButtonClicked += (_, isDetected) =>
            {
                // DE COMENTAT CAND AVEM SENZORI REALI
                _proximitySensor.SimulateDetection(isDetected); // acum apeleaza _proximitySensor.UserDetectionChanged
            };

            // !!! VA INLOCUI BUTONUL DE SIMULARE din StandbyWindow  (verificat cu timere in  clasa ProximitySensor)
            _proximitySensor.UserDetectionChanged += (_, isDetected) =>
            {
                if (isDetected && _isInStandby)
                    ExitStandbyMode();

                // DE COMENTAT CAND AVEM SENZORI REALI
                _proximitySensor.SimulateDetection(false);
            };
        }

        /// <summary>
        /// Verifică activitatea utilizatorului și actualizează timpul rămas până la intrarea în standby.
        /// </summary>
        private void InactivityTimer_Tick(object? sender, EventArgs e)
        {
            // Dacă fereastra e deja în standby, nu actualizăm nimic
            if (_isInStandby)
                return;

            // Calculăm timpul scurs de la ultima activitate
            var elapsed = DateTime.Now - _lastActivityTime;
            int secondsLeft = Math.Max(0, (int)(_inactivityThreshold.TotalSeconds - elapsed.TotalSeconds));

            // Actualizăm eticheta din interfață (dacă există)
            //lblCountdown.Content = $"Secunde până la standby: {secondsLeft}";

            // Intrăm în standby doar dacă timpul de inactivitate a depășit pragul
            // și senzorul de proximitate nu a detectat utilizatorul
            if (elapsed >= _inactivityThreshold && !_proximitySensor.IsUserDetected)
            {
                EnterStandbyMode();
            }
        }


        private void OnUserActivity(object? sender, EventArgs e)
        {
            _lastActivityTime = DateTime.Now;
            if (_isInStandby)
                ExitStandbyMode();
        }

        private void EnterStandbyMode()
        {
            _inactivityTimer.Stop(); // inutil functional, util pentru performanta generala. Restarted in ExitStandbyMode

            _isInStandby = true;

            foreach (Window win in Application.Current.Windows)
            {
                if (win is ProductDetailsWindow)
                {
                    win.Close();
                }
            }

            _standbyWindow.Show();
            this.Hide();
        }

        private void ExitStandbyMode()
        {
            if (_inactivityTimer.IsEnabled)
            {
                _inactivityTimer.Stop();
            }
            _inactivityTimer.Start();

            _isInStandby = false;
            _lastActivityTime = DateTime.Now;

            _standbyWindow.Hide();
            this.Show();
            this.Activate();
        }

    }
}
