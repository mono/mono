//
// System.Windows.Forms.PropertyTabChangedEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the PropertyTabChanged event of a PropertyGrid.
	/// </summary>
	[Serializable]
	public delegate void PropertyTabChangedEventHandler(object s, PropertyTabChangedEventArgs e);
}
