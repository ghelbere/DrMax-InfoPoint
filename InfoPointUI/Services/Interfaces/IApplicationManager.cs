namespace InfoPointUI.Services.Interfaces
{
    public interface IApplicationManager
    {
        void StartApplication();
        void ShowMainWindow();
        void ShowStandbyWindow();
        void ShutdownApplication();
        void ForceShutdown();
    }
}