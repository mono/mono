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

#define MAINTAIN_SIZE
#define MAINTAIN_RANKnot
#define MAINTAIN_HEIGHTnot
#define BAG
#define NCP

#define MAINTAIN_EXTREMAnot
#define TRACE_IDnot

#if BAG
#if !MAINTAIN_SIZE
#error  BAG defined without MAINTAIN_SIZE!
#endif
#endif


using System;
using MSG = System.Collections.Generic;

// NOTE NOTE NOTE NOTE
// This source file is used to produce both TreeBag<T> and TreeBag<T>
// It should be copied to a file called TreeBag.cs in which all code mentions of 
// TreeBag is changed to TreeBag and the preprocessor symbol BAG is defined.
// NOTE: there may be problems with documentation comments.

namespace C5
{
#if BAG
	/// <summary>
	/// An implementation of Red-Black trees as an indexed, sorted collection with bag semantics,
	/// cf. <a href="litterature.htm#CLRS">CLRS</a>. (<see cref="T:C5.TreeBag!1"/> for an 
	/// implementation with set semantics).
	/// <br/>
	/// The comparer (sorting order) may be either natural, because the item type is comparable 
	/// (generic: <see cref="T:C5.IComparable!1"/> or non-generic: System.IComparable) or it can
	/// be external and supplied by the user in the constructor.
	/// <br/>
	/// Each distinct item is only kept in one place in the tree - together with the number
	/// of times it is a member of the bag. Thus, if two items that are equal according
	/// </summary>
#else
	/// <summary>
	/// An implementation of Red-Black trees as an indexed, sorted collection with set semantics,
	/// cf. <a href="litterature.htm#CLRS">CLRS</a>. <see cref="T:C5.TreeBag!1"/> for a version 
	/// with bag semantics. <see cref="T:C5.TreeDictionary!2"/> for a sorted dictionary 
	/// based on this tree implementation.
	/// <p>
	/// The comparer (sorting order) may be either natural, because the item type is comparable 
	/// (generic: <see cref="T:C5.IComparable!1"/> or non-generic: System.IComparable) or it can
	/// be external and supplied by the user in the constructor.</p>
	///
	/// <p><i>TODO: describe performance here</i></p>
	/// <p><i>TODO: discuss persistence and its useful usage modes. Warn about the space
	/// leak possible with other usage modes.</i></p>
	/// </summary>
#endif
	public class TreeBag<T>: SequencedBase<T>, IIndexedSorted<T>, IPersistentSorted<T>
	{
		#region Feature
		/// <summary>
		/// A debugging aid for making the selected compilation alternatives 
		/// available to the user. (To be removed when selection is finally fixed
		/// for production version).
		/// </summary>
		[Flags]
		public enum Feature: short
		{
			/// <summary>
			/// Nothing
			/// </summary>
			Dummy = 0,
			/// <summary>
			/// Node copy persistence as explained in <a href="litterature.htm#Tarjan1">Tarjan1</a>
			/// </summary>
			NodeCopyPersistence = 2,
			/// <summary>
			/// Maintain sub tree sizes
			/// </summary>
			Sizes = 4,
			/// <summary>
			/// Maintain precise node heights
			/// </summary>
			Heights = 8,
			/// <summary>
			/// Maintain node ranks (~ black height)
			/// </summary>
			Ranks = 16,
			/// <summary>
			/// Maintain unique ids on tree nodes.
			/// </summary>
			Traceid = 32
		}



		static Feature features = Feature.Dummy
#if NCP
		| Feature.NodeCopyPersistence
#endif
#if MAINTAIN_RANK
			|Feature.Ranks
#endif
#if MAINTAIN_HEIGHT
			|Feature.Heights
#endif
#if MAINTAIN_SIZE
		| Feature.Sizes
#endif
#if TRACE_ID
		| Feature.Traceid
#endif
		;


		/// <summary>
		/// A debugging aid for making the selected compilation alternatives 
		/// available to the user. (To be removed when selection is finally fixed
		/// for production version).
		/// </summary>
		public static Feature Features { get { return features; } }

		#endregion

		#region Fields

		IComparer<T> comparer;

		Node root;

		int blackdepth = 0;

		//We double these stacks for the iterative add and remove on demand
		private int[] dirs = new int[2];

		private Node[] path = new Node[2];
#if NCP
		private bool isSnapShot = false;

		private SnapData snapdata;

		private int generation;

		private int maxsnapid = -1;

#endif
#if MAINTAIN_EXTREMA
		T min, max;
#endif
#if MAINTAIN_HEIGHT
		private short depth = 0;
#endif
		#endregion

		#region Util

		/// <summary>
		/// Fetch the left child of n taking node-copying persistence into
		/// account if relevant. 
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		private Node left(Node n)
		{
#if NCP
			if (isSnapShot)
			{
#if SEPARATE_EXTRA
				Node.Extra e = n.extra;

				if (e != null && e.lastgeneration >= treegen && e.leftnode)
					return e.oldref;
#else
				if (n.lastgeneration >= generation && n.leftnode)
					return n.oldref;
#endif
			}
#endif
			return n.left;
		}


		private Node right(Node n)
		{
#if NCP
			if (isSnapShot)
			{
#if SEPARATE_EXTRA
				Node.Extra e = n.extra;

				if (e != null && e.lastgeneration >= treegen && !e.leftnode)
					return e.oldref;
#else
				if (n.lastgeneration >= generation && !n.leftnode)
					return n.oldref;
#endif
			}
#endif
			return n.right;
		}


		//This method should be called by methods that use the internal 
		//traversal stack, unless certain that there is room enough
		private void stackcheck()
		{
			while (dirs.Length < 2 * blackdepth)
			{
				dirs = new int[2 * dirs.Length];
				path = new Node[2 * dirs.Length];
			}
		}

		#endregion

		#region Node nested class
			

		/// <summary>
		/// The type of node in a Red-Black binary tree
		/// </summary>
		class Node
		{
			public bool red = true;

			public T item;

			public Node left;

			public Node right;

#if MAINTAIN_SIZE
			public int size = 1;
#endif

#if BAG
			public int items = 1;
#endif

#if MAINTAIN_HEIGHT
			public short height; 
#endif

#if MAINTAIN_RANK
			public short rank = 1;
#endif

#if TRACE_ID
			public int id = sid++;
			public static int sid = 0;
#endif

#if NCP
			public int generation;
#if SEPARATE_EXTRA
			internal class Extra
			{
				public int lastgeneration;

				public Node oldref;

				public bool leftnode;

				//public Node next;
			}

			public Extra extra;

#else
			public int lastgeneration = -1;

			public Node oldref;

			public bool leftnode;
#endif

			/// <summary>
			/// Update a child pointer
			/// </summary>
			/// <param name="cursor"></param>
			/// <param name="leftnode"></param>
			/// <param name="child"></param>
			/// <param name="maxsnapid"></param>
			/// <param name="generation"></param>
			/// <returns>True if node was *copied*</returns>
			internal static bool update(ref Node cursor, bool leftnode, Node child, int maxsnapid, int generation)
			{
				Node oldref = leftnode ? cursor.left : cursor.right;

				if (child == oldref)
					return false;

				bool retval = false;

				if (cursor.generation <= maxsnapid)
				{ 
#if SEPARATE_EXTRA
					if (cursor.extra == null)
					{
						Extra extra = cursor.extra = new Extra();	

						extra.leftnode = leftnode;
						extra.lastgeneration = maxsnapid;
						extra.oldref = oldref;
					}
					else if (cursor.extra.leftnode != leftnode || cursor.extra.lastgeneration < maxsnapid)
#else
					if (cursor.lastgeneration == -1)
					{
						cursor.leftnode = leftnode;
						cursor.lastgeneration = maxsnapid;
						cursor.oldref = oldref;
					}
					else if (cursor.leftnode != leftnode || cursor.lastgeneration < maxsnapid)
#endif
					{
						CopyNode(ref cursor, maxsnapid, generation);
						retval = true;
					}
				}

				if (leftnode)
					cursor.left = child;
				else
					cursor.right = child;

				return retval;
			}


			//If cursor.extra.lastgeneration==maxsnapid, the extra pointer will 
			//always be used in the old copy of cursor. Therefore, after 
			//making the clone, we should update the old copy by restoring
			//the child pointer and setting extra to null.
			//OTOH then we cannot clean up unused Extra objects unless we link
			//them together in a doubly linked list.
			public static bool CopyNode(ref Node cursor, int maxsnapid, int generation)
			{
				if (cursor.generation <= maxsnapid)
				{
					cursor = (Node)(cursor.MemberwiseClone());
					cursor.generation = generation;
#if SEPARATE_EXTRA
					cursor.extra = null;
#else
					cursor.lastgeneration = -1;
#endif
#if TRACE_ID
					cursor.id = sid++;
#endif
					return true;
				}
				else
					return false;
			}

#endif
		}

		#endregion

		#region Constructors
			
		/// <summary>
		/// Create a red-black tree collection with natural comparer and item hasher.
		/// </summary>
		public TreeBag()
		{
			comparer = ComparerBuilder.FromComparable<T>.Examine();
			itemhasher = HasherBuilder.ByPrototype<T>.Examine();
		}


		/// <summary>
		/// Create a red-black tree collection with an external comparer (and natural item hasher,
		/// assumed consistent).
		/// </summary>
		/// <param name="c">The external comparer</param>
		public TreeBag(IComparer<T> c)
		{
			comparer = c;
			itemhasher = HasherBuilder.ByPrototype<T>.Examine();
		}


		/// <summary>
		/// Create a red-black tree collection with an external comparer aand an external
		/// item hasher, assumed consistent.
		/// </summary>
		/// <param name="c">The external comparer</param>
		/// <param name="h">The external item hasher</param>
		public TreeBag(IComparer<T> c, IHasher<T> h)
		{
			comparer = c;
			itemhasher = h;
		}

		#endregion

		#region TreeBag.Enumerator nested class

		/// <summary>
		/// An enumerator for a red-black tree collection. Based on an explicit stack
		/// of subtrees waiting to be enumerated. Currently only used for the tree set 
		/// enumerators (tree bag enumerators use an iterator block based enumerator).
		/// </summary>
		public class Enumerator: MSG.IEnumerator<T>
		{
			#region Private Fields
			TreeBag<T> tree;

			bool valid = false;

			int stamp;

			T current;

			Node cursor;

			Node[] path; // stack of nodes

			int level = 0;
			#endregion
			/// <summary>
			/// Create a tree enumerator
			/// </summary>
			/// <param name="tree">The red-black tree to enumerate</param>
			public Enumerator(TreeBag<T> tree)
			{
				this.tree = tree;
				stamp = tree.stamp;
				path = new Node[2 * tree.blackdepth];
				cursor = new Node();
				cursor.right = tree.root;
			}


			/// <summary>
			/// Undefined if enumerator is not valid (MoveNext hash been called returning true)
			/// </summary>
			/// <value>The current item of the enumerator.</value>
			[Tested]
			public T Current
			{
				[Tested]
				get
				{
					if (valid)
						return current;
					else
						throw new InvalidOperationException();
				}
			}


			//Maintain a stack of nodes that are roots of
			//subtrees not completely exported yet. Invariant:
			//The stack nodes together with their right subtrees
			//consists of exactly the items we have not passed
			//yet (the top of the stack holds current item).
			/// <summary>
			/// Move enumerator to next item in tree, or the first item if
			/// this is the first call to MoveNext. 
			/// <exception cref="InvalidOperationException"/> if underlying tree was modified.
			/// </summary>
			/// <returns>True if enumerator is valid now</returns>
			[Tested]
			public bool MoveNext()
			{
				tree.modifycheck(stamp);
				if (cursor.right != null)
				{
					path[level] = cursor = cursor.right;
					while (cursor.left != null)
						path[++level] = cursor = cursor.left;
				}
				else if (level == 0)
					return valid = false;
				else
					cursor = path[--level];

				current = cursor.item;
				return valid = true;
			}


			#region IDisposable Members for Enumerator

			bool disposed;


			/// <summary>
			/// Call Dispose(true) and then suppress finalization of this enumerator.
			/// </summary>
			[Tested]
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}


