//
// System.Windows.Forms.AnchorStyles.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms
{
	[Flags]
	[Editor ("System.Windows.Forms.Design.AnchorEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
	public enum AnchorStyles
	{
		Bottom = 2,
		Left = 4,
		None = 0,
		Right = 8,
		Top = 1
	}
}
