//
// Node.cs: Used to contain the output tree before display.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Mono.TypeReflector
{
	public sealed class Node : ICloneable {

		INodeFormatter formatter;
		INodeFinder    finder;

		NodeInfo       nodeInfo;

		public Node (INodeFormatter formatter, INodeFinder finder)
		{
			this.formatter = formatter;
			this.finder = finder;
		}

		public object Clone ()
		{
			return MemberwiseClone();
		}

		public string Description {
			get {return formatter.GetDescription(nodeInfo);}
		}

		public NodeCollection GetChildren ()
		{
			NodeCollection children = new NodeCollection ();
			NodeInfoCollection nic = finder.GetChildren (nodeInfo);
			// foreach (NodeInfo o in finder.GetChildren (nodeInfo)) {
			foreach (NodeInfo o in nic) {
				Node n = (Node) this.Clone ();
				n.NodeInfo = o;
				children.Add (n);
			}
			return children;
		}

		public NodeInfo NodeInfo {
			get {return nodeInfo;}
			set {nodeInfo = value;}
		}

		/*
		public override string ToString ()
		{
			Console.Write ("(Description='{0}'", Description);
			if (Children.Count > 0) {
				Console.Write (" Children={{");
				foreach (Node node in Children)
					Console.Write (node);
				Console.Write ("}}");
			}
			Console.Write (")");
			return "foo!";
			return new StackTrace().ToString();
		}
		 */
	}

	public class NodeCollection : CollectionBase {

		internal NodeCollection ()
		{
		}

		public Node this [int index] {
			get {return (Node) InnerList[index];}
			set {InnerList[index] = value;}
		}

		public int Add (Node value)
		{
			return InnerList.Add (value);
		}

		public void AddRange (Node[] values)
		{
			foreach (Node node in values)
				Add (node);
		}
	}
}

