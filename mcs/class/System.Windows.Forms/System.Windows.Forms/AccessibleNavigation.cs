//
// System.Windows.Forms.AccessibleNavigation.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies a values for navigating among accessible objects.
	/// </summary>

	public enum AccessibleNavigation {

		//Values were verified with enumcheck.
		Down = 2,
		FirstChild = 7,
		LastChild = 8,
		Left = 3,
		Next = 5,
		Previous = 6,
		Right = 4,
		Up = 1
	}
}
