namespace InfoPointUI.Services.Interfaces
{
    public interface ILoyaltyCardValidator
    {
        bool ValidateEAN13(string cardCode);
        Task<CardValidationResult> ValidateCardAsync(string cardCode);
    }

    public class CardValidationResult
    {
        public bool IsValid { get; set; }
        public string? ClientName { get; set; }
        public bool IsActive { get; set; }
        public string? ErrorMessage { get; set; }
    }
}