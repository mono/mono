//
// System.Windows.Forms.NodeLabelEditEventArgs.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Partially completed by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class NodeLabelEditEventArgs : EventArgs {
			private TreeNode node;
			private string label;
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

		//
		//  --- Public Properties
		//
//		[MonoTODO]
//		public bool CancelEdit {
//			get {
//				throw new NotImplementedException ();
//			}
//			set {
//				throw new NotImplementedException ();
//			}
//		}
		public string Label {
			get {
				return label;
			}
		}
		public TreeNode Node {
			get {
				return node;
			}
		}

		//
		//  --- Public Methods
		//
//		[MonoTODO]
//		public virtual bool Equals(object o)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public static bool Equals(object o1, object o2)
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual int GetHashCode()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public Type GetType()
//		{
//			throw new NotImplementedException ();
//		}
//		[MonoTODO]
//		public virtual string ToString()
//		{
//			throw new NotImplementedException ();
//		}
//
//		//
//		//  --- Protected Methods
//		//
//		[MonoTODO]
//		protected object MemberwiseClone()
//		{
//			throw new NotImplementedException ();
//		}
//
//		//
//		//  --- DeConstructor
//		//
//		[MonoTODO]
//		~NodeLabelEditEventArgs()
//		{
//			throw new NotImplementedException ();
//		}
	 }
}
