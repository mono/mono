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
	 }
}
