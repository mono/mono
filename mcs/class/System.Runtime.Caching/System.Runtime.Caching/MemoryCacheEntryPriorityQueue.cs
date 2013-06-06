//
// Author(s):
//  Marek Habersack <mhabersack@novell.com>
//
// (C) 2009-2010 Novell, Inc (http://novell.com)
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace System.Runtime.Caching
{
	sealed partial class MemoryCacheEntryPriorityQueue
	{
		const int INITIAL_HEAP_SIZE = 32;
		const int HEAP_RESIZE_THRESHOLD = 8192;
		
		MemoryCacheEntry[] heap;
		int heapSize = 0;
		int heapCount = 0;
		ReaderWriterLockSlim queueLock;

		public int Count {
			get { return heapCount; }
		}

		public int Size {
			get { return heapSize; }
		}
		
		public MemoryCacheEntryPriorityQueue ()
		{
			queueLock = new ReaderWriterLockSlim ();
		}

		MemoryCacheEntry[] GetHeapWithGrow ()
		{
			if (heap == null) {
				heap = new MemoryCacheEntry [INITIAL_HEAP_SIZE];
				heapSize = INITIAL_HEAP_SIZE;
				heapCount = 0;
				return heap;
			}

			if (heapCount >= heapSize) {
				checked {
					heapSize <<= 1;
				}

				if (heapSize <= Int32.MaxValue)
					Array.Resize <MemoryCacheEntry> (ref heap, heapSize);
			}

			return heap;
		}

		MemoryCacheEntry[] GetHeapWithShrink ()
		{
			if (heap == null)
				return null;

			if (heapSize > HEAP_RESIZE_THRESHOLD) {
				int halfTheSize, newSize;
				checked {
					halfTheSize = heapSize >> 1;
					newSize = halfTheSize + (heapCount / 3);
				}

				if ((heapCount < halfTheSize) && newSize > -1)
					Array.Resize <MemoryCacheEntry> (ref heap, newSize);
			}
			
			return heap;
		}
		
		public void Enqueue (MemoryCacheEntry item)
		{
			if (item == null)
				return;

			bool locked = false;
			MemoryCacheEntry[] heap;
			
			try {
				queueLock.EnterWriteLock ();
				locked = true;
				heap = GetHeapWithGrow ();
				heap [checked(heapCount++)] = item;
				BubbleUp (heap);
			} finally {
				if (locked)
					queueLock.ExitWriteLock ();
			}
		}

		public MemoryCacheEntry Dequeue ()
		{
			MemoryCacheEntry ret = null;
			MemoryCacheEntry[] heap;
			bool locked = false;
			int index;
			
			try {
				queueLock.EnterWriteLock ();
				locked = true;
				heap = GetHeapWithShrink ();
				if (heap == null || heapCount == 0)
					return null;

				ret = heap [0];
				index = checked(--heapCount);
				heap [0] = heap [index];
				heap [index] = null;
				
				if (heapCount > 0)
					BubbleDown (heap);

				return ret;
			} finally {
				if (locked)
					queueLock.ExitWriteLock ();
			}
		}

		public MemoryCacheEntry Peek ()
		{
			bool locked = false;
			
			try {
				queueLock.EnterReadLock ();
				locked = true;
				if (heap == null || heapCount == 0)
					return null;

				return heap [0];
			} finally {
				if (locked)
					queueLock.ExitReadLock ();
			}
		}
		
		void BubbleDown (MemoryCacheEntry[] heap)
		{
			int index = 0;
			int left = 1;
			int right = 2;
			MemoryCacheEntry item = heap [0];
			int selected = (right < heapCount && heap [right].ExpiresAt < heap [left].ExpiresAt) ? 2 : 1;

			while (selected < heapCount && heap [selected].ExpiresAt < item.ExpiresAt) {
				heap [index] = heap [selected];
				index = selected;
				left = checked((index << 1) + 1);
				right = left + 1;
				selected = right < heapCount && heap [right].ExpiresAt < heap [left].ExpiresAt ? right : left;
			}
			heap [index] = item;
		}
		
		void BubbleUp (MemoryCacheEntry[] heap)
		{
			int index, parentIndex;
			MemoryCacheEntry parent, item;
			
			if (heapCount <= 1)
				return;
			
			index = checked(heapCount - 1);
			parentIndex = checked((index - 1) >> 1);

			item = heap [index];
			while (index > 0) {
				parent = heap [parentIndex];
				if (heap [index].ExpiresAt >= parent.ExpiresAt)
					break;
				
				heap [index] = parent;
				index = parentIndex;
				parentIndex = (index - 1) >> 1;
			}

			heap [index] = item;
		}
	}
}
