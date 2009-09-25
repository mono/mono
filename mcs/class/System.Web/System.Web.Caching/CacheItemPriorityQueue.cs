//
// System.Web.Caching.CacheItem
//
// Author(s):
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2009 Novell, Inc (http://novell.com)
//
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
using System.Text;

namespace System.Web.Caching
{
	sealed class CacheItemPriorityQueue
	{
		sealed class Node
		{
			public CacheItem Data;
		
			public Node Left;
			public Node Right;
			public Node Parent;
			public Node Next;
			public Node Prev;

			public CacheItem SwapData (CacheItem newData)
			{
				CacheItem ret = Data;
				Data = newData;
				
				return ret;
			}

			public Node (CacheItem data)
			{
				Data = data;
			}
		}
		
		Node root;
		Node lastAdded;
		Node firstParent;
		Node lastParent;

		public void Enqueue (CacheItem item)
		{
			if (item == null)
				return;

			Node node = new Node (item);
			if (root == null) {
				root = lastAdded = lastParent = firstParent = node;
				return;
			}

			if (lastParent.Left != null && lastParent.Right != null) {
				lastParent = lastParent.Next;
				if (lastParent == null) {
					lastParent = firstParent = firstParent.Left;
					lastAdded = null;
				}
			}

			node.Parent = lastParent;			
			if (lastParent.Left == null)
				lastParent.Left = node;
			else
				lastParent.Right = node;

			if (lastAdded != null) {
				lastAdded.Next = node;
				node.Prev = lastAdded;
			}
			
			lastAdded = node;
			BubbleUp (node);
		}

		public CacheItem Dequeue ()
		{
			Console.WriteLine ("{0}.Dequeue ()", this);
			if (root == null)
				return null;

			CacheItem ret;
			if (root.Left == null && root.Right == null) {
				ret = root.Data;
				root = null;
				if (ret.Disabled) {
					Console.WriteLine ("\troot disabled, returning null");
					return null;
				}
				
				return ret;
			}

			ret = root.Data;
			do {
				Node last = lastAdded;
				if (last == null)
					return null;
				
				if (last.Prev == null) {
					Node parent = last.Parent;
					while (true) {
						if (parent.Next == null)
							break;
						parent = parent.Next;
					}
					lastAdded = parent;
				} else {
					lastAdded = last.Prev;
					lastAdded.Next = null;
				}

				if (last.Parent.Left == last)
					last.Parent.Left = null;
				else
					last.Parent.Right = null;
			
				root.Data = last.Data;
				BubbleDown (root);

				if (ret.Disabled)
					Console.WriteLine ("\titem {0} disabled, ignoring", ret.ExpiresAt);
			} while (ret.Disabled);
			
			return ret;
		}

		public CacheItem Peek ()
		{
			if (root == null)
				return null;

			return root.Data;
		}
		
		void BubbleDown (Node item)
		{
			if (item == null || (item.Left == null && item.Right == null))
				return;

			if (item.Left == null)
				SwapBubbleDown (item, item.Right);
			else if (item.Right == null)
				SwapBubbleDown (item, item.Left);
			else {
				if (item.Left.Data.ExpiresAt < item.Right.Data.ExpiresAt)
					SwapBubbleDown (item, item.Left);
				else
					SwapBubbleDown (item, item.Right);
			}
		}

		void SwapBubbleDown (Node item, Node otherItem)
		{
			if (otherItem.Data.ExpiresAt < item.Data.ExpiresAt) {
				item.Data = otherItem.SwapData (item.Data);
				BubbleDown (otherItem);
			}
		}
		
		void BubbleUp (Node item)
		{
			if (item == null || item.Data == null)
				return;
			
			Node parent = item.Parent;
			if (parent == null)
				return;
			
			if (item.Data.ExpiresAt > parent.Data.ExpiresAt)
				return;

			item.Data = parent.SwapData (item.Data);
			
			BubbleUp (parent);
		}
		
		public string GetDotScript ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("graph CacheItemPriorityQueue {\n");
			sb.Append ("\tnode [color=lightblue, style=filled];\n");
			if (root != null) {
				if (root.Left == null && root.Right == null)
					sb.AppendFormat ("\t{0};", root.Data.ExpiresAt);
				else
					TraverseTree (sb, root);
			}
			sb.Append ("}\n");

			return sb.ToString ();
		}

		void TraverseTree (StringBuilder sb, Node root)
		{
			if (root.Left != null) {
				sb.AppendFormat ("\t{0} -- {1};\n", root.Data.ExpiresAt, root.Left.Data.ExpiresAt);
				TraverseTree (sb, root.Left);
			}

			if (root.Right != null) {
				sb.AppendFormat ("\t{0} -- {1};\n", root.Data.ExpiresAt, root.Right.Data.ExpiresAt);
				TraverseTree (sb, root.Right);
			}
		}
	}
}
