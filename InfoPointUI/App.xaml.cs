using InfoPointUI.Services;
using InfoPointUI.Services.Interfaces;
using InfoPointUI.ViewModels;
using InfoPointUI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
                //standbyService?.StartHumanDetection();

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
            // ✅ 1. ADĂUGĂ CONFIGURAȚIA
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // ✅ FIXED: Use AppContext.BaseDirectory for base path
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // ✅ 2. LOGGING CU NLOG
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog();
            });

            // ✅ 3. EXTRAGE SETĂRILE DIN CONFIGURARE
            var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7175";
            var tabletId = configuration["TabletSettings:Id"] ?? "TAB-999";

            // ✅ 4. UN SINGUR HTTPCLIENT PENTRU TOATE SERVICIILE API (ACELAȘI SERVER)
            services.AddHttpClient("InfoPointApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Tablet-ID", tabletId); // Header comun
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (message, cert, chain, errors) => true
            });

            // ✅ 5. REGISTREAZĂ SERVICIILE CARE FOLOSESC ACELAȘI HTTPCLIENT
            // ApiService pentru căutare produse
            services.AddSingleton<IApiService>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("InfoPointApi");
                var logger = provider.GetRequiredService<ILogger<ApiService>>();
                return new ApiService(httpClient, logger);
            });

            // LoyaltyCardValidatorService pentru validare card (ACELAȘI SERVER!)
            services.AddSingleton<ILoyaltyCardValidator>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("InfoPointApi"); // ACELAȘI CLIENT!
                var logger = provider.GetRequiredService<ILogger<LoyaltyCardValidatorService>>();
                return new LoyaltyCardValidatorService(httpClient, logger);
            });

            // ✅ 6. HTTP CLIENT PENTRU IMAGINI (SEPARAT, FĂRĂ BASE ADDRESS)
            services.AddHttpClient("ImageClient", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                // ❌ NU seta BaseAddress aici - fiecare imagine are URL-ul ei
            });

            // ✅ 7. SERVICIILE EXISTENTE
            // Standby Services
            services.AddSingleton<IStandbyService, StandbyService>();
            services.AddSingleton<IStandbyManager, StandbyManager>();
            services.AddSingleton<IApplicationManager, ApplicationManager>();
            services.AddSingleton<SmartHumanDetectionService>();

            // Card Services
            services.AddSingleton<ICardService, CardService>();

            // Notification Service
            services.AddSingleton<INotificationService, NotificationService>();

            // ✅ 8. VIEWMODELS
            services.AddTransient<MainViewModel>(provider =>
            {
                var cardService = provider.GetRequiredService<ICardService>();
                var apiService = provider.GetRequiredService<IApiService>();
                var configuration = provider.GetRequiredService<IConfiguration>();
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var notificationService = provider.GetRequiredService<INotificationService>();

                return new MainViewModel(cardService, apiService, httpClientFactory, configuration, notificationService);
            });

            //services.AddTransient<ProductDetailsViewModel>();

            // ✅ 9. WINDOWS
            services.AddTransient<CardScanWindow>();
            services.AddSingleton<MainWindow>();
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