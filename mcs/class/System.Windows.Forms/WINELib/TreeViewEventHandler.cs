//
// System.Windows.Forms.TreeViewEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the method that will handle the 
	/// AfterCheck, AfterCollapse, AfterExpand, or AfterSelect event of a TreeView.
	/// </summary>
	//[Serializable]
	public delegate void TreeViewEventHandler(object sender, TreeViewEventArgs e);
}
