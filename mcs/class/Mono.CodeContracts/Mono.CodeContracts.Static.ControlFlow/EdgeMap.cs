// 
// EdgeMap.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow {
	class EdgeMap<Tag> : IEnumerable<Edge<CFGBlock, Tag>>, IGraph<CFGBlock, Tag> {
		private readonly List<Edge<CFGBlock, Tag>> edges;

		public EdgeMap (List<Edge<CFGBlock, Tag>> edges)
		{
			this.edges = edges;
			Resort ();
		}

		public ICollection<Pair<Tag, CFGBlock>> this [CFGBlock node]
		{
			get { return new Successors (this, FindStartIndex (node)); }
		}

		#region IEnumerable<Edge<CFGBlock,Tag>> Members
		public IEnumerator<Edge<CFGBlock, Tag>> GetEnumerator ()
		{
			return this.edges.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		#endregion

		#region IGraph<CFGBlock,Tag> Members
		IEnumerable<CFGBlock> IGraph<CFGBlock, Tag>.Nodes
		{
			get { throw new InvalidOperationException(); }
		}

		IEnumerable<Pair<Tag, CFGBlock>> IGraph<CFGBlock, Tag>.Successors (CFGBlock node)
		{
			return this [node];
		}
		#endregion

		public EdgeMap<Tag> Reverse ()
		{
			var newEdges = new List<Edge<CFGBlock, Tag>> (this.edges.Count);

			newEdges.AddRange (this.edges.Select (edge => edge.Reversed ()));

			return new EdgeMap<Tag> (newEdges);
		}

		private static int CompareFirstBlockIndex (Edge<CFGBlock, Tag> edge1, Edge<CFGBlock, Tag> edge2)
		{
			int cmp = edge1.From.Index - edge2.From.Index;
			if (cmp == 0)
				cmp = edge1.To.Index - edge2.To.Index;

			return cmp;
		}

		private int FindStartIndex (CFGBlock from)
		{
			//binary search
			int l = 0;
			int r = this.edges.Count;
			while (l < r) {
				int median = (l + r)/2;
				int medianBlockIndex = this.edges [median].From.Index;

				if (medianBlockIndex == from.Index) {
					while (median > 0 && this.edges [median - 1].From.Index == medianBlockIndex)
						--median;
					return median;
				}

				if (medianBlockIndex < from.Index)
					l = median + 1;
				else
					r = median;
			}

			return this.edges.Count;
		}

		public void Filter (Predicate<Edge<CFGBlock, Tag>> keep)
		{
			var notKeepEdges = new List<int> ();
			for (int i = 0; i < this.edges.Count; i++) {
				if (!keep (this.edges [i]))
					notKeepEdges.Add (i);
			}

			if (notKeepEdges.Count == 0)
				return;

			int ix = 0;
			foreach (int i in notKeepEdges) {
				this.edges.RemoveAt (i - ix);
				ix++;
			}
		}

		public void Resort ()
		{
			this.edges.Sort (CompareFirstBlockIndex);
		}

		#region Nested type: Successors
		private struct Successors : ICollection<Pair<Tag, CFGBlock>> {
			private readonly int start_index;
			private readonly EdgeMap<Tag> underlying;

			public Successors (EdgeMap<Tag> underlying, int startIndex)
			{
				this.underlying = underlying;
				this.start_index = startIndex;
			}

			#region ICollection<Pair<Tag,CFGBlock>> Members
			public IEnumerator<Pair<Tag, CFGBlock>> GetEnumerator ()
			{
				List<Edge<CFGBlock, Tag>> edges = this.underlying.edges;
				if (this.start_index < edges.Count) {
					int index = this.start_index;
					int blockIndex = edges [index].From.Index;
					do {
						yield return new Pair<Tag, CFGBlock> (edges [index].Tag, edges [index].To);
						++index;
					} while (index < edges.Count && edges [index].From.Index == blockIndex);
				}
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			public void Add (Pair<Tag, CFGBlock> item)
			{
				throw new InvalidOperationException ();
			}

			public void Clear ()
			{
				throw new InvalidOperationException ();
			}

			public bool Contains (Pair<Tag, CFGBlock> item)
			{
				throw new NotImplementedException ();
			}

			public void CopyTo (Pair<Tag, CFGBlock>[] array, int arrayIndex)
			{
				throw new NotImplementedException ();
			}

			public bool Remove (Pair<Tag, CFGBlock> item)
			{
				throw new InvalidOperationException ();
			}

			public int Count
			{
				get
				{
					int index = this.start_index;
					List<Edge<CFGBlock, Tag>> edges = this.underlying.edges;
					if (index >= edges.Count)
						return 0;
					int blockIndex = edges [index].From.Index;

					int count = 0;
					do {
						++count;
						++index;
					} while (index < edges.Count && edges [index].From.Index == blockIndex);

					return count;
				}
			}

			public bool IsReadOnly
			{
				get { return true; }
			}
			#endregion
		}
		#endregion
	}
}
