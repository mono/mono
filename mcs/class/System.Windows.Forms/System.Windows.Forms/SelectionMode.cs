//
// System.Windows.Forms.SelectionMode.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	public enum SelectionMode
	{
		MultiExtended = 3,
		MultiSimple = 2,
		None = 0,
		One = 1
	}
}
