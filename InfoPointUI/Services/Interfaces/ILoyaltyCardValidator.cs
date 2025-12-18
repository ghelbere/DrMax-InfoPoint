using InfoPoint.Models;
namespace InfoPointUI.Services.Interfaces
{
    public interface ILoyaltyCardValidator
    {
        bool ValidateEAN13(string cardCode);
        Task<CardValidationResult> ValidateCardAsync(string cardCode);
    }

}