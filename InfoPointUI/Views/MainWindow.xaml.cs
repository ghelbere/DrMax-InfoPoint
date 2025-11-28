using InfoPointUI.Helpers;
using InfoPointUI.Models;
using InfoPointUI.Sensors;
using InfoPointUI.Services;
using InfoPointUI.ViewModels;
using InfoPointUI.Views.ProductDetails;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace InfoPointUI.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? ViewModel => DataContext as MainViewModel;
        private SwipeGestureHandler? _swipeHandler;

        private ProximitySensor _proximitySensor = null!;

        private readonly IStandbyService? _standbyService;


        public MainWindow(MainViewModel viewModell, IStandbyService standbyService)
        {
            InitializeComponent();

            DataContext = viewModell;

            // Register with standby service
            standbyService.RegisterActiveWindow(this);

            txtCardNumber.DataContext = (App)Application.Current;

            this.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Application.Current.Shutdown();
                }

            };

            InitializeComponentsAndEvents();

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

        protected override void OnClosed(EventArgs e)
        {
            Loaded -= Window_Loaded;
            base.OnClosed(e);
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
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true; // oprește închiderea
                }
            };
        }

        private void OnScanCard(object sender, RoutedEventArgs e)
        {
            var cardWindow = new CardScanWindow();
            cardWindow.ShowDialog();
            FocusSearchBox();
        }

        private void DebugCloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TestStandbyButton_Click(object sender, RoutedEventArgs e)
        {
            var standbyService = App.Current.GetService<IStandbyService>();
            standbyService.ForceStandbyMode();
        }

        internal void FocusSearchBox()
        {
            var searchBox = FindName("SearchTextBox") as TextBox;
            if (searchBox != null && searchBox.IsVisible && searchBox.IsEnabled)
            {
                searchBox.Focus();
                searchBox.SelectAll();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // Delay mic pentru a se asigura că fereastra e complet activă
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FocusSearchBox();
            }), DispatcherPriority.ContextIdle);
        }
    }
}
