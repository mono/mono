//
// System.Windows.Forms.TreeViewCancelEventArgs
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class TreeViewCancelEventArgs : CancelEventArgs {

		private TreeNode node;
		private TreeViewAction action;
		//
		//  --- Public Constructors
		//
		public TreeViewCancelEventArgs(TreeNode node, bool cancel, TreeViewAction action) : base(cancel)
		{
			this.node = node;
			this.action = action;
		}
		
		#region Public Properties
		/// <summary>
		///	Action Property
		/// </summary>
		///
		/// <remarks>
		///	Gets the type of TreeViewAction that raised the event.
		/// </remarks>
		public TreeViewAction Action 
		{
			get {
				return action;
			}
		}

		/// <summary>
		///	Node Property
		/// </summary>
		///
		/// <remarks>
		///	Gets the tree node to be checked, expanded, collapsed, or selected.
		/// </remarks>
		public TreeNode Node 
		{
			get {
				return node;
			}
		}
		#endregion

	}
}
