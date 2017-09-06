/*******************
Contructor for passing decoded result
  -Bhomit
  *******************/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Cognizant.SmartPick.ScanningManager.BarcodeScanner
{
    public interface IBarcodeParser
    {
		ParserResult Decode(Color32[] colors, int width, int height);
    }
}

