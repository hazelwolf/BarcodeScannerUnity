/*
 This Scanner Is used to manage the Camera & the parser and provide:
 * Simple methods : Scan / Stop
 * Simple events : OnStatus / StatusChanged
 - Bhomit */

using System;
using UnityEngine;
#if !UNITY_WEBGL
using System.Threading;
#endif
namespace Cognizant.SmartPick.ScanningManager.BarcodeScanner
{
    public class InitiateBarcodeScanner : IBarcodeScanner
    {
        //
        public event EventHandler OnReady;
        public event EventHandler StatusChanged;

        //
        public IWebcam Camera { get; private set; }
        public IBarcodeParser Parser { get; private set; }
        public ScannerSettings Settings { get; private set; }

        //
        private BarcodeScannerStatus status;
        public BarcodeScannerStatus Status
        {
            get { return status; }
            private set
            {
                status = value;
                if (StatusChanged != null)
                {
                    StatusChanged.Invoke(this, EventArgs.Empty);
                }
            }
        }

        // Store information about last image / results (use the update loop to access camera and callback)
        private Color32[] pixels = null;
        private Action<string,string> Callback;
        private ParserResult Result;

        //
        private bool parserPixelAvailable = false;
        private float mainThreadLastDecode = 0;
        private int webcamFrameDelayed = 0;
        private int webcamLastChecksum = -1;


        public InitiateBarcodeScanner() : this(null, null, null) { }
        public InitiateBarcodeScanner(ScannerSettings settings) : this(settings, null, null) { }
        public InitiateBarcodeScanner(IBarcodeParser parser, IWebcam webcam) : this(null, parser, webcam) { }

        public InitiateBarcodeScanner(ScannerSettings settings, IBarcodeParser parser, IWebcam webcam)
        {
            // Check Device Authorization
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                throw new Exception("This Webcam Library can't work without the webcam authorization");
            }

            Status = BarcodeScannerStatus.Initialize;

            // Default Properties
            Settings = (settings == null) ? new ScannerSettings() : settings;
            Parser = (parser == null) ? new ZXingParser(Settings) : parser;
            Camera = (webcam == null) ? new UnityWebcam(Settings) : webcam;
        }

        /// <summary>
        /// Used to start Scanning
        /// </summary>
        /// <param name="callback"></param>
        public void Scan(Action<string, string> callback)
        {
            if (Callback != null)
            {
                Debug.Log(this + " Already Scan");
                return;
            }
            Callback = callback;

            Debug.Log(this + " SimpleScanner -> Start Scan");
            Status = BarcodeScannerStatus.Running;

#if !UNITY_WEBGL
            if (Settings.ScannerBackgroundThread)
            {

                CodeScannerThread = new Thread(ThreadDecodeQR);
                CodeScannerThread.Start();
            }
#endif
        }

        /// <summary>
        /// Used to Stop Scanning
        /// </summary>
        public void Stop()
        {
            Stop(false);
        }

        /// <summary>
        /// Used to Stop Scanning internaly (can be forced)
        /// </summary>
        private void Stop(bool forced)
        {
            if (!forced && Callback == null)
            {
                //Log.Warning(this + " No Scan running");
                return;
            }

            // Stop thread / Clean callback
            Debug.Log(this + " SimpleScanner -> Stop Scan");
            // #if !UNITY_WEBGL
            // if (CodeScannerThread != null)
            // {
            // 	CodeScannerThread.Abort();
            // }
            // #endif

            Callback = null;
            Status = BarcodeScannerStatus.Paused;
        }

        /// <summary>
        /// Used to be sure that everything is properly clean
        /// </summary>
        public void Destroy()
        {
            // clean events
            OnReady = null;
            StatusChanged = null;

            // Stop it
            Stop(true);

            // clean returns
            Callback = null;
            Result = null;
            pixels = null;
            parserPixelAvailable = false;

            // clean camera
            Camera.Destroy();
            Camera = null;
            Parser = null;
        }

        #region Unthread

