//
// NodeGrouper.cs: A general way to sub-group nodes.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

namespace Mono.TypeReflector.Finders
{
	public delegate void NodeGrouper (NodeInfoCollection c, NodeInfo root, object extra);

	public class NodeGroup {
		private string      groupName;
		private NodeGrouper grouper;
		private object      extra;

		public NodeGroup (string name, object extra, NodeGrouper grouper)
		{
			this.groupName = name;
			this.grouper = grouper;
			this.extra = extra;
		}

		public NodeGroup (string name, NodeGrouper grouper)
			: this (name, grouper, null)
		{
		}

		public string GroupName {
			get {return groupName;}
		}

		public NodeGrouper Grouper {
			get {return grouper;}
		}

		public void Invoke (NodeInfoCollection c, NodeInfo root)
		{
			grouper (c, root, extra);
		}

		public override string ToString ()
		{
			return groupName;
		}
	}
}

