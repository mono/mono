//
// System.Windows.Forms.SelectionMode.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	[Flags]
	public enum SelectionMode {
		MultiExtended = 3,
		MultiSimple = 2,
		None = 0,
		One = 1
	}
}