			/// <summary>
			/// Remove the internal data (notably the stack array).
			/// </summary>
			/// <param name="disposing">True if called from Dispose(),
			/// false if called from the finalizer</param>
			protected virtual void Dispose(bool disposing)
			{
				if (!disposed)
				{
					if (disposing)
					{
					}

					current = default(T);
					cursor = null;
					path = null;
					disposed = true;
				}
			}


			/// <summary>
			/// Finalizer for enumeratir
			/// </summary>
			~Enumerator()
			{
				Dispose(false);
			}
			#endregion

		}
#if NCP
		/// <summary>
		/// An enumerator for a snapshot of a node copy persistent red-black tree
		/// collection.
		/// </summary>
		public class SnapEnumerator: MSG.IEnumerator<T>
		{
			#region Private Fields
			TreeBag<T> tree;

			bool valid = false;

			int stamp;
#if BAG
			int togo;
#endif

			T current;

			Node cursor;

			Node[] path; // stack of nodes

			int level;
			#endregion

			/// <summary>
			/// Creta an enumerator for a snapshot of a node copy persistent red-black tree
			/// collection
			/// </summary>
			/// <param name="tree">The snapshot</param>
			public SnapEnumerator(TreeBag<T> tree)
			{
				this.tree = tree;
				stamp = tree.stamp;
				path = new Node[2 * tree.blackdepth];
				cursor = new Node();
				cursor.right = tree.root;
			}


			#region MSG.IEnumerator<T> Members

			/// <summary>
			/// Move enumerator to next item in tree, or the first item if
			/// this is the first call to MoveNext. 
			/// <exception cref="InvalidOperationException"/> if underlying tree was modified.
			/// </summary>
			/// <returns>True if enumerator is valid now</returns>
			[Tested]
			public bool MoveNext()
			{
				tree.modifycheck(stamp);//???

#if BAG
				if (--togo>0)
					return true;
#endif
				Node next = tree.right(cursor);

				if (next != null)
				{
					path[level] = cursor = next;
					next = tree.left(cursor);
					while (next != null)
					{
						path[++level] = cursor = next;
						next = tree.left(cursor);
					}
				}
				else if (level == 0)
					return valid = false;
				else
					cursor = path[--level];

#if BAG
				togo = cursor.items;
#endif
				current = cursor.item;
				return valid = true;
			}


			/// <summary>
			/// Undefined if enumerator is not valid (MoveNext hash been called returning true)
			/// </summary>
			/// <value>The current value of the enumerator.</value>
			[Tested]
			public T Current
			{
				[Tested]
				get
				{
					if (valid)
						return current;
					else
						throw new InvalidOperationException();
				}
			}

			#endregion

			#region IDisposable Members

			[Tested]
			void System.IDisposable.Dispose()
			{
				tree = null;
				valid = false;
				current = default(T);
				cursor = null;
				path = null;
			}

			#endregion
		}
#endif
		#endregion

		#region IEnumerable<T> Members

		private MSG.IEnumerator<T> getEnumerator(Node node, int origstamp)
		{
			if (node == null)
				yield break;

			if (node.left != null)
			{
				MSG.IEnumerator<T> child = getEnumerator(node.left, origstamp);

				while (child.MoveNext())
				{
					modifycheck(origstamp);
					yield return child.Current;
				}
			}
#if BAG
			int togo = node.items;
			while (togo-- > 0)
			{
				modifycheck(origstamp);
				yield return node.item;
			}
#else
			modifycheck(origstamp);
			yield return node.item;
#endif
			if (node.right != null)
			{
				MSG.IEnumerator<T> child = getEnumerator(node.right, origstamp);

				while (child.MoveNext())
				{
					modifycheck(origstamp);
					yield return child.Current;
				}
			}
		}


		/// <summary>
		/// Create an enumerator for this tree
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<T> GetEnumerator()
		{
#if NCP
			if (isSnapShot)
				return new SnapEnumerator(this);
#endif
#if BAG
			return getEnumerator(root,stamp);
#else
			return new Enumerator(this);
#endif
		}

		#endregion

		#region ISink<T> Members
			
