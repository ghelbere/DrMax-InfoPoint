using InfoPoint.Models;
using InfoPointUI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace InfoPointUI.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        // HttpClient ESTE INJECTAT
        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("ApiService initialized");
        }

        public async Task<PagedProductResult<ProductDto>> SearchProductsPagedAsync(
            string query,
            string tabletId,
            string? category,
            int page,
            int pageSize)

        {
            if (String.IsNullOrEmpty(query))
                return new PagedProductResult<ProductDto>
                {
                    Items = new List<ProductDto>(),
                    PageSize = 0,
                    TotalItems = 0
                };

            try
            {
                var url = $"{_httpClient.BaseAddress}api/products/paged?term={query}&tabletId={tabletId}&page={page}&pageSize={pageSize}";

                if (!string.IsNullOrEmpty(category))
                    url += $"&category={HttpUtility.UrlEncode(category)}";

                _logger.LogDebug($"API Call: {url}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var products = await response.Content.ReadFromJsonAsync<PagedProductResult<ProductDto>>()
                    ?? new PagedProductResult<ProductDto>();

                // TODO: Serverul trebuie să returneze paginarea reală
                return products;
            }
            catch (Exception ex)
            {
                // log error
                _logger.LogError(ex, $"SearchProductsPagedAsync failed for query: '{query}'");

                // notify user
                var _notificationService = App.Current.GetService<INotificationService>();
                _notificationService?.ShowError("Eroare la conectarea cu serverul. Verificați conexiunea la internet sau contactați suportul tehnic.", "Eroare server");

                return new PagedProductResult<ProductDto>
                {
                    Items = new()
                    {
                        new() { Id = -1, Name = "Eroare server", Price = 0.00m, Location = "Server oprit", ImageUrl = "", Category = "Toate" }
                    },
                    TotalItems = 1,
                    PageSize = pageSize
                };
            }
        }
    }
}