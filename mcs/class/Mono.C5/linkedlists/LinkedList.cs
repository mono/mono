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

#define LISTORDERnot
#define EXTLISTORDER
using System;
using System.Diagnostics;
using MSG=System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A list collection class based on a doubly linked list data structure.
	/// </summary>
	public class LinkedList<T>: SequencedBase<T>, IList<T>
	{
		#region Fields
		/// <summary>
		/// IExtensible.Add(T) always does AddLast(T), fIFO determines 
		/// if T Remove() does RemoveFirst() or RemoveLast()
		/// </summary>
		bool fIFO = true;

#if LISTORDER || EXTLISTORDER
		/// <summary>
		/// True if we maintain tags for node ordering (false for plain linked list, true for hashed linked list).
		/// </summary>
		protected bool maintaintags = false;
#endif

#if EXTLISTORDER
		int taggroups;
#endif

		//Invariant:  startsentinel != null && endsentinel != null
		//If size==0: startsentinel.next == endsentinel && endsentinel.prev == startsentinel
		//Else:      startsentinel.next == First && endsentinel.prev == Last)
		/// <summary>
		/// Node to the left of first node 
		/// </summary>
		protected Node startsentinel;
		/// <summary>
		/// Node to the right of last node
		/// </summary>
		protected Node endsentinel;
		/// <summary>
		/// Offset of this view in underlying list
		/// </summary>
		protected int offset;

		/// <summary>
		/// underlying list of theis view (or null)
		/// </summary>
		protected LinkedList<T> underlying;

		#endregion

		#region Util

		bool equals(T i1, T i2) { return itemhasher.Equals(i1, i2); }


		/// <summary>
		/// Check if it is valid to perform updates and increment stamp.
		/// <exception cref="InvalidOperationException"/> if check fails.
		/// <br/>This method should be called at the start of any public modifying methods.
		/// </summary>
		protected override void updatecheck()
		{
			modifycheck();
			base.updatecheck();
			if (underlying != null)
				underlying.stamp++;
		}


		/// <summary>
		/// Check if we are a view that the underlyinglist has only been updated through us.
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


		Node insert(Node succ, T item)
		{
			Node newnode = new Node(item, succ.prev, succ);

			succ.prev.next = newnode;
			succ.prev = newnode;
			size++;
			if (underlying != null)
				underlying.size++;

#if LISTORDER
			//TODO: replace with Debug.Assert(!maintaintags)
			if (maintaintags)
				settag(newnode);
#elif EXTLISTORDER
			if (maintaintags)
				settag(newnode);
#endif

			return newnode;
		}


		/// <summary>
		/// Insert a Node before another one. Unchecked internal version.
		/// </summary>
		/// <param name="succ">The successor to be</param>
		/// <param name="newnode">Node to insert</param>
		protected void insertNode(Node succ, Node newnode)
		{
			newnode.next = succ;
			newnode.prev = succ.prev;
			succ.prev.next = newnode;
			succ.prev = newnode;
			size++;
			if (underlying != null)
				underlying.size++;

#if LISTORDER
			if (maintaintags)
				settag(newnode);
#elif EXTLISTORDER
			if (maintaintags)
				settag(newnode);
#endif
		}


		/// <summary>
		/// Remove a node. Unchecked internal version.
		/// </summary>
		/// <param name="node">Node to remove</param>
		/// <returns>The item of the removed node</returns>
		protected T remove(Node node)
		{
			node.prev.next = node.next;
			node.next.prev = node.prev;
			size--;
			if (underlying != null)
				underlying.size--;
#if EXTLISTORDER
			if (maintaintags)
				removefromtaggroup(node);
#endif
			return node.item;
		}



#if LISTORDER
		//const int MaxTag = int.MaxValue;

		protected void settag(Node node)
		{
			//Note: the (global) sentinels have tag==0 and all other tags are positive.
			Node pred = node.prev, succ = node.next;
			if (pred.tag < succ.tag - 1)
			{
				//Note:
				//node.tag-pred.tag = (succ.tag-pred.tag) / 2 > 0
                //succ.tag-node.tag = succ.tag-pred.tag - (succ.tag-pred.tag) / 2 > 0
				node.tag = pred.tag + (succ.tag-pred.tag) / 2;
				return;
			}

			if (succ.tag == 0 && pred.tag < int.MaxValue)
			{
				//Note:
				//node.tag-pred.tag = 1 + (int.MaxValue-pred.tag) / 2 > 0
				//node.tag <=int.MaxValue
				node.tag = pred.tag + 1 + (int.MaxValue - pred.tag) / 2;
				return;
			}

			node.tag = node.prev.tag;
			redistributetags(node);
		}
		
		private void redistributetags(Node node)
		{
			Node pred = node, succ = node;

			//Node start = underlying == null ? startsentinel : underlying.startsentinel;
			//Node end = underlying == null ? endsentinel : underlying.endsentinel;
			double limit = 1, bigt = Math.Pow(size, 1.0/29);//?????
			int bits = 1, count = 1, lowmask = 0, himask = 0, target = 0;

			do
			{
				bits++;
				lowmask = (1 << bits) - 1;
				himask = ~lowmask;
				target = node.tag & himask;
				while (pred.prev.tag > 0 && (pred.prev.tag & himask) == target)
				{ count++; pred = pred.prev; }

				while (succ.next.tag > 0 && (succ.next.tag & himask) == target)
				{ count++; succ = succ.next; }

				limit *= bigt;
			} while (count > limit);

			//redistibute tags
			//Console.WriteLine("Redistributing {0} tags at {1} bits around item {2}", count, bits, node.item);

			int delta = lowmask / (count+1);

			for (int i = 1; i <= count; i++)
			{
				pred.tag = target + i * delta;
				//Console.Write("({0} -> {1})", pred.item, pred.tag);
				pred = pred.next;
			}
			//Console.WriteLine("{0}:{1}:{2}/",count,size,Check());
		}
#elif EXTLISTORDER
		const int wordsize = 32;

		const int lobits = 3;

		const int hibits = lobits + 1;

		const int losize = 1 << lobits;

		const int hisize = 1 << hibits;

		const int logwordsize = 5;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="pred"></param>
		/// <param name="succ"></param>
		/// <param name="lowbound"></param>
		/// <param name="highbound"></param>
		/// <returns></returns>
		protected TagGroup gettaggroup(Node pred, Node succ, out int lowbound, out int highbound)
		{
			TagGroup predgroup = pred.taggroup, succgroup = succ.taggroup;

			if (predgroup == succgroup)
			{
				lowbound = pred.tag + 1;
				highbound = succ.tag - 1;
				return predgroup;
			}
			else if (predgroup.first != null)
			{
				lowbound = pred.tag + 1;
				highbound = int.MaxValue;
				return predgroup;
			}
			else if (succgroup.first != null)
			{
				lowbound = int.MinValue;
				highbound = succ.tag - 1;
				return succgroup;
			}
			else
			{
				lowbound = int.MinValue;
				highbound = int.MaxValue;
				return new TagGroup();
			}
		}


		/// <summary>
		/// Put a tag on a node (already inserted in the list). Split taggroups and renumber as 
		/// necessary.
		/// </summary>
		/// <param name="node">The node to tag</param>
		protected void settag(Node node)
		{
			Node pred = node.prev, succ = node.next;
			TagGroup predgroup = pred.taggroup, succgroup = succ.taggroup;

			if (predgroup == succgroup)
			{
				node.taggroup = predgroup;
				predgroup.count++;
				if (pred.tag + 1 == succ.tag)
					splittaggroup(predgroup);
				else
					node.tag = (pred.tag + 1) / 2 + (succ.tag - 1) / 2;
			}
			else if (predgroup.first != null)
			{
				node.taggroup = predgroup;
				predgroup.last = node;
				predgroup.count++;
				if (pred.tag == int.MaxValue)
					splittaggroup(predgroup);
				else
					node.tag = pred.tag / 2 + int.MaxValue / 2 + 1;
			}
			else if (succgroup.first != null)
			{
				node.taggroup = succgroup;
				succgroup.first = node;
				succgroup.count++;
				if (succ.tag == int.MinValue)
					splittaggroup(node.taggroup);
				else
					node.tag = int.MinValue / 2 + (succ.tag - 1) / 2;
			}
			else
			{
				Debug.Assert(taggroups == 0);

				TagGroup newgroup = new TagGroup();

				taggroups = 1;
				node.taggroup = newgroup;
				newgroup.first = newgroup.last = node;
				newgroup.count = 1;
				return;
			}
		}


		/// <summary>
		/// Remove a node from its taggroup.
		/// <br/> When this is called, node must already have been removed from the underlying list
		/// </summary>
		/// <param name="node">The node to remove</param>
		protected void removefromtaggroup(Node node)
		{
			//
			TagGroup taggroup = node.taggroup;

			if (--taggroup.count == 0)
			{
				taggroups--;
				return;
			}

			if (node == taggroup.first)
				taggroup.first = node.next;

			if (node == taggroup.last)
				taggroup.last = node.prev;

			//node.taggroup = null;
			if (taggroup.count != losize)
				return;

			TagGroup otg;

			if ((otg = taggroup.first.prev.taggroup).count <= losize)
				taggroup.first = otg.first;
			else if ((otg = taggroup.last.next.taggroup).count <= losize)
				taggroup.last = otg.last;
			else
				return;

			Node n = otg.first;

			for (int i = 0, length = otg.count; i < length; i++)
			{
				n.taggroup = taggroup;
				n = n.next;
			}

			taggroup.count += otg.count;
			taggroups--;
			n = taggroup.first;

			const int ofs = wordsize - hibits;

			for (int i = 0, count = taggroup.count; i < count; i++)
			{
				n.tag = (i - losize) << ofs; //(i-8)<<28 
				n = n.next;
			}
		}


		/// <summary>
		/// Split a tag group to make rom for more tags.
		/// </summary>
		/// <param name="taggroup">The tag group</param>
		protected void splittaggroup(TagGroup taggroup)
		{
			Node n = taggroup.first;
			int ptgt = taggroup.first.prev.taggroup.tag;
			int ntgt = taggroup.last.next.taggroup.tag;

			Debug.Assert(ptgt + 1 <= ntgt - 1);

			int ofs = wordsize - hibits;
			int newtgs = taggroup.count / hisize - 1;
			int tgtdelta = (int)((ntgt + 0.0 - ptgt) / (newtgs + 2)), tgtag = ptgt;

			tgtdelta = tgtdelta == 0 ? 1 : tgtdelta;
			for (int j = 0; j < newtgs; j++)
			{
				TagGroup newtaggroup = new TagGroup();

				newtaggroup.tag = (tgtag = tgtag >= ntgt - tgtdelta ? ntgt : tgtag + tgtdelta);
				newtaggroup.first = n;
				newtaggroup.count = hisize;
				for (int i = 0; i < hisize; i++)
				{
					n.taggroup = newtaggroup;
					n.tag = (i - losize) << ofs; //(i-8)<<28 
					n = n.next;
				}

				newtaggroup.last = n.prev;
			}

			int rest = taggroup.count - hisize * newtgs;

			taggroup.first = n;
			taggroup.count = rest;
			taggroup.tag = (tgtag = tgtag >= ntgt - tgtdelta ? ntgt : tgtag + tgtdelta);					ofs--;
			for (int i = 0; i < rest; i++)
			{
				n.tag = (i - hisize) << ofs; //(i-16)<<27 
				n = n.next;
			}

			taggroup.last = n.prev;
			taggroups += newtgs;
			if (tgtag == ntgt)
				redistributetaggroups(taggroup);
		}


		private void redistributetaggroups(TagGroup taggroup)
		{
			TagGroup pred = taggroup, succ = taggroup, tmp;
			double limit = 1, bigt = Math.Pow(taggroups, 1.0 / 30);//?????
			int bits = 1, count = 1, lowmask = 0, himask = 0, target = 0;

			do
			{
				bits++;
				lowmask = (1 << bits) - 1;
				himask = ~lowmask;
				target = taggroup.tag & himask;
#if FIXME
				while ((tmp = pred.first.prev.taggroup).first != null && (tmp.tag & himask) == target)
				{ count++; pred = tmp; }

				while ((tmp = succ.last.next.taggroup).last != null && (tmp.tag & himask) == target)
				{ count++; succ = tmp; }
#else
				for (tmp = pred.first.prev.taggroup; (tmp.first != null) && ((tmp.tag & himask) == target);)
				{ count++; pred = tmp; }

				for (tmp = succ.last.next.taggroup; (tmp..last != null) && ((tmp.tag & himask) == target);)
				{ count++; succ = tmp; }
#endif

				limit *= bigt;
			} while (count > limit);

			//redistibute tags
			int lob = pred.first.prev.taggroup.tag, upb = succ.last.next.taggroup.tag;
			int delta = upb / (count + 1) - lob / (count + 1);

			Debug.Assert(delta > 0);
			for (int i = 0; i < count; i++)
			{
				pred.tag = lob + (i + 1) * delta;
				pred = pred.last.next.taggroup;
			}
		}
#endif

		#endregion

		#region Constructors
		/// <summary>
		/// Create a linked list with en external item hasher
		/// </summary>
		/// <param name="itemhasher">The external hasher</param>
		public LinkedList(IHasher<T> itemhasher)
		{
			this.itemhasher = itemhasher;
#if EXTLISTORDER		
			startsentinel = new Node(default(T));
			endsentinel = new Node(default(T));
			startsentinel.next = endsentinel;
			endsentinel.prev = startsentinel;

			//It isused that these are different:
			startsentinel.taggroup = new TagGroup();
			startsentinel.taggroup.tag = int.MinValue;
			startsentinel.taggroup.count = 0;
			endsentinel.taggroup = new TagGroup();
			endsentinel.taggroup.tag = int.MaxValue;
			endsentinel.taggroup.count = 0;
#else
			startsentinel = endsentinel = new Node(default(T));
			startsentinel.next = endsentinel.prev = startsentinel;
#endif
			size = stamp = 0;
		}


		/// <summary>
		/// Create a linked list with the nmatural item hasher
		/// </summary>
		public LinkedList() : this(HasherBuilder.ByPrototype<T>.Examine()) { }

		#endregion

		#region Nested classes

		/// <summary>
		/// An individual cell in the linked list
		/// </summary>
		protected class Node
		{
			/// <summary>
			/// Previous-node reference
			/// </summary>
			public Node prev;

			/// <summary>
			/// Next-node reference
			/// </summary>
			public Node next;

			/// <summary>
			/// Node item
			/// </summary>
			public T item;
#if LISTORDER
			internal int tag;
#elif EXTLISTORDER
			internal int tag;

			internal TagGroup taggroup;


			internal bool precedes(Node that)
			{
				//Debug.Assert(taggroup != null, "taggroup field null");
				//Debug.Assert(that.taggroup != null, "that.taggroup field null");
				int t1 = taggroup.tag;
				int t2 = that.taggroup.tag;

				return t1 < t2 ? true : t1 > t2 ? false : tag < that.tag;
			}
#endif
			/// <summary>
			/// Create node
			/// </summary>
			/// <param name="item">Item to insert</param>
			[Tested]
			public Node(T item)
			{
				this.item = item;
			}


			/// <summary>
			/// Create node, specifying predecessor and successor
			/// </summary>
			/// <param name="item"></param>
			/// <param name="prev"></param>
			/// <param name="next"></param>
			[Tested]
			public Node(T item, Node prev, Node next)
			{
				this.item = item; this.prev = prev; this.next = next;
			}


			/// <summary>
			/// Pretty print node
			/// </summary>
			/// <returns>Formatted node</returns>
			public override string ToString()
			{
#if LISTORDER || EXTLISTORDER
				return String.Format("Node: (item={0}, tag={1})", item, tag);
#else
				return String.Format("Node(item={0})", item);
#endif
			}
		}

#if EXTLISTORDER		
		/// <summary>
		/// A group of nodes with the same high tag. Purpose is to be
		/// able to tell the sequence order of two nodes without having to scan through
		/// the list.
		/// </summary>
		protected class TagGroup
		{
			internal int tag, count;

			internal Node first, last;


			/// <summary>
			/// Pretty print a tag group
			/// </summary>
			/// <returns>Formatted tag group</returns>
			public override string ToString()
			{ return String.Format("TagGroup(tag={0}, cnt={1}, fst={2}, lst={3})", tag, count, first, last); }
		}
#endif

		#endregion 

		#region Range nested class

		class Range: CollectionValueBase<T>, IDirectedCollectionValue<T>
		{
			int start, count, stamp;

			LinkedList<T> list;

			bool forwards;


			internal Range(LinkedList<T> list, int start, int count, bool forwards)
			{
				this.list = list;this.stamp = list.stamp;
				this.start = start;this.count = count;this.forwards = forwards;
			}


			[Tested]
			public override int Count { [Tested]get { list.modifycheck(stamp); return count; } }


            public override Speed CountSpeed { get { list.modifycheck(stamp); return Speed.Constant; } }

            [Tested]
			public override MSG.IEnumerator<T> GetEnumerator()
			{
				int togo = count;

				list.modifycheck(stamp);
				if (togo == 0)
					yield break;

				Node cursor = forwards ? list.get(start) : list.get(start + count - 1);

				yield return cursor.item;
				while (--togo > 0)
				{
					cursor = forwards ? cursor.next : cursor.prev;
					list.modifycheck(stamp);
					yield return cursor.item;
				}
			}


			[Tested]
			public IDirectedCollectionValue<T> Backwards()
			{
				list.modifycheck(stamp);

				Range b = (Range)MemberwiseClone();

				b.forwards = !forwards;
				return b;
			}


			[Tested]
			IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }


			[Tested]
			public EnumerationDirection Direction
			{
				[Tested]
				get
				{ return forwards ? EnumerationDirection.Forwards : EnumerationDirection.Backwards; }
			}
		}


		#endregion

		#region IList<T> Members

		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <value>The first item in this list.</value>
		[Tested]
		public virtual T First
		{
			[Tested]
			get
			{
				modifycheck();
				if (size == 0)
					throw new InvalidOperationException("List is empty");

				return startsentinel.next.item;
			}
		}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <value>The last item in this list.</value>
		[Tested]
		public virtual T Last
		{
			[Tested]
			get
			{
				modifycheck();
				if (size == 0)
					throw new InvalidOperationException("List is empty");

				return endsentinel.prev.item;
			}
		}


		/// <summary>
		/// Since <code>Add(T item)</code> always add at the end of the list,
		/// this describes if list has FIFO or LIFO semantics.
		/// </summary>
		/// <value>True if the <code>Remove()</code> operation removes from the
		/// start of the list, false if it removes from the end.</value>
		[Tested]
		public bool FIFO
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
		/// <param name="index">The index of the item to fetch or store.</param>
		[Tested]
		public virtual T this[int index]
		{
			[Tested]
			get { modifycheck(); return get(index).item; }
			[Tested]
			set { updatecheck(); get(index).item = value; }
		}


		/// <summary>
		/// Return the node at position n
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		protected Node get(int n)
		{
			if (n < 0 || n >= size)
				throw new IndexOutOfRangeException();
			else if (n < size / 2)
			{              // Closer to front
				Node node = startsentinel;

				for (int i = 0; i <= n; i++)
					node = node.next;

				return node;
			}
			else
			{                            // Closer to end
				Node node = endsentinel;

				for (int i = size; i > n; i--)
					node = node.prev;

				return node;
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
			insert(i == size ? endsentinel : get(i), item);
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

			Node succ, node;
			int count = 0;

			succ = i == size ? endsentinel : get(i);
			node = succ.prev;
#if LISTORDER
			//TODO: replace with Debug.Assert(!maintaintags)
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
#if LISTORDER
				//TODO: remove
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
			if (underlying != null)
				underlying.size += count;
#if LISTORDER
			//TODO: remove 
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
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target before which to insert.</param>
		[Tested]
		public virtual void InsertBefore(T item, T target)
		{
			updatecheck();

			Node node = startsentinel.next;
			int i = 0;

			if (!find(target, ref node, ref i))
				throw new ArgumentException("Target item not found");

			insert(node, item);
		}


		/// <summary>
		/// Insert an item right after the last(???) occurrence of some target item.
		/// <exception cref="ArgumentException"/> if target	is not found
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target after which to insert.</param>
		[Tested]
		public virtual void InsertAfter(T item, T target)
		{
			updatecheck();

			Node node = endsentinel.prev;
			int i = size - 1;

			if (!dnif(target, ref node, ref i))
				throw new ArgumentException("Target item not found");

			insert(node.next, item);
		}


		/// <summary>
		/// Insert an item at the front of this list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public virtual void InsertFirst(T item)
		{
			updatecheck();
			insert(startsentinel.next, item);
		}

		/// <summary>
		/// Insert an item at the back of this list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		[Tested]
		public virtual void InsertLast(T item)
		{
			updatecheck();
			insert(endsentinel, item);
		}


		/// <summary>
		/// Create a new list consisting of the results of mapping all items of this
		/// list.
		/// </summary>
		/// <param name="mapper">The delegate definging the map.</param>
		/// <returns>The new list.</returns>
		[Tested]
        public IList<V> Map<V>(Mapper<T, V> mapper)
        {
            modifycheck();

            LinkedList<V> retval = new LinkedList<V>();
            return map<V>(mapper, retval);
        }

        /// <summary>
        /// Create a new list consisting of the results of mapping all items of this
        /// list. The new list will use a specified hasher for the item type.
        /// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The delegate defining the map.</param>
        /// <param name="hasher">The hasher to use for the new list</param>
        /// <returns>The new list.</returns>
        public IList<V> Map<V>(Mapper<T, V> mapper, IHasher<V> hasher)
        {
            modifycheck();

            LinkedList<V> retval = new LinkedList<V>(hasher);
            return map<V>(mapper, retval);
        }

        private IList<V> map<V>(Mapper<T, V> mapper, LinkedList<V> retval)
        {
            Node cursor = startsentinel.next;
            LinkedList<V>.Node mcursor = retval.startsentinel;

#if LISTORDER
			//TODO: replace with Debug.Assert(!retval.maintaintags)
			double tagdelta = int.MaxValue / (size + 1.0);
			int count = 1;
#elif EXTLISTORDER
            double tagdelta = int.MaxValue / (size + 1.0);
            int count = 1;
            LinkedList<V>.TagGroup taggroup = null;

            if (retval.maintaintags)
            {
                taggroup = new LinkedList<V>.TagGroup();
                retval.taggroups = 1;
                taggroup.count = size;
            }
#endif
            while (cursor != endsentinel)
            {
                mcursor.next = new LinkedList<V>.Node(mapper(cursor.item), mcursor, null);
                cursor = cursor.next;
                mcursor = mcursor.next;
#if LISTORDER
				//TODO: remove
				if (retval.maintaintags) mcursor.tag = (int)(tagdelta * count++);
#elif EXTLISTORDER
                if (retval.maintaintags)
                {
                    mcursor.taggroup = taggroup;
                    mcursor.tag = (int)(tagdelta * count++);
                }
#endif
            }

            retval.endsentinel.prev = mcursor;
            mcursor.next = retval.endsentinel;
            retval.size = size; return retval;
        }


        /// <summary>
		/// Remove one item from the list: from the front if <code>FIFO</code>
		/// is true, else from the back.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T Remove()
		{
			return fIFO ? RemoveFirst() : RemoveLast();
		}


		/// <summary>
		/// Remove one item from the front of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T RemoveFirst()
		{
			updatecheck();
			if (size == 0)
                throw new InvalidOperationException("List is empty");

            T item = startsentinel.next.item;

			if (size == 1)
				clear();
			else
				remove(startsentinel.next);

			return item;
		}


		/// <summary>
		/// Remove one item from the back of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public virtual T RemoveLast()
		{
			updatecheck();
			if (size == 0)
                throw new InvalidOperationException("List is empty");

            Node toremove = endsentinel.prev;
			T item = toremove.item;

			if (size == 1)
				clear();
			else
				remove(toremove);

			return item;
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

			LinkedList<T> retval = (LinkedList<T>)MemberwiseClone();

			retval.underlying = underlying != null ? underlying : this;
			retval.offset = offset + start;
			retval.size = count;
			retval.startsentinel = start == 0 ? startsentinel : get(start - 1);
			retval.endsentinel = start + count == size ? endsentinel : get(start + count);
#if DEBUG
			Check();
#endif
			return retval;
		}


		/// <summary>
		/// Null if this list is not a view.
		/// </summary>
        /// <value>Underlying list for view.</value>
        [Tested]
        public IList<T> Underlying { [Tested]get { modifycheck(); return underlying; } }


        /// <summary>
		/// </summary>
		/// <value>Offset for this list view or 0 for a underlying list.</value>
		[Tested]
		public int Offset { [Tested]get { modifycheck(); return offset; } }


		/// <summary>
		/// Slide this list view along the underlying list.
		/// <exception cref="InvalidOperationException"/> if this list is not a view.
		/// <exception cref="ArgumentOutOfRangeException"/> if the operation
		/// would bring either end of the view outside the underlying list.
		/// </summary>
		/// <param name="offset">The signed amount to slide: positive to slide
		/// towards the end.</param>
		[Tested]
		public void Slide(int offset)
		{
			modifycheck();
			if (underlying == null)
				throw new InvalidOperationException("List not a view");

			if (offset + this.offset < 0 || offset + this.offset + size > underlying.size)
				throw new ArgumentOutOfRangeException();

			if (offset == 0) return;

			if (offset > 0)
				for (int i = 0; i < offset; i++)
				{
					endsentinel = endsentinel.next;
					startsentinel = startsentinel.next;
				}
			else
				for (int i = 0; i < -offset; i++)
				{
					endsentinel = endsentinel.prev;
					startsentinel = startsentinel.prev;
				}

			this.offset += offset;
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
		public void Slide(int offset, int size)
		{
			modifycheck();
			if (underlying == null)
				throw new InvalidOperationException("List not a view");

			if (offset + this.offset < 0 || offset + this.offset + size > underlying.size)
				throw new ArgumentOutOfRangeException();

			Node node = startsentinel;

			if (offset > 0)
				for (int i = 0; i < offset; i++)
					node = node.next;
			else
				for (int i = 0; i < -offset; i++)
					node = node.prev;

			startsentinel = node;

			int enddelta = offset + size - this.size;

			node = endsentinel;
			if (enddelta > 0)
				for (int i = 0; i < enddelta; i++)
					node = node.next;
			else
				for (int i = 0; i < -enddelta; i++)
					node = node.prev;

			endsentinel = node;
			this.size = size;
			this.offset += offset;
		}


		/// <summary>
		/// Reverse the list so the items are in the opposite sequence order.
		/// </summary>
		[Tested]
		public void Reverse() { Reverse(0, size); }


		//Question: should we swap items or move nodes around?
		//The first seems much more efficient unless the items are value types 
		//with a large memory footprint.
		//(Swapping will do count*3/2 T assignments, linking around will do 
		// 4*count ref assignments; note that ref assignments are more expensive 
		//than copying non-ref bits)
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
			if (count == 0)
				return;

			Node a = get(start), b = get(start + count - 1);

			for (int i = 0; i < count / 2; i++)
			{
				T swap = a.item;a.item = b.item;b.item = swap;
#if LISTORDER
				//Do nothing!
#elif EXTLISTORDER
				//Neither
#endif
				a = a.next;b = b.prev;
			}
		}


		/// <summary>
		/// Check if this list is sorted according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		/// <returns>True if the list is sorted, else false.</returns>
		[Tested]
		public bool IsSorted(IComparer<T> c)
		{
			modifycheck();
			if (size <= 1)
				return true;

			Node node = startsentinel.next;
			T prevItem = node.item;

			node = node.next;
			while (node != endsentinel)
			{
				if (c.Compare(prevItem, node.item) > 0)
					return false;
				else
				{
					prevItem = node.item;
					node = node.next;
				}
			}

			return true;
		}


		// Sort the linked list using mergesort
		/// <summary>
		/// Sort the items of the list according to a specific sorting order.
		/// The sorting is stable.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		[Tested]
		public void Sort(IComparer<T> c)
		{
			updatecheck();

			// Build a linked list of non-empty runs.
			// The prev field in first node of a run points to next run's first node
			if (size == 0)
				return;

#if EXTLISTORDER
			if (maintaintags && underlying != null)
			{
				Node cursor = startsentinel.next;

				while (cursor != endsentinel)
				{
					cursor.taggroup.count--;
					cursor = cursor.next;
				}
			}
#endif
			Node runTail = startsentinel.next;
			Node prevNode = startsentinel.next;

			endsentinel.prev.next = null;
			while (prevNode != null)
			{
				Node node = prevNode.next;

				while (node != null && c.Compare(prevNode.item, node.item) <= 0)
				{
					prevNode = node;
					node = prevNode.next;
				}

				// Completed a run; prevNode is the last node of that run
				prevNode.next = null;	// Finish the run
				runTail.prev = node;	// Link it into the chain of runs
				runTail = node;
				if (c.Compare(endsentinel.prev.item, prevNode.item) <= 0)
					endsentinel.prev = prevNode;	// Update last pointer to point to largest

				prevNode = node;		// Start a new run
			}

			// Repeatedly merge runs two and two, until only one run remains
			while (startsentinel.next.prev != null)
			{
				Node run = startsentinel.next;
				Node newRunTail = null;

				while (run != null && run.prev != null)
				{ // At least two runs, merge
					Node nextRun = run.prev.prev;
					Node newrun = mergeRuns(run, run.prev, c);

					if (newRunTail != null)
						newRunTail.prev = newrun;
					else
						startsentinel.next = newrun;

					newRunTail = newrun;
					run = nextRun;
				}

				if (run != null) // Add the last run, if any
					newRunTail.prev = run;
			}

			endsentinel.prev.next = endsentinel;
			startsentinel.next.prev = startsentinel;

			//assert invariant();
			//assert isSorted();
#if LISTORDER
			if (maintaintags)
			{
				int span = (endsentinel.tag > 0 ? endsentinel.tag - 1 : int.MaxValue) - startsentinel.tag;

				Debug.Assert(span >= size);

				double tagdelta = span / (size + 0.0);
				int count = 1;
				Node cursor = startsentinel.next;

				while (cursor != endsentinel)
				{
					cursor.tag = startsentinel.tag + (int)(tagdelta * count++);
					cursor = cursor.next;
				}
			}
#elif EXTLISTORDER
			if (maintaintags)
			{
				Node cursor = startsentinel.next, end = endsentinel;
				int tag, taglimit;
				TagGroup t = gettaggroup(startsentinel, endsentinel, out tag, out taglimit);
				int tagdelta = taglimit / (size + 1) - tag / (size + 1);

				tagdelta = tagdelta == 0 ? 1 : tagdelta;
				if (underlying == null)
					taggroups = 1;

				while (cursor != end)
				{
					tag = tag + tagdelta > taglimit ? taglimit : tag + tagdelta;
					cursor.tag = tag;
					t.count++;
					cursor = cursor.next;
				}

				if (tag == taglimit)
					splittaggroup(t);
			}
#endif
		}


		private static Node mergeRuns(Node run1, Node run2, IComparer<T> c)
		{
			//assert run1 != null && run2 != null;
			Node prev;
			bool prev1;	// is prev from run1?

			if (c.Compare(run1.item, run2.item) <= 0)
			{
				prev = run1;
				prev1 = true;
				run1 = run1.next;
			}
			else
			{
				prev = run2;
				prev1 = false;
				run2 = run2.next;
			}

			Node start = prev;

			//assert start != null;
			start.prev = null;
			while (run1 != null && run2 != null)
			{
				if (prev1)
				{
					//assert prev.next == run1;
					//Comparable run2item = (Comparable)run2.item;
					while (run1 != null && c.Compare(run2.item, run1.item) >= 0)
					{
						prev = run1;
						run1 = prev.next;
					}

					if (run1 != null)
					{ // prev.item <= run2.item < run1.item; insert run2
						prev.next = run2;
						run2.prev = prev;
						prev = run2;
						run2 = prev.next;
						prev1 = false;
					}
				}
				else
				{
					//assert prev.next == run2;
					//Comparable run1item = (Comparable)run1.item;
					while (run2 != null && c.Compare(run1.item, run2.item) > 0)
					{
						prev = run2;
						run2 = prev.next;
					}

					if (run2 != null)
					{ // prev.item < run1.item <= run2.item; insert run1
						prev.next = run1;
						run1.prev = prev;
						prev = run1;
						run1 = prev.next;
						prev1 = true;
					}
				}
			}

			//assert !(run1 != null && prev1) && !(run2 != null && !prev1);
			if (run1 != null)
			{ // last run2 < all of run1; attach run1 at end
				prev.next = run1;
				run1.prev = prev;
			}
			else if (run2 != null)
			{ // last run1 
				prev.next = run2;
				run2.prev = prev;
			}

			return start;
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

			ArrayList<T> a = new ArrayList<T>();

			a.AddAll(this);
			a.Shuffle(rnd);

			Node cursor = startsentinel.next;
			int j = 0;

			while (cursor != endsentinel)
			{
				cursor.item = a[j++];
				cursor = cursor.next;
			}
		}

		#endregion

		#region IIndexed<T> Members


		/// <summary>
		/// <exception cref="IndexOutOfRangeException"/>.
		/// </summary>
		/// <value>The directed collection of items in a specific index interval.</value>
		/// <param name="start">The low index of the interval (inclusive).</param>
        /// <param name="count">The size of the range.</param>
        [Tested]
        public IDirectedCollectionValue<T> this[int start, int count]
		{
			[Tested]
			get
			{
				modifycheck();
				checkRange(start, count);
				return new Range(this, start, count, true);
			}
		}


		/// <summary>
		/// Searches for an item in the list going forwrds from the start.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of item from start.</returns>
		[Tested]
		public virtual int IndexOf(T item)
		{
			modifycheck();

			Node node = startsentinel.next;
			int index = 0;

			if (find(item, ref node, ref index))
				return index;
			else
				return -1;
		}


		/// <summary>
		/// Searches for an item in the list going backwords from the end.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of of item from the end.</returns>
		[Tested]
		public virtual int LastIndexOf(T item)
		{
			modifycheck();

			Node node = endsentinel.prev;
			int index = size - 1;

			if (dnif(item, ref node, ref index))
				return index;
			else
				return -1;
		}


		private bool find(T item, ref Node node, ref int index)
		{
			while (node != endsentinel)
			{
				//if (item.Equals(node.item))
				if (itemhasher.Equals(item, node.item))
					return true;

				index++;
				node = node.next;
			}

			return false;
		}


		private bool dnif(T item, ref Node node, ref int index)
		{
			while (node != startsentinel)
			{
				//if (item.Equals(node.item))
				if (itemhasher.Equals(item, node.item))
					return true;

				index--;
				node = node.prev;
			}

			return false;
		}


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
			return remove(get(i));
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
			if (count == 0)
				return;

			//for small count: optimize
			//make an optimal get(int i, int j, ref Node ni, ref Node nj)?
			Node a = get(start), b = get(start + count - 1);
#if EXTLISTORDER
			if (maintaintags)
			{
				Node c = a;
				TagGroup t = a.taggroup;

				while (c.taggroup == t && c != b.next)
				{
					removefromtaggroup(c);
					c = c.next;
				}

				if (c != b.next)
				{
					Debug.Assert(b.taggroup != t);
					c = b;
					t = b.taggroup;
					while (c.taggroup == t)
					{
						removefromtaggroup(c);
						c = c.prev;
					}
				}
			}
#endif
			a.prev.next = b.next;
			b.next.prev = a.prev;
			if (underlying != null)
				underlying.size -= count;

			size -= count;
		}

		
		#endregion

		#region ISequenced<T> Members

		[Tested]
		int ISequenced<T>.GetHashCode() { modifycheck(); return sequencedhashcode(); }


		[Tested]
		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ modifycheck(); return sequencedequals(that); }

		#endregion

		#region IDirectedCollection<T> Members

		/// <summary>
		/// Create a collection containing the same items as this collection, but
		/// whose enumerator will enumerate the items backwards. The new collection
		/// will become invalid if the original is modified. Method typicaly used as in
		/// <code>foreach (T x in coll.Backwards()) {...}</code>
		/// </summary>
		/// <returns>The backwards collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> Backwards()
		{ return this[0, size].Backwards(); }

		#endregion

		#region IDirectedEnumerable<T> Members

		[Tested]
		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }
		
		#endregion

		#region IEditableCollection<T> Members

		/// <summary>
		/// The value is symbolic indicating the type of asymptotic complexity
		/// in terms of the size of this collection (worst-case or amortized as
		/// relevant).
		/// </summary>
		/// <value>Speed.Linear</value>
		[Tested]
		public virtual Speed ContainsSpeed { [Tested]get { return Speed.Linear; } }


		[Tested]
		int ICollection<T>.GetHashCode()
		{ modifycheck(); return unsequencedhashcode(); }


		[Tested]
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
		{
			modifycheck();

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (itemhasher.Equals(item, node.item))
					return true;

				node = node.next;
			}

			return false;
		}


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

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (equals(item, node.item))
				{
					item = node.item;
					return true;
				}

				node = node.next;
			}

			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. Will update a single item.
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		[Tested]
		public virtual bool Update(T item)
		{
			updatecheck();

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (equals(item, node.item))
				{
					node.item = item;
					return true;
				}

				node = node.next;
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
		/// to with a binary copy of the supplied value; else add the value to the collection. 
		/// </summary>
		/// <param name="item">Value to add or update.</param>
		/// <returns>True if the item was updated (hence not added).</returns>
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
		/// Remove a particular item from this collection. Since the collection has bag
		/// semantics only one copy equivalent to the supplied item is removed. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public virtual bool Remove(T item)
		{
			updatecheck();

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (itemhasher.Equals(item, node.item))
				{
					remove(node);
					return true;
				}

				node = node.next;
			}

			return false;
		}


		/// <summary>
		/// Remove a particular item from this collection if found (only one copy). 
		/// If an item was removed, report a binary copy of the actual item removed in 
		/// the argument.
		/// </summary>
		/// <param name="item">The value to remove on input.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public virtual bool RemoveWithReturn(ref T item)
		{
			updatecheck();

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (itemhasher.Equals(item, node.item))
				{
					item = node.item;
					remove(node);
					return true;
				}

				node = node.next;
			}

			return false;
		}


		/// <summary>
		/// Remove all items in another collection from this one, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public virtual void RemoveAll(MSG.IEnumerable<T> items)
		{
			//Use an auxiliary hashbag should speed from O(n*m) to O(n+m) but use more memory
			updatecheck();
			if (size == 0)
				return;

			bool[] paired = new bool[size];
			int index, toretain = size;
			Node node;

			foreach (T item in items)
			{
				node = startsentinel.next;
				index = 0;
				while (node != endsentinel)
				{
					if (itemhasher.Equals(item, node.item) && !paired[index])
					{
						if (--toretain == 0)
						{
							clear();
							return;
						}

						paired[index] = true;
						goto cont;
					}

					node = node.next;
					index++;
				}
			cont :

				;
			}

			if (toretain == size)
				return;

			if (underlying != null)
				underlying.size -= size - toretain;

			node = startsentinel.next;
			size = toretain;
			index = 0;
			while (paired[index])
			{
#if EXTLISTORDER
				if (maintaintags) removefromtaggroup(node);
#endif
				node = node.next;
				index++;
			}

			if (index > 0)
			{
				startsentinel.next = node;
				node.prev = startsentinel;
			}

			while (true)
			{
				while (--toretain > 0 && !paired[++index])
					node = node.next;

				Node localend = node;

				if (toretain == 0)
				{
#if EXTLISTORDER
					node = node.next;
					while (node != endsentinel)
					{
						if (maintaintags) removefromtaggroup(node);

						node = node.next;
					}
#endif
					//fixup at end
					endsentinel.prev = localend;
					localend.next = endsentinel;
					break;
				}

				node = node.next;
				while (paired[index])
				{
#if EXTLISTORDER
					if (maintaintags) removefromtaggroup(node);
#endif
					node = node.next;
					index++;
				}

				localend.next = node;
				node.prev = localend;
			}
		}


		/// <summary>
		/// Remove all items from this collection.
		/// </summary>
		[Tested]
		public virtual void Clear()
		{
			updatecheck();
			clear();
		}


		void clear()
		{
#if EXTLISTORDER
			if (maintaintags)
			{
				if (underlying != null)
				{
					Node n = startsentinel.next;

					while (n != endsentinel)
					{
						n.next.prev = startsentinel;
						startsentinel.next = n.next;
						removefromtaggroup(n);
						n = n.next;
					}
				}
				else
				{
					taggroups = 0;
				}
			}
#endif
			endsentinel.prev = startsentinel;
			startsentinel.next = endsentinel;
			if (underlying != null)
				underlying.size -= size;

			size = 0;
		}


		/// <summary>
		/// Remove all items not in some other collection from this one, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to retain.</param>
		[Tested]
		public virtual void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();
			if (size == 0)
				return;

			bool[] paired = new bool[size];
			int index, pairs = 0;
			Node node;

			foreach (T item in items)
			{
				node = startsentinel.next;
				index = 0;
				while (node != endsentinel)
				{
					if (itemhasher.Equals(item, node.item) && !paired[index])
					{
						if (++pairs == size)
							return;

						paired[index] = true;
						goto cont;
					}

					node = node.next;
					index++;
				}
			cont :

				;
			}

			if (pairs == 0)
			{
				clear();
				return;
			}

			if (underlying != null)
				underlying.size -= size - pairs;

			node = startsentinel.next;
			size = pairs;
			index = 0;
			while (!paired[index])
			{
#if EXTLISTORDER
				if (maintaintags) removefromtaggroup(node);
#endif
				node = node.next;
				index++;
			}

			if (index > 0)
			{
				startsentinel.next = node;
				node.prev = startsentinel;
			}

			while (true)
			{
				while (--pairs > 0 && paired[++index])
					node = node.next;

				Node localend = node;

				if (pairs == 0)
				{
#if EXTLISTORDER
					node = node.next;
					while (node != endsentinel)
					{
						if (maintaintags) removefromtaggroup(node);

						node = node.next;
					}
#endif
					endsentinel.prev = localend;
					localend.next = endsentinel;
					break;
				}

				node = node.next;
				while (!paired[index])
				{
#if EXTLISTORDER
					if (maintaintags) removefromtaggroup(node);
#endif
					node = node.next;
					index++;
				}

				localend.next = node;
				node.prev = localend;
			}
		}


		/// <summary>
		/// Check if this collection contains all the values in another collection
		/// with respect to multiplicities.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		[Tested]
		public virtual bool ContainsAll(MSG.IEnumerable<T> items)
		{
			modifycheck();

			bool[] paired = new bool[size];

			foreach (T item in items)
			{
				int index = 0;
				Node node = startsentinel.next;

				while (node != endsentinel)
				{
					if (itemhasher.Equals(item, node.item) && !paired[index])
					{
						paired[index] = true;
						goto cont;
					}

					node = node.next;
					index++;
				}

				return false;
			cont :
				;
			}

			return true;
		}


		/// <summary>
		/// Create a new list consisting of the items of this list satisfying a 
		/// certain predicate.
		/// </summary>
		/// <param name="filter">The filter delegate defining the predicate.</param>
		/// <returns>The new list.</returns>
		[Tested]
		public IList<T> FindAll(Filter<T> filter)
		{
			LinkedList<T> retval = new LinkedList<T>();

			modifycheck();

			Node cursor = startsentinel.next;
			Node mcursor = retval.startsentinel;

#if LISTORDER
			double tagdelta = int.MaxValue / (size + 1.0);
#elif EXTLISTORDER
			double tagdelta = int.MaxValue / (size + 1.0);
			int count = 1;
			TagGroup taggroup = null;

			if (retval.maintaintags)
			{
				taggroup = new TagGroup();
				retval.taggroups = 1;
			}
#endif
			while (cursor != endsentinel)
			{
				if (filter(cursor.item))
				{
					mcursor.next = new Node(cursor.item, mcursor, null);
					mcursor = mcursor.next;
					retval.size++;
#if LISTORDER
					if (retval.maintaintags)
						mcursor.tag = (int)(retval.size * tagdelta);
#elif EXTLISTORDER
					if (retval.maintaintags)
					{
						mcursor.taggroup = taggroup;
						mcursor.tag = (int)(tagdelta * count++);
					}
#endif
				}

				cursor = cursor.next;
			}

#if EXTLISTORDER	
			if (retval.maintaintags)
				taggroup.count = retval.size;
#endif
			retval.endsentinel.prev = mcursor;
			mcursor.next = retval.endsentinel;
			return retval;
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
			int retval = 0;

			modifycheck();

			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				if (itemhasher.Equals(node.item, item))
					retval++;

				node = node.next;
			}

			return retval;
		}


		/// <summary>
		/// Remove all items equivalent to a given value.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		[Tested]
		public virtual void RemoveAllCopies(T item)
		{
			updatecheck();

			int removed = 0;
			Node node = startsentinel.next;

			while (node != endsentinel)
			{
				//Here we could loop to collect more matching adjacent nodes in one 
				//splice, but with some overhead for the general case.
				//se retailall for an example
				//if (node.item.Equals(item))
				if (itemhasher.Equals(node.item, item))
				{
					removed++;
					node.prev.next = node.next;
					node.next.prev = node.prev;
#if EXTLISTORDER
					if (maintaintags)
						removefromtaggroup(node);
#endif
				}

				node = node.next;
			}

			if (removed > 0)
			{
				size -= removed;
				if (underlying != null)
					underlying.size -= removed;
			}

			return;
		}
		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>The number of items in this collection</value>
		[Tested]
		public override int Count { [Tested]get { modifycheck(); return size; } }

		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Create an enumerator for the collection
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<T> GetEnumerator()
		{
			Node cursor = startsentinel.next;
			int startstamp = this.stamp;

			while (cursor != endsentinel)
			{
				modifycheck(startstamp);
				yield return cursor.item;
				cursor = cursor.next;
			}
		}

		#endregion

		#region ISink<T> Members
		/// <summary>
		/// Add an item to this collection if possible. 
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True.</returns>
		[Tested]
		public virtual bool Add(T item)
		{
			updatecheck();
			insert(endsentinel, item);
			return true;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <value>True since this collection has bag semantics.</value>
		[Tested]
		public virtual bool AllowsDuplicates { [Tested]get { return true; } }


		/// <summary>
		/// Add the elements from another collection to this collection. 
		/// </summary>
		/// <param name="items">The items to add.</param>
		[Tested]
		public virtual void AddAll(MSG.IEnumerable<T> items) { InsertAll(size, items); }

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. 
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public virtual void AddAll<U>(MSG.IEnumerable<U> items) where U : T
        {
            //TODO: implement
        }


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
        
        #region Diagnostic

        /// <summary>
		/// Check the sanity of this list
		/// </summary>
		/// <returns>true if sane</returns>
		[Tested]
		public virtual bool Check()
		{
			bool retval = true;

			if (underlying != null && underlying.stamp != stamp)
			{
				Console.WriteLine("underlying != null && underlying.stamp({0}) != stamp({1})", underlying.stamp, stamp);
				retval = false;
			}

			if (startsentinel == null)
			{
				Console.WriteLine("startsentinel == null");
				retval = false;
			}

			if (endsentinel == null)
			{
				Console.WriteLine("endsentinel == null");
				retval = false;
			}

			if (size == 0)
			{
				if (startsentinel != null && startsentinel.next != endsentinel)
				{
					Console.WriteLine("size == 0 but startsentinel.next != endsentinel");
					retval = false;
				}

				if (endsentinel != null && endsentinel.prev != startsentinel)
				{
					Console.WriteLine("size == 0 but endsentinel.prev != startsentinel");
					retval = false;
				}
			}

			if (startsentinel == null)
				return retval;

			int count = 0;
			Node node = startsentinel.next, prev = startsentinel;
#if EXTLISTORDER
			int taggroupsize = 0, oldtaggroupsize = losize + 1, seentaggroups = 0;
			TagGroup oldtg = null;

			if (maintaintags && underlying == null)
			{
				TagGroup tg = startsentinel.taggroup;

				if (tg.count != 0 || tg.first != null || tg.last != null || tg.tag != int.MinValue)
				{
					Console.WriteLine("Bad startsentinel tag group: {0}", tg);
					retval = false;
				}

				tg = endsentinel.taggroup;
				if (tg.count != 0 || tg.first != null || tg.last != null || tg.tag != int.MaxValue)
				{
					Console.WriteLine("Bad endsentinel tag group: {0}", tg);
					retval = false;
				}
			}
#endif
			while (node != endsentinel)
			{
				count++;
				if (node.prev != prev)
				{
					Console.WriteLine("Bad backpointer at node {0}", count);
					retval = false;
				}
#if LISTORDER
				if (maintaintags && node.prev.tag >= node.tag)
				{
					Console.WriteLine("node.prev.tag ({0}) >= node.tag ({1}) at index={2} item={3} ", node.prev.tag, node.tag, count, node.item);
					retval = false;
				}
#elif EXTLISTORDER
				if (maintaintags && underlying == null)
				{
					if (!node.prev.precedes(node))
					{
						Console.WriteLine("node.prev.tag ({0}, {1}) >= node.tag ({2}, {3}) at index={4} item={5} ", node.prev.taggroup.tag, node.prev.tag, node.taggroup.tag, node.tag, count, node.item);
						retval = false;
					}

					if (node.taggroup != oldtg)
					{
						if (oldtg != null)
						{
							if (oldtg.count != taggroupsize)
							{
								Console.WriteLine("Bad taggroupsize: oldtg.count ({0}) != taggroupsize ({1}) at index={2} item={3}", oldtg.count, taggroupsize, count, node.item);
								retval = false;
							}

							if (oldtaggroupsize <= losize && taggroupsize <= losize)
							{
								Console.WriteLine("Two small taggroups in a row: oldtaggroupsize ({0}), taggroupsize ({1}) at index={2} item={3}", oldtaggroupsize, taggroupsize, count, node.item);
								retval = false;
							}

							oldtaggroupsize = taggroupsize;
						}

						seentaggroups++;
						oldtg = node.taggroup;
						taggroupsize = 1;
					}
					else
					{
						taggroupsize++;
					}
				}
#endif
				prev = node;
				node = node.next;
				if (node == null)
				{
					Console.WriteLine("Null next pointer at node {0}", count);
					return false;
				}
			}

#if EXTLISTORDER
			if (maintaintags && underlying == null && size > 0)
			{
				oldtg = node.prev.taggroup;
				if (oldtg != null)
				{
					if (oldtg.count != taggroupsize)
					{
						Console.WriteLine("Bad taggroupsize: oldtg.count ({0}) != taggroupsize ({1}) at index={2} item={3}", oldtg.count, taggroupsize, count, node.item);
						retval = false;
					}

					if (oldtaggroupsize <= losize && taggroupsize <= losize)
					{
						Console.WriteLine("Two small taggroups in a row: oldtaggroupsize ({0}), taggroupsize ({1}) at index={2} item={3}", oldtaggroupsize, taggroupsize, count, node.item);
						retval = false;
					}
				}

				if (seentaggroups != taggroups)
				{
					Console.WriteLine("seentaggroups ({0}) != taggroups ({1}) (at size {2})", seentaggroups, taggroups, size);
					retval = false;
				}
			}
#endif
			if (count != size)
			{
				Console.WriteLine("size={0} but enumeration gives {1} nodes ", size, count);
				retval = false;
			}

			return retval;
		}

		#endregion	
	}
}
#endif