		/// <summary>
		/// Add item to tree. If already there, return the found item in the second argument.
		/// </summary>
		/// <param name="item">Item to add</param>
        /// <param name="founditem">item found</param>
        /// <param name="update">whether item in node should be updated</param>
        /// <param name="wasfound">true if found in bag, false if not found or tre is a set</param>
        /// <returns>True if item was added</returns>
        bool addIterative(T item, ref T founditem, bool update, out bool wasfound)
        {
            wasfound = false;
            if (root == null)
			{
				root = new Node();
				root.red = false;
				blackdepth = 1;
#if MAINTAIN_EXTREMA
				root.item = min = max = item;
#else
				root.item = item;
#endif
#if NCP
				root.generation = generation;
#endif
#if MAINTAIN_HEIGHT
				depth = 0;
#endif
				return true;
			}

			stackcheck();

			int level = 0;
			Node cursor = root;

			while (true)
			{
                int comp = comparer.Compare(cursor.item, item);

                if (comp == 0)
				{
                    founditem = cursor.item;

#if BAG
                    wasfound = true;
#if NCP
					Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
					cursor.items++;
					cursor.size++;
					if (update)
						cursor.item = item;

					update = true;

#else
                    if (update)
                    {
#if NCP
                        Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
                        cursor.item = item;
                    }
#endif

                    while (level-- > 0)
                    {
                        if (update)
						{
							Node kid = cursor;

							cursor = path[level];
#if NCP
							Node.update(ref cursor, dirs[level] > 0, kid, maxsnapid, generation);
#endif
#if BAG
							cursor.size++;
#endif
						}

						path[level] = null;
					}
#if BAG
					return true;
#else
					if (update)
						root = cursor;

					return false;
#endif
				}

				//else
				Node child = comp > 0 ? cursor.left : cursor.right;

				if (child == null)
				{
					child = new Node();
					child.item = item;
#if NCP
					child.generation = generation;
					Node.update(ref cursor, comp > 0, child, maxsnapid, generation);
#else
					if (comp > 0) { cursor.left = child; }
					else { cursor.right = child; }
#endif
#if MAINTAIN_SIZE
					cursor.size++;
#endif
#if MAINTAIN_HEIGHT
					fixheight(cursor);
#endif
					dirs[level] = comp;
					break;
				}
				else
				{
					dirs[level] = comp;
					path[level++] = cursor;
					cursor = child;
				}
			}

			//We have just added the red node child to "cursor"
			while (cursor.red)
			{
				//take one step up:
				Node child = cursor;

				cursor = path[--level];
				path[level] = null;
#if NCP
				Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#endif
#if MAINTAIN_SIZE
				cursor.size++;
#endif
				int comp = dirs[level];
				Node childsibling = comp > 0 ? cursor.right : cursor.left;

				if (childsibling != null && childsibling.red)
				{
					//Promote
#if MAINTAIN_RANK
					cursor.rank++;
#endif
#if MAINTAIN_HEIGHT
					fixheight(cursor);
#endif
					child.red = false;
#if NCP
					Node.update(ref cursor, comp < 0, childsibling, maxsnapid, generation);
#endif
					childsibling.red = false;

					//color cursor red & take one step up the tree unless at root
					if (level == 0)
					{
						root = cursor;
						blackdepth++;
						return true;
					}
					else
					{
						cursor.red = true;
#if NCP
						child = cursor;
						cursor = path[--level];
						Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#endif
						path[level] = null;
#if MAINTAIN_SIZE
						cursor.size++;
#endif
#if MAINTAIN_HEIGHT
						fixheight(cursor);
#endif
					}
				}
				else
				{
					//ROTATE!!!
					int childcomp = dirs[level + 1];

					cursor.red = true;
					if (comp > 0)
					{
						if (childcomp > 0)
						{//zagzag
#if NCP
							Node.update(ref cursor, true, child.right, maxsnapid, generation);
							Node.update(ref child, false, cursor, maxsnapid, generation);
#else
							cursor.left = child.right;
							child.right = cursor;
#endif
							cursor = child;
						}
						else
						{//zagzig
							Node badgrandchild = child.right;
#if NCP
							Node.update(ref cursor, true, badgrandchild.right, maxsnapid, generation);
							Node.update(ref child, false, badgrandchild.left, maxsnapid, generation);
							Node.CopyNode(ref badgrandchild, maxsnapid, generation);
#else
							cursor.left = badgrandchild.right;
							child.right = badgrandchild.left;
#endif
							badgrandchild.left = child;
							badgrandchild.right = cursor;
							cursor = badgrandchild;
						}
					}
					else
					{//comp < 0
						if (childcomp < 0)
						{//zigzig
#if NCP
							Node.update(ref cursor, false, child.left, maxsnapid, generation);
							Node.update(ref child, true, cursor, maxsnapid, generation);
#else
							cursor.right = child.left;
							child.left = cursor;
#endif
							cursor = child;
						}
						else
						{//zigzag
							Node badgrandchild = child.left;
#if NCP
							Node.update(ref cursor, false, badgrandchild.left, maxsnapid, generation);
							Node.update(ref child, true, badgrandchild.right, maxsnapid, generation);
							Node.CopyNode(ref badgrandchild, maxsnapid, generation);
#else
							cursor.right = badgrandchild.left;
							child.left = badgrandchild.right;
#endif
							badgrandchild.right = child;
							badgrandchild.left = cursor;
							cursor = badgrandchild;
						}
					}

					cursor.red = false;

#if MAINTAIN_SIZE
					Node n;

#if BAG
					n = cursor.right;
					cursor.size = n.size = (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + n.items;
					n = cursor.left;
					n.size = (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + n.items;
					cursor.size += n.size + cursor.items;
#else
					n = cursor.right;
					cursor.size = n.size = (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + 1;
					n = cursor.left;
					n.size = (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + 1;
					cursor.size += n.size + 1;
#endif
#endif
#if MAINTAIN_HEIGHT
					fixheight(cursor.right);
					fixheight(cursor.left);
					fixheight(cursor);
#endif
					if (level == 0)
					{
						root = cursor;
						return true;
					}
					else
					{
						child = cursor;
						cursor = path[--level];
						path[level] = null;
#if NCP
						Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#else
						if (dirs[level] > 0)
							cursor.left = child;
						else
							cursor.right = child;
#endif
#if MAINTAIN_SIZE
						cursor.size++;
#endif
#if MAINTAIN_HEIGHT
						fixheight(cursor);
#endif
						break;
					}
				}
			}
#if NCP
			bool stillmore = true;
#endif
			while (level > 0)
			{
				Node child = cursor;

				cursor = path[--level];
				path[level] = null;
#if NCP
				if (stillmore)
					stillmore = Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#endif
#if MAINTAIN_SIZE
				cursor.size++;
#endif
#if MAINTAIN_HEIGHT
				fixheight(cursor);
#endif
			}

			root = cursor;
			return true;
		}


		/// <summary>
		/// Add an item to this collection if possible. If this collection has set
		/// semantics, the item will be added if not already in the collection. If
		/// bag semantics, the item will always be added.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True if item was added.</returns>
		[Tested]
		public bool Add(T item)
		{
			updatecheck();

			//Note: blackdepth of the tree is set inside addIterative
			T j = default(T);
            bool tmp;

            if (addIterative(item, ref j, false, out tmp))
			{
				size++;
#if MAINTAIN_EXTREMA
				if (Compare(item, min) < 0)
					min = item;
				else if (Compare(item, max) > 0)
					max = item;
#endif
#if MAINTAIN_HEIGHT
				depth = root.height;
#endif
				return true;
			}
			else
				return false;
		}


		/// <summary>
		/// Add the elements from another collection to this collection. If this
		/// collection has set semantics, only items not already in the collection
		/// will be added.
		/// </summary>
		/// <param name="items">The items to add.</param>
		[Tested]
		public void AddAll(MSG.IEnumerable<T> items)
		{
			int c = 0;
			T j = default(T);
            bool tmp;

            updatecheck();
			foreach (T i in items)
				if (addIterative(i, ref j, false, out tmp)) c++;

			size += c;
		}

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. If this
        /// collection has set semantics, only items not already in the collection
        /// will be added.
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public void AddAll<U>(MSG.IEnumerable<U> items) where U : T
        {
            int c = 0;
            T j = default(T);
            bool tmp;

            updatecheck();
            foreach (T i in items)
                if (addIterative(i, ref j, false, out tmp)) c++;

            size += c;
        }


        /// <summary>
		/// Add all the items from another collection with an enumeration order that 
		/// is increasing in the items. <para>The idea is that the implementation may use
		/// a faster algorithm to merge the two collections.</para>
		/// <exception cref="ArgumentException"/> if the enumerated items turns out
		/// not to be in increasing order.
		/// </summary>
		/// <param name="items">The collection to add.</param>
		[Tested]
		public void AddSorted(MSG.IEnumerable<T> items)
		{
			if (size > 0)
				AddAll(items);
			else
			{
				updatecheck();
				addSorted(items, true);
			}
		}

		#region add-sorted helpers
		
		//Create a RB tree from x+2^h-1  (x < 2^h, h>=1) nodes taken from a
		//singly linked list of red nodes using only the right child refs.
		//The x nodes at depth h+1 will be red, the rest black.
		//(h is the blackdepth of the resulting tree)
		static Node maketreer(ref Node rest, int blackheight, int maxred, int red)
		{
			if (blackheight == 1)
			{
				Node top = rest;

				rest = rest.right;
				if (red > 0)
				{
					top.right = null;
					rest.left = top;
					top = rest;
#if BAG
					top.size += top.left.size;
#elif MAINTAIN_SIZE
					top.size = 1 + red;
#endif
					rest = rest.right;
					red--;
				}

				if (red > 0)
				{
#if BAG
					top.size += rest.size;
#endif
					top.right = rest;
					rest = rest.right;
					top.right.right = null;
				}
				else
					top.right = null;

				top.red = false;
				return top;
			}
			else
			{
				maxred >>=1;

				int lred = red > maxred ? maxred : red;
				Node left = maketreer(ref rest, blackheight - 1, maxred, lred);
				Node top = rest;

				rest = rest.right;
				top.left = left;
				top.red = false;
#if MAINTAIN_RANK
				top.rank = (short)blackheight;
#endif
				top.right = maketreer(ref rest, blackheight - 1, maxred, red - lred);
#if BAG
				top.size = top.items + top.left.size + top.right.size;
#elif MAINTAIN_SIZE
				top.size = (maxred << 1) - 1 + red;
#endif
				return top;
			}
		}


		void addSorted(MSG.IEnumerable<T> items, bool safe)
		{
			MSG.IEnumerator<T> e = items.GetEnumerator();;
			if (size > 0)
				throw new ApplicationException("This can't happen");

			if (!e.MoveNext())
				return;

			//To count theCollect 
			Node head = new Node(), tail = head;
			int z = 1;
			T lastitem = tail.item = e.Current;
#if BAG
			int ec=0;
#endif

			while (e.MoveNext())
			{
#if BAG
				T thisitem = e.Current;
				int comp = comparer.Compare(lastitem, thisitem);
				if (comp>0)
					throw new ArgumentException("Argument not sorted");
				if (comp == 0)
				{
					tail.items++;
					ec++;
				}
				else
				{
					tail.size = tail.items;
					z++;
					tail.right = new Node();
					tail = tail.right;
					lastitem = tail.item = thisitem;
#if NCP
					tail.generation = generation;
#endif
				}
#else
				z++;
				tail.right = new Node();
				tail = tail.right;
				tail.item = e.Current;
				if (safe)
				{
					if (comparer.Compare(lastitem, tail.item) >= 0)
						throw new ArgumentException("Argument not sorted");

					lastitem = tail.item;
				}
#if NCP
				tail.generation = generation;
#endif
#endif
			}
#if BAG
			tail.size = tail.items;
#endif				
			int blackheight = 0, red = z, maxred = 1;

			while (maxred <= red)
			{
				red -= maxred;
				maxred <<= 1;
				blackheight++;
			}

			root = TreeBag<T>.maketreer(ref head, blackheight, maxred, red);
			blackdepth = blackheight;
			size = z;
#if BAG
			size += ec;
#endif				
			return;
		}

		#endregion

#if BAG
		/// <summary></summary>
		/// <value>True since this collection has bag semantics.</value>
		[Tested]
		public bool AllowsDuplicates { [Tested]get { return true; } }
#else
		/// <summary></summary>
		/// <value>False since this tree has set semantics.</value>
		[Tested]
		public bool AllowsDuplicates { [Tested]get { return false; } }
#endif

		#endregion

		#region IEditableCollection<T> Members
			

		/// <summary>
		/// The value is symbolic indicating the type of asymptotic complexity
		/// in terms of the size of this collection (worst-case or amortized as
		/// relevant).
		/// </summary>
		/// <value>Speed.Log</value>
		[Tested]
		public Speed ContainsSpeed { [Tested]get { return Speed.Log; } }


		[Tested]
		int ICollection<T>.GetHashCode() { return unsequencedhashcode(); }


		[Tested]
		bool ICollection<T>.Equals(ICollection<T> that)
		{ return unsequencedequals(that); }


		/// <summary>
		/// Check if this collection contains (an item equivalent to according to the
		/// itemhasher) a particular value.
		/// </summary>
		/// <param name="item">The value to check for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public bool Contains(T item)
		{
			Node next; int comp = 0;

			next = root;
			while (next != null)
			{
                comp = comparer.Compare(next.item, item);
                if (comp == 0)
					return true;

				next = comp < 0 ? right(next) : left(next);
			}

			return false;
		}


		//Variant for dictionary use
		//Will return the actual matching item in the ref argument.
		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the items is in this collection.</returns>
		[Tested]
		public bool Find(ref T item)
		{
			Node next; int comp = 0;

			next = root;
			while (next != null)
			{
                comp = comparer.Compare(next.item, item);
                if (comp == 0)
				{
					item = next.item;
					return true;
				}

				next = comp < 0 ? right(next) : left(next);
			}

			return false;
		}


		/// <summary>
		/// Find or add the item to the tree. If the tree does not contain
		/// an item equivalent to this item add it, else return the exisiting
		/// one in the ref argument. 
		///
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if item was found</returns>
		[Tested]
		public bool FindOrAdd(ref T item)
		{
			updatecheck();
            bool wasfound;

            //Note: blackdepth of the tree is set inside addIterative
			if (addIterative(item, ref item, false, out wasfound))
			{
				size++;
#if MAINTAIN_EXTREMA
				if (Compare(item, min) < 0)
					min = item;
				else if (Compare(item, max) > 0)
					max = item;
#endif
#if MAINTAIN_HEIGHT
				depth = root.height;
#endif
				return wasfound;
			}
			else
				return true;

		}


		//For dictionary use. 
		//If found, the matching entry will be updated with the new item.
		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. If the collection has bag semantics,
		/// this updates all equivalent copies in
		/// the collection.
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		[Tested]
		public bool Update(T item)
		{
			updatecheck();
#if NCP
			stackcheck();

			int level = 0;
#endif
			Node cursor = root;
			int comp = 0;

			while (cursor != null)
			{
                comp = comparer.Compare(cursor.item, item);
                if (comp == 0)
				{
#if NCP
					Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
					cursor.item = item;
#if NCP
					while (level > 0)
					{
						Node child = cursor;

						cursor = path[--level];
						path[level] = null;
#if NCP
						Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#else
						if (Node.CopyNode(maxsnapid, ref cursor, generation))
						{
							if (dirs[level] > 0)
								cursor.left = child;
							else
								cursor.right = child;
						}
#endif
					}

					root = cursor;
#endif
					return true;
				}
#if NCP
				dirs[level] = comp;
				path[level++] = cursor;
#endif
				cursor = comp < 0 ? cursor.right : cursor.left;
			}

			return false;
		}


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value; else add the value to the collection. 
		///
		/// <p>NOTE: the bag implementation is currently wrong!</p>
		/// </summary>
		/// <param name="item">Value to add or update.</param>
		/// <returns>True if the item was found and updated (hence not added).</returns>
		[Tested]
		public bool UpdateOrAdd(T item)
		{
			updatecheck();
            bool wasfound;

            //Note: blackdepth of the tree is set inside addIterative
			if (addIterative(item, ref item, true, out wasfound))
			{
				size++;
#if MAINTAIN_EXTREMA
				if (Compare(item, min) < 0)
					min = item;
				else if (Compare(item, max) > 0)
					max = item;
#endif
#if MAINTAIN_HEIGHT
				depth = root.height;
#endif
				return wasfound;
			}
			else
				return true;
		}


		/// <summary>
		/// Remove a particular item from this collection. If the collection has bag
		/// semantics only one copy equivalent to the supplied item is removed. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public bool Remove(T item)
		{
			updatecheck();
			if (root == null)
				return false;

			return removeIterative(ref item, false);
		}

		/// <summary>
		/// Remove a particular item from this collection if found. If the collection
		/// has bag semantics only one copy equivalent to the supplied item is removed,
		/// which one is implementation dependent. 
		/// If an item was removed, report a binary copy of the actual item removed in 
		/// the argument.
		/// </summary>
		/// <param name="item">The value to remove on input.</param>
		/// <returns>True if the item was found (and removed).</returns>
		[Tested]
		public bool RemoveWithReturn(ref T item)
		{
			updatecheck();
			if (root == null)
				return false;

			return removeIterative(ref item, false);
		}


		private bool removeIterative(ref T item, bool all)
		{
			//Stage 1: find item
			stackcheck();

			int level = 0, comp;
			Node cursor = root;

			while (true)
			{
                comp = comparer.Compare(cursor.item, item);
                if (comp == 0)
				{
					item = cursor.item;
#if BAG
					if (!all && cursor.items > 1)
					{
#if NCP
						Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
						cursor.items--;
						cursor.size--;
						while (level-- > 0)
						{
							Node kid = cursor;

							cursor = path[level];
#if NCP
							Node.update(ref cursor, dirs[level] > 0,  kid,maxsnapid,generation);
#endif
							cursor.size--;
							path[level] = null;
						}
						size--;
						return true;
					}
#endif
					break;
				}

				Node child = comp > 0 ? cursor.left : cursor.right;

				if (child == null)
					return false;

				dirs[level] = comp;
				path[level++] = cursor;
				cursor = child;
			}

			return removeIterativePhase2(cursor, level);
		}


		private bool removeIterativePhase2(Node cursor, int level)
		{
			if (size == 1)
			{
				clear();
				return true;
			}

#if MAINTAIN_EXTREMA
			if (Compare(cursor.item, min) == 0)
				min = cursor.right != null ? cursor.right.item : path[level - 1].item;
			else if (Compare(cursor.item, max) == 0)
				max = cursor.left != null ? cursor.left.item : path[level - 1].item;
#endif
#if BAG
			int removedcount = cursor.items;
			size -= removedcount;
#else
			//We are certain to remove one node:
			size--;
#endif
			//Stage 2: if item's node has no null child, find predecessor
			int level_of_item = level;

			if (cursor.left != null && cursor.right != null)
			{
				dirs[level] = 1;
				path[level++] = cursor;
				cursor = cursor.left;
				while (cursor.right != null)
				{
					dirs[level] = -1;
					path[level++] = cursor;
					cursor = cursor.right;
				}
#if NCP
				Node.CopyNode(ref path[level_of_item], maxsnapid, generation);
#endif
				path[level_of_item].item = cursor.item;
#if BAG
				path[level_of_item].items = cursor.items;
#endif
			}

			//Stage 3: splice out node to be removed
			Node newchild = cursor.right == null ? cursor.left : cursor.right;
			bool demote_or_rotate = newchild == null && !cursor.red;

			//assert newchild.red 
			if (newchild != null)
			{
				newchild.red = false;
			}

			if (level == 0)
			{
				root = newchild;
#if MAINTAIN_HEIGHT
				depth = 0;
#endif
				return true;
			}

			level--;
			cursor = path[level];
			path[level] = null;

			int comp = dirs[level];
			Node childsibling;
#if NCP
			Node.update(ref cursor, comp > 0, newchild, maxsnapid, generation);
#else
			if (comp > 0)
				cursor.left = newchild;
			else
				cursor.right = newchild;
#endif
			childsibling = comp > 0 ? cursor.right : cursor.left;
#if BAG
			cursor.size -= removedcount;
#elif MAINTAIN_SIZE
			cursor.size--;
#endif
#if MAINTAIN_HEIGHT
			fixheight(cursor);
#endif

			//Stage 4: demote till we must rotate
			Node farnephew = null, nearnephew = null;

			while (demote_or_rotate)
			{
				if (childsibling.red)
					break; //rotate 2+?

				farnephew = comp > 0 ? childsibling.right : childsibling.left;
				if (farnephew != null && farnephew.red)
					break; //rotate 1b

				nearnephew = comp > 0 ? childsibling.left : childsibling.right;
				if (nearnephew != null && nearnephew.red)
					break; //rotate 1c

				//demote cursor
				childsibling.red = true;
#if MAINTAIN_RANK
				cursor.rank--;
#endif
				if (level == 0)
				{
					cursor.red = false;
					blackdepth--;
#if MAINTAIN_HEIGHT
					depth = root.height;
#endif
#if NCP
					root = cursor;
#endif
					return true;
				}
				else if (cursor.red)
				{
					cursor.red = false;
					demote_or_rotate = false;
					break; //No rotation
				}
				else
				{
					Node child = cursor;

					cursor = path[--level];
					path[level] = null;
					comp = dirs[level];
					childsibling = comp > 0 ? cursor.right : cursor.left;
#if NCP
					Node.update(ref cursor, comp > 0, child, maxsnapid, generation);
#endif
#if BAG
					cursor.size -= removedcount;
#elif MAINTAIN_SIZE
					cursor.size--;
#endif
#if MAINTAIN_HEIGHT
					fixheight(cursor);
#endif
				}
			}

			//Stage 5: rotate 
			if (demote_or_rotate)
			{
				//At start:
				//parent = cursor (temporary for swapping nodes)
				//childsibling is the sibling of the updated child (x)
				//cursor is always the top of the subtree
				Node parent = cursor;

				if (childsibling.red)
				{//Case 2 and perhaps more. 
					//The y.rank == px.rank >= x.rank+2 >=2 so both nephews are != null 
					//(and black). The grandnephews are children of nearnephew
					Node neargrandnephew, fargrandnephew;

					if (comp > 0)
					{
						nearnephew = childsibling.left;
						farnephew = childsibling.right;
						neargrandnephew = nearnephew.left;
						fargrandnephew = nearnephew.right;
					}
					else
					{
						nearnephew = childsibling.right;
						farnephew = childsibling.left;
						neargrandnephew = nearnephew.right;
						fargrandnephew = nearnephew.left;
					}

					if (fargrandnephew != null && fargrandnephew.red)
					{//Case 2+1b
#if NCP
						Node.CopyNode(ref nearnephew, maxsnapid, generation);

						//The end result of this will always be e copy of parent
						Node.update(ref parent, comp < 0, neargrandnephew, maxsnapid, generation);
						Node.update(ref childsibling, comp > 0, nearnephew, maxsnapid, generation);
#endif
						if (comp > 0)
						{
							nearnephew.left = parent;
							parent.right = neargrandnephew;
						}
						else
						{
							nearnephew.right = parent;
							parent.left = neargrandnephew;
						}

						cursor = childsibling;
						childsibling.red = false;
						nearnephew.red = true;
						fargrandnephew.red = false;
#if MAINTAIN_RANK
						nearnephew.rank++;
						parent.rank--;
#endif
#if BAG
						cursor.size = parent.size;
						nearnephew.size = cursor.size - cursor.items - farnephew.size;
						parent.size = nearnephew.size - nearnephew.items - fargrandnephew.size;
#elif MAINTAIN_SIZE
						cursor.size = parent.size;
						nearnephew.size = cursor.size - 1 - farnephew.size;
						parent.size = nearnephew.size - 1 - fargrandnephew.size;
#endif
#if MAINTAIN_HEIGHT
						fixheight(parent);
						fixheight(nearnephew);
						fixheight(cursor);
#endif
					}
					else if (neargrandnephew != null && neargrandnephew.red)
					{//Case 2+1c
#if NCP
						Node.CopyNode(ref neargrandnephew, maxsnapid, generation);
#endif
						if (comp > 0)
						{
#if NCP
							Node.update(ref childsibling, true, neargrandnephew, maxsnapid, generation);
							Node.update(ref nearnephew, true, neargrandnephew.right, maxsnapid, generation);
							Node.update(ref parent, false, neargrandnephew.left, maxsnapid, generation);
#else
							childsibling.left = neargrandnephew;
							nearnephew.left = neargrandnephew.right;
							parent.right = neargrandnephew.left;
#endif
							neargrandnephew.left = parent;
							neargrandnephew.right = nearnephew;
						}
						else
						{
#if NCP
							Node.update(ref childsibling, false, neargrandnephew, maxsnapid, generation);
							Node.update(ref nearnephew, false, neargrandnephew.left, maxsnapid, generation);
							Node.update(ref parent, true, neargrandnephew.right, maxsnapid, generation);
#else
							childsibling.right = neargrandnephew;
							nearnephew.right = neargrandnephew.left;
							parent.left = neargrandnephew.right;
#endif
							neargrandnephew.right = parent;
							neargrandnephew.left = nearnephew;
						}

						cursor = childsibling;
						childsibling.red = false;
#if MAINTAIN_RANK
						neargrandnephew.rank++;
						parent.rank--;
#endif
#if BAG
						cursor.size = parent.size;
						parent.size = parent.items + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
						nearnephew.size = nearnephew.items + (nearnephew.left == null ? 0 : nearnephew.left.size) + (nearnephew.right == null ? 0 : nearnephew.right.size);
						neargrandnephew.size = neargrandnephew.items + parent.size + nearnephew.size;
#elif MAINTAIN_SIZE
						cursor.size = parent.size;
						parent.size = 1 + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
						nearnephew.size = 1 + (nearnephew.left == null ? 0 : nearnephew.left.size) + (nearnephew.right == null ? 0 : nearnephew.right.size);
						neargrandnephew.size = 1 + parent.size + nearnephew.size;
#endif
#if MAINTAIN_HEIGHT
						fixheight(parent);
						fixheight(nearnephew);
						fixheight(neargrandnephew);
						fixheight(cursor);
#endif
					}
					else
					{//Case 2 only
#if NCP
						Node.update(ref parent, comp < 0, nearnephew, maxsnapid, generation);
						Node.update(ref childsibling, comp > 0, parent, maxsnapid, generation);
#else
						if (comp > 0)
						{
							childsibling.left = parent;
							parent.right = nearnephew;
						}
						else
						{
							childsibling.right = parent;
							parent.left = nearnephew;
						}
#endif
						cursor = childsibling;
						childsibling.red = false;
						nearnephew.red = true;
#if MAINTAIN_RANK
						parent.rank--;
#endif
#if BAG
						cursor.size = parent.size;
						parent.size -= farnephew.size + cursor.items;
#elif MAINTAIN_SIZE
						cursor.size = parent.size;
						parent.size -= farnephew.size + 1;
#endif
#if MAINTAIN_HEIGHT
						fixheight(parent);
						fixheight(cursor);
#endif
					}
				}
				else if (farnephew != null && farnephew.red)
				{//Case 1b
					nearnephew = comp > 0 ? childsibling.left : childsibling.right;		
#if NCP
					Node.update(ref parent, comp < 0, nearnephew, maxsnapid, generation);
					Node.CopyNode(ref childsibling, maxsnapid, generation);
					if (comp > 0)
					{
						childsibling.left = parent;
						childsibling.right = farnephew;
					}
					else
					{
						childsibling.right = parent;
						childsibling.left = farnephew;
					}
#else
					if (comp > 0)
					{
						childsibling.left = parent;
						parent.right = nearnephew;
					}
					else
					{
						childsibling.right = parent;
						parent.left = nearnephew;
					}
#endif
					cursor = childsibling;
					cursor.red = parent.red;
					parent.red = false;
					farnephew.red = false;

#if MAINTAIN_RANK
					childsibling.rank++;
					parent.rank--;
#endif
#if BAG
					cursor.size = parent.size;
					parent.size -= farnephew.size + cursor.items;
#elif MAINTAIN_SIZE
					cursor.size = parent.size;
					parent.size -= farnephew.size + 1;
#endif
#if MAINTAIN_HEIGHT
					fixheight(parent);
					fixheight(cursor);
#endif
				}
				else if (nearnephew != null && nearnephew.red)
				{//Case 1c
#if NCP
					Node.CopyNode(ref nearnephew, maxsnapid, generation);
#endif
					if (comp > 0)
					{
#if NCP
						Node.update(ref childsibling, true, nearnephew.right, maxsnapid, generation);
						Node.update(ref parent, false, nearnephew.left, maxsnapid, generation);
#else
						childsibling.left = nearnephew.right;
						parent.right = nearnephew.left;
#endif
						nearnephew.left = parent;
						nearnephew.right = childsibling;
					}
					else
					{
#if NCP
						Node.update(ref childsibling, false, nearnephew.left, maxsnapid, generation);
						Node.update(ref parent, true, nearnephew.right, maxsnapid, generation);
#else
						childsibling.right = nearnephew.left;
						parent.left = nearnephew.right;
#endif
						nearnephew.right = parent;
						nearnephew.left = childsibling;
					}

					cursor = nearnephew;
					cursor.red = parent.red;
					parent.red = false;
#if MAINTAIN_RANK
					nearnephew.rank++;
					parent.rank--;
#endif
#if BAG
					cursor.size = parent.size;
					parent.size = parent.items + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
					childsibling.size = childsibling.items + (childsibling.left == null ? 0 : childsibling.left.size) + (childsibling.right == null ? 0 : childsibling.right.size);
#elif MAINTAIN_SIZE
					cursor.size = parent.size;
					parent.size = 1 + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
					childsibling.size = 1 + (childsibling.left == null ? 0 : childsibling.left.size) + (childsibling.right == null ? 0 : childsibling.right.size);
#endif
#if MAINTAIN_HEIGHT
					fixheight(parent);
					fixheight(childsibling);
					fixheight(cursor);
#endif
				}
				else
				{//Case 1a can't happen
					throw new Exception("Case 1a can't happen here");
				}

				//Resplice cursor:
				if (level == 0)
				{
					root = cursor;
				}
				else
				{
					Node swap = cursor;

					cursor = path[--level];
					path[level] = null;
#if NCP
					Node.update(ref cursor, dirs[level] > 0, swap, maxsnapid, generation);
#else
				
					if (dirs[level] > 0)
						cursor.left = swap;
					else
						cursor.right = swap;
#endif
#if BAG
					cursor.size -= removedcount;
#elif MAINTAIN_SIZE
					cursor.size--;
#endif
#if MAINTAIN_HEIGHT
					fixheight(cursor);
#endif
				}
			}

			//Stage 6: fixup to the root
			while (level > 0)
			{
				Node child = cursor;

				cursor = path[--level];
				path[level] = null;
#if NCP
				if (child != (dirs[level] > 0 ? cursor.left : cursor.right))
					Node.update(ref cursor, dirs[level] > 0, child, maxsnapid, generation);
#endif
#if BAG
				cursor.size -= removedcount;
#elif MAINTAIN_SIZE
				cursor.size--;
#endif
#if MAINTAIN_HEIGHT
				fixheight(cursor);
#endif
			}

#if MAINTAIN_HEIGHT
			depth = root.height;
#endif
#if NCP
			root = cursor;
#endif
			return true;
		}


		/// <summary>
		/// Remove all items from this collection.
		/// </summary>
		[Tested]
		public void Clear()
		{
			updatecheck();
			clear();
		}


		private void clear()
		{
			size = 0;
			root = null;
			blackdepth = 0;
#if MAINTAIN_HEIGHT
			depth = 0;
#endif
		}


		/// <summary>
		/// Remove all items in another collection from this one. If this collection
		/// has bag semantics, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public void RemoveAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			T jtem;

			foreach (T item in items)
			{
				if (root == null)
					break;

				jtem = item;
				removeIterative(ref jtem, false);
			}
		}


		/// <summary>
		/// Remove all items not in some other collection from this one. If this collection
		/// has bag semantics, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to retain.</param>
		[Tested]
		public void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			//A much more efficient version is possible if items is sorted like this.
			//Well, it is unclear how efficient it would be.
			//We could use a marking method!?
			TreeBag<T> t = (TreeBag<T>)MemberwiseClone();

			t.Clear();
			foreach (T item in items)
				if (ContainsCount(item) > t.ContainsCount(item))
					t.Add(item);

			root = t.root;
			size = t.size;
			blackdepth = t.blackdepth;
#if MAINTAIN_HEIGHT
			depth = t.depth;
#endif
		}


		/// <summary>
		/// Check if this collection contains all the values in another collection.
		/// If this collection has bag semantics (<code>NoDuplicates==false</code>)
		/// the check is made with respect to multiplicities, else multiplicities
		/// are not taken into account.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		[Tested]
		public bool ContainsAll(MSG.IEnumerable<T> items)
		{
			//This is worst-case O(m*logn)
			foreach (T item in items)
				if (!Contains(item)) return false;

			return true;
		}


		//Higher order:
		/// <summary>
		/// Create a new indexed sorted collection consisting of the items of this
		/// indexed sorted collection satisfying a certain predicate.
		/// </summary>
		/// <param name="filter">The filter delegate defining the predicate.</param>
		/// <returns>The new indexed sorted collection.</returns>
		[Tested]
		public IIndexedSorted<T> FindAll(Filter<T> filter)
		{
			TreeBag<T> res = new TreeBag<T>(comparer);
			MSG.IEnumerator<T> e = GetEnumerator();
			Node head = null, tail = null;
			int z = 0;
#if BAG
			int ec = 0;
#endif
			while (e.MoveNext())
			{
				T thisitem = e.Current;
#if BAG
				//We could document that filter will only be called 
				//once on each unique item. That might even be good for the user!
				if (tail!=null && comparer.Compare(thisitem, tail.item) == 0)
				{
					tail.items++;
					ec++;
					continue;
				}
#endif
				if (filter(thisitem))
				{
					if (head == null)
					{
						head = tail = new Node();
					}
					else
					{
#if BAG
						tail.size = tail.items;
#endif
						tail.right = new Node();
						tail = tail.right;
					}

					tail.item = thisitem;
					z++;
				}
			}
#if BAG
			if (tail!=null)
				tail.size = tail.items;
#endif

			if (z == 0)
				return res;

			int blackheight = 0, red = z, maxred = 1;

			while (maxred <= red)
			{
				red -= maxred;
				maxred <<= 1;
				blackheight++;
			}

			res.root = TreeBag<T>.maketreer(ref head, blackheight, maxred, red);
			res.blackdepth = blackheight;
			res.size = z;
#if BAG
			res.size += ec;
#endif
			return res;
		}


		/// <summary>
		/// Create a new indexed sorted collection consisting of the results of
		/// mapping all items of this list.
		/// <exception cref="ArgumentException"/> if the map is not increasing over 
		/// the items of this collection (with respect to the two given comparison 
		/// relations).
		/// </summary>
		/// <param name="mapper">The delegate definging the map.</param>
		/// <param name="c">The comparion relation to use for the result.</param>
		/// <returns>The new sorted collection.</returns>
		[Tested]
		public IIndexedSorted<V> Map<V>(Mapper<T,V> mapper, IComparer<V> c)
		{
			TreeBag<V> res = new TreeBag<V>(c);

			if (size == 0)
				return res;

			MSG.IEnumerator<T> e = GetEnumerator();
			TreeBag<V>.Node head = null, tail = null;
			V oldv = default(V);
			int z = 0;
#if BAG
			T lastitem = default(T);
#endif
			while (e.MoveNext())
			{
				T thisitem = e.Current;
#if BAG
				//We could document that mapper will only be called 
				//once on each unique item. That might even be good for the user!
				if (tail != null && comparer.Compare(thisitem, lastitem) == 0)
				{
					tail.items++;
					continue;
				}
#endif
				V newv = mapper(thisitem);

				if (head == null)
				{
					head = tail = new TreeBag<V>.Node();
					z++;
				}
				else
				{
					int comp = c.Compare(oldv, newv);
#if BAG
					if (comp == 0)
					{
						tail.items++;
						continue;
					}
					if (comp > 0)
#else
					if (comp >= 0)
#endif
						throw new ArgumentException("mapper not monotonic");
#if BAG
					tail.size = tail.items;
#endif
					tail.right = new TreeBag<V>.Node();
					tail = tail.right;
					z++;
				}
#if BAG
				lastitem = thisitem;
#endif
				tail.item = oldv = newv;
			}

#if BAG
			tail.size = tail.items;
#endif

			int blackheight = 0, red = z, maxred = 1;

			while (maxred <= red)
			{
				red -= maxred;
				maxred <<= 1;
				blackheight++;
			}

			res.root = TreeBag<V>.maketreer(ref head, blackheight, maxred, red);
			res.blackdepth = blackheight;
			res.size = size;
			return res;
		}


		//below is the bag utility stuff
		/// <summary>
		/// Count the number of items of the collection equal to a particular value.
		/// Returns 0 if and only if the value is not in the collection.
		/// </summary>
		/// <param name="item">The value to count.</param>
		/// <returns>The number of copies found.</returns>
		[Tested]
		public int ContainsCount(T item)
		{
#if BAG
			Node next; int comp = 0;

			next = root;
			while (next != null)
			{
				comp = comparer.Compare(next.item, item);
				if (comp == 0)
					return next.items;

				next = comp < 0 ? right(next) : left(next);
			}

			return 0;
#else
			//Since we are strictly NoDuplicates we just do
			return Contains(item) ? 1 : 0;
#endif
		}


		/// <summary>
		/// Remove all items equivalent to a given value.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		[Tested]
		public void RemoveAllCopies(T item)
		{
#if BAG
			updatecheck();
			removeIterative(ref item, true);
#else
			
			Remove(item);
#endif
		}


		#endregion

		#region IIndexed<T> Members
			
		private Node findNode(int i)
		{
#if NCP
			if (isSnapShot)
				throw new NotSupportedException("Indexing not supported for snapshots");
#endif
#if MAINTAIN_SIZE
			Node next = root;

			if (i >= 0 && i < size)
				while (true)
				{
					int j = next.left == null ? 0 : next.left.size;

					if (i > j)
					{
#if BAG
						i -= j + next.items;					
						if (i<0)
							return next;
#else
						i -= j + 1;
#endif
						next = next.right;
					}
					else if (i == j)
						return next;
					else
						next = next.left;
				}

			throw new IndexOutOfRangeException();
#else
			throw new NotSupportedException();
#endif
		}


		/// <summary>
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <value>The i'th item of this list.</value>
		/// <param name="i">the index to lookup</param>
		[Tested]
		public T this[int i] { [Tested]	get { return findNode(i).item; } }


		/// <summary>
		/// Searches for an item in the list going forwrds from the start.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of item from start.</returns>
		[Tested]
		public int IndexOf(T item)
		{
			int upper;

			return indexOf(item, out upper);
		}


		private int indexOf(T item, out int upper)
		{
#if NCP
			if (isSnapShot)
				throw new NotSupportedException("Indexing not supported for snapshots");
#endif
#if MAINTAIN_SIZE
			int ind = 0; Node next = root;

			while (next != null)
			{
                int comp = comparer.Compare(item, next.item);

                if (comp < 0)
					next = next.left;
				else
				{
					int leftcnt = next.left == null ? 0 : next.left.size;

					if (comp == 0)
					{
#if BAG
						upper = ind + leftcnt + next.items - 1;
						return ind + leftcnt;
#else
						return upper = ind + leftcnt;
#endif
					}
					else
					{
#if BAG
						ind = ind + next.items + leftcnt;
#else
						ind = ind + 1 + leftcnt;
#endif
						next = next.right;
					}
				}
			}
#endif
			upper = -1;
			return -1;
		}


		/// <summary>
		/// Searches for an item in the list going backwords from the end.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of of item from the end.</returns>
		[Tested]
		public int LastIndexOf(T item)
		{
#if BAG
			int res;
			indexOf(item, out res);
			return res;
#else
			//We have NoDuplicates==true for the set
			return IndexOf(item);
#endif
		}


		/// <summary>
		/// Remove the item at a specific position of the list.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <param name="i">The index of the item to remove.</param>
		/// <returns>The removed item.</returns>
		[Tested]
		public T RemoveAt(int i)
		{
			updatecheck();
#if MAINTAIN_SIZE
			if (i < 0 || i >= size)
				throw new IndexOutOfRangeException("Index out of range for sequenced collection");

			//We must follow the pattern of removeIterative()
			while (dirs.Length < 2 * blackdepth)
			{
				dirs = new int[2 * dirs.Length];
				path = new Node[2 * dirs.Length];
			}

			int level = 0;
			Node cursor = root;

			while (true)
			{
				int j = cursor.left == null ? 0 : cursor.left.size;

				if (i > j)
				{
#if BAG
					i -= j + cursor.items;
					if (i<0)
						break;
#else
					i -= j + 1;
#endif
					dirs[level] = -1;
					path[level++] = cursor;
					cursor = cursor.right;
				}
				else if (i == j)
					break;
				else
				{
					dirs[level] = 1;
					path[level++] = cursor;
					cursor = cursor.left;
				}
			}

			T retval = cursor.item;

#if BAG
			if (cursor.items>1)
			{
				resplicebag(level, cursor);
				size--;
				return retval;
			}
#endif
			removeIterativePhase2(cursor, level);
			return retval;
#else
			throw new NotSupportedException();
#endif
		}

#if BAG
		private void resplicebag(int level, Node cursor)
		{
#if NCP
			Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
			cursor.items--;
			cursor.size--;
			while (level-- > 0)
			{
				Node kid = cursor;

				cursor = path[level];
#if NCP
				Node.update(ref cursor, dirs[level] > 0, kid, maxsnapid, generation);
#endif
				cursor.size--;
				path[level] = null;
			}
		}
#endif
		/// <summary>
		/// Remove all items in an index interval.
		/// <exception cref="IndexOutOfRangeException"/>???. 
		/// </summary>
		/// <param name="start">The index of the first item to remove.</param>
		/// <param name="count">The number of items to remove.</param>
		[Tested]
		public void RemoveInterval(int start, int count)
		{
			if (start < 0 || count < 0)
				throw new ArgumentOutOfRangeException();

			if (start + count > this.size)
				throw new ArgumentException();

			updatecheck();

			//This is terrible for large count. We should split the tree at 
			//the endpoints of the range and fuse the parts!
			//We really need good internal destructive split and catenate functions!
			for (int i = 0; i < count; i++)
				RemoveAt(start);
		}


		/// <summary>
		/// <exception cref="IndexOutOfRangeException"/>.
		/// </summary>
		/// <value>The directed collection of items in a specific index interval.</value>
		/// <param name="start">The low index of the interval (inclusive).</param>
		/// <param name="end">The high index of the interval (exclusive).</param>
		[Tested]
		public IDirectedCollectionValue<T> this[int start, int end]
		{
			[Tested]
			get
			{
				checkRange(start, end - start);
				return new Interval(this, start, end - start, true);
			}
		}

		#region Interval nested class
		class Interval: CollectionValueBase<T>, IDirectedCollectionValue<T>
		{
			int start, length, stamp;

			bool forwards;

			TreeBag<T> tree;


			internal Interval(TreeBag<T> tree, int start, int count, bool forwards)
			{
#if NCP
				if (tree.isSnapShot)
					throw new NotSupportedException("Indexing not supported for snapshots");
#endif
				this.start = start; this.length = count;this.forwards = forwards;
				this.tree = tree; this.stamp = tree.stamp;
			}


			[Tested]
            public override int Count { [Tested]get { return length; } }


            public override Speed CountSpeed { get { return Speed.Constant; } }
            
            [Tested]
            public override MSG.IEnumerator<T> GetEnumerator()
			{
#if MAINTAIN_SIZE
				tree.modifycheck(stamp);
#if BAG
				int togo;
#endif
				Node cursor = tree.root;
				Node[] path = new Node[2 * tree.blackdepth];
				int level = 0, totaltogo = length;

				if (totaltogo == 0)
					yield break;

				if (forwards)
				{
					int i = start;

					while (true)
					{
						int j = cursor.left == null ? 0 : cursor.left.size;

						if (i > j)
						{
#if BAG
							i -= j + cursor.items;
							if (i < 0)
							{
								togo = cursor.items + i;
								break;
							}
#else
							i -= j + 1;
#endif
							cursor = cursor.right;
						}
						else if (i == j)
						{
#if BAG
							togo = cursor.items;
#endif
							break;
						}
						else
						{
							path[level++] = cursor;
							cursor = cursor.left;
						}
					}

					T current = cursor.item;

					while (totaltogo-- > 0)
					{
						yield return current;
						tree.modifycheck(stamp);
#if BAG
						if (--togo > 0)
							continue;
#endif
						if (cursor.right != null)
						{
							path[level] = cursor = cursor.right;
							while (cursor.left != null)
								path[++level] = cursor = cursor.left;
						}
						else if (level == 0)
							yield break;
						else
							cursor = path[--level];

						current = cursor.item;
#if BAG
						togo = cursor.items;
#endif
					}
				}
				else
				{
					int i = start + length - 1;

					while (true)
					{
						int j = cursor.left == null ? 0 : cursor.left.size;

						if (i > j)
						{
#if BAG
							if (i - j < cursor.items)
							{
								togo = i - j + 1;
								break;
							}
							i -= j + cursor.items;
#else
							i -= j + 1;
#endif
							path[level++] = cursor;
							cursor = cursor.right;
						}
						else if (i == j)
						{
#if BAG
							togo = 1;
#endif
							break;
						}
						else
						{
							cursor = cursor.left;
						}
					}

					T current = cursor.item;

					while (totaltogo-- > 0)
					{
						yield return current;
						tree.modifycheck(stamp);
#if BAG
						if (--togo > 0)
							continue;
#endif
						if (cursor.left != null)
						{
							path[level] = cursor = cursor.left;
							while (cursor.right != null)
								path[++level] = cursor = cursor.right;
						}
						else if (level == 0)
							yield break;
						else
							cursor = path[--level];

						current = cursor.item;
#if BAG
						togo = cursor.items;
#endif
					}
				}

#else
			throw new NotSupportedException();
#endif
			}


			[Tested]
			public IDirectedCollectionValue<T> Backwards()
			{ return new Interval(tree, start, length, !forwards); }


			[Tested]
			IDirectedEnumerable<T> C5.IDirectedEnumerable<T>.Backwards()
			{ return Backwards(); }


			[Tested]
			public EnumerationDirection Direction
			{
				[Tested]
				get
				{
					return forwards ? EnumerationDirection.Forwards : EnumerationDirection.Backwards;
				}
			}
		}
		#endregion

		/// <summary>
		/// Create a collection containing the same items as this collection, but
		/// whose enumerator will enumerate the items backwards. The new collection
		/// will become invalid if the original is modified. Method typicaly used as in
		/// <code>foreach (T x in coll.Backwards()) {...}</code>
		/// </summary>
		/// <returns>The backwards collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> Backwards() { return RangeAll().Backwards(); }


		[Tested]
		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }

		#endregion

		#region ISequenced Members
		[Tested]
		int ISequenced<T>.GetHashCode() { return sequencedhashcode(); }


		[Tested]
		bool ISequenced<T>.Equals(ISequenced<T> that) { return sequencedequals(that); }
		#endregion

		#region PriorityQueue Members

        /// <summary>
        /// The comparer object supplied at creation time for this collection
        /// </summary>
        /// <value>The comparer</value>
        public IComparer<T> Comparer { get { return comparer; } }


        /// <summary>
		/// Find the current least item of this priority queue.
		/// </summary>
		/// <returns>The least item.</returns>
		[Tested]
		public T FindMin()
		{
			if (size == 0)
				throw new InvalidOperationException("Priority queue is empty");
#if MAINTAIN_EXTREMA
			return min;
#else
			Node cursor = root, next = left(cursor);

			while (next != null)
			{
				cursor = next;
				next = left(cursor);
			}

			return cursor.item;
#endif
		}


		/// <summary>
		/// Remove the least item from this  priority queue.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public T DeleteMin()
		{
			updatecheck();

			//persistence guard?
			if (size == 0)
				throw new InvalidOperationException("Priority queue is empty");

			//We must follow the pattern of removeIterative()
			stackcheck();

			int level = 0;
			Node cursor = root;

			while (cursor.left != null)
			{
				dirs[level] = 1;
				path[level++] = cursor;
				cursor = cursor.left;
			}

			T retval = cursor.item;

#if BAG
			if (cursor.items > 1)
			{
				resplicebag(level, cursor);
				size--;
				return retval;
			}
#endif
			removeIterativePhase2(cursor, level);
			return retval;
		}


		/// <summary>
		/// Find the current largest item of this priority queue.
		/// </summary>
		/// <returns>The largest item.</returns>
		[Tested]
		public T FindMax()
		{
			if (size == 0)
				throw new InvalidOperationException("Priority queue is empty");

#if MAINTAIN_EXTREMA
			return max;
#else
			Node cursor = root, next = right(cursor);

			while (next != null)
			{
				cursor = next;
				next = right(cursor);
			}

			return cursor.item;
#endif
		}


		/// <summary>
		/// Remove the largest item from this  priority queue.
		/// </summary>
		/// <returns>The removed item.</returns>
		[Tested]
		public T DeleteMax()
		{
			//persistence guard?
			updatecheck();
			if (size == 0)
				throw new InvalidOperationException("Priority queue is empty");

			//We must follow the pattern of removeIterative()
			stackcheck();

			int level = 0;
			Node cursor = root;

			while (cursor.right != null)
			{
				dirs[level] = -1;
				path[level++] = cursor;
				cursor = cursor.right;
			}

			T retval = cursor.item;

#if BAG
			if (cursor.items > 1)
			{
				resplicebag(level, cursor);
				size--;
				return retval;
			}
#endif
			removeIterativePhase2(cursor, level);
			return retval;
		}
		#endregion

		#region IPredecesorStructure<T> Members

		/// <summary>
		/// Find the strict predecessor in the sorted collection of a particular value,
		/// i.e. the largest item in the collection less than the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is less than or equal to the minimum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the predecessor for.</param>
		/// <returns>The predecessor.</returns>
		[Tested]
		public T Predecessor(T item)
		{
			Node cursor = root, bestsofar = null;

			while (cursor != null)
			{
                int comp = comparer.Compare(cursor.item, item);

                if (comp < 0)
				{
					bestsofar = cursor;
					cursor = right(cursor);
				}
				else if (comp == 0)
				{
					cursor = left(cursor);
					while (cursor != null)
					{
						bestsofar = cursor;
						cursor = right(cursor);
					}
				}
				else
					cursor = left(cursor);
			}

			if (bestsofar != null)
				return bestsofar.item;
			else
				throw new ArgumentOutOfRangeException("item", item, "Below minimum of set");
		}


		/// <summary>
		/// Find the weak predecessor in the sorted collection of a particular value,
		/// i.e. the largest item in the collection less than or equal to the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is less than the minimum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the weak predecessor for.</param>
		/// <returns>The weak predecessor.</returns>
		[Tested]
		public T WeakPredecessor(T item)
		{
			Node cursor = root, bestsofar = null;

			while (cursor != null)
			{
                int comp = comparer.Compare(cursor.item, item);

                if (comp < 0)
				{
					bestsofar = cursor;
					cursor = right(cursor);
				}
				else if (comp == 0)
					return cursor.item;
				else
					cursor = left(cursor);
			}

			if (bestsofar != null)
				return bestsofar.item;
			else
				throw new ArgumentOutOfRangeException("item", item, "Below minimum of set");
		}


		/// <summary>
		/// Find the strict successor in the sorted collection of a particular value,
		/// i.e. the least item in the collection greater than the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is greater than or equal to the maximum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the successor for.</param>
		/// <returns>The successor.</returns>
		[Tested]
		public T Successor(T item)
		{
			Node cursor = root, bestsofar = null;

			while (cursor != null)
			{
                int comp = comparer.Compare(cursor.item, item);

                if (comp > 0)
				{
					bestsofar = cursor;
					cursor = left(cursor);
				}
				else if (comp == 0)
				{
					cursor = right(cursor);
					while (cursor != null)
					{
						bestsofar = cursor;
						cursor = left(cursor);
					}
				}
				else
					cursor = right(cursor);
			}

			if (bestsofar != null)
				return bestsofar.item;
			else
				throw new ArgumentOutOfRangeException("item", item, "Above maximum of set");
		}


		/// <summary>
		/// Find the weak successor in the sorted collection of a particular value,
		/// i.e. the least item in the collection greater than or equal to the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is greater than the maximum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the weak successor for.</param>
		/// <returns>The weak successor.</returns>
		[Tested]
		public T WeakSuccessor(T item)
		{
			Node cursor = root, bestsofar = null;

			while (cursor != null)
			{
                int comp = comparer.Compare(cursor.item, item);

                if (comp == 0)
					return cursor.item;
				else if (comp > 0)
				{
					bestsofar = cursor;
					cursor = left(cursor);
				}
				else
					cursor = right(cursor);
			}

			if (bestsofar != null)
				return bestsofar.item;
			else
				throw new ArgumentOutOfRangeException("item", item, "Above maximum of set");
		}

		#endregion
		
		#region ISorted<T> Members

		/// <summary>
		/// Query this sorted collection for items greater than or equal to a supplied value.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive).</param>
		/// <returns>The result directed collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> RangeFrom(T bot)
		{ return new Range(this, true, bot, false, default(T), EnumerationDirection.Forwards); }


		/// <summary>
		/// Query this sorted collection for items between two supplied values.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive).</param>
		/// <param name="top">The upper bound (exclusive).</param>
		/// <returns>The result directed collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> RangeFromTo(T bot, T top)
		{ return new Range(this, true, bot, true, top, EnumerationDirection.Forwards); }


		/// <summary>
		/// Query this sorted collection for items less than a supplied value.
		/// </summary>
		/// <param name="top">The upper bound (exclusive).</param>
		/// <returns>The result directed collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> RangeTo(T top)
		{ return new Range(this, false, default(T), true, top, EnumerationDirection.Forwards); }


		/// <summary>
		/// Create a directed collection with the same items as this collection.
		/// </summary>
		/// <returns>The result directed collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> RangeAll()
		{ return new Range(this, false, default(T), false, default(T), EnumerationDirection.Forwards); }


		[Tested]
		IDirectedEnumerable<T> ISorted<T>.RangeFrom(T bot) { return RangeFrom(bot); }


		[Tested]
		IDirectedEnumerable<T> ISorted<T>.RangeFromTo(T bot, T top) { return RangeFromTo(bot, top); }


		[Tested]
		IDirectedEnumerable<T> ISorted<T>.RangeTo(T top) { return RangeTo(top); }


		//Utility for CountXxxx. Actually always called with strict = true.
		private int countTo(T item, bool strict)
		{
#if NCP
			if (isSnapShot)
				throw new NotSupportedException("Indexing not supported for snapshots");
#endif
#if MAINTAIN_SIZE
			int ind = 0, comp = 0; Node next = root;

			while (next != null)
			{
                comp = comparer.Compare(item, next.item);
                if (comp < 0)
					next = next.left;
				else
				{
					int leftcnt = next.left == null ? 0 : next.left.size;
#if BAG
					if (comp == 0)
						return strict ? ind + leftcnt : ind + leftcnt + next.items;
					else
					{
						ind = ind + next.items + leftcnt;
						next = next.right;
					}
#else
					if (comp == 0)
						return strict ? ind + leftcnt : ind + leftcnt + 1;
					else
					{
						ind = ind + 1 + leftcnt;
						next = next.right;
					}
#endif
				}
			}

			//if we get here, we are at the same side of the whole collection:
			return ind;
#else
			throw new NotSupportedException("Code compiled w/o size!");
#endif
		}


		/// <summary>
		/// Perform a search in the sorted collection for the ranges in which a
		/// non-decreasing function from the item type to <code>int</code> is
		/// negative, zero respectively positive. If the supplied cut function is
		/// not non-decreasing, the result of this call is undefined.
		/// </summary>
		/// <param name="c">The cut function <code>T</code> to <code>int</code>, given
		/// as an <code>IComparable&lt;T&gt;</code> object, where the cut function is
		/// the <code>c.CompareTo(T that)</code> method.</param>
		/// <param name="low">Returns the largest item in the collection, where the
		/// cut function is negative (if any).</param>
		/// <param name="lowIsValid">True if the cut function is negative somewhere
		/// on this collection.</param>
		/// <param name="high">Returns the least item in the collection, where the
		/// cut function is positive (if any).</param>
		/// <param name="highIsValid">True if the cut function is positive somewhere
		/// on this collection.</param>
		/// <returns></returns>
		[Tested]
		public bool Cut(IComparable<T> c, out T low, out bool lowIsValid, out T high, out bool highIsValid)
		{
			Node cursor = root, lbest = null, rbest = null;
			bool res = false;

			while (cursor != null)
			{
				int comp = c.CompareTo(cursor.item);

				if (comp > 0)
				{
					lbest = cursor;
					cursor = right(cursor);
				}
				else if (comp < 0)
				{
					rbest = cursor;
					cursor = left(cursor);
				}
				else
				{
					res = true;

					Node tmp = left(cursor);

					while (tmp != null && c.CompareTo(tmp.item) == 0)
						tmp = left(tmp);

					if (tmp != null)
					{
						lbest = tmp;
						tmp = right(tmp);
						while (tmp != null)
						{
							if (c.CompareTo(tmp.item) > 0)
							{
								lbest = tmp;
								tmp = right(tmp);
							}
							else
								tmp = left(tmp);
						}
					}

					tmp = right(cursor);
					while (tmp != null && c.CompareTo(tmp.item) == 0)
						tmp = right(tmp);

					if (tmp != null)
					{
						rbest = tmp;
						tmp = left(tmp);
						while (tmp != null)
						{
							if (c.CompareTo(tmp.item) < 0)
							{
								rbest = tmp;
								tmp = left(tmp);
							}
							else
								tmp = right(tmp);
						}
					}

					break;
				}
			}

			if (highIsValid = (rbest != null))
				high = rbest.item;
			else
				high = default(T);

			if (lowIsValid = (lbest != null))
				low = lbest.item;
			else
				low = default(T);

			return res;
		}


		/// <summary>
		/// Determine the number of items at or above a supplied threshold.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive)</param>
		/// <returns>The number of matcing items.</returns>
		[Tested]
		public int CountFrom(T bot) { return size - countTo(bot, true); }


		/// <summary>
		/// Determine the number of items between two supplied thresholds.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive)</param>
		/// <param name="top">The upper bound (exclusive)</param>
		/// <returns>The number of matcing items.</returns>
		[Tested]
		public int CountFromTo(T bot, T top)
		{
            if (comparer.Compare(bot, top) >= 0)
                return 0;

			return countTo(top, true) - countTo(bot, true);
		}


		/// <summary>
		/// Determine the number of items below a supplied threshold.
		/// </summary>
		/// <param name="top">The upper bound (exclusive)</param>
		/// <returns>The number of matcing items.</returns>
		[Tested]
		public int CountTo(T top) { return countTo(top, true); }


		/// <summary>
		/// Remove all items of this collection above or at a supplied threshold.
		/// </summary>
		/// <param name="low">The lower threshold (inclusive).</param>
		[Tested]
		public void RemoveRangeFrom(T low)
		{
			updatecheck();

			int count = CountFrom(low);

			if (count == 0)
				return;

			for (int i = 0; i < count; i++)
				DeleteMax();
		}


		/// <summary>
		/// Remove all items of this collection between two supplied thresholds.
		/// </summary>
		/// <param name="low">The lower threshold (inclusive).</param>
		/// <param name="hi">The upper threshold (exclusive).</param>
		[Tested]
		public void RemoveRangeFromTo(T low, T hi)
		{
			updatecheck();

			int count = CountFromTo(low, hi);

			if (count == 0)
				return;

			for (int i = 0; i < count; i++)
				Remove(Predecessor(hi));
		}


		/// <summary>
		/// Remove all items of this collection below a supplied threshold.
		/// </summary>
		/// <param name="hi">The upper threshold (exclusive).</param>
		[Tested]
		public void RemoveRangeTo(T hi)
		{
			updatecheck();

			int count = CountTo(hi);

			if (count == 0)
				return;

			for (int i = 0; i < count; i++)
				DeleteMin();
		}

		#endregion

		#region IPersistent<T> Members

		private bool disposed;



		/// <summary>
		/// If this tree is a snapshot, remove registration in base tree
		/// </summary>
		[Tested]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing) { }
#if NCP
				if (isSnapShot)
				{
					snapdata.Remove(generation, disposing);
					snapdata = null;
					root = null;
					dirs = null;
					path = null;
					comparer = null;
					disposed = true;
				}
				else { }
#endif
			}
		}


		/// <summary>
		/// If this tree is a snapshot, remove registration in base tree
		/// </summary>
		~TreeBag()
		{
			Dispose(false);
		}


		/// <summary>
		/// Make a (read-only) snap shot of this collection.
		/// </summary>
		/// <returns>The snap shot.</returns>
		[Tested]
		public ISorted<T> Snapshot()
		{
#if NCP
			if (isSnapShot)
				throw new InvalidOperationException("Cannot snapshot a snapshot");

			if (snapdata == null)
			{
				snapdata = new SnapData(this);
			}

			snapdata.Add(generation, this);

			TreeBag<T> res = (TreeBag<T>)MemberwiseClone();

			res.isReadOnly = true; res.isSnapShot = true;
			maxsnapid = generation++;
			return res;
#endif
		}

		#endregion

		#region Snapdata nested class
		//In a class by itself: the tree itself and snapshots must have a ref to 
		//the snapids, but we do not want snapshots to have a (strong) ref to the full
		//updatable tree, which should be garbagecollectable even if there are still 
		//live snapshots!
		//
		//Access to SnapData should be thread safe since we expect finalisers 
		//of snapshots to remove a snapid that is garbagecollected without 
		//having been explicitly Disposed. Therefore we access SnapData through 
		//synchronized method/properties.

