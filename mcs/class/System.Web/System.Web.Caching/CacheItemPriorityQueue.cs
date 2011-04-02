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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace System.Web.Caching
{
	sealed partial class CacheItemPriorityQueue
	{
		const int INITIAL_HEAP_SIZE = 32;
		const int HEAP_RESIZE_THRESHOLD = 8192;
		
		CacheItem[] heap;
		int heapSize = 0;
		int heapCount = 0;

		// See comment for the cacheLock field at top of System.Web.Caching/Cache.cs
		ReaderWriterLockSlim queueLock;

		public int Count {
			get { return heapCount; }
		}

		public int Size {
			get { return heapSize; }
		}
		
		public CacheItemPriorityQueue ()
		{
			queueLock = new ReaderWriterLockSlim ();
			InitDebugMode ();
		}

		void ResizeHeap (int newSize)
		{
			CacheItem[] oldHeap = heap;
			Array.Resize <CacheItem> (ref heap, newSize);
			heapSize = newSize;
			
			// TODO: The code helps the GC in case the array is pinned. In such instance clearing
			// the old array will release references to the CacheItems stored in there. If the
			// array is not pinned, otoh, this is a waste of time.
			// Currently we don't know if the array is pinned or not so it's safer to always clear it.
			// However when we have more precise stack scanning the code should be
			// revisited.
			if (oldHeap != null) {
				((IList)oldHeap).Clear ();
				oldHeap = null;
			}
		}
		
		CacheItem[] GetHeapWithGrow ()
		{
			if (heap == null) {
				heap = new CacheItem [INITIAL_HEAP_SIZE];
				heapSize = INITIAL_HEAP_SIZE;
				heapCount = 0;
				return heap;
			}

			if (heapCount >= heapSize)
				ResizeHeap (heapSize <<= 1);

			return heap;
		}

		CacheItem[] GetHeapWithShrink ()
		{
			if (heap == null)
				return null;

			if (heapSize > HEAP_RESIZE_THRESHOLD) {
				int halfTheSize = heapSize >> 1;

				if (heapCount < halfTheSize)
					ResizeHeap (halfTheSize + (heapCount / 3));
			}
			
			return heap;
		}
		
		public void Enqueue (CacheItem item)
		{
			if (item == null)
				return;

			CacheItem[] heap;
			
			try {
				queueLock.EnterWriteLock ();
				heap = GetHeapWithGrow ();
				heap [heapCount++] = item;
				BubbleUp (heap);
				
				AddSequenceEntry (item, EDSequenceEntryType.Enqueue);
			} finally {
				// See comment at the top of the file, above queueLock declaration
				queueLock.ExitWriteLock ();
			}
		}

		public CacheItem Dequeue ()
		{
			CacheItem ret = null;
			CacheItem[] heap;
			int index;
			
			try {
				queueLock.EnterWriteLock ();
				heap = GetHeapWithShrink ();
				if (heap == null || heapCount == 0)
					return null;

				ret = heap [0];
				index = --heapCount;
				heap [0] = heap [index];
				heap [index] = null;
				
				if (heapCount > 0)
					BubbleDown (heap);

				AddSequenceEntry (ret, EDSequenceEntryType.Dequeue);
				return ret;
			} finally {
				// See comment at the top of the file, above queueLock declaration
				queueLock.ExitWriteLock ();
			}
		}

		public CacheItem Peek ()
		{
			CacheItem ret;
			
			try {
				queueLock.EnterReadLock ();
				if (heap == null || heapCount == 0)
					return null;

				ret = heap [0];
				AddSequenceEntry (ret, EDSequenceEntryType.Peek);
				
				return ret;
			} finally {
				// See comment at the top of the file, above queueLock declaration
				queueLock.ExitReadLock ();
			}
		}
		
		void BubbleDown (CacheItem[] heap)
		{
			int index = 0;
			int left = 1;
			int right = 2;
			CacheItem item = heap [0];
			int selected = (right < heapCount && heap [right].ExpiresAt < heap [left].ExpiresAt) ? 2 : 1;

			while (selected < heapCount && heap [selected].ExpiresAt < item.ExpiresAt) {
				heap [index] = heap [selected];
				index = selected;
				left = (index << 1) + 1;
				right = left + 1;
				selected = right < heapCount && heap [right].ExpiresAt < heap [left].ExpiresAt ? right : left;
			}
			heap [index] = item;
		}
		
		void BubbleUp (CacheItem[] heap)
		{
			int index, parentIndex;
			CacheItem parent, item;
			
			if (heapCount <= 1)
				return;
			
			index = heapCount - 1;
			parentIndex = (index - 1) >> 1;

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
