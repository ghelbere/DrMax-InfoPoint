using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoPointUI.Views.ProductDetails
{
    /// <summary>
    /// Interaction logic for ProductDetailsWindow.xaml
    /// </summary>
    public partial class ProductDetailsWindow : Window
    {
        private bool _isNavigating = false;

        public ProductDetailsWindow()
        {
            InitializeComponent();
            // Poți adăuga aici logica pentru butoane:
            // - Navigare înapoi
            // - Navigare home
            // - Chemare consultant (integrare cu sistem Media)
            // - Localizare (eventual tranziție către hartă extinsă)

        }

        public ProductDetailsWindow(ProductDto product)
        {
            InitializeComponent();
            FrameHost.Navigate(new ProductDetailsPage(product));


        }

        public static void Show(ProductDto product)
        {
            var window = new ProductDetailsWindow(product)
            {
                Owner =  Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            window.ShowDialog();
        }

        private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // Prevenim navigarea dublă în timpul animației
            if (_isNavigating) return;
            _isNavigating = true;

            // Pornim animația de fade-out
            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                FillBehavior = FillBehavior.Stop
            };

            fadeOut.Completed += (s, _) =>
            {
                // Navigăm după ce s-a terminat fade-out-ul
                FrameHost.Navigate(e.Uri ?? e.Content);
            };

            FrameHost.BeginAnimation(OpacityProperty, fadeOut);

            // Oprim navigarea normală (o vom face manual mai sus)
            e.Cancel = true;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Pornim fade-in după navigare
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                FillBehavior = FillBehavior.HoldEnd
            };

            FrameHost.BeginAnimation(OpacityProperty, fadeIn);
            _isNavigating = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Asculți toate clickurile din frame
            FrameHost.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnAnyButtonClick));
        }

        private void OnAnyButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button btn && (btn.Tag as string) == "CloseWindow")
            {
                this.Close();
            }
        }

    }

}
