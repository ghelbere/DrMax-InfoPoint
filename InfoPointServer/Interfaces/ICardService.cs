using InfoPoint.Models;

namespace InfoPointServer.Interfaces
{
    public interface ICardService
    {
        Task<CardValidationResult> ValidateCardAsync(string cardCode);
    }
}