#if NCP
		class SnapData
		{
			TreeBag<int> snapids = new TreeBag<int>(new IC());

			WeakReference master;


			internal SnapData(TreeBag<T> tree)
			{
				master = new WeakReference(tree);
			}


			[Tested]
			public bool Add(int i, TreeBag<T> tree)
			{
				lock (this)
				{
					bool res = snapids.Add(i);

					//assert the following will be i:
					tree.maxsnapid = snapids.Count > 0 ? snapids.FindMax() : -1;
					return res;
				}
			}


			[Tested]
			public bool Remove(int i, bool updmaxsnapid)
			{
				lock (this)
				{
					bool res = snapids.Remove(i);

					if (updmaxsnapid)
					{
						//Is this safe or/and overkill?
						object t = master.Target;

						if (t != null && master.IsAlive)
						{
							((TreeBag<T>)t).maxsnapid = snapids.Count > 0 ? snapids.FindMax() : -1;
						}
					}

					return res;
				}
			}
		}

#endif
		#endregion

		#region TreeBag.Range nested class
			
		internal class Range: CollectionValueBase<T>, IDirectedCollectionValue<T>
		{
			//We actually need exclusive upper and lower bounds, and flags to 
			//indicate whether the bound is present (we canot rely on default(T))
			private int stamp;

			private TreeBag<T> basis;

			private T lowend, highend;

			private bool haslowend, hashighend;

			EnumerationDirection direction;


			[Tested]
			public Range(TreeBag<T> basis, bool haslowend, T lowend, bool hashighend, T highend, EnumerationDirection direction)
			{
				this.basis = basis;
				stamp = basis.stamp;

				//lowind will be const; should we cache highind?
				this.lowend = lowend; //Inclusive
				this.highend = highend;//Exclusive
				this.haslowend = haslowend;
				this.hashighend = hashighend;
				this.direction = direction;
			}
			#region IEnumerable<T> Members


			#region TreeBag.Range.Enumerator nested class
			
			public class Enumerator: MSG.IEnumerator<T>
			{
				#region Private Fields
				private bool valid = false, ready = true;

				private IComparer<T> comparer;

				private T current;
#if BAG
				int togo;
#endif

				private Node cursor;

				private Node[] path; // stack of nodes

				private int level = 0;

				private Range range;

				private bool forwards;

				#endregion
				[Tested]
				public Enumerator(Range range)
				{
					comparer = range.basis.comparer;
					path = new Node[2 * range.basis.blackdepth];
					this.range = range;
					forwards = range.direction == EnumerationDirection.Forwards;
					cursor = new Node();
					if (forwards)
						cursor.right = range.basis.root;
					else
						cursor.left = range.basis.root;
					range.basis.modifycheck(range.stamp);
				}


				int compare(T i1, T i2) { return comparer.Compare(i1, i2); }


				/// <summary>
				/// Undefined if enumerator is not valid (MoveNext hash been called returning true)
				/// </summary>
				/// <value>The current value of the enumerator.</value>
				[Tested]
				public T Current
				{
					[Tested]
					get
					{
						if (valid)
							return current;
						else
							throw new InvalidOperationException();
					}
				}


				//Maintain a stack of nodes that are roots of
				//subtrees not completely exported yet. Invariant:
				//The stack nodes together with their right subtrees
				//consists of exactly the items we have not passed
				//yet (the top of the stack holds current item).
				/// <summary>
				/// Move enumerator to next item in tree, or the first item if
				/// this is the first call to MoveNext. 
				/// <exception cref="InvalidOperationException"/> if underlying tree was modified.
				/// </summary>
				/// <returns>True if enumerator is valid now</returns>
				[Tested]
				public bool MoveNext()
				{
					range.basis.modifycheck(range.stamp);
					if (!ready)
						return false;
#if BAG
					if (--togo> 0)
						return true;
#endif
					if (forwards)
					{
						if (!valid && range.haslowend)
						{
							cursor = cursor.right;
							while (cursor != null)
							{
								int comp = compare(cursor.item, range.lowend);

								if (comp > 0)
								{
									path[level++] = cursor;
#if NCP
									cursor = range.basis.left(cursor);
#else
									cursor = cursor.left;
#endif
								}
								else if (comp < 0)
								{
#if NCP
									cursor = range.basis.right(cursor);
#else
									cursor = cursor.right;
#endif
								}
								else
								{
									path[level] = cursor;
									break;
								}
							}

							if (cursor == null)
							{
								if (level == 0)
									return valid = ready = false;
								else
									cursor = path[--level];
							}
						}
#if NCP
						else if (range.basis.right(cursor) != null)
						{
							path[level] = cursor = range.basis.right(cursor);

							Node next = range.basis.left(cursor);

							while (next != null)
							{
								path[++level] = cursor = next;
								next = range.basis.left(cursor);
							}
						}
#else
						else if (cursor.right != null)
						{
							path[level] = cursor = cursor.right;
							while (cursor.left != null)
								path[++level] = cursor = cursor.left;
						}
#endif
						else if (level == 0)
							return valid = ready = false;
						else
							cursor = path[--level];

						current = cursor.item;
						if (range.hashighend && compare(current, range.highend) >= 0)
							return valid = ready = false;

#if BAG
						togo = cursor.items;
#endif
						return valid = true;
					}
					else
					{
						if (!valid && range.hashighend)
						{
							cursor = cursor.left;
							while (cursor != null)
							{
								int comp = compare(cursor.item, range.highend);

								if (comp < 0)
								{
									path[level++] = cursor;
#if NCP
									cursor = range.basis.right(cursor);
#else
									cursor = cursor.right;
#endif
								}
								else
								{
#if NCP
									cursor = range.basis.left(cursor);
#else
									cursor = cursor.left;
#endif
								}
							}

							if (cursor == null)
							{
								if (level == 0)
									return valid = ready = false;
								else
									cursor = path[--level];
							}
						}
#if NCP
						else if (range.basis.left(cursor) != null)
						{
							path[level] = cursor = range.basis.left(cursor);

							Node next = range.basis.right(cursor);

							while (next != null)
							{
								path[++level] = cursor = next;
								next = range.basis.right(cursor);
							}
						}
#else
						else if (cursor.left != null)
						{
							path[level] = cursor = cursor.left;
							while (cursor.right != null)
								path[++level] = cursor = cursor.right;
						}
#endif
						else if (level == 0)
							return valid = ready = false;
						else
							cursor = path[--level];

						current = cursor.item;
						if (range.haslowend && compare(current, range.lowend) < 0)
							return valid = ready = false;

#if BAG
						togo = cursor.items;
#endif
						return valid = true;
					}
				}


				[Tested]
				public void Dispose()
				{
					comparer = null;
					current = default(T);
					cursor = null;
					path = null;
					range = null;
				}
			}

			#endregion

			[Tested]
			public override MSG.IEnumerator<T> GetEnumerator() { return new Enumerator(this); }


			[Tested]
			public EnumerationDirection Direction { [Tested]get { return direction; } }


			#endregion

			#region Utility
			
			bool inside(T item)
			{
                return (!haslowend || basis.comparer.Compare(item, lowend) >= 0) && (!hashighend || basis.comparer.Compare(item, highend) < 0);
            }


			void checkstamp()
			{
				if (stamp < basis.stamp)
					throw new InvalidOperationException("Base collection was modified behind my back!");
			}


			void syncstamp() { stamp = basis.stamp; }
			
			#endregion

			[Tested]
			public IDirectedCollectionValue<T> Backwards()
			{
				Range b = (Range)MemberwiseClone();

				b.direction = direction == EnumerationDirection.Forwards ? EnumerationDirection.Backwards : EnumerationDirection.Forwards;
				return b;
			}


			[Tested]
			IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }


			[Tested]
			public override int Count
			{
				[Tested]
				get
				{
					return haslowend ? (hashighend ? basis.CountFromTo(lowend, highend) : basis.CountFrom(lowend)) : (hashighend ? basis.CountTo(highend) : basis.Count);
				}
			}

            //TODO: check that this is correct
            public override Speed CountSpeed { get { return Speed.Log; } }

        }

		#endregion

		#region fixheight utility

