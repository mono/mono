//
// System.Windows.Forms.TreeViewCancelEventArgs
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class TreeViewCancelEventArgs : CancelEventArgs {
		private TreeNode node;
		private TreeViewaction action;
		//
		//  --- Public Constructors
		//
		public TreeViewCancelEventArgs(TreeNode node, bool cancel, TreeViewAction action) : base(cancel)
		{
			this.node = node;
			this.action = action;
		}
		//
		// --- Public Properties
		//
		public TreeViewAction Action {
			get {
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
