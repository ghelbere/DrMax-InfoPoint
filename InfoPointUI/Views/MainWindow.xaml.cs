using InfoPointUI.Controls;
using InfoPointUI.Helpers;
using InfoPointUI.Sensors;
using InfoPointUI.Services;
using InfoPointUI.Services.Interfaces;
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

            //this.KeyDown += (_, e) =>
            //{
            //    if (e.Key == Key.Escape)
            //    {
            //        Application.Current.Shutdown();
            //    }

            //};

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

            _standbyService = standbyService;

#if !DEBUG
            btnStandbyButton.Visibility = Visibility.Collapsed;  
#endif

            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Verifică CTRL+SHIFT+ALT+Q
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    {
                        if (e.Key == Key.Q)
                        {
                            e.Handled = true; // Previne propagarea

                            // Confirmare și închidere
                            if (MessageBox.Show("Închideți aplicația?\n(Combinație secretă activată)",
                                    "Confirmare", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                TouchKeyboardManager.HideTouchKeyboard();
                                Application.Current.Shutdown();
                            }
                        }
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            TouchKeyboardManager.HideTouchKeyboard();
            Loaded -= Window_Loaded;
            base.OnClosed(e);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusSearchBox();
            Keyboard.Focus(SearchTextBox);

            ViewModel.PageSize = (int)(ProductItemsControl.ActualWidth / 250) * (int)(ProductItemsControl.ActualHeight / 205);


            _swipeHandler = new SwipeGestureHandler(grdItems, ProductItemsControl)
            {
                OnSwipeLeft = () => ViewModel?.NextPageCommand.Execute(null),
                OnSwipeRight = () => ViewModel?.PreviousPageCommand.Execute(null)
            };

            await TouchKeyboardManager.ShowTouchKeyboardAsync();

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

        private async void OnScanCard(object? sender, RoutedEventArgs? e)
        {
            try
            {
                TouchKeyboardManager.HideTouchKeyboard();
                var cardWindow = App.Current.GetService<CardScanWindow>();
                cardWindow.Owner = this;
                cardWindow.ShowDialog();
                FocusSearchBox();
            } finally
            {
                await TouchKeyboardManager.ShowTouchKeyboardAsync();
            }
        }

        private void TestStandbyButton_Click(object sender, RoutedEventArgs e)
        {
            var standbyService = App.Current.GetService<IStandbyService>();
            standbyService.ForceStandbyMode();
        }

        internal void FocusSearchBox()
        {
            var searchBox = FindName("SearchTextBox") as TextBox;
            if (searchBox == null || searchBox.IsFocused)
                return;

            if (searchBox != null && searchBox.IsVisible && searchBox.IsEnabled)
            {
                // oare e ok?
                //searchBox.Clear();
                //ViewModel?.Products.Clear();

                WindowManager.CloseIfOpen<CardScanWindow>();
                WindowManager.BringToFront<ProductDetailsWindow>(); // aduce fereastra în față dacă e deschisă

                // de aici e sigur bine :)
                searchBox.Focus();
                Keyboard.Focus(searchBox);
                SearchTextBox.CaretIndex = SearchTextBox.Text.Length; // daca fac Clear(), Length va fi tot timpul zero
                //searchBox.SelectAll(); // enervant pe tableta
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

        private void OnScanCardTouched(object sender, TouchEventArgs e)
        {
            e.Handled = true; // Previne propagarea evenimentului touch
            OnScanCard(sender, null);
        }

        private async void SearchTextBox_MouseDown(object sender, MouseButtonEventArgs? e)
        {
            TouchKeyboardManager.HideTouchKeyboard();
            await TouchKeyboardManager.ShowTouchKeyboardAsync();
        }

        private void SearchTextBox_TouchDown(object sender, TouchEventArgs e)
        {
            SearchTextBox_MouseDown(sender, null);
        }

        private void NextPageTouchDown(object sender, TouchEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.NextPageCommand.CanExecute(null))
            {
                vm.NextPageCommand.Execute(null);
            }
        }

        private void PreviousPageTouchDown(object sender, TouchEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.PreviousPageCommand.CanExecute(null))
            {
                vm.PreviousPageCommand.Execute(null);
            }
        }

        private void MainGrid_TouchDown(object sender, TouchEventArgs e)
        {
            TouchKeyboardManager.HideTouchKeyboard();
            e.Handled = true; // Previne propagarea evenimentului touch
        }
    }
}
