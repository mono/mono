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
		None = 0,
		Top = 1,
		Bottom = 2,
		left = 3,		
		Right = 4,
		Fill = 5,
	}
}