        /// <summary>
        /// Process Image Decoding in the main Thread
        /// Background Thread : OFF
        /// </summary>
        public void DecodeQR()
        {
            // Wait
            if (Status != BarcodeScannerStatus.Running || !parserPixelAvailable || Camera.Width == 0)
            {
                return;
            }

            // Process
            Debug.Log(this + " SimpleScanner -> Scan ... " + Camera.Width + " / " + Camera.Height);
            try
            {
                Result = Parser.Decode(pixels, Camera.Width, Camera.Height);
                parserPixelAvailable = false;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        #endregion

        #region Background Thread

#if !UNITY_WEBGL
        private Thread CodeScannerThread;

        /// <summary>
        /// Process Image Decoding in a Background Thread
        /// Background Thread : OFF
        /// </summary>
        public void ThreadDecodeQR()
        {
            while (Result == null)
            {
                // Wait
                if (Status != BarcodeScannerStatus.Running || !parserPixelAvailable || Camera.Width == 0)
                {
                    Thread.Sleep(Mathf.FloorToInt(Settings.ScannerDecodeInterval * 1000));
                    continue;
                }

                // Process
                Debug.Log(this + " SimpleScanner -> Scan ... " + Camera.Width + " / " + Camera.Height);
                try
                {
                    Result = Parser.Decode(pixels, Camera.Width, Camera.Height);
                    parserPixelAvailable = false;

                    // Sleep a little bit and set the signal to get the next frame
                    Thread.Sleep(Mathf.FloorToInt(Settings.ScannerDecodeInterval * 1000));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
#endif

        #endregion

        /// <summary>
        /// Be sure that the camera metadata is stable (thanks Unity) and wait until then (increment delayFrameWebcam)
        /// </summary>
        /// <returns></returns>
        private bool WebcamInitialized()
        {
            // If webcam information still change, reset delayFrame
            if (webcamLastChecksum != Camera.GetChecksum())
            {
                webcamLastChecksum = Camera.GetChecksum();
                webcamFrameDelayed = 0;
                return false;
            }

            // Increment delayFrame
            if (webcamFrameDelayed < Settings.ScannerDelayFrameMin)
            {
                webcamFrameDelayed++;
                return false;
            }

            Camera.SetSize();
            webcamFrameDelayed = 0;
            return true;
        }

        /// <summary>
        /// This Update Loop is used to :
        /// * Wait the Camera is really ready
        /// * Bring back Callback to the main thread when using Background Thread
        /// * To execute image Decoding When not using the background Thread
        /// </summary>
        public void Update()
        {
            // If not ready, wait
            if (!Camera.IsReady())
            {
                Debug.Log(this + " Camera Not Ready Yet ...");
                if (status != BarcodeScannerStatus.Initialize)
                {
                    Status = BarcodeScannerStatus.Initialize;
                }
                return;
            }

            // If the app start for the first time (select size & onReady Event)
            if (Status == BarcodeScannerStatus.Initialize)
            {
                if (WebcamInitialized())
                {
                    Debug.Log(this + " Camera is Ready ");

                    Status = BarcodeScannerStatus.Paused;

                    if (OnReady != null)
                    {
                        OnReady.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            if (Status == BarcodeScannerStatus.Running)
            {
                // Call the callback if a result is there
                if (Result != null)
                {
                    //
                    //Log.Info(Result);
                    Callback(Result.Type, Result.Value);

                    // clean and return
                    Result = null;
                    parserPixelAvailable = false;
                    return;
                }

                // Get the image as an array of Color32
                pixels = Camera.GetPixels(pixels);
                parserPixelAvailable = true;

                // If background thread OFF, do the decode main thread with 500ms of pause for UI
                if (!Settings.ScannerBackgroundThread && mainThreadLastDecode < Time.realtimeSinceStartup - Settings.ScannerDecodeInterval)
                {
                    DecodeQR();
                    mainThreadLastDecode = Time.realtimeSinceStartup;
                }
            }
        }    
    }
}
