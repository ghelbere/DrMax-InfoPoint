using System.Windows;
using System.Windows.Input;
using InfoPointUI.Models;
using InfoPointUI.ViewModels;
using InfoPointUI.Helpers;

namespace InfoPointUI.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? ViewModel => DataContext as MainViewModel;
        private SwipeGestureHandler? _swipeHandler;

        public MainWindow()
        {
            InitializeComponent();

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
    }
}
