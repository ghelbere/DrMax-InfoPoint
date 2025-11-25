namespace InfoPointUI.Services
{
    public interface IApplicationManager
    {
        void StartApplication();
        void ShowMainWindow();
        void ShowStandbyWindow();
        void ShutdownApplication();
    }
}