//
// System.Windows.Forms.NodeLabelEditEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Partially completed by Dennis Hayes (dennish@raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class NodeLabelEditEventArgs : EventArgs {

			#region Fields

			private TreeNode node;
			private string label = "";
			private bool canceledit = false;
			
			#endregion

			//
			//  --- Constructor
			//
			public NodeLabelEditEventArgs(TreeNode node)
			{
				this.node = node;
			}
			public NodeLabelEditEventArgs(TreeNode node, string label)
			{
				this.node = node;
				this.label = label;
			}

			#region Public Properties

			public bool CancelEdit {
				get {
					return canceledit;
				}
				set {
					canceledit = value;
				}
			}
			public string Label 
			{
				get {
					return label;
				}
			}
			public TreeNode Node {
				get {
					return node;
				}
			}

			#endregion

			#region Public Methods

			/// <summary>
			///	Equality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two NodeLabelEditEventArgs objects.
			///	The return value is based on the equivalence of
			///	label, Node and CancelEdit Property
			///	of the two NodeLabelEditEventArgs.
			/// </remarks>
			public static bool operator == (NodeLabelEditEventArgs NodeLabelEditEventArgsA, NodeLabelEditEventArgs NodeLabelEditEventArgsB) 
			{
				return (NodeLabelEditEventArgsA.Label == NodeLabelEditEventArgsB.Label) && (NodeLabelEditEventArgsA.Node == NodeLabelEditEventArgsB.Node) && (NodeLabelEditEventArgsA.CancelEdit == NodeLabelEditEventArgsB.CancelEdit);
			}
		
			/// <summary>
			///	Inequality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two NodeLabelEditEventArgs objects.
			///	The return value is based on the equivalence of
			///	label, Node and CancelEdit Property
			///	of the two NodeLabelEditEventArgs.
			/// </remarks>
			public static bool operator != (NodeLabelEditEventArgs NodeLabelEditEventArgsA, NodeLabelEditEventArgs NodeLabelEditEventArgsB) 
			{
				return (NodeLabelEditEventArgsA.Label != NodeLabelEditEventArgsB.Label) || (NodeLabelEditEventArgsA.Node != NodeLabelEditEventArgsB.Node) || (NodeLabelEditEventArgsA.CancelEdit != NodeLabelEditEventArgsB.CancelEdit);
			}

			/// <summary>
			///	Equals Method
			/// </summary>
			///
			/// <remarks>
			///	Checks equivalence of this
			///	PropertyTabChangedEventArgs and another
			///	object.
			/// </remarks>
			public override bool Equals (object obj) 
			{
				if (!(obj is NodeLabelEditEventArgs))return false;
				return (this == (NodeLabelEditEventArgs) obj);
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
			///	Formats the object as a string.
			/// </remarks>
			[MonoTODO]
			public override string ToString () 
			{
				//FIXME: add class specific stuff;
				return base.ToString();
			}

		#endregion

	 }
}
