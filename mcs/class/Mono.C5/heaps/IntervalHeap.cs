#if NET_2_0
/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using System;
using System.Diagnostics;
using MSG = System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A priority queue class based on an interval heap data structure.
	/// </summary>
	public class IntervalHeap<T>: CollectionValueBase<T>, IPriorityQueue<T>
	{
		#region Fields
		struct Interval
		{
			internal T first, last;


			public override string ToString() { return String.Format("[{0}; {1}]", first, last); }
		}



		object syncroot = new object();

        int stamp;

        IComparer<T> comparer;

		Interval[] heap;

		int size;
		#endregion

		#region Util
		void heapifyMin(int i)
		{
			int j = i, minpt = j;
			T pv = heap[j].first, min = pv;

			while (true)
			{
				int l = 2 * j + 1, r = l + 1;
				T lv, rv, other;

				if (2 * l < size && comparer.Compare(lv = heap[l].first, min) < 0) { minpt = l; min = lv; }

                if (2 * r < size && comparer.Compare(rv = heap[r].first, min) < 0) { minpt = r; min = rv; }

                if (minpt == j)
					break;

				other = heap[minpt].last;
				heap[j].first = min;
                if (2 * minpt + 1 < size && comparer.Compare(pv, other) > 0)
                { heap[minpt].last = pv; pv = other; }

				min = pv;
				j = minpt;
			}

			if (minpt != i)
				heap[minpt].first = min;
		}


		void heapifyMax(int i)
		{
			int j = i, maxpt = j;
			T pv = heap[j].last, max = pv;

			while (true)
			{
				int l = 2 * j + 1, r = l + 1;
				T lv, rv, other;

                if (2 * l + 1 < size && comparer.Compare(lv = heap[l].last, max) > 0) { maxpt = l; max = lv; }

                if (2 * r + 1 < size && comparer.Compare(rv = heap[r].last, max) > 0) { maxpt = r; max = rv; }

                if (maxpt == j)
					break;

				other = heap[maxpt].first;
				heap[j].last = max;
                if (comparer.Compare(pv, other) < 0)
                {
					heap[maxpt].first = pv;
					pv = other;
				}

				max = pv;
				j = maxpt;
			}

			if (maxpt != i)
				heap[maxpt].last = max;
		}


		void bubbleUpMin(int i)
		{
			if (i > 0)
			{
				T min = heap[i].first, iv = min;
				int p = (i + 1) / 2 - 1;

				while (i > 0)
				{
                    if (comparer.Compare(iv, min = heap[p = (i + 1) / 2 - 1].first) < 0)
                    {
						heap[i].first = min; min = iv;
						i = p;
					}
					else
						break;
				}

				heap[i].first = iv;
			}
		}


		void bubbleUpMax(int i)
		{
			if (i > 0)
			{
				T max = heap[i].last, iv = max;
				int p = (i + 1) / 2 - 1;

				while (i > 0)
				{
                    if (comparer.Compare(iv, max = heap[p = (i + 1) / 2 - 1].last) > 0)
                    {
						heap[i].last = max; max = iv;
						i = p;
					}
					else
						break;
				}

				heap[i].last = iv;
			}
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Create an interval heap with natural item comparer and default initial capacity (16)
		/// </summary>
		public IntervalHeap() : this(16) { }


		/// <summary>
		/// Create an interval heap with external item comparer and default initial capacity (16)
		/// </summary>
		/// <param name="c">The external comparer</param>
		public IntervalHeap(IComparer<T> c) : this(c,16) { }


		/// <summary>
		/// Create an interval heap with natural item comparer and prescribed initial capacity
		/// </summary>
		/// <param name="capacity">The initial capacity</param>
		public IntervalHeap(int capacity)  : this(ComparerBuilder.FromComparable<T>.Examine(),capacity) { }


		/// <summary>
		/// Create an interval heap with external item comparer and prescribed initial capacity
		/// </summary>
		/// <param name="c">The external comparer</param>
		/// <param name="capacity">The initial capacity</param>
		public IntervalHeap(IComparer<T> c, int capacity)
		{
			comparer = c;

			int length = 1;

			while (length < capacity) length <<= 1;

			heap = new Interval[length];
		}
		#endregion

		#region IPriorityQueue<T> Members

		/// <summary>
		/// Find the current least item of this priority queue.
		/// <exception cref="InvalidOperationException"/> if queue is empty
		/// </summary>
		/// <returns>The least item.</returns>
		[Tested]
		public T FindMin()
		{
			if (size == 0)
				throw new InvalidOperationException("Heap is empty");

			return heap[0].first;
		}


		/// <summary>
		/// Remove the least item from this  priority queue.
		/// <exception cref="InvalidOperationException"/> if queue is empty
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public T DeleteMin()
		{
            stamp++;
            if (size == 0)
                throw new InvalidOperationException("Heap is empty");

			T retval = heap[0].first;;
			if (size == 1)
			{
				size = 0;
				heap[0].first = default(T);
			}
			else
			{
				int ind = (size - 1) / 2;

				if (size % 2 == 0)
				{
					heap[0].first = heap[ind].last;
					heap[ind].last = default(T);
				}
				else
				{
					heap[0].first = heap[ind].first;
					heap[ind].first = default(T);
				}

				size--;
				heapifyMin(0);
			}

			return retval;
		}


		/// <summary>
		/// Find the current largest item of this priority queue.
		/// <exception cref="InvalidOperationException"/> if queue is empty
		/// </summary>
		/// <returns>The largest item.</returns>
		[Tested]
		public T FindMax()
		{
			if (size == 0)
				throw new InvalidOperationException("Heap is empty");
			else if (size == 1)
				return heap[0].first;
			else
				return heap[0].last;
		}


		/// <summary>
		/// Remove the largest item from this  priority queue.
		/// <exception cref="InvalidOperationException"/> if queue is empty
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public T DeleteMax()
		{
            stamp++;
            if (size == 0)
                throw new InvalidOperationException("Heap is empty");

			T retval;

			if (size == 1)
			{
				size = 0;
				retval = heap[0].first;
				heap[0].first = default(T);
				return retval;
			}
			else
			{
				retval = heap[0].last;

				int ind = (size - 1) / 2;

				if (size % 2 == 0)
				{
					heap[0].last = heap[ind].last;
					heap[ind].last = default(T);
				}
				else
				{
					heap[0].last = heap[ind].first;
					heap[ind].first = default(T);
				}

				size--;
				heapifyMax(0);
				return retval;
			}
		}


        /// <summary>
        /// The comparer object supplied at creation time for this collection
        /// </summary>
        /// <value>The comparer</value>
        public IComparer<T> Comparer { get { return comparer; } }

		#endregion

		#region ISink<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>True since this collection has bag semantics</value>
		[Tested]
		public bool AllowsDuplicates { [Tested]get { return true; } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>The distinguished object to use for locking to synchronize multithreaded access</value>
		[Tested]
		public object SyncRoot { [Tested]get { return syncroot; } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>True if this collection is empty.</value>
		[Tested]
		public bool IsEmpty { [Tested]get { return size == 0; } }


		/// <summary>
		/// Add an item to this priority queue.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True</returns>
		[Tested]
		public bool Add(T item)
		{
            stamp++;
            if (size == 0)
			{
				size = 1;
				heap[0].first = item;
				return true;
			}

			if (size == 2 * heap.Length)
			{
				Interval[] newheap = new Interval[2 * heap.Length];

				Array.Copy(heap, newheap, heap.Length);
				heap = newheap;
			}

			if (size % 2 == 0)
			{
				int i = size / 2, p = (i + 1) / 2 - 1;
				T tmp;

				size++;
                if (comparer.Compare(item, tmp = heap[p].last) > 0)
                {
					heap[i].first = tmp;
					heap[p].last = item;
					bubbleUpMax(p);
				}
				else
				{
					heap[i].first = item;
                    if (comparer.Compare(item, heap[p].first) < 0)
                        bubbleUpMin(i);
				}
			}
			else
			{
				int i = size / 2;
				T other = heap[i].first;

				size++;
                if (comparer.Compare(item, other) < 0)
                {
					heap[i].first = item;
					heap[i].last = other;
					bubbleUpMin(i);
				}
				else
				{
					heap[i].last = item;
					bubbleUpMax(i);
				}
			}

			return true;
		}


		/// <summary>
		/// Add the elements from another collection to this collection. 
		/// </summary>
		/// <param name="items">The items to add.</param>
		[Tested]
		public void AddAll(MSG.IEnumerable<T> items)
		{
            //TODO: avoid incrementing stamp repeatedly
			foreach (T item in items)
				Add(item);
		}

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. 
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public void AddAll<U>(MSG.IEnumerable<U> items) where U : T
        {
            //TODO: avoid incrementing stamp repeatedly
            foreach (T item in items)
                Add(item);
        }

		#endregion

        #region ICollection<T> members
        /// <summary>
        /// 
        /// </summary>
        /// <value>The size of this collection</value>
        [Tested]
        public override int Count { [Tested]get { return size; } }


        /// <summary>
        /// The value is symbolic indicating the type of asymptotic complexity
        /// in terms of the size of this collection (worst-case or amortized as
        /// relevant).
        /// </summary>
        /// <value>A characterization of the speed of the 
        /// <code>Count</code> property in this collection.</value>
        public override Speed CountSpeed { get { return Speed.Constant; } }

        /// <summary>
        /// Create an enumerator for the collection
        /// <para>Note: the enumerator does *not* enumerate the items in sorted order, 
        /// but in the internal table order.</para>
        /// </summary>
        /// <returns>The enumerator(SIC)</returns>
        [Tested]
        public override MSG.IEnumerator<T> GetEnumerator()
        {
            int mystamp = stamp;
            for (int i = 0; i < size; i++)
            {
                if (mystamp != stamp) throw new InvalidOperationException();
                yield return i % 2 == 0 ? heap[i >> 1].first : heap[i >> 1].last;
            }
            yield break;
        }


        #endregion


		#region Diagnostics
		private bool check(int i, T min, T max)
		{
			bool retval = true;
			Interval interval = heap[i];
			T first = interval.first, last = interval.last;

			if (2 * i + 1 == size)
			{
                if (comparer.Compare(min, first) > 0)
                {
					Console.WriteLine("Cell {0}: parent.first({1}) > first({2})  [size={3}]", i, min, first, size);
					retval = false;
				}

                if (comparer.Compare(first, max) > 0)
                {
					Console.WriteLine("Cell {0}: first({1}) > parent.last({2})  [size={3}]", i, first, max, size);
					retval = false;
				}

				return retval;
			}
			else
			{
                if (comparer.Compare(min, first) > 0)
                {
					Console.WriteLine("Cell {0}: parent.first({1}) > first({2})  [size={3}]", i, min, first, size);
					retval = false;
				}

                if (comparer.Compare(first, last) > 0)
                {
					Console.WriteLine("Cell {0}: first({1}) > last({2})  [size={3}]", i, first, last, size);
					retval = false;
				}

                if (comparer.Compare(last, max) > 0)
                {
					Console.WriteLine("Cell {0}: last({1}) > parent.last({2})  [size={3}]", i, last, max, size);
					retval = false;
				}

				int l = 2 * i + 1, r = l + 1;

				if (2 * l < size)
					retval = retval && check(l, first, last);

				if (2 * r < size)
					retval = retval && check(r, first, last);
			}

			return retval;
		}


		/// <summary>
		/// Check the integrity of the internal data structures of this collection.
		/// Only avaliable in DEBUG builds???
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		[Tested]
		public bool Check()
		{
			if (size == 0)
				return true;

			if (size == 1)
				return (object)(heap[0].first) != null;

			return check(0, heap[0].first, heap[0].last);
		}

		#endregion
	}
}
#endif
