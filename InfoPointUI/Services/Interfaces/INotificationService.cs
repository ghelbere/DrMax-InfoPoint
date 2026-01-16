using System.Threading.Tasks;

namespace InfoPointUI.Services.Interfaces
{
    /// <summary>
    /// Service pentru afișarea notificărilor către utilizator
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Afișează o notificare informativă
        /// </summary>
        Task ShowInformationAsync(string message, string title = "Informație");

        /// <summary>
        /// Afișează o notificare de eroare
        /// </summary>
        Task ShowErrorAsync(string message, string title = "Eroare");

        /// <summary>
        /// Afișează o notificare de succes
        /// </summary>
        Task ShowSuccessAsync(string message, string title = "Succes");

        /// <summary>
        /// Afișează o notificare de confirmare (da/nu)
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirmare");

        /// <summary>
        /// Afișează o notificare cu buton de OK (sync pentru situații simple)
        /// </summary>
        void ShowInformation(string message, string title = "Informație");

        /// <summary>
        /// Afișează o eroare cu buton de OK (sync pentru situații simple)
        /// </summary>
        void ShowError(string message, string title = "Eroare");
    }
}