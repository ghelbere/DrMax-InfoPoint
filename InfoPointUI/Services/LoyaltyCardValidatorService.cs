// Services/LoyaltyCardValidatorService.cs
using InfoPoint.Models;
using InfoPointUI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace InfoPointUI.Services
{
    public class LoyaltyCardValidatorService : ILoyaltyCardValidator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoyaltyCardValidatorService> _logger;

        // Păstrează metoda statică existentă pentru compatibilitate
        public static bool IsValid(string cardCode) => IsValidEAN13(cardCode);

        public LoyaltyCardValidatorService(HttpClient httpClient, ILogger<LoyaltyCardValidatorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool ValidateEAN13(string cardCode)
        {
            // Mută aici logica ta existentă din LoyaltyCardValidator.IsValid
            return IsValidEAN13(cardCode);
        }

        public async Task<CardValidationResult> ValidateCardAsync(string cardCode)
        {
            // PAS 1: Validare locală EAN13
            if (!ValidateEAN13(cardCode))
            {
                return new CardValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Format card invalid"
                };
            }

            // PAS 2: Validare prin API
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/client/card-validate",
                    new { CardCode = cardCode });

                if (response.IsSuccessStatusCode)
                {
                    var clientDto = await response.Content.ReadFromJsonAsync<ClientDto>();

                    return new CardValidationResult
                    {
                        IsValid = true,
                        ClientName = clientDto?.FullName ?? "Client",
                        IsActive = true, // Sau din clientDto
                        ErrorMessage = null
                    };
                }
                else
                {
                    return new CardValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Card invalid sau inactiv"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API validation failed for card: {cardCode}");
                return new CardValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Eroare de conexiune"
                };
            }
        }

        private static bool IsValidEAN13(string code)
        {
            // COPIEAZĂ AICI LOGICA TA EXISTENTĂ din LoyaltyCardValidator.IsValid
            // Exemplu:
            if (string.IsNullOrWhiteSpace(code) || code.Length != 13)
                return false;

            // Doar cifre
            foreach (char c in code)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Calcul checksum EAN-13
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = code[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            int lastDigit = code[12] - '0';

            return checkDigit == lastDigit;
        }
    }
}