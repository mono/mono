//
// System.Windows.Forms.ErrorIconAlignment.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies constants indicating the locations that an error icon can appear
  /// in relation to the control with an error. 
	/// </summary>
	//[Serializable]
	public enum ErrorIconAlignment {

		//Values were verified with enumcheck.
		TopLeft = 0,
		TopRight = 1,
		MiddleLeft = 2,
		MiddleRight = 3,
		BottomLeft = 4,
		BottomRight = 5,
	}
}
