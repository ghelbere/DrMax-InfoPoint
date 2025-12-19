using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoPointUI.Services.Interfaces
{
    public interface ISmartHumanDetectionService : IDisposable
    {
        void StartDetection();
        void StopDetection();
        bool IsPersonDetected { get; }

        event EventHandler PersonDetected;
        event EventHandler NoPersonDetected;
    }
}
