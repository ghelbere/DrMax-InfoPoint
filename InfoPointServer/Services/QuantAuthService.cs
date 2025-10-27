using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace InfoPointServer.Services
{
    public class QuantAuthService
    {
        private readonly HttpClient _httpClient;
        private string _cachedToken = String.Empty;
        private DateTime _tokenExpiry;

        public QuantAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetValidTokenAsync(string username, string password, CancellationToken cancellationToken)
        {
            // Dacă tokenul este încă valid, îl returnăm din cache
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var payload = new
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/authenticate", payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            _cachedToken = result?.Token ?? throw new Exception("Token missing from response.");
            _tokenExpiry = DateTime.UtcNow.AddMinutes(29); // conservator, sub 30 min

            return _cachedToken;
        }

        private class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
