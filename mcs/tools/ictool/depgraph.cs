//
// file:	depgraph.cs
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(C) 2002
//

using System;
using System.Collections;

class DependencyGraph {
	public DependencyGraph () {
		nodes = new Hashtable ();
	}

	public void AddNode (object o) {
		if (!nodes.Contains (o))
			nodes.Add (o, new Node (o));
	}

	public void AddEdge (object from, object to) {
		if (!nodes.Contains (from))
			AddNode (from);
		if (!nodes.Contains (to))
			AddNode (from);

		Node from_node = (Node)nodes[from];
		Node to_node = (Node)nodes[to];

		from_node.edges.Add (to_node);
	}

	public IList TopologicalSort () {
		foreach (Node node in nodes.Values)
			node.marked = false;

		IList list = new ArrayList ();
		foreach (Node node in nodes.Values) {
			if (!node.marked)
				Visit (node, list);
		}

		return list;
	}

	// private

	private void Visit (Node node, IList list) {
		node.marked = true;
		foreach (Node adj in node.edges) {
			if (!adj.marked)
				Visit (adj, list);
		}

		list.Insert (0, node.value);
	}

	private class Node {
		public Node (object o) {
			this.value = o;
			this.edges = new ArrayList ();
		}

		public object value;
		public ArrayList edges;
		public bool marked;
	}

	private Hashtable nodes;
}
