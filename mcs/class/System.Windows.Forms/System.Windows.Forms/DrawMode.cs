//
// System.Windows.Forms.DrawMode
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
  /// Specifies how the elements of a control are drawn.
	/// </summary>
	[Serializable]
	public enum DrawMode
	{
		Normal = 0,
		OwnerDrawFixed = 1,
		OwnerDrawVariable = 2,
	}
}
