/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
namespace Monodoc.Lucene.Net.Util
{
	/// <summary>A PriorityQueue maintains a partial ordering of its elements such that the
	/// least element can always be found in constant time.  Put()'s and pop()'s
	/// require log(size) time. 
	/// </summary>
	public abstract class PriorityQueue
	{
		private System.Object[] heap;
		private int size;
		private int maxSize;
		
		/// <summary>Determines the ordering of objects in this priority queue.  Subclasses
		/// must define this one method. 
		/// </summary>
		public abstract bool LessThan(System.Object a, System.Object b);
		
		/// <summary>Subclass constructors must call this. </summary>
		protected internal void  Initialize(int maxSize)
		{
			size = 0;
			int heapSize = maxSize + 1;
			heap = new System.Object[heapSize];
			this.maxSize = maxSize;
		}
		
		/// <summary> Adds an Object to a PriorityQueue in log(size) time.
		/// If one tries to add more objects than maxSize from initialize
		/// a RuntimeException (ArrayIndexOutOfBound) is thrown.
		/// </summary>
		public void  Put(System.Object element)
		{
			size++;
			heap[size] = element;
			UpHeap();
		}
		
		/// <summary> Adds element to the PriorityQueue in log(size) time if either
		/// the PriorityQueue is not full, or not lessThan(element, top()).
		/// </summary>
		/// <param name="">element
		/// </param>
		/// <returns> true if element is added, false otherwise.
		/// </returns>
		public virtual bool Insert(System.Object element)
		{
			if (size < maxSize)
			{
				Put(element);
				return true;
			}
			else if (size > 0 && !LessThan(element, Top()))
			{
				heap[1] = element;
				AdjustTop();
				return true;
			}
			else
				return false;
		}
		
		/// <summary>Returns the least element of the PriorityQueue in constant time. </summary>
		public System.Object Top()
		{
			if (size > 0)
				return heap[1];
			else
				return null;
		}
		
		/// <summary>Removes and returns the least element of the PriorityQueue in log(size)
		/// time. 
		/// </summary>
		public System.Object Pop()
		{
			if (size > 0)
			{
				System.Object result = heap[1]; // save first value
				heap[1] = heap[size]; // move last to first
				heap[size] = null; // permit GC of objects
				size--;
				DownHeap(); // adjust heap
				return result;
			}
			else
				return null;
		}
		
		/// <summary>Should be called when the Object at top changes values.  Still log(n)
		/// worst case, but it's at least twice as fast to <pre>
		/// { pq.top().change(); pq.adjustTop(); }
		/// </pre> instead of <pre>
		/// { o = pq.pop(); o.change(); pq.push(o); }
		/// </pre>
		/// </summary>
		public void  AdjustTop()
		{
			DownHeap();
		}
		
		
		/// <summary>Returns the number of elements currently stored in the PriorityQueue. </summary>
		public int Size()
		{
			return size;
		}
		
		/// <summary>Removes all entries from the PriorityQueue. </summary>
		public void  Clear()
		{
			for (int i = 0; i <= size; i++)
				heap[i] = null;
			size = 0;
		}
		
		private void  UpHeap()
		{
			int i = size;
			System.Object node = heap[i]; // save bottom node
            int j = (int) (((uint) i) >> 1);
			while (j > 0 && LessThan(node, heap[j]))
			{
				heap[i] = heap[j]; // shift parents down
				i = j;
                j = (int) (((uint) j) >> 1);
			}
			heap[i] = node; // install saved node
		}
		
		private void  DownHeap()
		{
			int i = 1;
			System.Object node = heap[i]; // save top node
			int j = i << 1; // find smaller child
			int k = j + 1;
			if (k <= size && LessThan(heap[k], heap[j]))
			{
				j = k;
			}
			while (j <= size && LessThan(heap[j], node))
			{
				heap[i] = heap[j]; // shift up child
				i = j;
				j = i << 1;
				k = j + 1;
				if (k <= size && LessThan(heap[k], heap[j]))
				{
					j = k;
				}
			}
			heap[i] = node; // install saved node
		}
	}
}