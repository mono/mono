//
// System.Windows.Forms.MouseEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the MouseDown, MouseUp, or MouseMove 
	/// event of a form, control, or other component.
	/// </summary>
	//[Serializable]
	public delegate void MouseEventHandler(object sender, MouseEventArgs e);
}
