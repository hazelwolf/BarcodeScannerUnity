/*******************
Simple interface that initializes unity webcam texture parameters
  -Bhomit
  *******************/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cognizant.SmartPick.ScanningManager.BarcodeScanner
{
        public interface IWebcam
        {
            // 
            Texture Texture { get; }
            int Width { get; }
            int Height { get; }

            //
            void SetSize();
            bool IsReady();
            bool IsPlaying();
            void Play();
            void Stop();
            void Destroy();

            //
            Color32[] GetPixels(Color32[] data);
            float GetRotation();
            bool IsVerticalyMirrored();
            Vector3 GetEulerAngles();
            Vector3 GetScale();
            int GetChecksum();
        }
    }

