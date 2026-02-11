using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace InfoPointUI.Services
{
    /// <summary>
    /// Manager static pentru tastatura touch nativă Windows
    /// </summary>
    public static class TouchKeyboardManager
    {
        #region DLL Imports

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        #endregion

        #region Constants

        private const string KEYBOARD_WINDOW_CLASS = "IPTip_Main_Window";
        private const string TABTIP_EXE_PATH = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
        private const string TABTIP_EXE_PATH_ALTERNATIVE = @"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe";

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint SC_CLOSE = 0xF060;

        private const int SM_CONVERTIBLESLATEMODE = 0x2003;
        private const int SM_TABLETPC = 0x56;

        #endregion

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        #endregion

        #region Proprietăți private

        private static IntPtr _keyboardHandle = IntPtr.Zero;
        private static bool? _hasPhysicalKeyboardCache = null;
        private static DateTime _lastKeyboardCheck = DateTime.MinValue;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(10);

        #endregion

        #region Metode publice

        /// <summary>
        /// Afișează tastatura touch (forțează afișarea chiar dacă setarea este "never")
        /// </summary>
        /// <returns>True dacă tastatura a fost afișată cu succes</returns>
        public static async Task<bool> ShowTouchKeyboardAsync()
        {
            try
            {
                // Verifică dacă avem tastatură fizică
                //if (HasPhysicalKeyboard())
                //{
                //    Debug.WriteLine("⚠️ Tastatura touch nu a fost afișată - există tastatură fizică");
                //    return false;
                //}

                // Găsește sau pornește TabTip
                if (!await EnsureTabTipRunningAsync())
                {
                    Debug.WriteLine("❌ Nu s-a putut porni TabTip.exe");
                    return false;
                }

                // Așteaptă să se încarce
                await Task.Delay(300);

                // Găsește fereastra tastaturii
                _keyboardHandle = FindWindow(KEYBOARD_WINDOW_CLASS, null);
                if (_keyboardHandle == IntPtr.Zero)
                {
                    Debug.WriteLine("❌ Fereastra tastaturii nu a fost găsită");
                    return false;
                }

                // Forțează afișarea
                ShowWindow(_keyboardHandle, SW_SHOWNOACTIVATE);

                // Activează fereastra
                ShowWindow(_keyboardHandle, SW_RESTORE);

                Debug.WriteLine("✅ Tastatura touch afișată cu succes");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Eroare la afișarea tastaturii touch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ascunde tastatura touch
        /// </summary>
        /// <returns>True dacă tastatura a fost ascunsă cu succes</returns>
        public static bool HideTouchKeyboard()
        {
            try
            {
                // Găsește fereastra tastaturii dacă nu o avem în cache
                if (_keyboardHandle == IntPtr.Zero)
                {
                    _keyboardHandle = FindWindow(KEYBOARD_WINDOW_CLASS, null);
                }

                if (_keyboardHandle != IntPtr.Zero)
                {
                    ShowWindow(_keyboardHandle, SW_HIDE);
                    // Închide complet fereastra (nu doar o ascunde)
                    PostMessage(_keyboardHandle, WM_SYSCOMMAND, new IntPtr(SC_CLOSE), IntPtr.Zero);
                    Thread.Sleep(200);

                    // Termină procesul TabTip pentru a fi siguri
                    TerminateTabTipProcess();

                    _keyboardHandle = IntPtr.Zero;
                    Debug.WriteLine("✅ Tastatura touch ascunsă/închisă");
                    return true;
                }

                Debug.WriteLine("ℹ️ Tastatura touch nu era vizibilă");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Eroare la ascunderea tastaturii touch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Comută starea tastaturii touch (afișează/ascunde)
        /// </summary>
        /// <returns>Noua stare (true = vizibilă, false = ascunsă)</returns>
        public static async Task<bool> ToggleTouchKeyboardAsync()
        {
            if (IsTouchKeyboardVisible())
            {
                HideTouchKeyboard();
                return false;
            }
            else
            {
                return await ShowTouchKeyboardAsync();
            }
        }

        /// <summary>
        /// Verifică dacă tastatura touch este vizibilă
        /// </summary>
        public static bool IsTouchKeyboardVisible()
        {
            try
            {
                var handle = FindWindow(KEYBOARD_WINDOW_CLASS, null);
                if (handle == IntPtr.Zero) return false;

                var placement = new WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);

                if (GetWindowPlacement(handle, ref placement))
                {
                    return ( placement.showCmd > 0 );
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifică dacă dispozitivul are tastatură fizică conectată
        /// </summary>
        public static bool HasPhysicalKeyboard()
        {
            // Cache pentru a evita verificări prea frecvente
            if (_hasPhysicalKeyboardCache.HasValue &&
                (DateTime.Now - _lastKeyboardCheck) < _cacheDuration)
            {
                return _hasPhysicalKeyboardCache.Value;
            }

            try
            {
                bool hasKeyboard = CheckPhysicalKeyboardViaWMI() ||
                                   CheckPhysicalKeyboardViaSystemMetrics();

                _hasPhysicalKeyboardCache = hasKeyboard;
                _lastKeyboardCheck = DateTime.Now;

                return hasKeyboard;
            }
            catch
            {
                // În caz de eroare, presupunem că există tastatură pentru a evita afișarea necorespunzătoare
                return true;
            }
        }

        /// <summary>
        /// Verifică dacă dispozitivul este în mod tabletă
        /// </summary>
        public static bool IsTabletMode()
        {
            try
            {
                int tabletMode = GetSystemMetrics(SM_CONVERTIBLESLATEMODE);
                return tabletMode == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifică dacă dispozitivul este o tabletă
        /// </summary>
        public static bool IsTabletDevice()
        {
            try
            {
                return GetSystemMetrics(SM_TABLETPC) != 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determină dacă ar trebui să folosească tastatura touch
        /// </summary>
        public static bool ShouldUseTouchKeyboard()
        {
            return !HasPhysicalKeyboard() && (IsTabletMode() || IsTabletDevice());
        }

        #endregion

        #region Metode private

        /// <summary>
        /// Asigură că TabTip.exe rulează
        /// </summary>
        private static async Task<bool> EnsureTabTipRunningAsync()
        {
            try
            {
                // Verifică dacă procesul TabTip rulează deja
                if (IsTabTipProcessRunning())
                {
                    return true;
                }

                // Încearcă să pornească TabTip.exe
                return await StartTabTipProcessAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Eroare la pornirea TabTip: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifică dacă procesul TabTip rulează
        /// </summary>
        private static bool IsTabTipProcessRunning()
        {
            try
            {
                var processes = Process.GetProcessesByName("TabTip");
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Pornește procesul TabTip.exe
        /// </summary>
        private static async Task<bool> StartTabTipProcessAsync()
        {
            try
            {
                // Încearcă calea principală
                if (System.IO.File.Exists(TABTIP_EXE_PATH))
                {
                    StartProcess(TABTIP_EXE_PATH);
                    return true;
                }

                // Încearcă calea alternativă
                if (System.IO.File.Exists(TABTIP_EXE_PATH_ALTERNATIVE))
                {
                    StartProcess(TABTIP_EXE_PATH_ALTERNATIVE);
                    return true;
                }

                // Încearcă să găsească în System32 sau alte locații
                var tabTipPath = FindTabTipInSystem();
                if (!string.IsNullOrEmpty(tabTipPath))
                {
                    StartProcess(tabTipPath);
                    return true;
                }

                Debug.WriteLine("❌ TabTip.exe nu a fost găsit în locațiile așteptate");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Eroare la pornirea TabTip.exe: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pornește un proces fără a afișa fereastră
        /// </summary>
        private static void StartProcess(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }

        /// <summary>
        /// Caută TabTip.exe în sistem
        /// </summary>
        private static string FindTabTipInSystem()
        {
            var possiblePaths = new[]
            {
                @"C:\Windows\System32\TabTip.exe",
                @"C:\Windows\SysWOW64\TabTip.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\microsoft shared\ink\TabTip.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Common Files\microsoft shared\ink\TabTip.exe"
            };

            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Termină procesul TabTip
        /// </summary>
        private static void TerminateTabTipProcess()
        {
            try
            {
                var processes = Process.GetProcessesByName("TabTip");
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(1000);
                    }
                    catch
                    {
                        // Ignoră erori la terminarea procesului
                    }
                }
            }
            catch
            {
                // Ignoră erori generale
            }
        }

        /// <summary>
        /// Verifică existența tastaturii fizice via WMI
        /// </summary>
        private static bool CheckPhysicalKeyboardViaWMI()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Keyboard");
                foreach (var obj in searcher.Get())
                {
                    string description = obj["Description"]?.ToString() ?? "";
                    string pnpId = obj["PNPDeviceId"]?.ToString() ?? "";

                    // Exclude tastaturile virtuale și HID generice
                    bool isVirtual = description.Contains("virtual", StringComparison.OrdinalIgnoreCase) ||
                                    pnpId.Contains("ROOT\\", StringComparison.OrdinalIgnoreCase) ||
                                    (description.Contains("HID", StringComparison.OrdinalIgnoreCase) &&
                                     pnpId.Contains("HID\\", StringComparison.OrdinalIgnoreCase));

                    if (!isVirtual)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // WMI nu este disponibil
            }

            return false;
        }

        /// <summary>
        /// Verifică existența tastaturii fizice via SystemMetrics
        /// </summary>
        private static bool CheckPhysicalKeyboardViaSystemMetrics()
        {
            try
            {
                const int SM_KEYBOARDPRESENT = 0x0B;
                int hasKeyboard = GetSystemMetrics(SM_KEYBOARDPRESENT);
                return hasKeyboard != 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Metode de extensie pentru controale WPF

        /// <summary>
        /// Atașează tastatura touch automat la un TextBox
        /// </summary>
        public static void AttachToTextBox(System.Windows.Controls.TextBox textBox)
        {
            if (textBox == null) return;

            textBox.GotFocus += async (s, e) =>
            {
                if (ShouldUseTouchKeyboard())
                {
                    await ShowTouchKeyboardAsync();
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                // Ascunde tastatura doar dacă niciun alt TextBox nu e în focus
                if (!IsAnyTextBoxFocused())
                {
                    HideTouchKeyboard();
                }
            };
        }

        /// <summary>
        /// Atașează tastatura touch automat la un PasswordBox
        /// </summary>
        public static void AttachToPasswordBox(System.Windows.Controls.PasswordBox passwordBox)
        {
            if (passwordBox == null) return;

            passwordBox.GotFocus += async (s, e) =>
            {
                if (ShouldUseTouchKeyboard())
                {
                    await ShowTouchKeyboardAsync();
                }
            };

            passwordBox.LostFocus += (s, e) =>
            {
                if (!IsAnyInputElementFocused())
                {
                    HideTouchKeyboard();
                }
            };
        }

        /// <summary>
        /// Verifică dacă există vreun TextBox în focus
        /// </summary>
        private static bool IsAnyTextBoxFocused()
        {
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
            return focusedElement is System.Windows.Controls.TextBox;
        }

        /// <summary>
        /// Verifică dacă există vreun element de input în focus
        /// </summary>
        private static bool IsAnyInputElementFocused()
        {
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
            return focusedElement is System.Windows.Controls.TextBox ||
                   focusedElement is System.Windows.Controls.PasswordBox ||
                   focusedElement is System.Windows.Controls.RichTextBox ||
                   focusedElement is System.Windows.Controls.ComboBox;
        }

        #endregion
    }
}