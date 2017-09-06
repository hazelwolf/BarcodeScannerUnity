/*################
Defines Barcode methods that can be directly called inside unity script
*StartScannerCamera- Initiates the camera, should be in start of application manager;
*StartScanner - Starts the decoding method and gives barcode type and barcode value;
*UpdateScanner - Updates scanner at every frame this inside your scripts update method;
- Bhomit
###############*/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Cognizant.SmartPick.ScanningManager.BarcodeScanner;

namespace Cognizant.SmartPick.ScanningManager
{
    public class BarcodeScannerManager 
    {
        private string decodeValue;

        public IBarcodeScanner BarcodeScanner;

        private float RestartTime;

        
        // Use this for initialization of camera takes a raw image as inputwhich needs to be set in Unity Editor
        void StartScannerCamera()
        {

            //Initizalize webcam and BarcodeScanner

            BarcodeScanner = new InitiateBarcodeScanner();

            BarcodeScanner.Camera.Play();

            // Display the camera texture through a RawImage
            BarcodeScanner.OnReady += (sender, arg) =>
            {
                //Set Orientation & Texture
                Image.transform.localEulerAngles = BarcodeScanner.Camera.GetEulerAngles();
                Image.transform.localScale = BarcodeScanner.Camera.GetScale();
                Image.texture = BarcodeScanner.Camera.Texture;

                // Keep Image Aspect Ratio
                var rect = Image.GetComponent<RectTransform>();
                var newHeight = rect.sizeDelta.x * BarcodeScanner.Camera.Height / BarcodeScanner.Camera.Width;
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);

                RestartTime = Time.realtimeSinceStartup;

            };

        }

        public void UpdateScanner(string eventHandler, GameObject game, Component compo)
        {
            BarcodeScanner.Update();
            if (BarcodeScanner != null)
            {
                BarcodeScanner.Update();
            }

            // Check if the Scanner need to be started or restarted
            if (RestartTime != 0 && RestartTime < Time.realtimeSinceStartup)
            {
                StartScanner(eventHandler, game, compo);
                RestartTime = 0;
            }
        }


        //Starts scanner and returs barcode value and barcode type which is being decoded
        public void StartScanner(string sendThisMessage, GameObject Application, Component component)
        {
            BarcodeScanner.Scan((barCodeType, barCodeValue) => {
                BarcodeScanner.Stop();
                RestartTime += Time.realtimeSinceStartup + 1f;
                decodeValue = barCodeValue;
                #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
                #endif
                Application.GetComponent<Component>().SendMessage(sendThisMessage, barCodeValue, SendMessageOptions.RequireReceiver);
            });
            Debug.Log(decodeValue);
        }
    }
}
