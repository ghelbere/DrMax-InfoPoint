using InfoPoint.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InfoPointServer.Services
{
    public class QuantPlacementService
    {
        private readonly HttpClient _httpClient;
        private readonly QuantAuthService _authService;
        private readonly string? _username;
        private readonly string? _password;

        public QuantPlacementService(HttpClient httpClient, QuantAuthService authService, string username, string password)
        {
            _httpClient = httpClient;
            _authService = authService;
            _username = username;
            _password = password;

            //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        public async Task<List<ProductPlacementDto>> GetPlacementsAsync(List<string> productCodes, string storeId, CancellationToken cancellationToken)
        {
            // 1. Inițiere export
            var exportRequest = new
            {
                StoreId = storeId,
                PlanogramStates = new[] { "IMPLEMENTED" },
                ProductCodes = productCodes
            };

            var token = await _authService.GetValidTokenAsync(_username??"", _password??"", cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            var exportResponse = await _httpClient.PostAsJsonAsync("/v1/export/placed-products", exportRequest, cancellationToken);
            exportResponse.EnsureSuccessStatusCode();

            var taskId = (await exportResponse.Content.ReadFromJsonAsync<TaskResponse>(cancellationToken: cancellationToken))?.TaskId;
            if (string.IsNullOrEmpty(taskId))
                throw new Exception("No task ID returned.");

            // 2. Retry logic cu polling
            const int maxRetries = 10;
            const int delayMs = 500;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var statusResponse = await _httpClient.GetAsync($"/v1/export/status/{taskId}", cancellationToken);
                if (!statusResponse.IsSuccessStatusCode)
                    throw new Exception("Failed to check task status.");

                var status = await statusResponse.Content.ReadFromJsonAsync<ExportStatusResponse>(cancellationToken: cancellationToken);
                if (status?.Status == "COMPLETED")
                    break;

                await Task.Delay(delayMs, cancellationToken);
            }

            // 3. Interogare rezultat
            var resultResponse = await _httpClient.GetAsync($"/v1/export/placed-products/{taskId}", cancellationToken);
            resultResponse.EnsureSuccessStatusCode();

            var json = await resultResponse.Content.ReadAsStringAsync(cancellationToken);
            var placements = JsonSerializer.Deserialize<List<ProductPlacementDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return placements ?? new List<ProductPlacementDto>();
        }

        private class TaskResponse
        {
            public string TaskId { get; set; } = string.Empty;
        }

        private class ExportStatusResponse
        {
            public string Status { get; set; } = string.Empty;
        }
    }
}

