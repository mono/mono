/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

namespace Mono.Lucene.Net.Util
{
	
	/// <summary>A PriorityQueue maintains a partial ordering of its elements such that the
	/// least element can always be found in constant time.  Put()'s and pop()'s
	/// require log(size) time.
	/// 
	/// <p/><b>NOTE</b>: This class pre-allocates a full array of
	/// length <code>maxSize+1</code>, in {@link #initialize}.
	/// 
	/// </summary>
	public abstract class PriorityQueue
	{
		private int size;
		private int maxSize;
		protected internal System.Object[] heap;
		
		/// <summary>Determines the ordering of objects in this priority queue.  Subclasses
		/// must define this one method. 
		/// </summary>
		public abstract bool LessThan(System.Object a, System.Object b);
		
		/// <summary> This method can be overridden by extending classes to return a sentinel
		/// object which will be used by {@link #Initialize(int)} to fill the queue, so
		/// that the code which uses that queue can always assume it's full and only
		/// change the top without attempting to insert any new object.<br/>
		/// 
		/// Those sentinel values should always compare worse than any non-sentinel
		/// value (i.e., {@link #LessThan(Object, Object)} should always favor the
		/// non-sentinel values).<br/>
		/// 
		/// By default, this method returns false, which means the queue will not be
		/// filled with sentinel values. Otherwise, the value returned will be used to
		/// pre-populate the queue. Adds sentinel values to the queue.<br/>
		/// 
		/// If this method is extended to return a non-null value, then the following
		/// usage pattern is recommended:
		/// 
		/// <pre>
		/// // extends getSentinelObject() to return a non-null value.
		/// PriorityQueue pq = new MyQueue(numHits);
		/// // save the 'top' element, which is guaranteed to not be null.
		/// MyObject pqTop = (MyObject) pq.top();
		/// &lt;...&gt;
		/// // now in order to add a new element, which is 'better' than top (after 
		/// // you've verified it is better), it is as simple as:
		/// pqTop.change().
		/// pqTop = pq.updateTop();
		/// </pre>
		/// 
		/// <b>NOTE:</b> if this method returns a non-null value, it will be called by
		/// {@link #Initialize(int)} {@link #Size()} times, relying on a new object to
		/// be returned and will not check if it's null again. Therefore you should
		/// ensure any call to this method creates a new instance and behaves
		/// consistently, e.g., it cannot return null if it previously returned
		/// non-null.
		/// 
		/// </summary>
		/// <returns> the sentinel object to use to pre-populate the queue, or null if
		/// sentinel objects are not supported.
		/// </returns>
		protected internal virtual System.Object GetSentinelObject()
		{
			return null;
		}
		
		/// <summary>Subclass constructors must call this. </summary>
		protected internal void  Initialize(int maxSize)
		{
			size = 0;
			int heapSize;
            if (0 == maxSize)
                // We allocate 1 extra to avoid if statement in top()
                heapSize = 2;
            else
            {
                if (maxSize == Int32.MaxValue)
                {
                    // Don't wrap heapSize to -1, in this case, which
                    // causes a confusing NegativeArraySizeException.
                    // Note that very likely this will simply then hit
                    // an OOME, but at least that's more indicative to
                    // caller that this values is too big.  We don't +1
                    // in this case, but it's very unlikely in practice
                    // one will actually insert this many objects into
                    // the PQ:
                    heapSize = Int32.MaxValue;
                }
                else
                {
                    // NOTE: we add +1 because all access to heap is
                    // 1-based not 0-based.  heap[0] is unused.
                    heapSize = maxSize + 1;
                }
            }
			heap = new System.Object[heapSize];
			this.maxSize = maxSize;
			
			// If sentinel objects are supported, populate the queue with them
			System.Object sentinel = GetSentinelObject();
			if (sentinel != null)
			{
				heap[1] = sentinel;
				for (int i = 2; i < heap.Length; i++)
				{
					heap[i] = GetSentinelObject();
				}
				size = maxSize;
			}
		}
		
