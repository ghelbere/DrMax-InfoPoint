using InfoPointUI.Services;
using InfoPointUI.Services.Interfaces;
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

            // HTTP Client pentru API-ul de validare card
            services.AddHttpClient<ILoyaltyCardValidator, LoyaltyCardValidatorService>(client =>
            {
                // Configurează base address-ul din appsettings.json
                client.BaseAddress = new Uri("http://localhost:5000");
                client.Timeout = TimeSpan.FromSeconds(5);
            });

            // Card Services
            services.AddSingleton<ICardService, CardService>();

            // Windows
            services.AddTransient<CardScanWindow>();

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

        protected override void OnExit(ExitEventArgs e)
        {
            _applicationManager?.ShutdownApplication();
            _serviceProvider?.Dispose();

            // Cleanup NLog
            LogManager.Shutdown();

            base.OnExit(e);
        }

    }
}