//
// System.Windows.Forms.MeasureItemEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the MeasureItem event of the 
	/// ListBox, ComboBox, CheckedListBox, or MenuItem controls.
	/// </summary>
	[Serializable]
	public delegate void MeasureItemEventHandler(object sender, MeasureItemEventArgs e);
}
