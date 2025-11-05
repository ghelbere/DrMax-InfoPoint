using InfoPointUI.Views;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace InfoPointUI
{
    public partial class App : Application
    {
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
