// 
// CSnzi.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0
using System;

namespace System.Threading
{
	internal interface ICSnziNode
	{
		bool Arrive ();
		bool Depart ();
	}
	
	internal enum CSnziState
	{
		Open,
		Closed
	}
	
	internal struct QueryReturn
	{
		internal readonly bool NonZero;
		internal readonly bool Open;
		
		internal QueryReturn (bool nonzero, bool open)
		{
			NonZero = nonzero;
			Open = open;
		}
	}
	
	internal class CSnziLeafNode : ICSnziNode
	{
		int count;
		readonly ICSnziNode parent;
		
		public CSnziLeafNode (ICSnziNode parent)
		{
			this.parent = parent;
		}

		#region ICSnziNode implementation
		public bool Arrive ()
		{
			bool arrivedAtParent = false;
			int x;
			
			do {
				x = count;
				if (x == 0 && !arrivedAtParent) {
					if (parent.Arrive ())
						arrivedAtParent = true;
					else
						return false;
				}
			} while (Interlocked.CompareExchange (ref count, x + 1, x) != x);
			
			if (arrivedAtParent && x != 0)
				parent.Depart ();
			
			return true;
		}
		
		public bool Depart ()
		{
			int x = Interlocked.Decrement (ref count);
			if (x == 1)
				return parent.Depart ();
			else
				return true;
		}
		#endregion
		
	}
	
	internal class CSnziRootNode : ICSnziNode
	{
		int root;
		
		public int Count {
			get {
				return root & 0x7FFFFFFF;
			}
		}
					
		public CSnziState State {
			get {
				return (root >> 31) > 0 ? CSnziState.Open : CSnziState.Closed;
			}
		}
		
		public CSnziRootNode () : this (0, CSnziState.Open)
		{
			
		}
		
		public CSnziRootNode (int count, CSnziState state)
		{
			root = Encode (count, state);
		}

		#region ICSnziNode implementation
		public bool Arrive ()
		{
			int old;
			int c;
			CSnziState s;
			
			do {
				old = root;
				
				Decode (old, out c, out s);
				
				if (c == 0 && s == CSnziState.Closed)
					return false;
			} while (Interlocked.CompareExchange (ref root, Encode (c + 1, s), old) != old);
			
			return true;
		}
		
		public bool Depart ()
		{
			int old;
			int c;
			CSnziState s;
			
			do {
				old = root;	
				Decode (old, out c, out s); 
			} while (Interlocked.CompareExchange (ref root, Encode (c - 1, s), old) != old);
			
			return c != 0 && s != CSnziState.Closed;
		}
		#endregion
		
		public void Open ()
		{
			root = Encode (0, CSnziState.Open);
		}
		
		public bool Close ()
		{
			int old, newRoot;
			int c;
			CSnziState s;
			
			do {
				old = root;
	
				Decode (old, out c, out s);
				if (s != CSnziState.Open)
					return false;
				
				newRoot = Encode (c, CSnziState.Closed);
			} while (Interlocked.CompareExchange (ref root, newRoot, old) != old);
			
			return c == 0;
		}
		
		int Encode (int count, CSnziState state)
		{
			return (state == CSnziState.Open) ? (int)(((uint)count) | 0x80000000) : count & 0x7FFFFFFF;
		}
		
		void Decode (int code, out int count, out CSnziState state)
		{
			count = code & 0x7FFFFFFF;
			state = (code >> 31) > 0 ? CSnziState.Open : CSnziState.Closed;
		}
	}
	
	internal class CSnzi
	{
		CSnziRootNode root;
		CSnziLeafNode[] leafs;
		
		readonly int LeafCount = Environment.ProcessorCount * 2;
		
		public CSnzi ()
		{
			leafs = new CSnziLeafNode[LeafCount];
			root = new CSnziRootNode ();
			
			for (int i = 0; i < leafs.Length; i++) {
				leafs[i] = new CSnziLeafNode (root);
			}
		}
		
		public ICSnziNode Arrive ()
		{
			while (true) {
				if (root.State != CSnziState.Open)
					return null;
				
				ICSnziNode leaf = leafs[GetLeafIndex ()];
				if (leaf.Arrive ())
					return leaf;
				else {
					return null;
				}
			}
		}
		
		public bool Depart (ICSnziNode node)
		{
			return node.Depart ();
		}
		
		public bool Close ()
		{
			return root.Close ();
		}
		
		public void Open ()
		{
			root.Open ();
		}
		
		public QueryReturn Query ()
		{
			CSnziRootNode copy = root;
			
			return new QueryReturn (copy.Count > 0, copy.State == CSnziState.Open);
		}
		
		int GetLeafIndex ()
		{
			return (Thread.CurrentThread.ManagedThreadId - 1) % leafs.Length;
		}
	}
}
#endif
