using InfoPointUI.Services.Interfaces;

namespace InfoPointUI.Services
{
    public class CardService : ICardService
    {
        private readonly ILoyaltyCardValidator _validator;
        private string _currentCardCode;
        private CardValidationResult _currentValidation;

        public string CurrentCardCode
        {
            get => _currentCardCode;
            set
            {
                _currentCardCode = value;
                // TODO: notifica UI
            }
        }

        public CardValidationResult CurrentCardValidation => _currentValidation;

        public event EventHandler CardValidated;
        public event EventHandler<string> CardValidationFailed;

        public CardService(ILoyaltyCardValidator validator)
        {
            _validator = validator;
        }

        public async Task<CardValidationResult> ValidateAndStoreCardAsync(string cardCode)
        {
            var result = await _validator.ValidateCardAsync(cardCode);

            if (result.IsValid)
            {
                CurrentCardCode = cardCode;
                _currentValidation = result;
                CardValidated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                CurrentCardCode = string.Empty;
                _currentValidation = null;
                CardValidationFailed?.Invoke(this, result.ErrorMessage);
            }

            return result;
        }

        public void ClearCard()
        {
            CurrentCardCode = string.Empty;
            _currentValidation = null;
        }
    }
}