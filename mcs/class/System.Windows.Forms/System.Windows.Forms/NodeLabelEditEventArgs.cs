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
		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}
		//inherited
		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
		//inherited
		//public Type GetType(){
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Methods
		//
		//inheited
		//protected object MemberwiseClone()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- DeConstructor
		//
		[MonoTODO]
		~NodeLabelEditEventArgs()
		{
			throw new NotImplementedException ();
		}
	 }
}
