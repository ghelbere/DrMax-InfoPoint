// Services/LoyaltyCardValidatorService.cs
using InfoPoint.Models;
using InfoPointUI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace InfoPointUI.Services
{
    public class CardValidatorService : ILoyaltyCardValidator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CardValidatorService> _logger;

        public static bool IsValid(string cardCode) => IsValidEAN13(cardCode);

        public CardValidatorService(HttpClient httpClient, ILogger<CardValidatorService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool ValidateEAN13(string cardCode)
        {
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
            } else
            {
                //TODO: Remove this, this is only for local testing without API
                return new CardValidationResult
                {
                    IsValid = true,
                    ClientName = "Client",
                    CardNumber = cardCode,
                    IsActive = false,
                    ErrorMessage = null
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
                            CardNumber = cardCode,
                            IsActive = clientDto?.IsActive ?? false,
                            ErrorMessage = null
                        };
                    }
                    else
                    {
                        return new CardValidationResult
                        {
                            IsValid = false,
                            CardNumber = cardCode,
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