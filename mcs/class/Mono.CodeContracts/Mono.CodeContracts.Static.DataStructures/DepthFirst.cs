// 
// DepthFirst.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures {
	static class DepthFirst {
		public static void Visit<Node, EdgeInfo> (IGraph<Node, EdgeInfo> graph,
		                                          Predicate<Node> nodeStartVisitor,
		                                          EdgeVisitor<Node, EdgeInfo> edgeVisitor)
		{
			new Visitor<Node, EdgeInfo> (graph, nodeStartVisitor, edgeVisitor).VisitAll ();
		}

		public static void Visit<Node, EdgeInfo> (IGraph<Node, EdgeInfo> graph,
		                                          Node startNode,
		                                          Predicate<Node> nodeStartVisitor,
		                                          EdgeVisitor<Node, EdgeInfo> edgeVisitor)
		{
			new Visitor<Node, EdgeInfo> (graph, nodeStartVisitor, edgeVisitor).VisitSubGraphNonRecursive (startNode);
		}

		#region Nested type: Info
		public class Info<Node> {
			public readonly Node Parent;
			public readonly int StartTime;

			public int FinishTime;
			public bool SourceOfBackEdge;
			public bool TargetOfBackEdge;

			public Info (Node parent, int startTime)
			{
				this.Parent = parent;
				this.StartTime = startTime;
			}
		}
		#endregion

		#region Nested type: Visitor
		public class Visitor<Node, Edge> {
			private readonly HashSet<Tuple<Node, Edge, Node>> back_edges = new HashSet<Tuple<Node, Edge, Node>> ();
			private readonly EdgeVisitor<Node, Edge> edge_visitor;
			private readonly IGraph<Node, Edge> graph;

			private readonly Dictionary<Node, Info<Node>> history = new Dictionary<Node, Info<Node>> ();
			private readonly Action<Node> node_finish_visitor;
			private readonly Predicate<Node> node_start_visitor;
			private readonly Stack<SearchFrame> todo = new Stack<SearchFrame> ();
			private int time;

			public Visitor (IGraph<Node, Edge> graph, Predicate<Node> nodeStartVisitor, EdgeVisitor<Node, Edge> edgeVisitor)
				: this (graph, nodeStartVisitor, null, edgeVisitor)
			{
			}

			public Visitor (IGraph<Node, Edge> graph, Predicate<Node> nodeStartVisitor)
				: this (graph, nodeStartVisitor, null, null)
			{
			}

			public Visitor (IGraph<Node, Edge> graph, Predicate<Node> nodeStartVisitor, Action<Node> nodeFinishVisitor, EdgeVisitor<Node, Edge> edgeVisitor)
			{
				this.graph = graph;
				this.node_start_visitor = nodeStartVisitor;
				this.node_finish_visitor = nodeFinishVisitor;
				this.edge_visitor = edgeVisitor;
			}

			public virtual void VisitAll ()
			{
				foreach (Node node in this.graph.Nodes)
					VisitSubGraphNonRecursive (node);
			}

			public void VisitSubGraphNonRecursive (Node node)
			{
				ScheduleNode (node, default(Node));
				IterativeDFS ();
			}

			private void IterativeDFS ()
			{
				while (this.todo.Count > 0) {
					SearchFrame frame = this.todo.Peek ();
					if (frame.Edges.MoveNext ()) {
						Pair<Edge, Node> current = frame.Edges.Current;
						VisitEdgeNonRecursive (frame.Info, frame.Node, current.Key, current.Value);
					} else {
						if (this.node_finish_visitor != null)
							this.node_finish_visitor (frame.Node);
						frame.Info.FinishTime = ++this.time;
						this.todo.Pop ();
					}
				}
			}

			private void VisitEdgeNonRecursive (Info<Node> sourceInfo, Node source, Edge info, Node target)
			{
				if (this.edge_visitor != null)
					this.edge_visitor (source, info, target);

				Info<Node> targetInfo;
				if (this.history.TryGetValue (target, out targetInfo)) {
					if (targetInfo.FinishTime != 0)
						return;

					targetInfo.TargetOfBackEdge = true;
					sourceInfo.SourceOfBackEdge = true;
					this.back_edges.Add (new Tuple<Node, Edge, Node> (source, info, target));
				} else
					ScheduleNode (target, source);
			}

			private void VisitEdge (Info<Node> sourceInfo, Node source, Edge info, Node target)
			{
				if (this.edge_visitor != null)
					this.edge_visitor (source, info, target);

				Info<Node> targetInfo;
				if (this.history.TryGetValue (target, out targetInfo)) {
					if (targetInfo.FinishTime != 0)
						return;

					targetInfo.TargetOfBackEdge = true;
					sourceInfo.SourceOfBackEdge = true;
					this.back_edges.Add (new Tuple<Node, Edge, Node> (source, info, target));
				} else
					VisitSubGraph (target, source);
			}

			public void VisitSubGraph (Node node, Node parent)
			{
				if (this.history.ContainsKey (node))
					return;

				var info = new Info<Node> (parent, ++this.time);
				this.history [node] = info;

				if (this.node_start_visitor != null && !this.node_start_visitor (node))
					return;

				VisitSuccessors (info, node);

				if (this.node_finish_visitor != null)
					this.node_finish_visitor (node);

				info.FinishTime = ++this.time;
			}

			public void ScheduleNode (Node node, Node parent)
			{
				if (this.history.ContainsKey (node))
					return;

				var info = new Info<Node> (parent, ++this.time);
				this.history [node] = info;

				if (this.node_start_visitor != null && !this.node_start_visitor (node))
					return;

				this.todo.Push (new SearchFrame (node, this.graph.Successors (node).GetEnumerator (), info));
			}

			private void VisitSuccessors (Info<Node> info, Node node)
			{
				foreach (var successor in this.graph.Successors (node))
					VisitEdge (info, node, successor.Key, successor.Value);
			}

			public bool IsVisited (Node node)
			{
				return this.history.ContainsKey (node);
			}

			public bool IsBackEdge (Node source, Edge info, Node target)
			{
				return this.back_edges.Contains (new Tuple<Node, Edge, Node> (source, info, target));
			}

			public Info<Node> DepthFirstInfo (Node node)
			{
				return this.history [node];
			}

			#region Nested type: SearchFrame
			private struct SearchFrame {
				public readonly IEnumerator<Pair<Edge, Node>> Edges;
				public readonly Info<Node> Info;
				public readonly Node Node;

				public SearchFrame (Node node, IEnumerator<Pair<Edge, Node>> edges, Info<Node> info)
				{
					this.Node = node;
					this.Edges = edges;
					this.Info = info;
				}
			}
			#endregion
		}
		#endregion
	}
}
