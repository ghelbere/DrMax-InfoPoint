using Microsoft.Extensions.Configuration;
using NLog;
using System;

namespace InfoPointUI.Services
{
    public sealed class AppStateService
    {
        private static readonly Lazy<AppStateService> _instance = new Lazy<AppStateService>(() => new AppStateService());
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IConfiguration? _configuration;

        public static AppStateService Instance => _instance.Value;

        public string? LoyaltyCard { get; set; } = string.Empty;
        public string? TabletId { get; private set; }
        public string? StoreCode { get; private set; }
        public string? CurrentLocation { get; private set; }
        public DateTime? LastActivity { get; private set; }
        public bool IsCardScanned => !string.IsNullOrEmpty(LoyaltyCard);

        private AppStateService() { }

        public void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            TabletId = _configuration["TabletSettings:Id"] ?? "UNKNOWN-TABLET";
            StoreCode = _configuration["TabletSettings:StoreCode"] ?? "UNKNOWN-STORE";
            CurrentLocation = _configuration["TabletSettings:Location"] ?? "General";
            LastActivity = DateTime.Now;

            _logger.Info($"AppState initialized - Tablet: {TabletId}");
        }

        public void UpdateActivity() => LastActivity = DateTime.Now;
        public void ClearLoyaltyData() => LoyaltyCard = string.Empty;
    }
}