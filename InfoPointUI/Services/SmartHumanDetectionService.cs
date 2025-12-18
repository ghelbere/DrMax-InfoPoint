using OpenCvSharp;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace InfoPointUI.Services
{
    public class SmartHumanDetectionService
    {
        private readonly ILogger<SmartHumanDetectionService>? _logger;
        private VideoCapture _camera;
        private bool _isRunning;
        private Thread _detectionThread;

        private const double MIN_FACE_AREA_RATIO = 0.03;
        private const double MIN_BODY_AREA_RATIO = 0.15;
        private const int REQUIRED_CONSECUTIVE_FRAMES = 15;
        private const int ABSENCE_CONFIRMATION_FRAMES = 40;

        private int _consecutivePresenceFrames = 0;
        private int _consecutiveAbsenceFrames = 0;
        private bool _lastConfirmedState = false;

        public event EventHandler<bool> ConfirmedHumanPresenceChanged;

        public SmartHumanDetectionService(ILogger<SmartHumanDetectionService>? logger)
        {
            _logger = logger;
        }

        public void StartDetection()
        {
            _lastConfirmedState = false;

            if (_isRunning) return;

            try
            {
                _logger.LogInformation("Starting human detection service");

                _camera = new VideoCapture(0);
                _camera.Set(VideoCaptureProperties.FrameWidth, 640);
                _camera.Set(VideoCaptureProperties.FrameHeight, 480);

                if (!_camera.IsOpened())
                {
                    _logger.LogError("Could not open camera");
                    return;
                }

                _isRunning = true;
                _detectionThread = new Thread(DetectionLoop);
                _detectionThread.IsBackground = true;
                _detectionThread.Start();

                _logger.LogInformation("Human detection service started");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start human detection");
            }
        }

        private void DetectionLoop()
        {
            using var faceCascade = LoadCascade("haarcascade_frontalface_default.xml");
            using var upperBodyCascade = LoadCascade("haarcascade_upperbody.xml");

            while (_isRunning)
            {
                try
                {
                    using var frame = new Mat();
                    if (!_camera.Read(frame) || frame.Empty())
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    bool humanDetected = DetectHuman(frame, faceCascade, upperBodyCascade);
                    ProcessDetectionResult(humanDetected);

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in detection loop");
                    Thread.Sleep(1000);
                }
            }
        }

        private bool DetectHuman(Mat frame, CascadeClassifier faceCascade, CascadeClassifier upperBodyCascade)
        {
            using var grayFrame = new Mat();
            Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);
            Cv2.EqualizeHist(grayFrame, grayFrame);

            // Detectare fețe
            if (faceCascade != null)
            {
                var faces = faceCascade.DetectMultiScale(grayFrame, 1.1, 4, HaarDetectionTypes.ScaleImage, new Size(80, 80));
                foreach (var face in faces)
                {
                    double areaRatio = (face.Width * face.Height) / (double)(frame.Width * frame.Height);
                    if (areaRatio >= MIN_FACE_AREA_RATIO)
                        return true;
                }
            }

            // Detectare corp
            if (upperBodyCascade != null)
            {
                var bodies = upperBodyCascade.DetectMultiScale(grayFrame, 1.1, 3, HaarDetectionTypes.ScaleImage, new Size(100, 100));
                foreach (var body in bodies)
                {
                    double areaRatio = (body.Width * body.Height) / (double)(frame.Width * frame.Height);
                    if (areaRatio >= MIN_BODY_AREA_RATIO)
                        return true;
                }
            }

            return false;
        }

        private static CascadeClassifier? LoadCascade(string cascadeName)
        {
            try
            {
                var paths = new[] { cascadeName, $"cascade\\{cascadeName}", $"Assets\\{cascadeName}" };
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var path in paths)
                {
                    var relativePath = System.IO.Path.Combine(appDirectory, path);
                    if (System.IO.File.Exists(relativePath))
                        return new CascadeClassifier(relativePath);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void ProcessDetectionResult(bool detected)
        {
            if (detected)
            {
                _consecutivePresenceFrames++;
                _consecutiveAbsenceFrames = 0;

                if (_consecutivePresenceFrames >= REQUIRED_CONSECUTIVE_FRAMES) // && !_lastConfirmedState)
                {
                    _lastConfirmedState = true;
                    //_logger?.LogInformation("Human detected in front of tablet");
                    ConfirmedHumanPresenceChanged?.Invoke(this, true);
                }
            }
            else
            {
                _consecutiveAbsenceFrames++;
                _consecutivePresenceFrames = 0;

                if (_consecutiveAbsenceFrames >= ABSENCE_CONFIRMATION_FRAMES && _lastConfirmedState)
                {
                    _lastConfirmedState = false;
                    //_logger?.LogInformation("No human in front of tablet");
                    ConfirmedHumanPresenceChanged?.Invoke(this, false);
                }
            }
        }

        public void StopDetection()
        {
            _isRunning = false;
            _lastConfirmedState = false;
            _detectionThread?.Join(1000);
            _camera?.Release();
        }

        public bool IsRunning => _isRunning;
    }
}