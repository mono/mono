//
// System.Windows.Forms.PropertyValueChangedEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// The event handler class that is invoked when 
	/// a property in the grid is modified by the user. 
	/// </summary>
	[Serializable]
	public delegate void PropertyValueChangedEventHandler(object s, PropertyValueChangedEventArgs e);
}
