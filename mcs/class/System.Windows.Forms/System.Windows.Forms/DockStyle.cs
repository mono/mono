//
// System.Windows.Forms.DockStyle.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies the position and manner in which a control is docked.
	/// </summary>
	public enum DockStyle {

		//Values were verified with enumcheck.
		None = 0,
		Top = 1,
		Bottom = 2,
		left = 3,		
		Right = 4,
		Fill = 5,
	}
}
