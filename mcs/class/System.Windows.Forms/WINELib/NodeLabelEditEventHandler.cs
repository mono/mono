//
// System.Windows.Forms.NodeLabelEditEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the BeforeLabelEdit 
	/// and AfterLabelEdit events of a TreeView control. 
	/// </summary>
	//[Serializable]
	public delegate void NodeLabelEditEventHandler(object sender, NodeLabelEditEventArgs e);
}
