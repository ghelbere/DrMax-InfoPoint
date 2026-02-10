using InfoPoint.Models;
using System.Threading;

namespace InfoPointServer.Services
{
    public class CardService
    {
        public async Task<CardValidationResult> ValidateCardAsync(string cardCode, CancellationToken cancellationToken)
        {
            return new CardValidationResult
            {
                IsValid = true,
                ClientName = "John Doe",
                CardNumber = cardCode,
                IsActive = true,
                ErrorMessage = null
            };
        }
    }
}