#if MAINTAIN_HEIGHT
		public void fixheight(Node n)
		{
			int lh = n.left == null ? 0 : n.left.height + 1;
			int rh = n.right == null ? 0 : n.right.height + 1;

			n.height = (short)(lh > rh ? lh : rh);
		}
#endif

		#endregion

		#region Diagnostics
		/// <summary>
		/// Display this node on the console, and recursively its subnodes.
		/// </summary>
		/// <param name="n">Node to display</param>
		/// <param name="space">Indentation</param>
		private void minidump(Node n, string space)
		{
			if (n == null)
			{
				//	System.Console.WriteLine(space + "null");
			}
			else
			{
				minidump(n.right, space + "  ");
				Console.WriteLine(String.Format("{0} {4} (rank={5}, size={1}, items={8}, h={2}, gen={3}, id={6}){7}", space + n.item, 
#if MAINTAIN_SIZE
				n.size, 
#else
				0,
#endif
#if MAINTAIN_HEIGHT
				n.height, 
#else
				0,
#endif
#if NCP
				n.generation, 
#endif
				n.red ? "RED" : "BLACK", 
#if MAINTAIN_RANK
				n.rank, 
#else
				0,
#endif
#if TRACE_ID
					n.id,
#else
				0,
#endif
#if NCP
#if SEPARATE_EXTRA
				n.extra == null ? "" : String.Format(" [extra: lg={0}, c={1}, i={2}]", n.extra.lastgeneration, n.extra.leftnode ? "L" : "R", n.extra.oldref == null ? "()" : "" + n.extra.oldref.item),
#else
				n.lastgeneration == -1 ? "" : String.Format(" [extra: lg={0}, c={1}, i={2}]", n.lastgeneration, n.leftnode ? "L" : "R", n.oldref == null ? "()" : "" + n.oldref.item),
#endif
#else
				"",
#endif
#if BAG
				n.items
#else
				1
#endif
				));
				minidump(n.left, space + "  ");
			}
		}


		/// <summary>
		/// Print the tree structure to the console stdout.
		/// </summary>
		[Tested(via = "Sawtooth")]
		public void dump() { dump(""); }


		/// <summary>
		/// Print the tree structure to the console stdout.
		/// </summary>
		[Tested(via = "Sawtooth")]
		public void dump(string msg)
		{
			Console.WriteLine(String.Format(">>>>>>>>>>>>>>>>>>> dump {0} (count={1}, blackdepth={2}, depth={3}, gen={4})", msg, size, blackdepth,
#if MAINTAIN_HEIGHT
			depth
#else
			0
#endif
			, 
#if NCP
			generation
#endif
			));
			minidump(root, "");
			check("", Console.Out); Console.WriteLine("<<<<<<<<<<<<<<<<<<<");
		}


		/// <summary>
		/// Display this tree on the console.
		/// </summary>
		/// <param name="msg">Identifying string of this call to dump</param>
		/// <param name="err">Extra (error)message to include</param>
		void dump(string msg, string err)
		{
			Console.WriteLine(String.Format(">>>>>>>>>>>>>>>>>>> dump {0} (count={1}, blackdepth={2}, depth={3}, gen={4})", msg, size, blackdepth,
#if MAINTAIN_HEIGHT
			depth
#else
			0
#endif
			,  
#if NCP
			generation				
#endif
			));
			minidump(root, ""); Console.Write(err);
			Console.WriteLine("<<<<<<<<<<<<<<<<<<<");
		}


		/// <summary>
		/// Print warning m on o if b is false.
		/// </summary>
		/// <param name="b">Condition that should hold</param>
		/// <param name="n">Place (used for id display)</param>
		/// <param name="m">Message</param>
		/// <param name="o">Output stream</param>
		/// <returns>b</returns>
		bool massert(bool b, Node n, string m, System.IO.TextWriter o)
		{
			if (!b) o.WriteLine("*** Node (item={0}, id={1}): {2}", n.item, 
#if TRACE_ID
				n.id
#else
				0
#endif
				, m);

			return b;
		}


		bool rbminicheck(Node n, bool redp, int prank, System.IO.TextWriter o, out T min, out T max, out int blackheight, int maxgen)
		{//Red-Black invariant
			bool res = true;

			res = massert(!(n.red && redp), n, "RED parent of RED node", o) && res;
			res = massert(n.left == null || n.right != null || n.left.red, n, "Left child black, but right child empty", o) && res;
			res = massert(n.right == null || n.left != null || n.right.red, n, "Right child black, but left child empty", o) && res;
#if MAINTAIN_RANK
			res = massert(n.red == (n.rank == prank), n, "Bad color", o) && res;
			res = massert(prank <= n.rank + 1, n, "Parentrank-rank >= 2", o) && res;
			res = massert((n.left != null && n.right != null) || n.rank == 1, n, "Rank>1 but empty child", o) && res;
#endif
#if BAG
			bool sb = n.size == (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + n.items;

			res = massert(sb, n, "Bad size", o) && res;
#elif MAINTAIN_SIZE
			bool sb = n.size == (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + 1;

			res = massert(sb, n, "Bad size", o) && res;
#endif
#if MAINTAIN_HEIGHT
			int lh = n.left == null ? 0 : n.left.height + 1;
			int rh = n.right == null ? 0 : n.right.height + 1;

			res = massert(n.height == (lh < rh ? rh : lh), n, "Bad height", o) && res;
#endif
			int therank =
#if MAINTAIN_RANK
			n.rank;
#else
			0;
#endif
			min = max = n.item;

			T otherext;
			int lbh = 0, rbh = 0;

			if (n.left != null)
			{
				res = rbminicheck(n.left, n.red, therank, o, out min, out otherext, out lbh, generation) && res;
                res = massert(comparer.Compare(n.item, otherext) > 0, n, "Value not > all left children", o) && res;
            }

			if (n.right != null)
			{
				res = rbminicheck(n.right, n.red, therank, o, out otherext, out max, out rbh, generation) && res;
                res = massert(comparer.Compare(n.item, otherext) < 0, n, "Value not < all right children", o) && res;
            }

			res = massert(rbh == lbh, n, "Different blackheights of children", o) && res;
			blackheight = n.red ? rbh : rbh + 1;
#if MAINTAIN_RANK
			//The rank is the number of black nodes from this one to
			//the leaves, not counting this one, but counting 1 for the empty
			//virtual leaf nodes.
			res = massert(n.rank == rbh + 1, n, "rank!=blackheight " + blackheight, o) && res;
#endif
			return res;
		}




#if NCP

		bool rbminisnapcheck(Node n, System.IO.TextWriter o, out int size, out T min, out T max)
		{
			bool res = true;

			min = max = n.item;

			int lsz = 0, rsz = 0;
			T otherext;
#if SEPARATE_EXTRA
			Node.Extra extra = n.extra;
			Node child = (extra != null && extra.lastgeneration >= treegen && extra.leftnode) ? extra.oldref : n.left;
#else
			Node child = (n.lastgeneration >= generation && n.leftnode) ? n.oldref : n.left;
#endif
			if (child != null)
			{
				res = rbminisnapcheck(child, o, out lsz, out min, out otherext) && res;
                res = massert(comparer.Compare(n.item, otherext) > 0, n, "Value not > all left children", o) && res;
            }

#if SEPARATE_EXTRA
			child = (extra != null && extra.lastgeneration >= treegen && !extra.leftnode) ? extra.oldref : n.right;
#else
			child = (n.lastgeneration >= generation && !n.leftnode) ? n.oldref : n.right;
#endif
			if (child != null)
			{
				res = rbminisnapcheck(child, o, out rsz, out otherext, out max) && res;
                res = massert(comparer.Compare(n.item, otherext) < 0, n, "Value not < all right children", o) && res;
            }
#if BAG
			size = n.items + lsz + rsz;
#else
			size = 1 + lsz + rsz;
#endif
			return res;
		}
#endif

		/// <summary>
		/// Checks red-black invariant. Dumps tree to console if bad
		/// </summary>
		/// <param name="name">Title of dump</param>
		/// <returns>false if invariant violation</returns>
		[Tested(via = "Sawtooth")]
		public bool Check(string name)
		{
			System.Text.StringBuilder e = new System.Text.StringBuilder();
			System.IO.TextWriter o = new System.IO.StringWriter(e);

			if (!check(name, o))
				return true;
			else
			{
				dump(name, e.ToString());
				return false;
			}
		}


		/// <summary>
		/// Checks red-black invariant. Dumps tree to console if bad
		/// </summary>
		/// <returns>false if invariant violation</returns>
		[Tested]
		public bool Check()
		{
			//return check("", System.IO.TextWriter.Null);
			//Console.WriteLine("bamse");
			return Check("-");
		}


		bool check(string msg, System.IO.TextWriter o)
		{
			if (root != null)
			{
				T max, min;
				int blackheight;
#if NCP
				if (isSnapShot)
				{
					//Console.WriteLine("Im'a snapshot");
					int thesize;
					bool rv = rbminisnapcheck(root, o, out thesize, out min, out max);

					rv = massert(size == thesize, root, "bad snapshot size", o) && rv;
					return !rv;
				}
#endif
#if MAINTAIN_RANK
				bool res = rbminicheck(root, false, root.rank + 1, o, out min, out max, out blackheight, generation);

				res = massert(root.rank == blackdepth, root, "blackdepth!=root.rank", o) && res;
#else
				bool res = rbminicheck(root, false, 0, o, out min, out max, out blackheight, generation);
#endif
				res = massert(blackheight == blackdepth, root, "bad blackh/d", o) && res;
				res = massert(!root.red, root, "root is red", o) && res;
#if MAINTAIN_SIZE
				res = massert(root.size == size, root, "count!=root.size", o) && res;
#endif
#if MAINTAIN_HEIGHT
				res = massert(root.height == depth, root, "depth!=root.height", o) && res;
#endif
				return !res;
			}
			else
				return false;
		}
		#endregion			
	}
}

