namespace InfoPointUI.Services.Interfaces
{
    public interface ICardService
    {
        string CurrentCardCode { get; set; }
        CardValidationResult CurrentCardValidation { get; }
        event EventHandler CardValidated;
        event EventHandler<string> CardValidationFailed;

        Task<CardValidationResult> ValidateAndStoreCardAsync(string cardCode);
        void ClearCard();
    }
}