//
// System.Windows.Forms.SplitterEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the SplitterMoving and SplitterMoved events of a Splitter.
	/// </summary>
	[Serializable]
	public delegate void SplitterEventHandler(object sender, SplitterEventArgs e);
}
