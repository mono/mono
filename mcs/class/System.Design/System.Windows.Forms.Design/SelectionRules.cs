//
// System.Windows.Forms.Design.SelectionRules.cs
//
// Author:
//   Gert Driesen (drieseng@users.sourceforge.net)
// (C) 2004 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms.Design
{
	[Flags]
	public enum SelectionRules
	{
		AllSizeable = 15,
		BottomSizeable = 2,
		LeftSizeable = 4,
		Locked = -2147483648,
		Moveable = 268435456,
		None = 0,
		RightSizeable = 8,
		TopSizeable = 1,
		Visible = 1073741824
	}
}
