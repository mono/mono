//
// System.Windows.Forms.CaptionButton.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies the type of caption button to display.
	/// </summary>

	public enum CaptionButton {

		//Values were verified with enumcheck.
		Close = 0,
		Minimize = 1,
		Maximize = 2,
		Restore = 3,
		Help = 4,
	}
}
