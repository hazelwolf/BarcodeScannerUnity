using System.Collections;
using System.Collections.Generic;

namespace Cognizant.SmartPick.ScanningManager
{
	public abstract class IScanner{

		public abstract string QRCodeScanner ();
		public abstract void ImagerScanner ();
	}
}
