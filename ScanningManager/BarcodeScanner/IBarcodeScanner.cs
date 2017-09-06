using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cognizant.SmartPick.ScanningManager.BarcodeScanner
{
    public interface IBarcodeScanner
    {
            event EventHandler StatusChanged;
            event EventHandler OnReady;

            BarcodeScannerStatus Status { get; }

            IBarcodeParser Parser { get; }
            IWebcam Camera { get; }
            ScannerSettings Settings { get; }

            void Scan(Action<string, string> Callback);
            void Stop();
            void Update();
            void Destroy();

        }
    }

