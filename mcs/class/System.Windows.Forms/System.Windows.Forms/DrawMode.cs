//
// System.Windows.Forms.DrawMode.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies how the elements of a control are drawn.
	/// </summary>

	public enum DrawMode {

		//Values were verified with enumcheck.
		Normal = 0,
		OwnerDrawFixed = 1,
		OwnerDrawVariable = 2,
	}
}
