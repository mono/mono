//
// System.Windows.Forms.DragAction
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
  /// Specifies how and if a drag-and-drop operation should continue.
	/// </summary>
	[Serializable]
//	[ComVisible(true)]
	public enum DragAction
	{
		Cancel,
		Continue,
		Drop 
	}
}