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
	/// A list collection based on a plain dynamic array data structure.
	/// Expansion of the internal array is performed by doubling on demand. 
	/// The internal array is only shrinked by the Clear method. 
	///
	/// <p>When the FIFO property is set to false this class works fine as a stack of T.
	/// When the FIFO property is set to true the class will function as a (FIFO) queue
	/// but very inefficiently, use a LinkedList (<see cref="T:C5.LinkedList!1"/>) instead.</p>
	/// </summary>
	public class ArrayList<T>: ArrayBase<T>, IList<T>
	{
		#region Fields

		/// <summary>
		/// The underlying list if we are a view, null else.
		/// </summary>
		protected ArrayList<T> underlying;

		/// <summary>
		/// The size of the underlying list.
		/// </summary>
		protected int underlyingsize;

		/// <summary>
		/// The underlying field of the FIFO property
		/// </summary>
		protected bool fIFO = true;

		#endregion

		#region Util

		bool equals(T i1, T i2) { return itemhasher.Equals(i1, i2); }


		/// <summary>
		/// Double the size of the internal array.
		/// </summary>
		protected override void expand()
		{ expand(2 * array.Length, underlyingsize); }


		/// <summary>
		/// Expand the internal array, resetting the index of the first unused element.
		/// </summary>
		/// <param name="newcapacity">The new capacity (will be rouded upwards to a power of 2).</param>
		/// <param name="newsize">The new count of </param>
		protected override void expand(int newcapacity, int newsize)
		{
			base.expand(newcapacity, newsize);
			if (underlying != null)
				underlying.array = array;
		}


		/// <summary>
		/// Check if it is valid to perform updates and increment stamp.
		/// <exception cref="InvalidOperationException"/> if check fails.
		/// </summary>
		protected override void updatecheck()
		{
			modifycheck();
			base.updatecheck();
			if (underlying != null)
				underlying.stamp++;
		}


		/// <summary>
		/// Check if we are a view that the underlying list has only been updated through us.
		/// <exception cref="InvalidOperationException"/> if check fails.
		/// <br/>
		/// This method should be called from enumerators etc to guard against 
		/// modification of the base collection.
		/// </summary>
		protected void modifycheck()
		{
			if (underlying != null && stamp != underlying.stamp)
				throw new InvalidOperationException("underlying list was modified");
		}


		/// <summary>
		/// Check that the list has not been updated since a particular time.
		/// <exception cref="InvalidOperationException"/> if check fails.
		/// </summary>
		/// <param name="stamp">The stamp indicating the time.</param>
		protected override void modifycheck(int stamp)
		{
			modifycheck();
			if (this.stamp != stamp)
				throw new InvalidOperationException("Collection was modified");
		}


		/// <summary>
		/// Increment or decrement the private size fields
		/// </summary>
		/// <param name="delta">Increment (with sign)</param>
		protected void addtosize(int delta)
		{
			size += delta;
			underlyingsize += delta;
			if (underlying != null)
			{
				underlying.size += delta;
				underlying.underlyingsize += delta;
			}
		}


		/// <summary>
		/// Internal version of IndexOf without modification checks.
		/// </summary>
		/// <param name="item">Item to look for</param>
		/// <returns>The index of first occurrence</returns>
		protected virtual int indexOf(T item)
		{
			for (int i = 0; i < size; i++)
				if (equals(item, array[offset + i]))
					return i;

			return -1;
		}


		/// <summary>
		/// Internal version of LastIndexOf without modification checks.
		/// </summary>
		/// <param name="item">Item to look for</param>
		/// <returns>The index of last occurrence</returns>
		protected virtual int lastIndexOf(T item)
		{
			for (int i = size - 1; i >= 0; i--)
				if (equals(item, array[offset + i]))
					return i;

			return -1;
		}


		/// <summary>
		/// Internal version of Insert with no modification checks.
		/// </summary>
		/// <param name="i">Index to insert at</param>
		/// <param name="item">Item to insert</param>
		protected override void insert(int i, T item)
		{
			if (underlyingsize == array.Length)
				expand();

			i += offset;
			if (i < underlyingsize)
				Array.Copy(array, i, array, i + 1, underlyingsize - i);

			array[i] = item;
			addtosize(1);
		}


		/// <summary>
		/// Internal version of RemoveAt with no modification checks.
		/// </summary>
		/// <param name="i">Index to remove at</param>
		/// <returns>The removed item</returns>
		protected virtual T removeAt(int i)
		{
			i += offset;

			T retval = array[i];

			addtosize(-1);
			if (underlyingsize > i)
				Array.Copy(array, i + 1, array, i, underlyingsize - i);

			array[underlyingsize] = default(T);
			return retval;
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Create an array list with default item hasher and initial capacity 8 items.
		/// </summary>
		public ArrayList() : this(8) { }


		/// <summary>
		/// Create an array list with external item hasher and initial capacity 8 items.
		/// </summary>
		/// <param name="hasher">The external hasher</param>
		public ArrayList(IHasher<T> hasher) : this(8,hasher) { }


		/// <summary>
		/// Create an array list with default item hasher and prescribed initial capacity.
		/// </summary>
		/// <param name="capacity">The prescribed capacity</param>
		public ArrayList(int capacity) : this(capacity,HasherBuilder.ByPrototype<T>.Examine()) { }


		/// <summary>
		/// Create an array list with external item hasher and prescribed initial capacity.
		/// </summary>
		/// <param name="capacity">The prescribed capacity</param>
		/// <param name="hasher">The external hasher</param>
		public ArrayList(int capacity, IHasher<T> hasher) : base(capacity,hasher) { }

		#endregion

        #region IList<T> Members

		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <value>The first item in this list.</value>
		[Tested]
		public virtual T First
		{
			[Tested]get
			{
				modifycheck();
				if (size == 0)
					throw new InvalidOperationException("List is empty");

				return array[offset];
			}
		}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <value>The last item in this list.</value>
		[Tested]
		public virtual T Last
		{
			[Tested]get
			{
				modifycheck();
				if (size == 0)
					throw new InvalidOperationException("List is empty");

				return array[offset + size - 1];
			}
		}


		/// <summary>
		/// Since <code>Add(T item)</code> always add at the end of the list,
		/// this describes if list has FIFO or LIFO semantics.
		/// </summary>
		/// <value>True if the <code>Remove()</code> operation removes from the
		/// start of the list, false if it removes from the end.</value>
		[Tested]
		public virtual bool FIFO
		{
			[Tested]
			get { modifycheck(); return fIFO; }
			[Tested]
			set { updatecheck(); fIFO = value; }
		}


		/// <summary>
		/// On this list, this indexer is read/write.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <value>The i'th item of this list.</value>
		/// <param name="i">The index of the item to fetch or store.</param>
		[Tested]
		public virtual T this[int i]
		{
			[Tested]
			get
			{
				modifycheck();
				if (i < 0 || i >= size)
					throw new IndexOutOfRangeException();

				return array[offset + i];
			}
			[Tested]
			set
			{
				updatecheck();
				if (i < 0 || i >= size)
					throw new IndexOutOfRangeException();

				array[offset + i] = value;
			}
		}


		/// <summary>
		/// Insert an item at a specific index location in this list. 
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.</summary>
		/// <param name="i">The index at which to insert.</param>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public virtual void Insert(int i, T item)
		{
			updatecheck();
			if (i < 0 || i > size)
				throw new IndexOutOfRangeException();

			insert(i, item);
		}


		/// <summary>
		/// Insert into this list all items from an enumerable collection starting 
		/// at a particular index.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.
		/// </summary>
		/// <param name="i">Index to start inserting at</param>
		/// <param name="items">Items to insert</param>
		[Tested]
		public virtual void InsertAll(int i, MSG.IEnumerable<T> items)
		{
			updatecheck();
			if (i < 0 || i > size)
				throw new IndexOutOfRangeException();

			int toadd = EnumerableBase<T>.countItems(items);

			if (toadd + underlyingsize > array.Length)
				expand(toadd + underlyingsize, underlyingsize);

			i += offset;
			if (underlyingsize > i)
				Array.Copy(array, i, array, i + toadd, underlyingsize - i);

			foreach (T item in items)
				array[i++] = item;

			addtosize(toadd);
		}

        internal virtual void InsertAll<U>(int i, MSG.IEnumerable<U> items) where U:T
        {
            updatecheck();
            if (i < 0 || i > size)
                throw new IndexOutOfRangeException();

            int toadd = EnumerableBase<U>.countItems(items);

            if (toadd + underlyingsize > array.Length)
                expand(toadd + underlyingsize, underlyingsize);

            i += offset;
            if (underlyingsize > i)
                Array.Copy(array, i, array, i + toadd, underlyingsize - i);

            foreach (T item in items)
                array[i++] = item;

            addtosize(toadd);
        }

        /// <summary>
		/// Insert an item right before the first occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target before which to insert.</param>
		[Tested]
		public virtual void InsertBefore(T item, T target)
		{
			updatecheck();

			int i = indexOf(target);

			if (i == -1)
				throw new ArgumentException("Target item not found");

			insert(i, item);
		}


		/// <summary>
		/// Insert an item right after the last(???) occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target after which to insert.</param>
		[Tested]
		public virtual void InsertAfter(T item, T target)
		{
			updatecheck();

			int i = lastIndexOf(target);

			if (i == -1)
				throw new ArgumentException("Target item not found");

			insert(i + 1, item);
		}


		/// <summary>
		/// Insert an item at the front of this list;
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public virtual void InsertFirst(T item)
		{
			updatecheck();
			insert(0, item);
		}


		/// <summary>
		/// Insert an item at the back of this list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public virtual void InsertLast(T item)
		{
			updatecheck();
			insert(size, item);
		}


		/// <summary>
		/// Create a new list consisting of the items of this list satisfying a 
		/// certain predicate.
		/// </summary>
		/// <param name="filter">The filter delegate defining the predicate.</param>
		/// <returns>The new list.</returns>
		[Tested]
		public virtual IList<T> FindAll(Filter<T> filter)
		{
			modifycheck();

			ArrayList<T> res = new ArrayList<T>(itemhasher);
			int j = 0, rescap = res.array.Length;

			for (int i = 0; i < size; i++)
			{
				T a = array[offset + i];

				if (filter(a))
				{
					if (j == rescap) res.expand(rescap = 2 * rescap, j);

					res.array[j++] = a;
				}
			}

			res.underlyingsize = res.size = j;
			return res;
		}


        /// <summary>
        /// Create a new list consisting of the results of mapping all items of this
        /// list. The new list will use the default hasher for the item type V.
        /// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The delegate defining the map.</param>
        /// <returns>The new list.</returns>
        [Tested]
        public virtual IList<V> Map<V>(Mapper<T, V> mapper)
        {
            modifycheck();

            ArrayList<V> res = new ArrayList<V>(size);

            return map<V>(mapper, res);
        }

        /// <summary>
        /// Create a new list consisting of the results of mapping all items of this
        /// list. The new list will use a specified hasher for the item type.
        /// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The delegate defining the map.</param>
        /// <param name="hasher">The hasher to use for the new list</param>
        /// <returns>The new list.</returns>
        public virtual IList<V> Map<V>(Mapper<T, V> mapper, IHasher<V> hasher)
        {
            modifycheck();

            ArrayList<V> res = new ArrayList<V>(size,hasher);

            return map<V>(mapper, res);
        }

        private IList<V> map<V>(Mapper<T, V> mapper, ArrayList<V> res)
        {
            if (size > 0)
                for (int i = 0; i < size; i++)
                    res.array[i] = mapper(array[offset + i]);

            res.underlyingsize = res.size = size;
            return res;
        }

/// <summary>
        /// Remove one item from the list: from the front if <code>FIFO</code>
		/// is true, else from the back.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T Remove() { return fIFO ? RemoveFirst() : RemoveLast(); }


		/// <summary>
		/// Remove one item from the fromnt of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T RemoveFirst()
		{
			updatecheck();
			if (size == 0)
				throw new InvalidOperationException("List is empty");

			return removeAt(0);
		}


		/// <summary>
		/// Remove one item from the back of the list.
		/// </summary>
		/// <exception cref="InvalidOperationException"> if this list is empty.</exception>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T RemoveLast()
		{
			updatecheck();
			if (size == 0)
				throw new InvalidOperationException("List is empty");

			return removeAt(size - 1);
		}


		/// <summary>
		/// Create a list view on this list. 
        /// <exception cref="ArgumentOutOfRangeException"/> if the start or count is negative
        /// <exception cref="ArgumentException"/> if the range does not fit within list.
        /// </summary>
        /// <param name="start">The index in this list of the start of the view.</param>
		/// <param name="count">The size of the view.</param>
		/// <returns>The new list view.</returns>
		[Tested]
		public virtual IList<T> View(int start, int count)
		{
            modifycheck();
            checkRange(start, count);
            
            ArrayList<T> retval = (ArrayList<T>)MemberwiseClone();

            retval.underlying = underlying != null ? underlying : this;
			retval.offset = start;
			retval.size = count;
			return retval;
		}


		/// <summary>
		/// Null if this list is not a view.
		/// </summary>
        /// <value>Underlying list for view.</value>
        [Tested]
		public IList<T> Underlying { [Tested]get { return underlying; } }


		/// <summary>
		/// </summary>
		/// <value>Offset for this list view or 0 for an underlying list.</value>
		[Tested]
		public int Offset { [Tested]get { return offset; } }


		/// <summary>
		/// Slide this list view along the underlying list.
		/// <exception cref="InvalidOperationException"/> if this list is not a view.
		/// <exception cref="ArgumentOutOfRangeException"/> if the operation
		/// would bring either end of the view outside the underlying list.
		/// </summary>
		/// <param name="offset">The signed amount to slide: positive to slide
		/// towards the end.</param>
		[Tested]
		public virtual void Slide(int offset)
		{
			modifycheck();
			if (underlying == null)
				throw new InvalidOperationException("Not a view");

			int newoffset = this.offset + offset;

			if (newoffset < 0 || newoffset + size > underlyingsize)
				throw new ArgumentOutOfRangeException();

			this.offset = newoffset;
		}


		/// <summary>
		/// Slide this list view along the underlying list, changing its size.
		/// <exception cref="InvalidOperationException"/> if this list is not a view.
		/// <exception cref="ArgumentOutOfRangeException"/> if the operation
		/// would bring either end of the view outside the underlying list.
		/// </summary>
		/// <param name="offset">The signed amount to slide: positive to slide
		/// towards the end.</param>
		/// <param name="size">The new size of the view.</param>
		[Tested]
		public virtual void Slide(int offset, int size)
		{
			modifycheck();
			if (underlying == null)
				throw new InvalidOperationException("Not a view");

			int newoffset = this.offset + offset;
			int newsize = size;

			if (newoffset < 0 || newsize < 0 || newoffset + newsize > underlyingsize)
				throw new ArgumentOutOfRangeException();

			this.offset = newoffset;
			this.size = newsize;
		}


		/// <summary>
		/// Reverst the list so the items are in the opposite sequence order.
		/// </summary>
		[Tested]
		public virtual void Reverse() { Reverse(0, size); }


		/// <summary>
		/// Reverst part of the list so the items are in the opposite sequence order.
		/// <exception cref="ArgumentException"/> if the count is negative.
		/// <exception cref="ArgumentOutOfRangeException"/> if the part does not fit
		/// into the list.
		/// </summary>
		/// <param name="start">The index of the start of the part to reverse.</param>
		/// <param name="count">The size of the part to reverse.</param>
		[Tested]
		public virtual void Reverse(int start, int count)
		{
			updatecheck();
			checkRange(start, count);
			start += offset;
			for (int i = 0, length = count / 2, end = start + count - 1; i < length; i++)
			{
				T swap = array[start + i];

				array[start + i] = array[end - i];
				array[end - i] = swap;
			}
		}


		/// <summary>
		/// Check if this list is sorted according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		/// <returns>True if the list is sorted, else false.</returns>
		[Tested]
		public virtual bool IsSorted(IComparer<T> c)
		{
			modifycheck();
			for (int i = offset + 1, end = offset + size; i < end; i++)
				if (c.Compare(array[i - 1], array[i]) > 0)
					return false;

			return true;
		}


		/// <summary>
		/// Sort the items of the list according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		[Tested]
		public virtual void Sort(IComparer<T> c)
		{
			updatecheck();
			Sorting.IntroSort<T>(array, offset, offset + size, c);
		}


		/// <summary>
		/// Randonmly shuffle the items of this list. 
		/// </summary>
		public virtual void Shuffle() { Shuffle(new C5Random()); }


		/// <summary>
		/// Shuffle the items of this list according to a specific random source.
		/// </summary>
		/// <param name="rnd">The random source.</param>
		public virtual void Shuffle(Random rnd)
		{
			updatecheck();
			for (int i = offset, top = offset + size, end = top - 1; i < end; i++)
			{
				int j = rnd.Next(i, top);

				if (j != i)
				{
					T tmp = array[i];

					array[i] = array[j];
					array[j] = tmp;
				}
			}
		}

		#endregion

		#region IIndexed<T> Members

		/// <summary>
		/// Search for an item in the list going forwrds from the start.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of item from start.</returns>
		[Tested]
		public virtual int IndexOf(T item) { modifycheck(); return indexOf(item); }


		/// <summary>
		/// Search for an item in the list going backwords from the end.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of of item from the end.</returns>
		[Tested]
		public virtual int LastIndexOf(T item) { modifycheck(); return lastIndexOf(item); }


		/// <summary>
		/// Remove the item at a specific position of the list.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <param name="i">The index of the item to remove.</param>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T RemoveAt(int i)
		{
			updatecheck();
			if (i < 0 || i >= size)
				throw new IndexOutOfRangeException("Index out of range for sequenced collection");

			return removeAt(i);
		}


		/// <summary>
		/// Remove all items in an index interval.
		/// <exception cref="IndexOutOfRangeException"/>???. 
		/// </summary>
		/// <param name="start">The index of the first item to remove.</param>
		/// <param name="count">The number of items to remove.</param>
		[Tested]
		public virtual void RemoveInterval(int start, int count)
		{
			updatecheck();
			checkRange(start, count);
			start += offset;
			Array.Copy(array, start + count, array, start, underlyingsize - start - count);
			addtosize(-count);
			Array.Clear(array, underlyingsize, count);
		}

		#endregion

		#region ISequenced<T> Members


		int ISequenced<T>.GetHashCode()
		{ modifycheck(); return sequencedhashcode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ modifycheck(); return sequencedequals(that); }

		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// The value is symbolic indicating the type of asymptotic complexity
		/// in terms of the size of this collection (worst-case or amortized as
		/// relevant).
		/// </summary>
		/// <value>Speed.Linear</value>
		[Tested]
		public virtual Speed ContainsSpeed { [Tested]get { return Speed.Linear; } }


		int ICollection<T>.GetHashCode()
		{ modifycheck(); return unsequencedhashcode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ modifycheck(); return unsequencedequals(that); }


		/// <summary>
		/// Check if this collection contains (an item equivalent to according to the
		/// itemhasher) a particular value.
		/// </summary>
		/// <param name="item">The value to check for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public virtual bool Contains(T item)
		{ modifycheck(); return indexOf(item) >= 0; }


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public virtual bool Find(ref T item)
		{
			modifycheck();

			int i;

			if ((i = indexOf(item)) >= 0)
			{
				item = array[offset + i];
				return true;
			}

			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. This will only update the first 
		/// mathching item.
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		[Tested]
		public virtual bool Update(T item)
		{
			updatecheck();

			int i;

			if ((i = indexOf(item)) >= 0)
			{
				array[offset + i] = item;
				return true;
			}

			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found. Else, add the item to the collection.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the item was found (hence not added).</returns>
		[Tested]
		public virtual bool FindOrAdd(ref T item)
		{
			updatecheck();
			if (Find(ref item))
				return true;

			Add(item);
			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. This will only update the first 
		/// mathching item.
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		[Tested]
		public virtual bool UpdateOrAdd(T item)
		{
			updatecheck();
			if (Update(item))
				return true;

			Add(item);
			return false;
		}


		/// <summary>
		/// Remove the first copy of a particular item from this collection. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public virtual bool Remove(T item)
		{
			updatecheck();

			int i = indexOf(item);

			if (i < 0)
				return false;

			removeAt(i);
			return true;
		}


		/// <summary>
		/// Remove the first copy of a particular item from this collection if found.
		/// If an item was removed, report a binary copy of the actual item removed in 
		/// the argument.
		/// </summary>
		/// <param name="item">The value to remove on input.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public virtual bool RemoveWithReturn(ref T item)
		{
			updatecheck();

			int i = indexOf(item);

			if (i < 0)
				return false;

			item = removeAt(i);
			return true;
		}


		/// <summary>
		/// Remove all items in another collection from this one, taking multiplicities into account.
		/// Matching items will be removed from the front. Current implementation is not optimal.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public virtual void RemoveAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			int[] toremove = new int[(size >>5) + 1];

			foreach (T item in items)
				for (int i = 0; i < size; i++)
					if ((toremove[i >>5] & (1 << (i & 31))) == 0 && equals(array[offset + i], item))
					{
						toremove[i >>5] |= 1 << (i & 31);
						break;
					}

			int j = offset;

			for (int i = 0; i < size; i++)
				if ((toremove[i >>5] & (1 << (i & 31))) == 0)
					array[j++] = array[offset + i];

			int removed = offset + size - j;

			Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
			addtosize(-removed);
			Array.Clear(array, underlyingsize, removed);
		}


		/// <summary>
		/// Remove all items from this collection, resetting internal array size.
		/// </summary>
		[Tested]
		public override void Clear()
		{
			updatecheck();
			if (underlying == null)
			{
				array = new T[8];
				underlyingsize = size = 0;
			}
			else
			{
				underlying.RemoveInterval(offset, size);
				size = 0;
				underlyingsize = underlying.size;
				stamp = underlying.stamp;
			}
		}


		/// <summary>
		/// Remove all items not in some other collection from this one, taking multiplicities into account.
		/// Items are retained front first.  Current implementation is not optimal.
		/// </summary>
		/// <param name="items">The items to retain.</param>
		[Tested]
		public virtual void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			int[] toretain = new int[(size >>5) + 1];

			foreach (T item in items)
				for (int i = 0; i < size; i++)
					if ((toretain[i >>5] & (1 << (i & 31))) == 0 && equals(array[offset + i], item))
					{
						toretain[i >>5] |= 1 << (i & 31);
						break;
					}

			int j = offset;

			for (int i = 0; i < size; i++)
				if ((toretain[i >>5] & (1 << (i & 31))) != 0)
					array[j++] = array[i + offset];

			int removed = offset + size - j;

			Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
			addtosize(-removed);
			Array.Clear(array, underlyingsize, removed);
		}


		/// <summary>
		/// Check if this collection contains all the values in another collection,
		/// taking multiplicities into account.
		/// Current implementation is not optimal.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		[Tested]
		public virtual bool ContainsAll(MSG.IEnumerable<T> items)
		{
			modifycheck();

			int[] matched = new int[(size >>5) + 1];

			foreach (T item in items)
			{
				for (int i = 0; i < size; i++)
					if ((matched[i >>5] & (1 << (i & 31))) == 0 && equals(array[i + offset], item))
					{
						matched[i >>5] |= 1 << (i & 31);
						goto next;
					}

				return false;
			next :
				;
			}

			return true;
		}


		/// <summary>
		/// Count the number of items of the collection equal to a particular value.
		/// Returns 0 if and only if the value is not in the collection.
		/// </summary>
		/// <param name="item">The value to count.</param>
		/// <returns>The number of copies found.</returns>
		[Tested]
		public virtual int ContainsCount(T item)
		{
			modifycheck();

			int count = 0;

			for (int i = 0; i < size; i++)
				if (equals(item, array[offset + i]))
					count++;

			return count;
		}


		/// <summary>
		/// Remove all items equal to a given one.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		[Tested]
		public virtual void RemoveAllCopies(T item)
		{
			updatecheck();

			int j = offset;

			for (int i = offset, end = offset + size; i < end; i++)
				if (!equals(item, array[i]))
					array[j++] = array[i];

			int removed = offset + size - j;

			Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
			addtosize(-removed);
			Array.Clear(array, underlyingsize, removed);
		}


		/// <summary>
		/// Check the integrity of the internal data structures of this array list.
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		[Tested]
		public override bool Check()
		{
			bool retval = true;

			if (underlyingsize > array.Length)
			{
				Console.WriteLine("underlyingsize ({0}) > array.Length ({1})", size, array.Length);
				return false;
			}

			if (offset + size > underlyingsize)
			{
				Console.WriteLine("offset({0})+size({1}) > underlyingsize ({2})", offset, size, underlyingsize);
				return false;
			}

			if (offset < 0)
			{
				Console.WriteLine("offset({0}) < 0", offset);
				return false;
			}

			for (int i = 0; i < underlyingsize; i++)
			{
				if ((object)(array[i]) == null)
				{
					Console.WriteLine("Bad element: null at (base)index {0}", i);
					retval = false;
				}
			}

			for (int i = underlyingsize, length = array.Length; i < length; i++)
			{
				if (!equals(array[i], default(T)))
				{
					Console.WriteLine("Bad element: != default(T) at (base)index {0}", i);
					retval = false;
				}
			}

			return retval;
		}

		#endregion

		#region IExtensible<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>True, indicating array list has bag semantics.</value>
		[Tested]
		public virtual bool AllowsDuplicates { [Tested]get { return true; } }


		/// <summary>
		/// Add an item to end of this list.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True</returns>
		[Tested]
		public virtual bool Add(T item)
		{
			updatecheck();
			insert(size, item);
			return true;
		}


		/// <summary>
		/// Add the elements from another collection to this collection. 
		/// </summary>
		/// <param name="items">The items to add.</param>
		[Tested]
		public virtual void AddAll(MSG.IEnumerable<T> items) { InsertAll(size, items); }

        /*public virtual*/ void C5.IExtensible<T>.AddAll<U>(MSG.IEnumerable<U> items) //where U : T 
        {
            InsertAll(size, items);
        }

		#endregion

		#region IDirectedEnumerable<T> Members

		/// <summary>
		/// Create a collection containing the same items as this collection, but
		/// whose enumerator will enumerate the items backwards. The new collection
		/// will become invalid if the original is modified. Method typicaly used as in
		/// <code>foreach (T x in coll.Backwards()) {...}</code>
		/// </summary>
		/// <returns>The backwards collection.</returns>
		[Tested]
		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }

		#endregion

        #region IStack<T> Members

        /// <summary>
        /// Push an item to the top of the stack.
        /// </summary>
        /// <param name="item">The item</param>
        [Tested]
        public void Push(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Pop the item at the top of the stack from the stack.
        /// </summary>
        /// <returns>The popped item.</returns>
        [Tested]
        public T Pop()
        {
            return RemoveLast();
        }

        #endregion

        #region IQueue<T> Members

        /// <summary>
        /// Enqueue an item at the back of the queue. 
        /// </summary>
        /// <param name="item">The item</param>
        [Tested]
        public void EnQueue(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Dequeue an item from the front of the queue.
        /// </summary>
        /// <returns>The item</returns>
        [Tested]
        public T DeQueue()
        {
            return RemoveFirst();
        }

        #endregion
    }
}
#endif
