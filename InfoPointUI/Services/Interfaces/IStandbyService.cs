using System.ComponentModel;

namespace InfoPointUI.Services.Interfaces
{
    public interface IStandbyService : INotifyPropertyChanged
    {
        bool IsInStandbyMode { get; }
        TimeSpan StandbyTimeout { get; set; }
        void ResetStandbyTimer();
        void ForceStandbyMode();
        void ForceActiveMode();
        void RegisterActiveWindow(System.Windows.Window window);
    }
}