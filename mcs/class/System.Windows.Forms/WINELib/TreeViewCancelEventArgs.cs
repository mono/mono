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

		#region Public Methods

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this TreeViewCancelEventArgs and another object.
		/// </remarks>
		public override bool Equals(object obj) 
		{
			if (!(obj is TreeViewCancelEventArgs)) 
				return false;
			
			return (this == (TreeViewCancelEventArgs) obj);
		}

		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two TreeViewCancelEventArgs objects. The return value is
		///	based on the equivalence of the node and action property
		///	of the two TreeViewCancelEventArgs.
		/// </remarks>
		public static bool operator == (TreeViewCancelEventArgs TreeViewCancelEventArgsA, TreeViewCancelEventArgs TreeViewCancelEventArgsB) 
		{
			return ((TreeViewCancelEventArgsA.action == TreeViewCancelEventArgsB.action) && (TreeViewCancelEventArgsA.Node == TreeViewCancelEventArgsB.Node)) ;
		}


		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two TreeViewCancelEventArgs objects. The return value is
		///	based on the equivalence of the node and action property
		///	of the two TreeViewCancelEventArgs.
		/// </remarks>
		public static bool operator != (TreeViewCancelEventArgs TreeViewCancelEventArgsA, TreeViewCancelEventArgs TreeViewCancelEventArgsB) 
		{
			return ((TreeViewCancelEventArgsA.action != TreeViewCancelEventArgsB.action) || (TreeViewCancelEventArgsA.Node != TreeViewCancelEventArgsB.Node)) ;
		}


		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		[MonoTODO]
		public override int GetHashCode () 
		{
			//FIXME: add class specific stuff;
			return base.GetHashCode();
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the TreeViewCancelEventArgs as a string.
		/// </remarks>
		[MonoTODO]
		public override string ToString () 
		{
			//FIXME: add class specific stuff;
			return base.ToString();
		}

		#endregion // Public Methods

	}
}
