using InfoPointUI.Views;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

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
        }
    }
}
