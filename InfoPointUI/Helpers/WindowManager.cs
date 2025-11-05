using System.Linq;
using System.Windows;

namespace InfoPointUI.Helpers
{
    public static class WindowManager
    {
        /// <summary>
        /// Verifică dacă o fereastră de tipul T este deschisă și vizibilă.
        /// </summary>
        public static bool IsOpen<T>() where T : Window
        {
            return Application.Current.Windows.OfType<T>().Any(w => w.IsVisible);
        }

        /// <summary>
        /// Returnează instanța ferestrei de tipul T, dacă există.
        /// </summary>
        public static T? GetWindow<T>() where T : Window
        {
            return Application.Current.Windows.OfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Închide fereastra de tipul T, dacă este deschisă.
        /// </summary>
        public static void CloseIfOpen<T>() where T : Window
        {
            var win = GetWindow<T>();
            win?.Close();
        }

        /// <summary>
        /// Aduce în față fereastra de tipul T, dacă este deschisă.
        /// </summary>
        public static void BringToFront<T>() where T : Window
        {
            var win = GetWindow<T>();
            if (win != null)
            {
                if (win.WindowState == WindowState.Minimized)
                    win.WindowState = WindowState.Normal;

                win.Activate();
                win.Topmost = true;   // forțează în față
                win.Topmost = false;  // resetează pentru comportament normal
                win.Focus();
            }
        }
    }
}
