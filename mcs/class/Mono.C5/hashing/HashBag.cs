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
	/// A bag collection based on a hash table of (item,count) pairs. 
	/// </summary>
	public class HashBag<T>: CollectionBase<T>, ICollection<T>
	{
		#region Fields
		HashSet<KeyValuePair<T,int>> dict;
		#endregion

		#region Constructors
		/// <summary>
		/// Create a hash bag with the deafult item hasher.
		/// </summary>
		public HashBag()
		{
			itemhasher = HasherBuilder.ByPrototype<T>.Examine();
			dict = new HashSet<KeyValuePair<T,int>>();
		}


		/// <summary>
		/// Create a hash bag with an external item hasher.
		/// </summary>
		/// <param name="h">The external hasher.</param>
		public HashBag(IHasher<T> h)
		{
			itemhasher = h;
			dict = new HashSet<KeyValuePair<T,int>>(new KeyValuePairHasher<T,int>(h));
		}
		#endregion

		#region IEditableCollection<T> Members

		/// <summary>
		/// The complexity of the Contains operation
		/// </summary>
		/// <value>Always returns Speed.Constant</value>
		[Tested]
		public virtual Speed ContainsSpeed { [Tested]get { return Speed.Constant; } }


		[Tested]
		int ICollection<T>.GetHashCode() { return unsequencedhashcode(); }


		[Tested]
		bool ICollection<T>.Equals(ICollection<T> that)
		{ return unsequencedequals(that); }


		/// <summary>
		/// Check if an item is in the bag 
		/// </summary>
		/// <param name="item">The item to look for</param>
		/// <returns>True if bag contains item</returns>
		[Tested]
		public virtual bool Contains(T item)
		{ return dict.Contains(new KeyValuePair<T,int>(item, 0)); }


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the bag and
		/// if so report the actual item object found.
		/// </summary>
		/// <param name="item">On entry, the item to look for.
		/// On exit the item found, if any</param>
		/// <returns>True if bag contains item</returns>
		[Tested]
		public virtual bool Find(ref T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			if (dict.Find(ref p))
			{
				item = p.key;
				return true;
			}

			return false;
		}


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the bag and
		/// if so replace the item object in the bag with the supplied one.
		/// </summary>
		/// <param name="item">The item object to update with</param>
		/// <returns>True if item was found (and updated)</returns>
		[Tested]
		public virtual bool Update(T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			updatecheck();

			//Note: we cannot just do dict.Update. There is of course a way
			//around if we use the implementation of hashset -which we do not want to do.
			//The hashbag is moreover mainly a proof of concept
			if (dict.Find(ref p))
			{
				p.key = item;
				dict.Update(p);
				return true;
			}

			return false;
		}


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the bag.
		/// If found, report the actual item object in the bag,
		/// else add the supplied one.
		/// </summary>
		/// <param name="item">On entry, the item to look for or add.
		/// On exit the actual object found, if any.</param>
		/// <returns>True if item was found</returns>
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
		/// Check if an item (collection equal to a supplied one) is in the bag and
		/// if so replace the item object in the set with the supplied one; else
		/// add the supplied one.
		/// </summary>
		/// <param name="item">The item to look for and update or add</param>
		/// <returns>True if item was updated</returns>
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
		/// Remove one copy af an item from the bag
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>True if item was (found and) removed </returns>
		[Tested]
		public virtual bool Remove(T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			updatecheck();
			if (dict.Find(ref p))
			{
				size--;
				if (p.value == 1)
					dict.Remove(p);
				else
				{
					p.value--;
					dict.Update(p);
				}

				return true;
			}

			return false;
		}


		/// <summary>
		/// Remove one copy of an item from the bag, reporting the actual matching item object.
		/// </summary>
		/// <param name="item">On entry the item to remove.
		/// On exit, the actual removed item object.</param>
		/// <returns>True if item was found.</returns>
		[Tested]
		public virtual bool RemoveWithReturn(ref T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			updatecheck();
			if (dict.Find(ref p))
			{
				item = p.key;
				size--;
				if (p.value == 1)
					dict.Remove(p);
				else
				{
					p.value--;
					dict.Update(p);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Remove all items in a supplied collection from this bag, counting multiplicities.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public virtual void RemoveAll(MSG.IEnumerable<T> items)
		{
			updatecheck();
			foreach (T item in items)
				Remove(item);
		}


		/// <summary>
		/// Remove all items from the bag, resetting internal table to initial size.
		/// </summary>
		[Tested]
		public virtual void Clear()
		{
			updatecheck();
			dict.Clear();
			size = 0;
		}


		/// <summary>
		/// Remove all items *not* in a supplied collection from this bag,
		/// counting multiplicities.
		/// </summary>
		/// <param name="items">The items to retain</param>
		[Tested]
		public virtual void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			HashBag<T> res = new HashBag<T>(itemhasher);

			foreach (T item in items)
				if (res.ContainsCount(item) < ContainsCount(item))
					res.Add(item);

			dict = res.dict;
			size = res.size;
		}


		/// <summary>
		/// Check if all items in a supplied collection is in this bag
		/// (counting multiplicities). 
		/// </summary>
		/// <param name="items">The items to look for.</param>
		/// <returns>True if all items are found.</returns>
		[Tested]
		public virtual bool ContainsAll(MSG.IEnumerable<T> items)
		{
			HashBag<T> res = new HashBag<T>(itemhasher);

			foreach (T item in items)
				if (res.ContainsCount(item) < ContainsCount(item))
					res.Add(item);
				else
					return false;

			return true;
		}


		/// <summary>
		/// Create an array containing all items in this bag (in enumeration order).
		/// </summary>
		/// <returns>The array</returns>
		[Tested]
		public override T[] ToArray()
		{
			T[] res = new T[size];
			int ind = 0;

			foreach (KeyValuePair<T,int> p in dict)
				for (int i = 0; i < p.value; i++)
					res[ind++] = p.key;

			return res;
		}


		/// <summary>
		/// Count the number of times an item is in this set.
		/// </summary>
		/// <param name="item">The item to look for.</param>
		/// <returns>The count</returns>
		[Tested]
		public virtual int ContainsCount(T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			if (dict.Find(ref p))
				return p.value;

			return 0;
		}


		/// <summary>
		/// Remove all copies of item from this set.
		/// </summary>
		/// <param name="item">The item to remove</param>
		[Tested]
		public virtual void RemoveAllCopies(T item)
		{
			updatecheck();

			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 0);

			if (dict.Find(ref p))
			{
				size -= p.value;
				dict.Remove(p);
			}
		}

		#endregion

		#region ICollection<T> Members


		/// <summary>
		/// Copy the items of this bag to part of an array.
		/// <exception cref="ArgumentOutOfRangeException"/> if i is negative.
		/// <exception cref="ArgumentException"/> if the array does not have room for the items.
		/// </summary>
		/// <param name="a">The array to copy to</param>
		/// <param name="i">The starting index.</param>
		[Tested]
		public override void CopyTo(T[] a, int i)
		{
			if (i < 0)
				throw new ArgumentOutOfRangeException();

			if (i + size > a.Length)
				throw new ArgumentException();

			foreach (KeyValuePair<T,int> p in dict)
				for (int j = 0; j < p.value; j++)
					a[i++] = p.key;
		}

		#endregion

		#region ISink<T> Members

		/// <summary>
		/// Report if this is a set collection.
		/// </summary>
		/// <value>Always true</value>
		[Tested]
		public virtual bool AllowsDuplicates { [Tested] get { return true; } }


		/// <summary>
		/// Add an item to this bag.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>Always true</returns>
		[Tested]
		public virtual bool Add(T item)
		{
			KeyValuePair<T,int> p = new KeyValuePair<T,int>(item, 1);

			updatecheck();
			if (dict.Find(ref p))
			{
				p.value++;
				dict.Update(p);
			}
			else
				dict.Add(p);

			size++;
			return true;
		}


		/// <summary>
		/// Add all items of a collection to this set.
		/// </summary>
		/// <param name="items">The items to add</param>
		[Tested]
        public virtual void AddAll(MSG.IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. 
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public virtual void AddAll<U>(MSG.IEnumerable<U> items) where U : T
        {
            foreach (T item in items)
                Add(item);
        }

        #endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Create an enumerator for this bag.
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<T> GetEnumerator()
		{
			int left;
			int mystamp = stamp;

			foreach (KeyValuePair<T,int> p in dict)
			{
				left = p.value;
				while (left > 0)
				{
					if (mystamp != stamp)
						throw new InvalidOperationException("Collection was changed");

					left--;
					yield return p.key;
				}
			}
		}
		#endregion

		#region Diagnostics
		/// <summary>
		/// Test internal structure of data (invariants)
		/// </summary>
		/// <returns>True if pass</returns>
		[Tested]
		public virtual bool Check()
		{
			bool retval = dict.Check();
			int count = 0;

			foreach (KeyValuePair<T,int> p in dict)
				count += p.value;

			if (count != size)
			{
				Console.WriteLine("count({0}) != size({1})", count, size);
				retval = false;
			}

			return retval;
		}
		#endregion
	}
}
