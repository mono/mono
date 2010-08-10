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
	public class SplitOrderedList<T>
	{
		static readonly byte[] reverseTable = {
			0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240, 8, 136, 72, 200, 40, 168, 104, 232, 24, 152, 88, 216, 56, 184, 120, 248, 4, 132, 68, 196, 36, 164, 100, 228, 20, 148, 84, 212, 52, 180, 116, 244, 12, 140, 76, 204, 44, 172, 108, 236, 28, 156, 92, 220, 60, 188, 124, 252, 2, 130, 66, 194, 34, 162, 98, 226, 18, 146, 82, 210, 50, 178, 114, 242, 10, 138, 74, 202, 42, 170, 106, 234, 26, 154, 90, 218, 58, 186, 122, 250, 6, 134, 70, 198, 38, 166, 102, 230, 22, 150, 86, 214, 54, 182, 118, 246, 14, 142, 78, 206, 46, 174, 110, 238, 30, 158, 94, 222, 62, 190, 126, 254, 1, 129, 65, 193, 33, 161, 97, 225, 17, 145, 81, 209, 49, 177, 113, 241, 9, 137, 73, 201, 41, 169, 105, 233, 25, 153, 89, 217, 57, 185, 121, 249, 5, 133, 69, 197, 37, 165, 101, 229, 21, 149, 85, 213, 53, 181, 117, 245, 13, 141, 77, 205, 45, 173, 109, 237, 29, 157, 93, 221, 61, 189, 125, 253, 3, 131, 67, 195, 35, 163, 99, 227, 19, 147, 83, 211, 51, 179, 115, 243, 11, 139, 75, 203, 43, 171, 107, 235, 27, 155, 91, 219, 59, 187, 123, 251, 7, 135, 71, 199, 39, 167, 103, 231, 23, 151, 87, 215, 55, 183, 119, 247, 15, 143, 79, 207, 47, 175, 111, 239, 31, 159, 95, 223, 63, 191, 127, 255
		};

		static readonly byte[] logTable = {
			0xFF, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
		};

		class Node 
		{
			bool marked;
			uint key;
			T data;
			
			public Node Next;
			
			public Node (uint key, T data)
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
			
			public uint Key {
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

		const int MaxLoad = 10;
		const int SegmentSize = 50;		

		Node head;
		Node tail;
		
		Node[][] buckets = new Node[1000][];
		int count;
		int size = 1;

		ManualResetEventSlim mres = new ManualResetEventSlim (true);
		SpinLock mresLock = new SpinLock ();
		
		public SplitOrderedList ()
		{
			head = new Node (0, default (T));
			tail = new Node (0, default (T));
			head.Next = tail;
		}
		
		public bool Insert (uint key, T data)
		{
			Node node = new Node (ComputeRegularKey (key), data);
			Node current;
			uint b = key % (uint)size;
			
			if (GetBucket (b) == null)
				InitializeBucket (b);
			if (!ListInsert (node, GetBucket (b), out current))
				return false;

			int csize = size;
			if (Interlocked.Increment (ref count) / csize > MaxLoad)
				Interlocked.CompareExchange (ref size, 2 * csize, csize);

			return true;
		}
		
		public bool Find (uint key)
		{
			uint b = key % (uint)size;
			if (GetBucket (b) == null)
				InitializeBucket (b);
			return ListFind (ComputeRegularKey (key), GetBucket (b));
		}

		public bool Delete (uint key)
		{
			uint b = key % (uint)size;
			if (GetBucket (b) == null)
				InitializeBucket (b);
			if (!ListDelete (GetBucket (b), ComputeRegularKey (key)))
				return false;

			Interlocked.Decrement (ref count);
			return true;
		}

		void InitializeBucket (uint b)
		{
			Node current;
			uint parent = GetParent (b);
			if (buckets[parent] == null)
				InitializeBucket ((uint)parent);

			Node dummy = new Node (ComputeDummyKey (b), default (T));
			if (!ListInsert (dummy, GetBucket (parent), out current))
				dummy = current;

			//buckets[b] = dummy;
			SetBucket (b, dummy);
		}
		
		// Find log2 of the integer
		uint GetParent (uint v)
		{
			uint t, tt;
			
			return (tt = v >> 16) > 0 ? 
				(t = tt >> 8) > 0 ? 24 + (uint)logTable[t] : 16 + (uint)logTable[tt] :
				(t = v >> 8) > 0 ? 8 + (uint)logTable[t] : (uint)logTable[v];
		}

		uint ComputeRegularKey (uint key)
		{
			return ComputeDummyKey (key | 0x80000000);
		}
		
		// Reverse integer bits
		uint ComputeDummyKey (uint key)
		{
			return ((uint)reverseTable[key & 0xff] << 24) | 
				((uint)reverseTable[(key >> 8) & 0xff] << 16) | 
				((uint)reverseTable[(key >> 16) & 0xff] << 8) |
				((uint)reverseTable[(key >> 24) & 0xff]);
		}

		Node GetBucket (uint index)
		{
			int segment = (int)(index / SegmentSize);
			CheckSegment (segment);
			if (buckets[segment] == null)
				return null;

			return buckets[segment][index % SegmentSize];
		}

		void SetBucket (uint index, Node node)
		{
			int segment = (int)(index / SegmentSize);
			CheckSegment (segment);
			if (buckets[segment] == null) {
				Node[] newSegment = new Node[SegmentSize];
				Interlocked.CompareExchange (ref buckets[segment], newSegment, null);
			}
			buckets[segment][index % SegmentSize] = node;
		}

		void CheckSegment (int segment)
		{
			while (segment >= buckets.Length) {
				bool shouldResize = false;
				bool taken = false;
				try {
					mresLock.Enter (ref taken);
					if (mres.IsSet) {
						shouldResize = true;
						mres.Reset ();
					}
				} finally {
					if (taken)
						mresLock.Exit ();
				}

				if (shouldResize) {
					Array.Resize (ref buckets, size / SegmentSize);
					mres.Set ();
				} else {
					mres.Wait ();
				}
			}
		}
		
		Node ListSearch (uint key, ref Node left, Node h)
		{
			Node leftNodeNext = null, rightNode = null;
			
		search_again:
			do {
				Node t = h;
				Node tNext = h.Next;
				
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
	
		bool ListDelete (Node startPoint, uint key) 
		{
			Node rightNode = null, rightNodeNext = null, leftNode = null;
			
			do {
				rightNode = ListSearch (key, ref leftNode, startPoint);
				if (rightNode == tail || rightNode.Key != key)
					return false;
				
				rightNodeNext = rightNode.Next;
				if (!rightNodeNext.Marked)
					if (Interlocked.CompareExchange (ref rightNode.Next, new MarkedNode (rightNodeNext), rightNodeNext) == rightNodeNext)
						break;
			} while (true);
			
			if (Interlocked.CompareExchange (ref leftNode.Next, rightNode, rightNodeNext) != rightNodeNext)
				rightNode = ListSearch (rightNode.Key, ref leftNode, head);
			
			return true;
		}
		
		bool ListInsert (Node newNode, Node startPoint, out Node current)
		{
			uint key = newNode.Key;
			Node rightNode = null, leftNode = null;
			
			do {
				rightNode = current = ListSearch (key, ref leftNode, startPoint);
				if (rightNode != tail && rightNode.Key == key)
					return false;
				
				newNode.Next = rightNode;
				if (Interlocked.CompareExchange (ref leftNode.Next, newNode, rightNode) == rightNode)
					return true;
			} while (true);
		}
		
		bool ListFind (uint key, Node startPoint)
		{
			Node rightNode = null, leftNode = null;
			
			rightNode = ListSearch (key, ref leftNode, startPoint);
			
			return rightNode != tail && rightNode.Key == key;
		}
	}
}

#endif
