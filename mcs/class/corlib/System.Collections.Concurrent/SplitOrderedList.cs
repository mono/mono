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

#if NET_4_0 || BOOTSTRAP_NET_4_0 || INSIDE_SYSTEM_WEB

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Collections.Concurrent
{
	internal class SplitOrderedList<T>
	{
		class Node
		{
			public readonly bool Marked;
			public readonly ulong Key;
			public T Data;
			
			public Node Next;
			
			public Node (ulong key, T data)
			{
				this.Key = key;
				this.Data = data;
			}

			// Used to create marked node
			public Node (Node wrapped)
			{
				this.Marked = true;
				this.Next = wrapped;
			}
		}

		const int MaxLoad = 5;
		/*const uint SegmentSize = 1u << SegmentShift;
		const int SegmentShift = 5;*/
		const uint BucketSize = 512;

		/*[ThreadStatic]
		Node[] segmentCache;*/

		Node head;
		Node tail;

		//Node[][] buckets = new Node[BucketSize][];
		Node[] buckets = new Node [BucketSize];
		int count;
		int size = 2;

		SimpleRwLock slim = new SimpleRwLock ();

		public SplitOrderedList ()
		{
			head = new Node (0, default (T));
			tail = new Node (ulong.MaxValue, default (T));
			head.Next = tail;
			SetBucket (0, head);
		}

		public int Count {
			get {
				return count;
			}
		}

		public T InsertOrUpdate (uint key, Func<T> addGetter, Func<T, T> updateGetter)
		{
			Node current;
			bool result = InsertInternal (key, default (T), addGetter, out current);

			if (result)
				return current.Data;

			// FIXME: this should have a CAS-like behavior
			return current.Data = updateGetter (current.Data);
		}

		public T InsertOrUpdate (uint key, T addValue, T updateValue)
		{
			Node current;
			if (InsertInternal (key, addValue, null, out current))
				return current.Data;

			// FIXME: this should have a CAS-like behavior
			return current.Data = updateValue;
		}
		
		public bool Insert (uint key, T data)
		{
			Node current;
			return InsertInternal (key, data, null, out current);
		}

		public T InsertOrGet (uint key, T data, Func<T> dataCreator)
		{
			Node current;
			InsertInternal (key, data, dataCreator, out current);
			return current.Data;
		}

		bool InsertInternal (uint key, T data, Func<T> dataCreator, out Node current)
		{
			Node node = new Node (ComputeRegularKey (key), data);

			uint b = key % (uint)size;
			Node bucket;

			if ((bucket = GetBucket (b)) == null)
				bucket = InitializeBucket (b);

			if (!ListInsert (node, bucket, out current, dataCreator))
				return false;

			int csize = size;
			if (Interlocked.Increment (ref count) / csize > MaxLoad && (csize & 0x40000000) == 0)
				Interlocked.CompareExchange (ref size, 2 * csize, csize);

			current = node;

			return true;
		}
		
		public bool Find (uint key, out T data)
		{
			Node node;
			uint b = key % (uint)size;
			data = default (T);
			Node bucket;

			if ((bucket = GetBucket (b)) == null)
				bucket = InitializeBucket (b);

			if (!ListFind (ComputeRegularKey (key), bucket, out node))
				return false;

			data = node.Data;

			return !node.Marked;
		}

		public bool CompareExchange (uint key, T data, Func<T, bool> check)
		{
			Node node;
			uint b = key % (uint)size;
			Node bucket;

			if ((bucket = GetBucket (b)) == null)
				bucket = InitializeBucket (b);

			if (!ListFind (ComputeRegularKey (key), bucket, out node))
				return false;

			if (!check (node.Data))
				return false;

			node.Data = data;

			return true;
		}

		public bool Delete (uint key, out T data)
		{
			uint b = key % (uint)size;
			Node bucket;

			if ((bucket = GetBucket (b)) == null)
				bucket = InitializeBucket (b);

			if (!ListDelete (bucket, ComputeRegularKey (key), out data))
				return false;

			Interlocked.Decrement (ref count);
			return true;
		}

		public IEnumerator<T> GetEnumerator ()
		{
			Node node = head.Next;

			while (node != tail) {
				while (node.Marked || (node.Key & 1) == 0) {
					node = node.Next;
					if (node == tail)
						yield break;
				}
				yield return node.Data;
				node = node.Next;
			}
		}

		Node InitializeBucket (uint b)
		{
			Node current;
			uint parent = GetParent (b);
			Node bucket;

			if ((bucket = GetBucket (parent)) == null)
				bucket = InitializeBucket (parent);

			Node dummy = new Node (ComputeDummyKey (b), default (T));
			if (!ListInsert (dummy, bucket, out current, null))
				return current;

			return SetBucket (b, dummy);
		}
		
		// Turn v's MSB off
		static uint GetParent (uint v)
		{
			uint t, tt;
			
			// Find MSB position in v
			var pos = (tt = v >> 16) > 0 ?
				(t = tt >> 8) > 0 ? 24 + logTable[t] : 16 + logTable[tt] :
				(t = v >> 8) > 0 ? 8 + logTable[t] : logTable[v];

			return (uint)(v & ~(1 << pos));
		}

		// Reverse integer bits and make sure LSB is set
		static ulong ComputeRegularKey (uint key)
		{
			return ComputeDummyKey (key) | 1;
		}
		
		// Reverse integer bits
		static ulong ComputeDummyKey (uint key)
		{
			return ((ulong)(((uint)reverseTable[key & 0xff] << 24) |
			                ((uint)reverseTable[(key >> 8) & 0xff] << 16) |
			                ((uint)reverseTable[(key >> 16) & 0xff] << 8) |
			                ((uint)reverseTable[(key >> 24) & 0xff]))) << 1;
		}

		// Bucket storage is abstracted in a simple two-layer tree to avoid too much memory resize
		Node GetBucket (uint index)
		{
			/*uint segment = index >> SegmentShift;

			if (buckets[segment] == null)
				return null;

			return buckets[segment][index & (SegmentSize - 1)];*/
			if (index >= buckets.Length)
				return null;
			return buckets[index];
		}

		Node SetBucket (uint index, Node node)
		{
			//uint segment = index >> SegmentShift;
			try {
				slim.EnterReadLock ();
				CheckSegment (index, true);

				/*if (buckets[segment] == null) {
					// Cache segment creation in case CAS fails
					Node[] newSegment = segmentCache == null ? new Node[SegmentSize] : segmentCache;
					segmentCache = Interlocked.CompareExchange (ref buckets[segment], newSegment, null) == null ? null : newSegment;
				}
				return buckets[segment][index & (SegmentSize - 1)] = node;*/

				Interlocked.CompareExchange (ref buckets[index], node, null);
				return buckets[index];
			} finally {
				slim.ExitReadLock ();
			}
		}

		// When we run out of space for bucket storage, we use a lock-based array resize
		void CheckSegment (uint segment, bool readLockTaken)
		{
			if (segment < buckets.Length)
				return;

			if (readLockTaken)
				slim.ExitReadLock ();
			try {
				slim.EnterWriteLock ();
				while (segment >= buckets.Length)
					Array.Resize (ref buckets, buckets.Length * 2);
			} finally {
				slim.ExitWriteLock ();
			}
			if (readLockTaken)
				slim.EnterReadLock ();
		}

		Node ListSearch (ulong key, ref Node left, Node h)
		{
			Node leftNodeNext = null, rightNode = null;

			do {
				Node t = h;
				Node tNext = t.Next;
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
						continue;
					else 
						return rightNode;
				}
				
				if (Interlocked.CompareExchange (ref left.Next, rightNode, leftNodeNext) == leftNodeNext) {
					if (rightNode != tail && rightNode.Next.Marked)
						continue;
					else
						return rightNode;
				}
			} while (true);
		}

		bool ListDelete (Node startPoint, ulong key, out T data)
		{
			Node rightNode = null, rightNodeNext = null, leftNode = null;
			data = default (T);
			
			do {
				rightNode = ListSearch (key, ref leftNode, startPoint);
				if (rightNode == tail || rightNode.Key != key)
					return false;

				data = rightNode.Data;
				
				rightNodeNext = rightNode.Next;
				if (!rightNodeNext.Marked)
					if (Interlocked.CompareExchange (ref rightNode.Next, new Node (rightNodeNext), rightNodeNext) == rightNodeNext)
						break;
			} while (true);
			
			if (Interlocked.CompareExchange (ref leftNode.Next, rightNodeNext, rightNode) != rightNodeNext)
				ListSearch (rightNode.Key, ref leftNode, startPoint);
			
			return true;
		}
		
		bool ListInsert (Node newNode, Node startPoint, out Node current, Func<T> dataCreator)
		{
			ulong key = newNode.Key;
			Node rightNode = null, leftNode = null;
			
			do {
				rightNode = current = ListSearch (key, ref leftNode, startPoint);
				if (rightNode != tail && rightNode.Key == key)
					return false;
				
				newNode.Next = rightNode;
				if (dataCreator != null)
					newNode.Data = dataCreator ();
				if (Interlocked.CompareExchange (ref leftNode.Next, newNode, rightNode) == rightNode)
					return true;
			} while (true);
		}
		
		bool ListFind (ulong key, Node startPoint, out Node data)
		{
			Node rightNode = null, leftNode = null;
			data = null;
			
			rightNode = ListSearch (key, ref leftNode, startPoint);
			data = rightNode;
			
			return rightNode != tail && rightNode.Key == key;
		}

		static readonly byte[] reverseTable = {
			0, 128, 64, 192, 32, 160, 96, 224, 16, 144, 80, 208, 48, 176, 112, 240, 8, 136, 72, 200, 40, 168, 104, 232, 24, 152, 88, 216, 56, 184, 120, 248, 4, 132, 68, 196, 36, 164, 100, 228, 20, 148, 84, 212, 52, 180, 116, 244, 12, 140, 76, 204, 44, 172, 108, 236, 28, 156, 92, 220, 60, 188, 124, 252, 2, 130, 66, 194, 34, 162, 98, 226, 18, 146, 82, 210, 50, 178, 114, 242, 10, 138, 74, 202, 42, 170, 106, 234, 26, 154, 90, 218, 58, 186, 122, 250, 6, 134, 70, 198, 38, 166, 102, 230, 22, 150, 86, 214, 54, 182, 118, 246, 14, 142, 78, 206, 46, 174, 110, 238, 30, 158, 94, 222, 62, 190, 126, 254, 1, 129, 65, 193, 33, 161, 97, 225, 17, 145, 81, 209, 49, 177, 113, 241, 9, 137, 73, 201, 41, 169, 105, 233, 25, 153, 89, 217, 57, 185, 121, 249, 5, 133, 69, 197, 37, 165, 101, 229, 21, 149, 85, 213, 53, 181, 117, 245, 13, 141, 77, 205, 45, 173, 109, 237, 29, 157, 93, 221, 61, 189, 125, 253, 3, 131, 67, 195, 35, 163, 99, 227, 19, 147, 83, 211, 51, 179, 115, 243, 11, 139, 75, 203, 43, 171, 107, 235, 27, 155, 91, 219, 59, 187, 123, 251, 7, 135, 71, 199, 39, 167, 103, 231, 23, 151, 87, 215, 55, 183, 119, 247, 15, 143, 79, 207, 47, 175, 111, 239, 31, 159, 95, 223, 63, 191, 127, 255
		};

		static readonly byte[] logTable = {
			0xFF, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
		};

		struct SimpleRwLock
		{
			const int RwWait = 1;
			const int RwWrite = 2;
			const int RwRead = 4;

			int rwlock;

			public void EnterReadLock ()
			{
				SpinWait sw = new SpinWait ();
				do {
					while ((rwlock & (RwWrite | RwWait)) > 0)
						sw.SpinOnce ();

					if ((Interlocked.Add (ref rwlock, RwRead) & (RwWait | RwWait)) == 0)
						return;

					Interlocked.Add (ref rwlock, -RwRead);
				} while (true);
			}

			public void ExitReadLock ()
			{
				Interlocked.Add (ref rwlock, -RwRead);
			}

			public void EnterWriteLock ()
			{
				SpinWait sw = new SpinWait ();
				do {
					int state = rwlock;
					if (state < RwWrite) {
						if (Interlocked.CompareExchange (ref rwlock, RwWrite, state) == state)
							return;
						state = rwlock;
					}
					// We register our interest in taking the Write lock (if upgradeable it's already done)
					while ((state & RwWait) == 0 && Interlocked.CompareExchange (ref rwlock, state | RwWait, state) != state)
						state = rwlock;
					// Before falling to sleep
					while (rwlock > RwWait)
						sw.SpinOnce ();
				} while (true);
			}

			public void ExitWriteLock ()
			{
				Interlocked.Add (ref rwlock, -RwWrite);
			}
		}
	}
}

#endif
