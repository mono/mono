//
// System.Windows.Forms.ErrorBlinkStyle.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies constants indicating when the error icon, supplied by an ErrorProvider, 
  /// should blink to alert the user that an error has occurred.
	/// </summary>
	public enum ErrorBlinkStyle {

		//Values were verified with enumcheck.
		BlinkIfDifferentError = 0,
		AlwaysBlink = 1,
		NeverBlink = 2,
	}
}
