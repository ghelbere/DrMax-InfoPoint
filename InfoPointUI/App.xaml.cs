using InfoPointUI.Views;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace InfoPointUI
{
    public partial class App : Application, INotifyPropertyChanged
    {
        private string? _loyaltyCardCode = String.Empty;

        public string? LoyaltyCardCode
        {
            get => _loyaltyCardCode;
            set
            {
                if (_loyaltyCardCode != value)
                {
                    _loyaltyCardCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            Debug.WriteLine("OnStartup called");

            var culture = new CultureInfo("ro-RO");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            var mainWindow = new MainWindow();
            mainWindow.Show();

            base.OnStartup(e);

            EventManager.RegisterClassHandler(typeof(Window),
                Window.PreviewKeyDownEvent,
                new KeyEventHandler((s, ev) =>
                {
                    if (ev.Key == Key.Escape && s is MainWindow)
                    {
                        ev.Handled = true; // doar MainWindow ignoră ESC
                    }
                }));
        }
    }
}
