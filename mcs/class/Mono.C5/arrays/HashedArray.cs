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
	/// A set collection based on a dynamic  array combined with a hash index
	/// for item to index lookup.
	/// </summary>
	public class HashedArrayList<T>: ArrayList<T>, IList<T>
	{
		#region Fields

		HashSet<KeyValuePair<T,int>> index;

		#endregion

		#region Util

		private void reindex(int start) { reindex(start, underlyingsize); }


		private void reindex(int start, int end)
		{
			for (int j = start; j < end; j++)
				index.Update(new KeyValuePair<T,int>(array[j], j));
		}


		/// <summary>
		/// Internal version of IndexOf without modification checks.
		/// </summary>
		/// <param name="item">Item to look for</param>
		/// <returns>The index of first occurrence</returns>
		protected override int indexOf(T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item);

			if (!index.Find(ref p) || p.value < offset || p.value >= offset + size)
				return -1;

			return p.value - offset;
		}


		/// <summary>
		/// Internal version of LastIndexOf without modification checks.
		/// </summary>
		/// <param name="item">Item to look for</param>
		/// <returns>The index of last occurrence</returns>
		protected override int lastIndexOf(T item) { return indexOf(item); }


		/// <summary>
		/// Internal version of Insert with no modification checks.
		/// <exception cref="ArgumentException"/> if item already in list.
		/// </summary>
		/// <param name="i">Index to insert at</param>
		/// <param name="item">Item to insert</param>
		protected override void insert(int i, T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, offset + i);

			if (index.FindOrAdd(ref p))
				throw new ArgumentException("Item already in indexed list");

			base.insert(i, item);
			reindex(i + 1);
		}


		/// <summary>
		/// Internal version of RemoveAt with no modification checks.
		/// </summary>
		/// <param name="i">Index to remove at</param>
		/// <returns>The removed item</returns>
		protected override T removeAt(int i)
		{
			T val = base.removeAt(i);

			index.Remove(new KeyValuePair<T,int>(val));
			reindex(offset + i);
			return val;
		}



		#endregion

		#region Constructors
		/// <summary>
		/// Create a hashed array list with the natural hasher 
		/// </summary>
		public HashedArrayList() : this(8) { }


		/// <summary>
		/// Create a hashed array list with the natural hasher and specified capacity
		/// </summary>
		/// <param name="cap">The initial capacity</param>
		public HashedArrayList(int cap) : this(cap,HasherBuilder.ByPrototype<T>.Examine()) { }


		/// <summary>
		/// Create a hashed array list with an external hasher
		/// </summary>
		/// <param name="hasher">The external hasher</param>
		public HashedArrayList(IHasher<T> hasher) : this(8,hasher) { }


		/// <summary>
		/// Create a hashed array list with an external hasher and specified capacity
		/// </summary>
		/// <param name="capacity">The initial capacity</param>
		/// <param name="hasher">The external hasher</param>
		public HashedArrayList(int capacity, IHasher<T> hasher) : base(capacity, hasher)
		{
			index = new HashSet<KeyValuePair<T,int>>(new KeyValuePairHasher<T,int>(hasher));
		}

		#endregion

		#region IList<T> Members

		/// <summary>
		/// Insert into this list all items from an enumerable collection starting 
		/// at a particular index.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.
		/// <exception cref="InvalidOperationException"/> if one of the items to insert is
		/// already in the list.
		/// </summary>
		/// <param name="i">Index to start inserting at</param>
		/// <param name="items">Items to insert</param>
		[Tested]
		public override void InsertAll(int i, MSG.IEnumerable<T> items)
		{
			updatecheck();
			if (i < 0 || i > size)
				throw new IndexOutOfRangeException();

			i += offset;

			int j = i, toadd;
			KeyValuePair<T,int> p = new KeyValuePair<T,int>();

			foreach (T item in items)
			{
				p.key = item;
				p.value = j;
				if (!index.FindOrAdd(ref p))	j++;
			}

			toadd = j - i;
			if (toadd + underlyingsize > array.Length)
				expand(toadd + underlyingsize, underlyingsize);

			if (underlyingsize > i)
				Array.Copy(array, i, array, j, underlyingsize - i);

			foreach (T item in items)
			{
				p.key = item;
				index.Find(ref p);
				if (i <= p.value && p.value < i + toadd)
					array[p.value] = item;
			}

			addtosize(toadd);
			reindex(j);
		}

        internal override void InsertAll<U>(int i, MSG.IEnumerable<U> items) //where U:T
        {
            updatecheck();
            if (i < 0 || i > size)
                throw new IndexOutOfRangeException();

            i += offset;

            int j = i, toadd;
            KeyValuePair<T, int> p = new KeyValuePair<T, int>();

            foreach (T item in items)
            {
                p.key = item;
                p.value = j;
                if (!index.FindOrAdd(ref p)) j++;
            }

            toadd = j - i;
            if (toadd + underlyingsize > array.Length)
                expand(toadd + underlyingsize, underlyingsize);

            if (underlyingsize > i)
                Array.Copy(array, i, array, j, underlyingsize - i);

            foreach (T item in items)
            {
                p.key = item;
                index.Find(ref p);
                if (i <= p.value && p.value < i + toadd)
                    array[p.value] = item;
            }

            addtosize(toadd);
            reindex(j);
        }

        /// <summary>
		/// Insert an item right before the first occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found.
		/// <exception cref="InvalidOperationException"/> if the item to insert is
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target before which to insert.</param>
		[Tested]
		public override void InsertBefore(T item, T target)
		{
			updatecheck();

			int ind = indexOf(target);

			if (ind < 0)
				throw new ArgumentException("Target item not found");

			insert(ind, item);
		}


		/// <summary>
		/// Insert an item right after the last(???) occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found.
		/// <exception cref="InvalidOperationException"/> if the item to insert is
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target after which to insert.</param>
		[Tested]
		public override void InsertAfter(T item, T target)
		{
			updatecheck();

			int ind = indexOf(target);

			if (ind < 0)
				throw new ArgumentException("Target item not found");

			insert(ind + 1, item);
		}


		/// <summary>
		/// Create a list view on this list. 
		/// <exception cref="ArgumentOutOfRangeException"/> if the view would not fit into
		/// this list.
		/// </summary>
		/// <param name="start">The index in this list of the start of the view.</param>
		/// <param name="count">The size of the view.</param>
		/// <returns>The new list view.</returns>
		[Tested]
		public override IList<T> View(int start, int count)
		{
			HashedArrayList<T> retval = (HashedArrayList<T>)MemberwiseClone();

			retval.underlying = underlying != null ? underlying : this;
			retval.offset = start;
			retval.size = count;
			return retval;
		}


		/// <summary>
		/// Reverst part of the list so the items are in the opposite sequence order.
		/// <exception cref="ArgumentException"/> if the count is negative.
		/// <exception cref="ArgumentOutOfRangeException"/> if the part does not fit
		/// into the list.
		/// </summary>
		/// <param name="start">The index of the start of the part to reverse.</param>
		/// <param name="count">The size of the part to reverse.</param>
		[Tested]
		public override void Reverse(int start, int count)
		{
			base.Reverse(start, count);
			reindex(offset + start, offset + start + count);
		}


		/// <summary>
		/// Sort the items of the list according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		[Tested]
		public override void Sort(IComparer<T> c)
		{
			base.Sort(c);
			reindex(offset, offset + size);
		}

		/// <summary>
		/// Shuffle the items of this list according to a specific random source.
		/// </summary>
		/// <param name="rnd">The random source.</param>
		public override void Shuffle(Random rnd)
		{
			base.Shuffle(rnd);
			reindex(offset, offset + size);
		}

		#endregion

		#region IIndexed<T> Members


		/// <summary>
		/// Remove all items in an index interval.
		/// <exception cref="IndexOutOfRangeException"/>???. 
		/// </summary>
		/// <param name="start">The index of the first item to remove.</param>
		/// <param name="count">The number of items to remove.</param>
		[Tested]
		public override void RemoveInterval(int start, int count)
		{
			updatecheck();

			KeyValuePair<T,int> p = new KeyValuePair<T,int>();

			for (int i = offset + start, end = offset + start + count; i < end; i++)
			{
				p.key = array[i];
				index.Remove(p);
			}

			base.RemoveInterval(start, count);
			reindex(offset + start);
		}

		#endregion
        
		#region ISequenced<T> Members

		int ISequenced<T>.GetHashCode()
		{ modifycheck(); return sequencedhashcode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ modifycheck(); return sequencedequals(that); }


	#endregion
        
		#region IEditableCollection<T> Members

		/// <summary>
		/// The value is symbolic indicating the type of asymptotic complexity
		/// in terms of the size of this collection (expected).
		/// </summary>
		/// <value>Speed.Constant</value>
		[Tested]
		public override Speed ContainsSpeed { [Tested]get { return Speed.Constant; } }


		/// <summary>
		/// Check if this collection contains (an item equivalent to according to the
		/// itemhasher) a particular value.
		/// </summary>
		/// <param name="item">The value to check for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public override bool Contains(T item)
		{ modifycheck(); return indexOf(item) >= 0; }


		/// <summary>
		/// Remove the first copy of a particular item from this collection. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public override bool Remove(T item)
		{
			updatecheck();

			int ind = indexOf(item);

			if (ind < 0)
				return false;

			removeAt(ind);
			return true;
		}


		/// <summary>
		/// Remove all items in another collection from this one, taking multiplicities into account.
		/// Matching items will be removed from the front. Current implementation is not optimal.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public override void RemoveAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			KeyValuePair<T,int> p = new KeyValuePair<T,int>();

			foreach (T item in items)
			{
				p.key = item;
				if (index.Find(ref p) && offset <= p.value && p.value < offset + size)
					index.Remove(p);
			}

			int removed = underlyingsize - index.Count, j = offset;

			for (int i = offset; i < underlyingsize; i++)
			{
				p.key = array[i];
				p.value = j;
				if (index.Update(p)) { array[j++] = p.key; }
			}

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
				index.Clear();
				base.Clear();
			}
			else
			{
				for (int i = offset, end = offset + size; i < end; i++)
					index.Remove(new KeyValuePair<T,int>(array[i]));

				base.Clear();
				reindex(offset);
			}
		}


		/// <summary>
		/// Remove all items not in some other collection from this one.
		/// </summary>
		/// <param name="items">The items to retain.</param>
		[Tested]
		public override void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			HashSet<T> toretain = new HashSet<T>(itemhasher);
			KeyValuePair<T,int> p = new KeyValuePair<T,int>();

			foreach (T item in items)
			{
				p.key = item;
				if (index.Find(ref p) && offset <= p.value && p.value < offset + size)
					toretain.Add(item);
			}

			int removed = size - toretain.Count, j = offset;

			for (int i = offset; i < offset + size; i++)
			{
				p.key = array[i];
				p.value = j;
				if (toretain.Contains(p.key))
				{
					index.Update(p);
					array[j++] = p.key;
				}
				else
				{
					index.Remove(p);
				}
			}

			Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
			addtosize(-removed);
			reindex(j);
			Array.Clear(array, underlyingsize, removed);
		}


		/// <summary>
		/// Check if this collection contains all the values in another collection.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		[Tested]
		public override bool ContainsAll(MSG.IEnumerable<T> items)
		{
			modifycheck();
			foreach (T item in items)
				if (indexOf(item) < 0)
					return false;

			return true;
		}


		/// <summary>
		/// Count the number of items of the collection equal to a particular value.
		/// Returns 0 if and only if the value is not in the collection.
		/// </summary>
		/// <param name="item">The value to count.</param>
		/// <returns>The number of copies found (0 or 1).</returns>
		[Tested]
		public override int ContainsCount(T item)
		{ modifycheck(); return indexOf(item) >= 0 ? 1 : 0; }


		/// <summary>
		/// Remove all items equal to a given one.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		[Tested]
		public override void RemoveAllCopies(T item) { Remove(item); }


		/// <summary>
		/// Check the integrity of the internal data structures of this array list.
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		[Tested]
		public override bool Check()
		{
			if (!base.Check())
				return false;

			bool retval = true;

			if (underlyingsize != index.Count)
			{
				Console.WriteLine("size ({0})!= index.Count ({1})", size, index.Count);
				retval = false;
			}

			for (int i = 0; i < underlyingsize; i++)
			{
				KeyValuePair<T,int> p = new KeyValuePair<T,int>(array[i]);

				if (!index.Find(ref p))
				{
					Console.WriteLine("Item {1} at {0} not in hashindex", i, array[i]);
					retval = false;
				}

				if (p.value != i)
				{
					Console.WriteLine("Item {1} at {0} has hashindex {2}", i, array[i], p.value);
					retval = false;
				}
			}

			return retval;
		}


		int ICollection<T>.GetHashCode() { return unsequencedhashcode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return unsequencedequals(that); }

		#endregion

		#region ISink<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>False, indicating hashed array list has set semantics.</value>
		[Tested]
		public override bool AllowsDuplicates { [Tested]get { return false; } }


		/// <summary>
		/// Add an item to end of this list if not already in list.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True if item was added</returns>
		[Tested]
		public override bool Add(T item)
		{
			updatecheck();

			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, size);

			if (index.FindOrAdd(ref p))
				return false;

			base.insert(size, item);
			reindex(size);
			return true;
		}

		#endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }

		#endregion
	}
}
#endif
