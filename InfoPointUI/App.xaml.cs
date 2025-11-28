using InfoPointUI.Services;
using InfoPointUI.ViewModels;
using InfoPointUI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace InfoPointUI
{
    public partial class App : Application, INotifyPropertyChanged
    {
        private ServiceProvider? _serviceProvider;
        private IApplicationManager? _applicationManager;
        private string? _loyaltyCardCode;

        /// <summary>
        /// Proprietate globală pentru a accesa instanța aplicației din orice loc
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Codul cardului de fidelitate scanat - accesibil global
        /// </summary>
        public string? LoyaltyCardCode
        {
            get => _loyaltyCardCode;
            set
            {
                if (_loyaltyCardCode != value)
                {
                    _loyaltyCardCode = value;
                    OnPropertyChanged();
                    OnLoyaltyCardCodeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event care se declanșează când cardul de fidelitate este scanat sau schimbat
        /// </summary>
        public event EventHandler? OnLoyaltyCardCodeChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Setează cultura pentru aplicație
            SetCultureInfo();

            // Configure NLog
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Starting DrMax InfoPoint application");

            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                _serviceProvider = serviceCollection.BuildServiceProvider();

                // Start the application using ApplicationManager
                _applicationManager = _serviceProvider.GetRequiredService<IApplicationManager>();
                _applicationManager.StartApplication();

                var standbyService = _serviceProvider.GetRequiredService<IStandbyService>() as StandbyService;
                standbyService?.StartHumanDetection();

                logger.Info("Application started successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application startup failed");
                throw;
            }
        }

        private void SetCultureInfo()
        {
            // Setează cultura pentru România
            var culture = new CultureInfo("ro-RO");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Logging with NLog
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog();
            });

            // Standby Services
            services.AddSingleton<IStandbyService, StandbyService>();
            services.AddSingleton<IStandbyManager, StandbyManager>();
            services.AddSingleton<IApplicationManager, ApplicationManager>();
            services.AddSingleton<SmartHumanDetectionService>();

            // Add your existing services here based on your current structure
            // services.AddSingleton<IProductsClient, ProductsClient>();
            // services.AddSingleton<IApiService, ApiService>();
            // services.AddSingleton<IDialogService, DialogService>();
            // services.AddSingleton<IScreenConfigurationClient, ScreenConfigurationClient>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
            // Note: StandbyWindow is created on-demand by StandbyManager
        }

        /// <summary>
        /// Metodă helper pentru a obține servicii din containerul de DI
        /// </summary>
        public T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("Service provider is not initialized");

            return _serviceProvider.GetService<T>() ??
                   throw new InvalidOperationException($"Service of type {typeof(T)} is not registered");
        }

        /// <summary>
        /// Setează codul cardului de fidelitate și loghează acțiunea
        /// </summary>
        public void SetLoyaltyCard(string cardCode)
        {
            var logger = LogManager.GetCurrentClassLogger();

            if (string.IsNullOrEmpty(cardCode))
            {
                logger.Info("Loyalty card cleared");
                LoyaltyCardCode = null;
            }
            else
            {
                logger.Info("Loyalty card scanned: {CardCode}", cardCode);
                LoyaltyCardCode = cardCode;
            }
        }

        /// <summary>
        /// Șterge cardul de fidelitate curent
        /// </summary>
        public void ClearLoyaltyCard()
        {
            SetLoyaltyCard(String.Empty);
        }

        /// <summary>
        /// Verifică dacă există un card de fidelitate activ
        /// </summary>
        public bool HasActiveLoyaltyCard => !string.IsNullOrEmpty(LoyaltyCardCode);

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationManager?.ShutdownApplication();
            _serviceProvider?.Dispose();

            // Cleanup NLog
            LogManager.Shutdown();

            base.OnExit(e);
        }

        /// <summary>
        /// Metodă pentru a forța aplicația în modul standby (pentru teste sau cerințe speciale)
        /// </summary>
        public void ForceStandbyMode()
        {
            var standbyService = GetService<IStandbyService>();
            standbyService.ForceStandbyMode();
        }

        /// <summary>
        /// Metodă pentru a forța aplicația în modul activ
        /// </summary>
        public void ForceActiveMode()
        {
            var standbyService = GetService<IStandbyService>();
            standbyService.ForceActiveMode();
        }

        /// <summary>
        /// Metodă pentru a reseta timer-ul de standby
        /// </summary>
        public void ResetStandbyTimer()
        {
            var standbyService = GetService<IStandbyService>();
            standbyService.ResetStandbyTimer();
        }
    }
}