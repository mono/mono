//
// System.Windows.Forms.PaintEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the Paint event of a Control class.
	/// </summary>
	[Serializable]
	public delegate void PaintEventHandler(object sender, PaintEventArgs e);
}
