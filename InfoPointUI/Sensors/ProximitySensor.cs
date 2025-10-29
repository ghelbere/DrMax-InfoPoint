using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoPointUI.Sensors
{
    public class ProximitySensor
    {
        private bool _isUserDetected;

        public bool IsUserDetected
        {
            get => _isUserDetected;
            private set
            {
                if (_isUserDetected != value)
                {
                    _isUserDetected = value;
                    UserDetectionChanged?.Invoke(this, _isUserDetected);
                }
            }
        }

        public event EventHandler<bool>? UserDetectionChanged;


        // DE COMENTAT CAND AVEM SENZORI REALI
        public void SimulateDetection(bool detected)
        {
            IsUserDetected = detected;
        }
    }
}