		/// <summary> Adds an Object to a PriorityQueue in log(size) time. If one tries to add
		/// more objects than maxSize from initialize a RuntimeException
		/// (ArrayIndexOutOfBound) is thrown.
		/// 
		/// </summary>
		/// <deprecated> use {@link #Add(Object)} which returns the new top object,
		/// saving an additional call to {@link #Top()}.
		/// </deprecated>
        [Obsolete("use Add(Object) which returns the new top object, saving an additional call to Top().")]
		public void  Put(System.Object element)
		{
			size++;
			heap[size] = element;
			UpHeap();
		}
		
		/// <summary> Adds an Object to a PriorityQueue in log(size) time. If one tries to add
		/// more objects than maxSize from initialize an
		/// {@link ArrayIndexOutOfBoundsException} is thrown.
		/// 
		/// </summary>
		/// <returns> the new 'top' element in the queue.
		/// </returns>
		public System.Object Add(System.Object element)
		{
			size++;
			heap[size] = element;
			UpHeap();
			return heap[1];
		}
		
		/// <summary> Adds element to the PriorityQueue in log(size) time if either the
		/// PriorityQueue is not full, or not lessThan(element, top()).
		/// 
		/// </summary>
		/// <param name="element">
		/// </param>
		/// <returns> true if element is added, false otherwise.
		/// </returns>
		/// <deprecated> use {@link #InsertWithOverflow(Object)} instead, which
		/// encourages objects reuse.
		/// </deprecated>
        [Obsolete("use InsertWithOverflow(Object) instead, which encourages objects reuse.")]
		public virtual bool Insert(System.Object element)
		{
			return InsertWithOverflow(element) != element;
		}
		
		/// <summary> insertWithOverflow() is the same as insert() except its
		/// return value: it returns the object (if any) that was
		/// dropped off the heap because it was full. This can be
		/// the given parameter (in case it is smaller than the
		/// full heap's minimum, and couldn't be added), or another
		/// object that was previously the smallest value in the
		/// heap and now has been replaced by a larger one, or null
		/// if the queue wasn't yet full with maxSize elements.
		/// </summary>
		public virtual System.Object InsertWithOverflow(System.Object element)
		{
			if (size < maxSize)
			{
				Put(element);
				return null;
			}
			else if (size > 0 && !LessThan(element, heap[1]))
			{
				System.Object ret = heap[1];
				heap[1] = element;
				AdjustTop();
				return ret;
			}
			else
			{
				return element;
			}
		}
		
		/// <summary>Returns the least element of the PriorityQueue in constant time. </summary>
		public System.Object Top()
		{
			// We don't need to check size here: if maxSize is 0,
			// then heap is length 2 array with both entries null.
			// If size is 0 then heap[1] is already null.
			return heap[1];
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
		
		/// <summary> Should be called when the Object at top changes values. Still log(n) worst
		/// case, but it's at least twice as fast to
		/// 
		/// <pre>
		/// pq.top().change();
		/// pq.adjustTop();
		/// </pre>
		/// 
		/// instead of
		/// 
		/// <pre>
		/// o = pq.pop();
		/// o.change();
		/// pq.push(o);
		/// </pre>
		/// 
		/// </summary>
		/// <deprecated> use {@link #UpdateTop()} which returns the new top element and
		/// saves an additional call to {@link #Top()}.
		/// </deprecated>
        [Obsolete("use UpdateTop() which returns the new top element and saves an additional call to Top()")]
		public void  AdjustTop()
		{
			DownHeap();
		}
		
		/// <summary> Should be called when the Object at top changes values. Still log(n) worst
		/// case, but it's at least twice as fast to
		/// 
		/// <pre>
		/// pq.top().change();
		/// pq.updateTop();
		/// </pre>
		/// 
		/// instead of
		/// 
		/// <pre>
		/// o = pq.pop();
		/// o.change();
		/// pq.push(o);
		/// </pre>
		/// 
		/// </summary>
		/// <returns> the new 'top' element.
		/// </returns>
		public System.Object UpdateTop()
		{
			DownHeap();
			return heap[1];
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
			{
				heap[i] = null;
			}
			size = 0;
		}
		
		private void  UpHeap()
		{
			int i = size;
			System.Object node = heap[i]; // save bottom node
			int j = SupportClass.Number.URShift(i, 1);
			while (j > 0 && LessThan(node, heap[j]))
			{
				heap[i] = heap[j]; // shift parents down
				i = j;
				j = SupportClass.Number.URShift(j, 1);
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
