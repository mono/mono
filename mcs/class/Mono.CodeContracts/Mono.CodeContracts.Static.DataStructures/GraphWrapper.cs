// 
// GraphWrapper.cs
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
	class GraphWrapper<Node, Info> : IGraph<Node, Info> {
		private readonly IEnumerable<Node> nodes;
		private readonly Func<Node, IEnumerable<Pair<Info, Node>>> successors;

		public GraphWrapper (IEnumerable<Node> nodes, Func<Node, IEnumerable<Pair<Info, Node>>> successors)
		{
			this.nodes = nodes;
			this.successors = successors;
		}

		#region Implementation of IGraph<Node,Info>
		public IEnumerable<Node> Nodes
		{
			get { return this.nodes; }
		}

		public IEnumerable<Pair<Info, Node>> Successors (Node node)
		{
			return this.successors (node);
		}
		#endregion
	}
}
