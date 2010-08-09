// SplitOrderedList.cs
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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
//
//

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

#if NET_4_0 || BOOTSTRAP_NET_4_0

namespace System.Collections.Concurrent
{
	internal class SplitOrderedList<T>
	{
		class Node 
		{
			bool marked;
			int key;
			T data;
			
			public Node Next;
			
			public Node (int key, T data)
				: this (false)
			{
				this.key = key;
				this.data = data;
			}
			
			protected Node (bool marked)
			{
				this.marked = marked;
			}
			
			public bool Marked {
				get {
					return marked;
				}
			}
			
			public int Key {
				get {
					return key;
				}
			}
			
			public T Data {
				get {
					return data;
				}
			}
		}
		
		class MarkedNode : Node
		{
			public MarkedNode (Node wrapped) : base (false)
			{
				Next = wrapped;
			}
		}
		
		Node head;
		Node tail;
		
		Node[] buckets = new Node [1];
		int count;
		int size = 1;
		
		public SplitOrderedList ()
		{
			head = new Node (0);
			tail = new Node (0);
			head.Next = tail;
			
			
		}
		
		public bool Insert (int key, T data)
		{
			int b = key % size;
			
		}
				
		void InitializeBucket (int index)
		{
			
		}
		
		int GetParent (int key)
		{
			int r = key;
			int c = 0;
			
			while ((r <<= 1) == 
		}
		
		Node ListSearch (int key, ref Node left)
		{
			Node leftNodeNext = null, rightNode = null;
			
		search_again:
			do {
				Node t = head;
				Node tNext = head.Next;
				
				do {
					if (!tNext.Marked) {
						left = t;
						leftNodeNext = tNext;
					}
					
					t = tNext.Marked ? tNext.Next : tNext;
					if (t == tail)
						break;
					
					tNext = t.Next;
				} while (tNext.Marked || t.Key < key);
				
				rightNode = t;
				
				if (leftNodeNext == rightNode) {
					if (rightNode != tail && rightNode.Next.Marked)
						goto search_again;
					else 
						return rightNode;
				}
				
				if (Interlocked.CompareExchange (ref left.Next, rightNode, leftNodeNext) == leftNodeNext) {
					if (rightNode != tail && rightNode.Next.Marked)
						goto search_again;
					else
						return rightNode;
				}
			} while (true);
		}
		
		bool ListDelete (int key) 
		{
			Node rightNode = null, rightNodeNext = null, leftNode = null;
			
			do {
				rightNode = ListSearch (key, ref leftNode);
				if (rightNode == tail || rightNode.Key != key)
					return false;
				
				rightNodeNext = rightNode.Next;
				if (!rightNodeNext.Marked)
					if (Interlocked.CompareExchange (ref rightNode.Next, new MarkedNode (rightNodeNext), rightNodeNext) == rightNodeNext)
						break;
			} while (true);
			
			if (Interlocked.CompareExchange (ref leftNode.Next, rightNode, rightNodeNext) != rightNodeNext)
				rightNode = ListSearch (rightNode.Key, ref leftNode);
			
			return true;
		}
		
		bool ListInsert (int key, T data)
		{
			Node newNode = new Node (key, data);
			Node rightNode = null, leftNode = null;
			
			do {
				rightNode = ListSearch (key, ref leftNode);
				if (rightNode != tail && rightNode.Key == key)
					return false;
				
				newNode.Next = rightNode;
				if (Interlocked.CompareExchange (ref leftNode.Next, newNode, rightNode) == rightNode)
					return true;
			} while (true);
		}
		
		bool ListFind (int key)
		{
			Node rightNode = null, leftNode = null;
			
			rightNode = ListSearch (key, ref leftNode);
			
			return rightNode != tail && rightNode.Key == key;
		}
		
		/*bool ListInsert (ref Node head, Node node)
		{
			int key = node.Key;
			
			Node curr;
			Node next;
			
			while (true) {
				if (ListFind (ref head, key, out curr, out next))
					// A node with the same key already exists
					return false;
				
				node.Next = curr;
				if (Interlocked.CompareExchange (prev, curr, node) == node)
					return true;
			}
		}
		
		bool ListDelete (ref Node head, int key)
		{
			
		}
		
		bool ListFind (ref Node head, int key, out Node curr, out Node next, out Node prev)
		{
		try_again:
				curr = prev = head;
				next = null;
			
			while (true) {
				if (curr == null)
					return false;
				
				next = curr;
				
				int ckey = curr.Key;
				if (head != curr)
					goto try_again;
				if (!curr.Marked) {
					if (ckey >= key)
						return ckey == key;
					prev = curr.Next;
				} else {
					if (Interlocked.CompareExchange (ref head, curr, next) != curr)
						goto try_again;
				}
				
				curr = next;
			}
		}*/
	}
}

