//
// System.Windows.Forms.TreeViewEventArgs
//
// Author:
//  Stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Completed by Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class TreeViewEventArgs : EventArgs {
		private TreeNode node;
		private TreeViewAction action;
		//
		//  --- Public Constructors
		//
		public TreeViewEventArgs(TreeNode node)
		{
			this.node = node;
		}
		public TreeViewEventArgs(TreeNode node, TreeViewAction action)
		{
			this.node = node;
			this.action = action;
		}
		//
		// --- Public Properties
		//
		public TreeViewAction Action {
			get	{
				return action;
			}
		}
		public TreeNode Node {
			get {
				return node;
			}
		}
	}
}
