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

#define LISTORDERnot
#define EXTLISTORDER
using System;
using System.Diagnostics;
using MSG=System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A list collection based on a doubly linked list data structure with 
	/// a hash index mapping item to node.
	/// </summary>
	public class HashedLinkedList<T>: LinkedList<T>, IList<T>
	{
		#region Fields

		HashDictionary<T,Node> dict;

		//Invariant:  base.underlying == basehashedlist
		HashedLinkedList<T> hashedunderlying;

		#endregion

		#region Constructors

		void init()
		{
#if LISTORDER || EXTLISTORDER
			maintaintags = true;
#endif
			dict = new HashDictionary<T,Node>(itemhasher);
		}
		

		/// <summary>
		/// Create a hashed linked list with an external item hasher.
		/// </summary>
		/// <param name="itemhasher">The external hasher</param>
		public HashedLinkedList(IHasher<T> itemhasher) : base(itemhasher) { init(); }


		/// <summary>
		/// Create a hashed linked list with the natural item hasher.
		/// </summary>
		public HashedLinkedList() : base() { init(); }

		#endregion

		#region Util

		bool contains(T item, out Node node)
		{
			if (!dict.Find(item,out node))
				return false;
			else
				return insideview(node);
		}

		
		void insert(Node succ, T item)
		{
			Node newnode = new Node(item);

			if (dict.FindOrAdd(item, ref newnode))
				throw new ArgumentException("Item already in indexed list");

			insertNode(succ, newnode);
		}


		private bool dictremove(T item, out Node node)
		{
			if (hashedunderlying == null)
			{
				if (!dict.Remove(item, out node))
					return false;
			}
			else
			{
				//We cannot avoid calling dict twice - have to intersperse the listorder test!
				if (!contains(item, out node))
					return false;

				dict.Remove(item);
			}

			return true;
		}


		bool insideview(Node node)
		{
			if (underlying == null)
				return true;

#if LISTORDER
			if (maintaintags)
				return (startsentinel.tag < node.tag && (endsentinel.tag == 0 || node.tag < endsentinel.tag));
			else
#elif EXTLISTORDER
			if (maintaintags)
				return (startsentinel.precedes(node) && node.precedes(endsentinel));
			else
#endif
			if (2 * size < hashedunderlying.size)
			{
				Node cursor = startsentinel.next;

				while (cursor != endsentinel)
				{
					if (cursor == node)
						return true;

					cursor = cursor.next;
				}

				return false;
			}
			else
			{
				Node cursor = hashedunderlying.startsentinel.next;

				while (cursor != startsentinel.next)
				{
					if (cursor == node)
						return false;

					cursor = cursor.next;
				}

				cursor = endsentinel;
				while (cursor != hashedunderlying.endsentinel)
				{
					if (cursor == node)
						return false;

					cursor = cursor.next;
				}

				return true;
			}
		}


		#endregion

		#region IList<T> Members

		/// <summary>
		/// On this list, this indexer is read/write.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <value>The i'th item of this list.</value>
		/// <param name="i">The index of the item to fetch or store.</param>
		[Tested]
		public override T this[int i]
		{
			[Tested]
			get
			{
				modifycheck();
				return base[i];
			}
			[Tested]
			set
			{
				updatecheck();

				Node n = get(i);

				if (itemhasher.Equals(value, n.item))
				{
					n.item = value;
					dict.Update(value, n);
				}
				else if (!dict.FindOrAdd(value, ref n))
				{
					dict.Remove(n.item);
					n.item = value;
				}
				else
					throw new ArgumentException("Item already in indexed list");
			}
		}


		/// <summary>
		/// Insert an item at a specific index location in this list. 
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.</summary>
		/// <exception cref="InvalidOperationException"/> if  the item is 
		/// already in the list.
		/// <param name="i">The index at which to insert.</param>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public override void Insert(int i, T item)
		{
			updatecheck();
			insert(i == size ? endsentinel : get(i), item);
		}


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

			Node succ, node;
			int count = 0;

			succ = i == size ? endsentinel : get(i);
			node = succ.prev;
#if LISTORDER
			//TODO: guard?
			int taglimit = i == size ? int.MaxValue : succ.tag - 1, thetag = node.tag;
#elif EXTLISTORDER
			TagGroup taggroup = null;
			int taglimit = 0, thetag = 0;

			if (maintaintags)
				taggroup = gettaggroup(node, succ, out thetag, out taglimit);
#endif
			foreach (T item in items)
			{
				Node tmp = new Node(item, node, null);

				if (!dict.FindOrAdd(item, ref tmp))
				{
#if LISTORDER
					if (maintaintags)
						tmp.tag = thetag < taglimit ? ++thetag : thetag;
#elif EXTLISTORDER
					if (maintaintags)
					{
						tmp.tag = thetag < taglimit ? ++thetag : thetag;
						tmp.taggroup = taggroup;
					}
#endif
					node.next = tmp;
					count++;
					node = tmp;
				}
				else
					throw new ArgumentException("Item already in indexed list");
			}

#if EXTLISTORDER
			if (maintaintags)
			{
				taggroup.count += count;
				taggroup.first = succ.prev;
				taggroup.last = node;
			}
#endif	
			succ.prev = node;
			node.next = succ;
			size += count;
			if (hashedunderlying != null)
				hashedunderlying.size += count;
#if LISTORDER
			if (maintaintags && node.tag == node.prev.tag)
				settag(node);
#elif EXTLISTORDER
			if (maintaintags)
			{
				if (node.tag == node.prev.tag)
					splittaggroup(taggroup);
			}
#endif
		}


		/// <summary>
		/// Insert an item right before the first occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found
		/// <exception cref="InvalidOperationException"/> if the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target before which to insert.</param>
		[Tested]
		public override void InsertBefore(T item, T target)
		{
			updatecheck();

			Node node;

			if (!contains(target, out node))
				throw new ArgumentException("Target item not found");

			insert(node, item);
		}


		/// <summary>
		/// Insert an item right after the last(???) occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found
		/// <exception cref="InvalidOperationException"/> if the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target after which to insert.</param>
		[Tested]
		public override void InsertAfter(T item, T target)
		{
			updatecheck();

			Node node;

			if (!contains(target, out node))
				throw new ArgumentException("Target item not found");

			insert(node.next, item);
		}



		/// <summary>
		/// Insert an item at the front of this list.
		/// <exception cref="InvalidOperationException"/> if the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public override void InsertFirst(T item)
		{
			updatecheck();
			insert(startsentinel.next, item);
		}

		/// <summary>
		/// Insert an item at the back of this list.
		/// <exception cref="InvalidOperationException"/> if  the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public override void InsertLast(T item)
		{
			updatecheck();
			insert(endsentinel, item);
		}


		/// <summary>
		/// Remove one item from the list: from the front if <code>FIFO</code>
		/// is true, else from the back.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public override T Remove()
		{
			T retval = base.Remove();

			dict.Remove(retval);
			return retval;
		}


		/// <summary>
		/// Remove one item from the front of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public override T RemoveFirst()
		{
			T retval = base.RemoveFirst();

			dict.Remove(retval);
			return retval;
		}


		/// <summary>
		/// Remove one item from the back of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public override T RemoveLast()
		{
			T retval = base.RemoveLast();

			dict.Remove(retval);
			return retval;
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
			checkRange(start, count);
			modifycheck();

			HashedLinkedList<T> retval = (HashedLinkedList<T>)MemberwiseClone();

			retval.underlying = retval.hashedunderlying = hashedunderlying != null ? hashedunderlying : this;
			retval.offset = start + offset;
			retval.startsentinel = start == 0 ? startsentinel : get(start - 1);
			retval.endsentinel = start + count == size ? endsentinel : get(start + count);
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
			//Duplicating linkedlist<T> code to minimize cache misses
			updatecheck();
			checkRange(start, count);
			if (count == 0)
				return;

			Node a = get(start), b = get(start + count - 1);

			for (int i = 0; i < count / 2; i++)
			{
				T swap = a.item;a.item = b.item;b.item = swap;
				dict[a.item] = a;dict[b.item] = b;
				a = a.next;b = b.prev;
			}
		}


		/// <summary>
		/// Shuffle the items of this list according to a specific random source.
		/// </summary>
		/// <param name="rnd">The random source.</param>
		public override void Shuffle(Random rnd)
		{
			updatecheck();

			ArrayList<T> a = new ArrayList<T>();

			a.AddAll(this);
			a.Shuffle(rnd);

			Node cursor = startsentinel.next;
			int j = 0;

			while (cursor != endsentinel)
			{
				dict[cursor.item = a[j++]] = cursor;
				cursor = cursor.next;
			}
		}

		#endregion		

		#region IIndexed<T> Members

		/// <summary>
		/// Searches for an item in the list going forwrds from the start.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of item from start.</returns>
		[Tested]
		public override int IndexOf(T item)
		{
			Node node;

			modifycheck();
			if (!dict.Find(item, out node))
				return -1;
#if LISTORDER
			if (maintaintags && !insideview(node))
				return -1;
#elif EXTLISTORDER
			if (maintaintags && !insideview(node))
				return -1;
#endif
			return base.IndexOf(item);
		}


		/// <summary>
		/// Searches for an item in the list going backwords from the end.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of of item from the end.</returns>
		[Tested]
		public override int LastIndexOf(T item) { return IndexOf(item); }


		/// <summary>
		/// Remove the item at a specific position of the list.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <param name="i">The index of the item to remove.</param>
		/// <returns>The removed item.</returns>
		[Tested]
		public override T RemoveAt(int i)
		{
			T retval = base.RemoveAt(i);

			dict.Remove(retval);
			return retval;
		}


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
			checkRange(start, count);
			if (count == 0)
				return;

			Node a, b;

			if (start <= size - start - count)
			{
				b = a = get(start);
#if EXTLISTORDER
				Node c = a.prev;
#endif
				for (int i = 0; i < count; i++)
				{
					dict.Remove(b.item); 
#if EXTLISTORDER
					if (maintaintags)
					{
						c.next = b;
						b.prev = c;
						removefromtaggroup(b);
					}
#endif
					b = b.next;
				}

				a.prev.next = b;
				b.prev = a.prev;
			}
			else
			{
				a = b = get(start + count - 1);
#if EXTLISTORDER
				Node c = b.next;
#endif
				for (int i = 0; i < count; i++)
				{
					dict.Remove(a.item); 
#if EXTLISTORDER
					if (maintaintags)
					{
						c.prev = a;
						a.next = c;
						removefromtaggroup(a);
					}
#endif
					a = a.prev;
				}

				a.next = b.next;
				b.next.prev = a;
			}

			size -= count;
			if (hashedunderlying != null)
				hashedunderlying.size -= count;
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
		/// in terms of the size of this collection (worst-case or amortized as
		/// relevant).
		/// </summary>
		/// <value>Speed.Constant</value>
		[Tested]
		public override Speed ContainsSpeed
		{
			[Tested]
			get
			{
#if LISTORDER || EXTLISTORDER
				return hashedunderlying == null || maintaintags ? Speed.Constant : Speed.Linear;
#else
				return basehashedlist == null ? Speed.Constant : Speed.Linear;
#endif
			}
		}


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
		public override bool Contains(T item)
		{
			Node node;

			modifycheck();
			return contains(item, out node);
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public override bool Find(ref T item)
		{
			Node node;

			modifycheck();
			if (contains(item, out node)) { item = node.item; return true; }

			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. 
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		[Tested]
		public override bool Update(T item)
		{
			Node node;

			updatecheck();
			if (contains(item, out node)) { node.item = item; return true; }

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
		public override bool FindOrAdd(ref T item)
		{
			updatecheck();

			//This is an extended myinsert:
			Node node = new Node(item);

			if (!dict.FindOrAdd(item, ref node))
			{
				insertNode(endsentinel, node);
				return false;
			}

			if (!insideview(node))
				throw new ArgumentException("Item alredy in indexed list but outside view");

			item = node.item;
			return true;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value; else add the value to the collection. 
		/// </summary>
		/// <param name="item">Value to add or update.</param>
		/// <returns>True if the item was found and updated (hence not added).</returns>
		[Tested]
		public override bool UpdateOrAdd(T item)
		{
			updatecheck();

			Node node = new Node(item);

			/*if (basehashedlist == null)
			{
				if (!dict.UpdateOrAdd(item, node))
					return false;
			}
			else
			{*/
				//NOTE: it is hard to do this without double access to the dictionary
				//in the update case
				if (dict.FindOrAdd(item, ref node))
				{
					if (!insideview(node))
						throw new ArgumentException("Item in indexed list but outside view");

					//dict.Update(item, node);
					node.item = item;
					return true;
				}
			//}

			insertNode(endsentinel, node);
			return false;
		}


		/// <summary>
		/// Remove a particular item from this collection. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public override bool Remove(T item)
		{
			updatecheck();

			Node node;

			if (!dictremove(item, out node))
				return false;

			remove(node);
			return true;
		}


		/// <summary>
		/// Remove a particular item from this collection if found. 
		/// If an item was removed, report a binary copy of the actual item removed in 
		/// the argument.
		/// </summary>
		/// <param name="item">The value to remove on input.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public override bool RemoveWithReturn(ref T item)
		{
			Node node;

			updatecheck();
			if (!dictremove(item, out node))
				return false;

			item = node.item;
			remove(node);
			return true;
		}


		/// <summary>
		/// Remove all items in another collection from this one. 
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public override void RemoveAll(MSG.IEnumerable<T> items)
		{
			Node node;

			updatecheck();
			foreach (T item in items)
				if (dictremove(item, out node))
					remove(node);
		}


		/// <summary>
		/// Remove all items from this collection.
		/// </summary>
		[Tested]
		public override void Clear()
		{
			updatecheck();
			if (hashedunderlying == null)
				dict.Clear();
			else
				foreach (T item in this)
					dict.Remove(item);

			base.Clear();
		}


		/// <summary>
		/// Remove all items not in some other collection from this one. 
		/// </summary>
		/// <param name="items">The items to retain.</param>
		[Tested]
		public override void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();
			if (hashedunderlying == null)
			{
				HashDictionary<T,Node> newdict = new HashDictionary<T,Node>(itemhasher);

				foreach (T item in items)
				{
					Node node;

					if (dict.Remove(item, out node))
						newdict.Add(item, node);
				}

				foreach (KeyValuePair<T,Node> pair in dict)
				{
					Node n = pair.value, p = n.prev, s = n.next; s.prev = p; p.next = s;
#if EXTLISTORDER
					if (maintaintags)
						removefromtaggroup(n);
#endif
				}

				dict = newdict;
				size = dict.Count;
				//For a small number of items to retain it might be faster to 
				//iterate through the list and splice out the chunks not needed
			}
			else
			{
				HashSet<T> newdict = new HashSet<T>(itemhasher);

				foreach (T item in this)
					newdict.Add(item);

				foreach (T item in items)
					newdict.Remove(item);

				Node n = startsentinel.next;

				while (n != endsentinel)
				{
					if (newdict.Contains(n.item))
					{
						dict.Remove(n.item);
						remove(n);
					}

					n = n.next;
				}
			}
		}

		/// <summary>
		/// Check if this collection contains all the values in another collection
		/// Multiplicities
		/// are not taken into account.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		[Tested]
		public override bool ContainsAll(MSG.IEnumerable<T> items)
		{
			Node node;

			modifycheck();
			foreach (T item in items)
				if (!contains(item, out node))
					return false;

			return true;
		}


		/// <summary>
		/// Count the number of items of the collection equal to a particular value.
		/// Returns 0 if and only if the value is not in the collection.
		/// </summary>
		/// <param name="item">The value to count.</param>
		/// <returns>The number of copies found.</returns>
		[Tested]
		public override int ContainsCount(T item) { return Contains(item) ? 1 : 0; }


		/// <summary>
		/// Remove all items equivalent to a given value.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		[Tested]
		public override void RemoveAllCopies(T item) { Remove(item); }

		#endregion

		#region ISink<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>False since this collection has set semantics.</value>
		[Tested]
        public override bool AllowsDuplicates { [Tested]get { return false; } }


        //This is *not* the same as AddLast!!
		/// <summary>
		/// Add an item to this collection if possible. Since this collection has set
		/// semantics, the item will be added if not already in the collection. 
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True if item was added.</returns>
		[Tested]
		public override bool Add(T item)
		{
			updatecheck();

			Node node = new Node(item);

			if (!dict.FindOrAdd(item, ref node))
			{
				insertNode(endsentinel, node);
				return true;
			}

			return false;
		}


		//Note: this is *not* equivalent to InsertRange int this Set situation!!!
		/// <summary>
		/// Add the elements from another collection to this collection.
		/// Only items not already in the collection
		/// will be added.
		/// </summary>
		/// <param name="items">The items to add.</param>
		[Tested]
		public override void AddAll(MSG.IEnumerable<T> items)
		{
			updatecheck();
			foreach (T item in items)
			{
				Node node = new Node(item);

				if (!dict.FindOrAdd(item, ref node))
					insertNode(endsentinel, node);
			}
		}

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. 
        /// Only items not already in the collection
        /// will be added.
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public override void AddAll<U>(MSG.IEnumerable<U> items) //where U:T
        {
            updatecheck();
            foreach (T item in items)
            {
                Node node = new Node(item);

                if (!dict.FindOrAdd(item, ref node))
                    insertNode(endsentinel, node);
            }
        }

        #endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }


		#endregion

		#region Diagnostics
		/// <summary>
		/// Check the integrity of the internal data structures of this collection.
		/// Only avaliable in DEBUG builds???
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		[Tested]
		public override bool Check()
		{
			if (!base.Check())
				return false;

			bool retval = true;

			if (hashedunderlying == null)
			{
				if (size != dict.Count)
				{
					Console.WriteLine("list.size ({0}) != dict.Count ({1})", size, dict.Count);
					retval = false;
				}

				Node n = startsentinel.next, n2;

				while (n != endsentinel)
				{
					if (!dict.Find(n.item, out n2))
					{
						Console.WriteLine("Item in list but not dict: {0}", n.item);
						retval = false;
					}
					else if (n != n2)
					{
						Console.WriteLine("Wrong node in dict for item: {0}", n.item);
						retval = false;
					}

					n = n.next;
				}
			}

			return retval;
		}
		#endregion
	}
}