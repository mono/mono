//
// System.Windows.Forms.DockStyle
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms
{

	/// <summary>
  /// Specifies the position and manner in which a control is docked.
	/// </summary>
	[Serializable]
	public enum DockStyle
	{
		Bottom = 1,
		Fill = 2,
		Left = 3,
		None = 0,
		Right = 4,
		Top = 5
	}
}