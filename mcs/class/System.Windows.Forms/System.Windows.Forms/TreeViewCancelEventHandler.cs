//
// System.Windows.Forms.TreeViewCancelEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (DENNISH@Raytek.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents the method that will handle the BeforeCheck, 
	/// BeforeCollapse, BeforeExpand, or BeforeSelect event of a TreeView.
	/// </summary>
	[Serializable]
	public delegate void TreeViewCancelEventHandler(object sender, TreeViewCancelEventArgs e);
}