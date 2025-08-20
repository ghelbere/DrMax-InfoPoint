using System.Net.Http;
using System.Net.Http.Json;
using InfoPointUI.Models;

namespace InfoPointUI.Services;

public class ApiService
{
    private readonly HttpClient _httpClient = new();

    public async Task<List<ProductDto>> SearchProductsAsync(string term, string tabletId, string? category)
    {
        string url = $"https://localhost:7051/api/products?term={term}&tabletId={tabletId}";

        if (!string.IsNullOrWhiteSpace(category))
            url += $"&category={Uri.EscapeDataString(category)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Tablet-ID", tabletId);

        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new();
        }
        catch
        {
            return new List<ProductDto>
            {
                new() { Id = -1, Name = "Eroare server", Price = 0.00m, Location = "Server oprit", ImageUrl = "", Category = "Toate" }
            };
        }

        return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
    }

    // 🔄 Noua metodă cu paginare
    public async Task<PagedResult<ProductDto>> SearchProductsPagedAsync(string term, string tabletId, string? category, int page, int pageSize)
    {
        string url = $"https://localhost:7051/api/products/paged?term={term}&tabletId={tabletId}&page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(category))
            url += $"&category={Uri.EscapeDataString(category)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Tablet-ID", tabletId);

        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new PagedResult<ProductDto> { Items = new() };
        }
        catch
        {
            return new PagedResult<ProductDto>
            {
                Items = new List<ProductDto>
            {
                new() { Id = -1, Name = "Eroare server", Price = 0.00m, Location = "Server oprit", ImageUrl = "", Category = "Toate" }
            },
                TotalItems = 1,
                PageSize = pageSize
            };
        }

        return await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>() ?? new PagedResult<ProductDto> { Items = new() };
    }
}
