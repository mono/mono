//
// System.Windows.Forms.DockStyle.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;
using System.Drawing.Design;

namespace System.Windows.Forms
{
	/// <summary>
	/// Specifies the position and manner in which a control is docked.
	/// </summary>
	[Editor ("System.Windows.Forms.Design.DockEditor, " + Consts.AssemblySystem_Design, typeof (UITypeEditor))]
	public enum DockStyle
	{
		None = 0,
		Top = 1,
		Bottom = 2,
		Left = 3,
		Right = 4,
		Fill = 5,
	}
}
