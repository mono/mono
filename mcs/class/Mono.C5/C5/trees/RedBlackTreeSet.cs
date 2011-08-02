/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
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
#define BAGnot
#define NCP

#if BAG
#if !MAINTAIN_SIZE
#error  BAG defined without MAINTAIN_SIZE!
#endif
#endif


using System;
using SCG = System.Collections.Generic;

// NOTE NOTE NOTE NOTE
// This source file is used to produce both TreeSet<T> and TreeBag<T>
// It should be copied to a file called TreeBag.cs in which all code mentions of 
// TreeSet is changed to TreeBag and the preprocessor symbol BAG is defined.
// NOTE: there may be problems with documentation comments.

namespace C5
{
#if BAG
  /// <summary>
  /// An implementation of Red-Black trees as an indexed, sorted collection with bag semantics,
  /// cf. <a href="litterature.htm#CLRS">CLRS</a>. (<see cref="T:C5.TreeSet`1"/> for an 
  /// implementation with set semantics).
  /// <br/>
  /// The comparer (sorting order) may be either natural, because the item type is comparable 
  /// (generic: <see cref="T:C5.IComparable`1"/> or non-generic: System.IComparable) or it can
  /// be external and supplied by the user in the constructor.
  /// <br/>
  /// Each distinct item is only kept in one place in the tree - together with the number
  /// of times it is a member of the bag. Thus, if two items that are equal according
  /// </summary>
#else
  /// <summary>
  /// An implementation of Red-Black trees as an indexed, sorted collection with set semantics,
  /// cf. <a href="litterature.htm#CLRS">CLRS</a>. <see cref="T:C5.TreeBag`1"/> for a version 
  /// with bag semantics. <see cref="T:C5.TreeDictionary`2"/> for a sorted dictionary 
  /// based on this tree implementation.
  /// <i>
  /// The comparer (sorting order) may be either natural, because the item type is comparable 
  /// (generic: <see cref="T:C5.IComparable`1"/> or non-generic: System.IComparable) or it can
  /// be external and supplied by the user in the constructor.</i>
  ///
  /// <i>TODO: describe performance here</i>
  /// <i>TODO: discuss persistence and its useful usage modes. Warn about the space
  /// leak possible with other usage modes.</i>
  /// </summary>
#endif
  [Serializable]
  public class TreeSet<T> : SequencedBase<T>, IIndexedSorted<T>, IPersistentSorted<T>
  {
    #region Fields

    SCG.IComparer<T> comparer;

    Node root;

    //TODO: wonder if we should remove that
    int blackdepth = 0;

    //We double these stacks for the iterative add and remove on demand
    //TODO: refactor dirs[] into bool fields on Node (?)
    private int[] dirs = new int[2];

    private Node[] path = new Node[2];
#if NCP
    //TODO: refactor into separate class
    bool isSnapShot = false;

    int generation;

    bool isValid = true;

    SnapRef snapList;
#endif
    #endregion

    #region Events

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public override EventTypeEnum ListenableEvents { get { return EventTypeEnum.Basic; } }

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
    [Serializable]
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

#if NCP
      //TODO: move everything into (separate) Extra
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
    /// Create a red-black tree collection with natural comparer and item equalityComparer.
    /// We assume that if <code>T</code> is comparable, its default equalityComparer 
    /// will be compatible with the comparer.
    /// </summary>
    /// <exception cref="NotComparableException">If <code>T</code> is not comparable.
    /// </exception>
    public TreeSet() : this(Comparer<T>.Default, EqualityComparer<T>.Default) { }


    /// <summary>
    /// Create a red-black tree collection with an external comparer. 
    /// <para>The itemequalityComparer will be a compatible 
    /// <see cref="T:C5.ComparerZeroHashCodeEqualityComparer`1"/> since the 
    /// default equalityComparer for T (<see cref="P:C5.EqualityComparer`1.Default"/>)
    /// is unlikely to be compatible with the external comparer. This makes the
    /// tree inadequate for use as item in a collection of unsequenced or sequenced sets or bags
    /// (<see cref="T:C5.ICollection`1"/> and <see cref="T:C5.ISequenced`1"/>)
    /// </para>
    /// </summary>
    /// <param name="comparer">The external comparer</param>
    public TreeSet(SCG.IComparer<T> comparer) : this(comparer, new ComparerZeroHashCodeEqualityComparer<T>(comparer)) { }

    /// <summary>
    /// Create a red-black tree collection with an external comparer and an external
    /// item equalityComparer, assumed consistent.
    /// </summary>
    /// <param name="comparer">The external comparer</param>
    /// <param name="equalityComparer">The external item equalityComparer</param>
    public TreeSet(SCG.IComparer<T> comparer, SCG.IEqualityComparer<T> equalityComparer)
      : base(equalityComparer)
    {
      if (comparer == null)
        throw new NullReferenceException("Item comparer cannot be null");
      this.comparer = comparer;
    }

    #endregion

    #region TreeSet.Enumerator nested class

    /// <summary>
    /// An enumerator for a red-black tree collection. Based on an explicit stack
    /// of subtrees waiting to be enumerated. Currently only used for the tree set 
    /// enumerators (tree bag enumerators use an iterator block based enumerator).
    /// </summary>
    internal class Enumerator : SCG.IEnumerator<T>
    {
      #region Private Fields
      TreeSet<T> tree;

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
      public Enumerator(TreeSet<T> tree)
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
      /// <exception cref="CollectionModifiedException"/> if underlying tree was modified.
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
      /// Finalizer for enumerator
      /// </summary>
      ~Enumerator()
      {
        Dispose(false);
      }
      #endregion


      #region IEnumerator Members

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      bool System.Collections.IEnumerator.MoveNext()
      {
        return MoveNext();
      }

      void System.Collections.IEnumerator.Reset()
      {
        throw new Exception("The method or operation is not implemented.");
      }

      #endregion
    }
#if NCP
    /// <summary>
    /// An enumerator for a snapshot of a node copy persistent red-black tree
    /// collection.
    /// </summary>
    internal class SnapEnumerator : SCG.IEnumerator<T>
    {
      #region Private Fields
      TreeSet<T> tree;

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
      public SnapEnumerator(TreeSet<T> tree)
      {
        this.tree = tree;
        stamp = tree.stamp;
        path = new Node[2 * tree.blackdepth];
        cursor = new Node();
        cursor.right = tree.root;
      }


      #region SCG.IEnumerator<T> Members

      /// <summary>
      /// Move enumerator to next item in tree, or the first item if
      /// this is the first call to MoveNext. 
      /// <exception cref="CollectionModifiedException"/> if underlying tree was modified.
      /// </summary>
      /// <returns>True if enumerator is valid now</returns>
      [Tested]
      public bool MoveNext()
      {
        tree.modifycheck(stamp);//???

#if BAG
        if (--togo > 0)
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

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      bool System.Collections.IEnumerator.MoveNext()
      {
        return MoveNext();
      }

      void System.Collections.IEnumerator.Reset()
      {
        throw new Exception("The method or operation is not implemented.");
      }

      #endregion
    }
#endif
    #endregion

    #region IEnumerable<T> Members

    private SCG.IEnumerator<T> getEnumerator(Node node, int origstamp)
    {
      if (node == null)
        yield break;

      if (node.left != null)
      {
        SCG.IEnumerator<T> child = getEnumerator(node.left, origstamp);

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
        SCG.IEnumerator<T> child = getEnumerator(node.right, origstamp);

        while (child.MoveNext())
        {
          modifycheck(origstamp);
          yield return child.Current;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NoSuchItemException">If tree is empty</exception>
    /// <returns></returns>
    [Tested]
    public override T Choose()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");

      if (size == 0)
        throw new NoSuchItemException();
      return root.item;
    }


    /// <summary>
    /// Create an enumerator for this tree
    /// </summary>
    /// <returns>The enumerator</returns>
    [Tested]
    public override SCG.IEnumerator<T> GetEnumerator()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
#if NCP
      if (isSnapShot)
        return new SnapEnumerator(this);
#endif
#if BAG
      return getEnumerator(root, stamp);
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
        root.item = item;
#if NCP
        root.generation = generation;
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
          bool nodeWasUpdated = true;
#if NCP
          Node.CopyNode(ref cursor, maxsnapid, generation);
#endif
          if (update)
            cursor.item = item;
          else
          {
            cursor.items++;
            cursor.size++;
          }
#else
          bool nodeWasUpdated = update;
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
            if (nodeWasUpdated)
            {
              Node kid = cursor;

              cursor = path[level];
#if NCP
              Node.update(ref cursor, dirs[level] > 0, kid, maxsnapid, generation);
#endif
#if BAG
              if (!update)
                cursor.size++;
#endif
            }

            path[level] = null;
          }
#if BAG
          return !update;
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      //Note: blackdepth of the tree is set inside addIterative
      T jtem = default(T);
      if (!add(item, ref jtem))
        return false;
      if (ActiveEvents != 0)
        raiseForAdd(jtem);
      return true;
    }

    /// <summary>
    /// Add an item to this collection if possible. If this collection has set
    /// semantics, the item will be added if not already in the collection. If
    /// bag semantics, the item will always be added.
    /// </summary>
    /// <param name="item">The item to add.</param>
    [Tested]
    void SCG.ICollection<T>.Add(T item)
    {
        Add(item);
    }

    private bool add(T item, ref T j)
    {
      bool wasFound;

      if (addIterative(item, ref j, false, out wasFound))
      {
        size++;
        if (!wasFound)
          j = item;
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Add the elements from another collection with a more specialized item type 
    /// to this collection. If this
    /// collection has set semantics, only items not already in the collection
    /// will be added.
    /// </summary>
    /// <typeparam name="U">The type of items to add</typeparam>
    /// <param name="items">The items to add</param>
    [Tested]
    public void AddAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      int c = 0;
      T j = default(T);
      bool tmp;

      bool raiseAdded = (ActiveEvents & EventTypeEnum.Added) != 0;
      CircularQueue<T> wasAdded = raiseAdded ? new CircularQueue<T>() : null;

      foreach (T i in items)
        if (addIterative(i, ref j, false, out tmp))
        {
          c++;
          if (raiseAdded)
            wasAdded.Enqueue(tmp ? j : i);
        }
      if (c == 0)
        return;

      size += c;
      //TODO: implement a RaiseForAddAll() method
      if (raiseAdded)
        foreach (T item in wasAdded)
          raiseItemsAdded(item, 1);
      if (((ActiveEvents & EventTypeEnum.Changed) != 0))
        raiseCollectionChanged();
    }


    /// <summary>
    /// Add all the items from another collection with an enumeration order that 
    /// is increasing in the items. <para>The idea is that the implementation may use
    /// a faster algorithm to merge the two collections.</para>
    /// <exception cref="ArgumentException"/> if the enumerated items turns out
    /// not to be in increasing order.
    /// </summary>
    /// <param name="items">The collection to add.</param>
    /// <typeparam name="U"></typeparam>
    [Tested]
    public void AddSorted<U>(SCG.IEnumerable<U> items) where U : T
    {
      if (size > 0)
        AddAll(items);
      else
      {
        if (!isValid)
          throw new ViewDisposedException("Snapshot has been disposed");
        updatecheck();
        addSorted(items, true, true);
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
        maxred >>= 1;

        int lred = red > maxred ? maxred : red;
        Node left = maketreer(ref rest, blackheight - 1, maxred, lred);
        Node top = rest;

        rest = rest.right;
        top.left = left;
        top.red = false;
        top.right = maketreer(ref rest, blackheight - 1, maxred, red - lred);
#if BAG
        top.size = top.items + top.left.size + top.right.size;
#elif MAINTAIN_SIZE
        top.size = (maxred << 1) - 1 + red;
#endif
        return top;
      }
    }


    void addSorted<U>(SCG.IEnumerable<U> items, bool safe, bool raise) where U : T
    {
      SCG.IEnumerator<U> e = items.GetEnumerator(); ;
      if (size > 0)
        throw new InternalException("This can't happen");

      if (!e.MoveNext())
        return;

      //To count theCollect 
      Node head = new Node(), tail = head;
      int z = 1;
      T lastitem = tail.item = e.Current;
#if BAG
      int ec = 0;
#endif

      while (e.MoveNext())
      {
#if BAG
        T thisitem = e.Current;
        int comp = comparer.Compare(lastitem, thisitem);
        if (comp > 0)
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

      root = TreeSet<T>.maketreer(ref head, blackheight, maxred, red);
      blackdepth = blackheight;
      size = z;
#if BAG
      size += ec;
#endif

      if (raise)
      {
        if ((ActiveEvents & EventTypeEnum.Added) != 0)
        {
          CircularQueue<T> wasAdded = new CircularQueue<T>();
          foreach (T item in this)
            wasAdded.Enqueue(item);
          foreach (T item in wasAdded)
            raiseItemsAdded(item, 1);
        }
        if ((ActiveEvents & EventTypeEnum.Changed) != 0)
          raiseCollectionChanged();
      }
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
    /// <summary>
    /// By convention this is true for any collection with set semantics.
    /// </summary>
    /// <value>True if only one representative of a group of equal items 
    /// is kept in the collection together with the total count.</value>
    public virtual bool DuplicatesByCounting { get { return true; } }

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

    /// <summary>
    /// Check if this collection contains (an item equivalent to according to the
    /// itemequalityComparer) a particular value.
    /// </summary>
    /// <param name="item">The value to check for.</param>
    /// <returns>True if the items is in this collection.</returns>
    [Tested]
    public bool Contains(T item)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
    /// itemequalityComparer to a particular value. If so, return in the ref argument (a
    /// binary copy of) the actual value found.
    /// </summary>
    /// <param name="item">The value to look for.</param>
    /// <returns>True if the items is in this collection.</returns>
    [Tested]
    public bool Find(ref T item)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      bool wasfound;

      //Note: blackdepth of the tree is set inside addIterative
      if (addIterative(item, ref item, false, out wasfound))
      {
        size++;
        if (ActiveEvents != 0 && !wasfound)
          raiseForAdd(item);
        return wasfound;
      }
      else
        return true;

    }


    //For dictionary use. 
    //If found, the matching entry will be updated with the new item.
    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// to with a binary copy of the supplied value. If the collection has bag semantics,
    /// this updates all equivalent copies in
    /// the collection.
    /// </summary>
    /// <param name="item">Value to update.</param>
    /// <returns>True if the item was found and hence updated.</returns>
    [Tested]
    public bool Update(T item)
    {
      T olditem = item;
      return Update(item, out olditem);
    }

    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// with a binary copy of the supplied value. If the collection has bag semantics,
    /// this updates all equivalent copies in
    /// the collection.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public bool Update(T item, out T olditem)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
          olditem = cursor.item;
#if BAG
          int items = cursor.items;
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
#if BAG
          if (ActiveEvents != 0)
            raiseForUpdate(item, olditem, items);
#else
          if (ActiveEvents != 0)
            raiseForUpdate(item, olditem);
#endif
          return true;
        }
#if NCP
        dirs[level] = comp;
        path[level++] = cursor;
#endif
        cursor = comp < 0 ? cursor.right : cursor.left;
      }

      olditem = default(T);
      return false;
    }


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// with a binary copy of the supplied value; else add the value to the collection. 
    ///
    /// <i>NOTE: the bag implementation is currently wrong! ?????</i>
    /// </summary>
    /// <param name="item">Value to add or update.</param>
    /// <returns>True if the item was found and updated (hence not added).</returns>
    [Tested]
    public bool UpdateOrAdd(T item)
    { T olditem; return UpdateOrAdd(item, out olditem); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public bool UpdateOrAdd(T item, out T olditem)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      bool wasfound;
      olditem = default(T);


      //Note: blackdepth of the tree is set inside addIterative
      if (addIterative(item, ref olditem, true, out wasfound))
      {
        size++;
        if (ActiveEvents != 0)
          raiseForAdd(wasfound ? olditem : item);
        return wasfound;
      }
      else
      {
#warning for bag implementation: count is wrong
        if (ActiveEvents != 0)
          raiseForUpdate(item, olditem, 1);
        return true;
      }
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      if (root == null)
        return false;

      int junk;
      bool retval = removeIterative(ref item, false, out junk);
      if (ActiveEvents != 0 && retval)
        raiseForRemove(item);
      return retval;
    }

    /// <summary>
    /// Remove a particular item from this collection if found. If the collection
    /// has bag semantics only one copy equivalent to the supplied item is removed,
    /// which one is implementation dependent. 
    /// If an item was removed, report a binary copy of the actual item removed in 
    /// the argument.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    /// <param name="removeditem">The removed value.</param>
    /// <returns>True if the item was found (and removed).</returns>
    [Tested]
    public bool Remove(T item, out T removeditem)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      removeditem = item;
      if (root == null)
        return false;

      int junk;
      bool retval = removeIterative(ref removeditem, false, out junk);
      if (ActiveEvents != 0 && retval)
        raiseForRemove(item);
      return retval;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item">input: item to remove; output: item actually removed</param>
    /// <param name="all">If true, remove all copies</param>
    /// <param name="wasRemoved"></param>
    /// <returns></returns>
    private bool removeIterative(ref T item, bool all, out int wasRemoved)
    {
      wasRemoved = 0;
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
              Node.update(ref cursor, dirs[level] > 0, kid, maxsnapid, generation);
#endif
              cursor.size--;
              path[level] = null;
            }
            size--;
            wasRemoved = 1;
            return true;
          }
          wasRemoved = cursor.items;
#else
          wasRemoved = 1;
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

#if BAG
      size -= cursor.items;
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
      cursor.size = cursor.items + (newchild == null ? 0 : newchild.size) + (childsibling == null ? 0 : childsibling.size);
#elif MAINTAIN_SIZE
      cursor.size--;
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
        if (level == 0)
        {
          cursor.red = false;
          blackdepth--;
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
          cursor.size = cursor.items + (child == null ? 0 : child.size) + (childsibling == null ? 0 : childsibling.size);
#elif MAINTAIN_SIZE
          cursor.size--;
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
#if BAG
            cursor.size = parent.size;
            nearnephew.size = cursor.size - cursor.items - farnephew.size;
            parent.size = nearnephew.size - nearnephew.items - fargrandnephew.size;
#elif MAINTAIN_SIZE
            cursor.size = parent.size;
            nearnephew.size = cursor.size - 1 - farnephew.size;
            parent.size = nearnephew.size - 1 - fargrandnephew.size;
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
#if BAG
            cursor.size = parent.size;
            parent.size -= farnephew.size + cursor.items;
#elif MAINTAIN_SIZE
            cursor.size = parent.size;
            parent.size -= farnephew.size + 1;
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

#if BAG
          cursor.size = parent.size;
          parent.size -= farnephew.size + cursor.items;
#elif MAINTAIN_SIZE
          cursor.size = parent.size;
          parent.size -= farnephew.size + 1;
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
#if BAG
          cursor.size = parent.size;
          parent.size = parent.items + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
          childsibling.size = childsibling.items + (childsibling.left == null ? 0 : childsibling.left.size) + (childsibling.right == null ? 0 : childsibling.right.size);
#elif MAINTAIN_SIZE
          cursor.size = parent.size;
          parent.size = 1 + (parent.left == null ? 0 : parent.left.size) + (parent.right == null ? 0 : parent.right.size);
          childsibling.size = 1 + (childsibling.left == null ? 0 : childsibling.left.size) + (childsibling.right == null ? 0 : childsibling.right.size);
#endif
        }
        else
        {//Case 1a can't happen
          throw new InternalException("Case 1a can't happen here");
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
          cursor.size = cursor.items + (cursor.right == null ? 0 : cursor.right.size) + (cursor.left == null ? 0 : cursor.left.size);
#elif MAINTAIN_SIZE
          cursor.size--;
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
        cursor.size = cursor.items + (cursor.right == null ? 0 : cursor.right.size) + (cursor.left == null ? 0 : cursor.left.size);
#elif MAINTAIN_SIZE
        cursor.size--;
#endif
      }

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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      if (size == 0)
        return;
      int oldsize = size;
      clear();
      if ((ActiveEvents & EventTypeEnum.Cleared) != 0)
        raiseCollectionCleared(true, oldsize);
      if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }


    private void clear()
    {
      size = 0;
      root = null;
      blackdepth = 0;
    }


    /// <summary>
    /// Remove all items in another collection from this one. If this collection
    /// has bag semantics, take multiplicities into account.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to remove.</param>
    [Tested]
    public void RemoveAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      T jtem;

      bool mustRaise = (ActiveEvents & (EventTypeEnum.Removed | EventTypeEnum.Changed)) != 0;
      RaiseForRemoveAllHandler raiseHandler = mustRaise ? new RaiseForRemoveAllHandler(this) : null;

      foreach (T item in items)
      {
        if (root == null)
          break;

        jtem = item;
        int junk;
        if (removeIterative(ref jtem, false, out junk) && mustRaise)
          raiseHandler.Remove(jtem);
      }
      if (mustRaise)
        raiseHandler.Raise();
    }

    /// <summary>
    /// Remove all items not in some other collection from this one. If this collection
    /// has bag semantics, take multiplicities into account.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to retain.</param>
    [Tested]
    public void RetainAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      //A much more efficient version is possible if items is sorted like this.
      //Well, it is unclear how efficient it would be.
      //We could use a marking method!?
#warning how does this work together with persistence?
      TreeSet<T> t = (TreeSet<T>)MemberwiseClone();

      T jtem = default(T);
      t.clear();
      foreach (T item in items)
        if (ContainsCount(item) > t.ContainsCount(item))
        {
          t.add(item, ref jtem);
        }
      if (size == t.size)
        return;

#warning improve (mainly for bag) by using a Node iterator instead of ItemMultiplicities()
      CircularQueue<KeyValuePair<T, int>> wasRemoved = null;
      if ((ActiveEvents & EventTypeEnum.Removed) != 0)
      {
        wasRemoved = new CircularQueue<KeyValuePair<T, int>>();
        SCG.IEnumerator<KeyValuePair<T, int>> ie = ItemMultiplicities().GetEnumerator();
        foreach (KeyValuePair<T, int> p in t.ItemMultiplicities())
        {
          //We know p.Key is in this!
          while (ie.MoveNext())
          {
            if (comparer.Compare(ie.Current.Key, p.Key) == 0)
            {
#if BAG
              int removed = ie.Current.Value - p.Value;
              if (removed > 0)
                wasRemoved.Enqueue(new KeyValuePair<T,int>(p.Key, removed));
#endif
              break;
            }
            else
              wasRemoved.Enqueue(ie.Current);
          }
        }
        while (ie.MoveNext())
          wasRemoved.Enqueue(ie.Current);
      }

      root = t.root;
      size = t.size;
      blackdepth = t.blackdepth;
      if (wasRemoved != null)
        foreach (KeyValuePair<T, int> p in wasRemoved)
          raiseItemsRemoved(p.Key, p.Value);
      if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }

    /// <summary>
    /// Check if this collection contains all the values in another collection.
    /// If this collection has bag semantics (<code>AllowsDuplicates==true</code>)
    /// the check is made with respect to multiplicities, else multiplicities
    /// are not taken into account.
    /// </summary>
    /// <param name="items">The </param>
    /// <typeparam name="U"></typeparam>
    /// <returns>True if all values in <code>items</code>is in this collection.</returns>
    [Tested]
    public bool ContainsAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      //TODO: fix bag implementation
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
    public IIndexedSorted<T> FindAll(Fun<T, bool> filter)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      TreeSet<T> res = new TreeSet<T>(comparer);
      SCG.IEnumerator<T> e = GetEnumerator();
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
        if (tail != null && comparer.Compare(thisitem, tail.item) == 0)
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
      if (tail != null)
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

      res.root = TreeSet<T>.maketreer(ref head, blackheight, maxred, red);
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
    public IIndexedSorted<V> Map<V>(Fun<T, V> mapper, SCG.IComparer<V> c)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      TreeSet<V> res = new TreeSet<V>(c);

      if (size == 0)
        return res;

      SCG.IEnumerator<T> e = GetEnumerator();
      TreeSet<V>.Node head = null, tail = null;
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
          head = tail = new TreeSet<V>.Node();
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
          tail.right = new TreeSet<V>.Node();
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

      res.root = TreeSet<V>.maketreer(ref head, blackheight, maxred, red);
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
      //Since we are strictly not AllowsDuplicates we just do
      return Contains(item) ? 1 : 0;
#endif
    }

#if BAG
    //TODO: make work with snapshots
    class Multiplicities : CollectionValueBase<KeyValuePair<T, int>>, ICollectionValue<KeyValuePair<T, int>>
    {
      TreeBag<T> treebag;
      int origstamp;
      internal Multiplicities(TreeBag<T> treebag) { this.treebag = treebag; this.origstamp = treebag.stamp; }
      public override KeyValuePair<T, int> Choose() { return new KeyValuePair<T, int>(treebag.root.item, treebag.root.items); }

      public override SCG.IEnumerator<KeyValuePair<T, int>> GetEnumerator()
      {
        return getEnumerator(treebag.root, origstamp); //TODO: NBNBNB
      }

      private SCG.IEnumerator<KeyValuePair<T, int>> getEnumerator(Node node, int origstamp)
      {
        if (node == null)
          yield break;

        if (node.left != null)
        {
          SCG.IEnumerator<KeyValuePair<T, int>> child = getEnumerator(node.left, origstamp);

          while (child.MoveNext())
          {
            treebag.modifycheck(origstamp);
            yield return child.Current;
          }
        }
        yield return new KeyValuePair<T, int>(node.item, node.items);
        if (node.right != null)
        {
          SCG.IEnumerator<KeyValuePair<T, int>> child = getEnumerator(node.right, origstamp);

          while (child.MoveNext())
          {
            treebag.modifycheck(origstamp);
            yield return child.Current;
          }
        }
      }

      public override bool IsEmpty { get { return treebag.IsEmpty; } }
      public override int Count { get { int i = 0; foreach (KeyValuePair<T, int> p in this) i++; return i; } } //TODO: make better
      public override Speed CountSpeed { get { return Speed.Linear; } } //TODO: make better
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual ICollectionValue<T> UniqueItems()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
#if BAG
      return new DropMultiplicity<T>(ItemMultiplicities());
#else
      return this;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual ICollectionValue<KeyValuePair<T, int>> ItemMultiplicities()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
#if BAG
      return new Multiplicities(this);
#else
      return new MultiplicityOne<T>(this);
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      int removed;
      if (removeIterative(ref item, true, out removed) && ActiveEvents != 0)
      {
        raiseForRemove(item, removed);
      }
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
            if (i < 0)
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
    /// 
    /// </summary>
    /// <value></value>
    public virtual Speed IndexingSpeed { get { return Speed.Log; } }


    //TODO: return -upper instead of -1 in case of not found
    /// <summary>
    /// Searches for an item in this indexed collection going forwards from the start.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of first occurrence from start of the item
    /// if found, else the two-complement 
    /// (always negative) of the index at which the item would be put if it was added.</returns>
    [Tested]
    public int IndexOf(T item)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
      upper = ~ind;
      return ~ind;
    }


    /// <summary>
    /// Searches for an item in the tree going backwords from the end.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of last occurrence from the end of item if found, 
    /// else the two-complement (always negative) of the index at which 
    /// the item would be put if it was added.</returns>
    [Tested]
    public int LastIndexOf(T item)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
#if BAG
      int res;
      indexOf(item, out res);
      return res;
#else
      //We have AllowsDuplicates==false for the set
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
      T retval = removeAt(i);
      if (ActiveEvents != 0)
        raiseForRemove(retval);
      return retval;
    }

    T removeAt(int i)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();
#if MAINTAIN_SIZE
      if (i < 0 || i >= size)
        throw new IndexOutOfRangeException("Index out of range for sequenced collectionvalue");

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
          if (i < 0)
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
      if (cursor.items > 1)
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      if (start < 0 || count < 0 || start + count > this.size)
        throw new ArgumentOutOfRangeException();

      updatecheck();

      if (count == 0)
        return;

      //This is terrible for large count. We should split the tree at 
      //the endpoints of the range and fuse the parts!
      //We really need good internal destructive split and catenate functions!
      //Alternative for large counts: rebuild tree using maketree()
      for (int i = 0; i < count; i++)
        removeAt(start);

      if ((ActiveEvents & EventTypeEnum.Cleared) != 0)
        raiseCollectionCleared(false, count);
      if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }


    /// <summary>
    /// <exception cref="IndexOutOfRangeException"/>.
    /// </summary>
    /// <value>The directed collection of items in a specific index interval.</value>
    /// <param name="start">The starting index of the interval (inclusive).</param>
    /// <param name="count">The length of the interval.</param>
    [Tested]
    public IDirectedCollectionValue<T> this[int start, int count]
    {
      [Tested]
      get
      {
        checkRange(start, count);
        return new Interval(this, start, count, true);
      }
    }

    #region Interval nested class
    class Interval : DirectedCollectionValueBase<T>, IDirectedCollectionValue<T>
    {
      readonly int start, length, stamp;

      readonly bool forwards;

      readonly TreeSet<T> tree;


      internal Interval(TreeSet<T> tree, int start, int count, bool forwards)
      {
#if NCP
        if (tree.isSnapShot)
          throw new NotSupportedException("Indexing not supported for snapshots");
#endif
        this.start = start; this.length = count; this.forwards = forwards;
        this.tree = tree; this.stamp = tree.stamp;
      }

      public override bool IsEmpty { get { return length == 0; } }

      [Tested]
      public override int Count { [Tested]get { return length; } }


      public override Speed CountSpeed { get { return Speed.Constant; } }


      public override T Choose()
      {
        if (length == 0)
          throw new NoSuchItemException();
        return tree[start];
      }

      [Tested]
      public override SCG.IEnumerator<T> GetEnumerator()
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
              if (cursor.items > i - j) {
                togo = cursor.items - (i - j);
                break;
              }
              i -= j + cursor.items;
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
            } // i < j, start point tree[start] is in left subtree
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
        else // backwards
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
            else // i <= j, end point tree[start+count-1] is in left subtree
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
      public override IDirectedCollectionValue<T> Backwards()
      { return new Interval(tree, start, length, !forwards); }


      [Tested]
      IDirectedEnumerable<T> C5.IDirectedEnumerable<T>.Backwards()
      { return Backwards(); }


      [Tested]
      public override EnumerationDirection Direction
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
    public override IDirectedCollectionValue<T> Backwards() { return RangeAll().Backwards(); }


    [Tested]
    IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }

    #endregion

    #region PriorityQueue Members

    /// <summary>
    /// The comparer object supplied at creation time for this collection
    /// </summary>
    /// <value>The comparer</value>
    public SCG.IComparer<T> Comparer { get { return comparer; } }


    /// <summary>
    /// Find the current least item of this priority queue.
    /// </summary>
    /// <returns>The least item.</returns>
    [Tested]
    public T FindMin()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      if (size == 0)
        throw new NoSuchItemException();
      Node cursor = root, next = left(cursor);

      while (next != null)
      {
        cursor = next;
        next = left(cursor);
      }

      return cursor.item;
    }


    /// <summary>
    /// Remove the least item from this  priority queue.
    /// </summary>
    /// <returns>The removed item.</returns>
    [Tested]
    public T DeleteMin()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      //persistence guard?
      if (size == 0)
        throw new NoSuchItemException();

      //We must follow the pattern of removeIterative()
      stackcheck();

      T retval = deleteMin();
      if (ActiveEvents != 0)
      {
        raiseItemsRemoved(retval, 1);
        raiseCollectionChanged();
      }
      return retval;
    }

    private T deleteMin()
    {
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
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      if (size == 0)
        throw new NoSuchItemException();

      Node cursor = root, next = right(cursor);

      while (next != null)
      {
        cursor = next;
        next = right(cursor);
      }

      return cursor.item;
    }


    /// <summary>
    /// Remove the largest item from this  priority queue.
    /// </summary>
    /// <returns>The removed item.</returns>
    [Tested]
    public T DeleteMax()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      //persistence guard?
      updatecheck();
      if (size == 0)
        throw new NoSuchItemException();

      //We must follow the pattern of removeIterative()
      stackcheck();

      T retval = deleteMax();
      if (ActiveEvents != 0)
      {
        raiseItemsRemoved(retval, 1);
        raiseCollectionChanged();
      }
      return retval;
    }

    private T deleteMax()
    {
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

    #region ISorted<T> Members

    /// <summary>
    /// Find the strict predecessor of item in the sorted collection,
    /// that is, the greatest item in the collection smaller than the item.
    /// </summary>
    /// <param name="item">The item to find the predecessor for.</param>
    /// <param name="res">The predecessor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a predecessor; otherwise false.</returns>
    public bool TryPredecessor(T item, out T res)
    {
        if (!isValid)
            throw new ViewDisposedException("Snapshot has been disposed");
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
        if (bestsofar == null)
        {
            res = default(T);
            return false;
        }
        else
        {
            res = bestsofar.item;
            return true;
        }
    }


    /// <summary>
    /// Find the strict successor of item in the sorted collection,
    /// that is, the least item in the collection greater than the supplied value.
    /// </summary>
    /// <param name="item">The item to find the successor for.</param>
    /// <param name="res">The successor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a successor; otherwise false.</returns>
    public bool TrySuccessor(T item, out T res)
    {
        if (!isValid)
            throw new ViewDisposedException("Snapshot has been disposed");
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

        if (bestsofar == null)
        {
            res = default(T);
            return false;
        }
        else
        {
            res = bestsofar.item;
            return true;
        }
    }


    /// <summary>
    /// Find the weak predecessor of item in the sorted collection,
    /// that is, the greatest item in the collection smaller than or equal to the item.
    /// </summary>
    /// <param name="item">The item to find the weak predecessor for.</param>
    /// <param name="res">The weak predecessor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a weak predecessor; otherwise false.</returns>
    public bool TryWeakPredecessor(T item, out T res)
    {
        if (!isValid)
            throw new ViewDisposedException("Snapshot has been disposed");
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
                res = cursor.item;
                return true;
            }
            else
                cursor = left(cursor);
        }
        if (bestsofar == null)
        {
            res = default(T);
            return false;
        }
        else
        {
            res = bestsofar.item;
            return true;
        }
    }


    /// <summary>
    /// Find the weak successor of item in the sorted collection,
    /// that is, the least item in the collection greater than or equal to the supplied value.
    /// </summary>
    /// <param name="item">The item to find the weak successor for.</param>
    /// <param name="res">The weak successor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a weak successor; otherwise false.</returns>
    public bool TryWeakSuccessor(T item, out T res)
    {
        if (!isValid)
            throw new ViewDisposedException("Snapshot has been disposed");
        Node cursor = root, bestsofar = null;

        while (cursor != null)
        {
            int comp = comparer.Compare(cursor.item, item);

            if (comp == 0)
            {
                res = cursor.item;
                return true;
            }
            else if (comp > 0)
            {
                bestsofar = cursor;
                cursor = left(cursor);
            }
            else
                cursor = right(cursor);
        }

        if (bestsofar == null)
        {
            res = default(T);
            return false;
        }
        else
        {
            res = bestsofar.item;
            return true;
        }
    }


    /// <summary>
    /// Find the strict predecessor in the sorted collection of a particular value,
    /// i.e. the largest item in the collection less than the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is less than or equal to the minimum of this collection.)</exception>
    /// <param name="item">The item to find the predecessor for.</param>
    /// <returns>The predecessor.</returns>
    [Tested]
    public T Predecessor(T item)
    {
        T res;
        if (TryPredecessor(item, out res))
            return res;
        else
            throw new NoSuchItemException();
    }


    /// <summary>
    /// Find the weak predecessor in the sorted collection of a particular value,
    /// i.e. the largest item in the collection less than or equal to the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is less than the minimum of this collection.)</exception>
    /// <param name="item">The item to find the weak predecessor for.</param>
    /// <returns>The weak predecessor.</returns>
    [Tested]
    public T WeakPredecessor(T item)
    {
        T res;
        if (TryWeakPredecessor(item, out res))
            return res;
        else
            throw new NoSuchItemException();
    }


    /// <summary>
    /// Find the strict successor in the sorted collection of a particular value,
    /// i.e. the least item in the collection greater than the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is greater than or equal to the maximum of this collection.)</exception>
    /// <param name="item">The item to find the successor for.</param>
    /// <returns>The successor.</returns>
    [Tested]
    public T Successor(T item)
    {
        T res;
        if (TrySuccessor(item, out res))
            return res;
        else
            throw new NoSuchItemException();
    }


    /// <summary>
    /// Find the weak successor in the sorted collection of a particular value,
    /// i.e. the least item in the collection greater than or equal to the supplied value.
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is greater than the maximum of this collection.)</exception>
    /// </summary>
    /// <param name="item">The item to find the weak successor for.</param>
    /// <returns>The weak successor.</returns>
    [Tested]
    public T WeakSuccessor(T item)
    {
        T res;
        if (TryWeakSuccessor(item, out res))
            return res;
        else
            throw new NoSuchItemException();
    }

   
    /// <summary>
    /// Query this sorted collection for items greater than or equal to a supplied value.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <returns>The result directed collection.</returns>
    [Tested]
    public IDirectedCollectionValue<T> RangeFrom(T bot)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return new Range(this, true, bot, false, default(T), EnumerationDirection.Forwards);
    }


    /// <summary>
    /// Query this sorted collection for items between two supplied values.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    [Tested]
    public IDirectedCollectionValue<T> RangeFromTo(T bot, T top)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return new Range(this, true, bot, true, top, EnumerationDirection.Forwards);
    }


    /// <summary>
    /// Query this sorted collection for items less than a supplied value.
    /// </summary>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    [Tested]
    public IDirectedCollectionValue<T> RangeTo(T top)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return new Range(this, false, default(T), true, top, EnumerationDirection.Forwards);
    }


    /// <summary>
    /// Create a directed collection with the same items as this collection.
    /// </summary>
    /// <returns>The result directed collection.</returns>
    [Tested]
    public IDirectedCollectionValue<T> RangeAll()
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return new Range(this, false, default(T), false, default(T), EnumerationDirection.Forwards);
    }


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
    /// non-increasing (i.e. weakly decrerasing) function from the item type to 
    /// <code>int</code> is
    /// negative, zero respectively positive. If the supplied cut function is
    /// not non-increasing, the result of this call is undefined.
    /// </summary>
    /// <param name="c">The cut function <code>T</code> to <code>int</code>, given
    /// as an <code>IComparable&lt;T&gt;</code> object, where the cut function is
    /// the <code>c.CompareTo(T that)</code> method.</param>
    /// <param name="low">Returns the largest item in the collection, where the
    /// cut function is positive (if any).</param>
    /// <param name="lowIsValid">True if the cut function is positive somewhere
    /// on this collection.</param>
    /// <param name="high">Returns the least item in the collection, where the
    /// cut function is negative (if any).</param>
    /// <param name="highIsValid">True if the cut function is negative somewhere
    /// on this collection.</param>
    /// <returns></returns>
    [Tested]
    public bool Cut(IComparable<T> c, out T low, out bool lowIsValid, out T high, out bool highIsValid)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
    public int CountFrom(T bot)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return size - countTo(bot, true);
    }


    /// <summary>
    /// Determine the number of items between two supplied thresholds.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive)</param>
    /// <param name="top">The upper bound (exclusive)</param>
    /// <returns>The number of matcing items.</returns>
    [Tested]
    public int CountFromTo(T bot, T top)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
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
    public int CountTo(T top)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      return countTo(top, true);
    }


    /// <summary>
    /// Remove all items of this collection above or at a supplied threshold.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    [Tested]
    public void RemoveRangeFrom(T low)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      int count = CountFrom(low);

      if (count == 0)
        return;

      stackcheck();
      CircularQueue<T> wasRemoved = (ActiveEvents & EventTypeEnum.Removed) != 0 ? new CircularQueue<T>() : null;

      for (int i = 0; i < count; i++)
      {
        T item = deleteMax();
        if (wasRemoved != null)
          wasRemoved.Enqueue(item);
      }
      if (wasRemoved != null)
        raiseForRemoveAll(wasRemoved);
      else if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }


    /// <summary>
    /// Remove all items of this collection between two supplied thresholds.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    /// <param name="hi">The upper threshold (exclusive).</param>
    [Tested]
    public void RemoveRangeFromTo(T low, T hi)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      int count = CountFromTo(low, hi);

      if (count == 0)
        return;

      CircularQueue<T> wasRemoved = (ActiveEvents & EventTypeEnum.Removed) != 0 ? new CircularQueue<T>() : null;
      int junk;
      for (int i = 0; i < count; i++)
      {
        T item = Predecessor(hi);
        removeIterative(ref item, false, out junk);
        if (wasRemoved != null)
          wasRemoved.Enqueue(item);
      }
      if (wasRemoved != null)
        raiseForRemoveAll(wasRemoved);
      else if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }


    /// <summary>
    /// Remove all items of this collection below a supplied threshold.
    /// </summary>
    /// <param name="hi">The upper threshold (exclusive).</param>
    [Tested]
    public void RemoveRangeTo(T hi)
    {
      if (!isValid)
        throw new ViewDisposedException("Snapshot has been disposed");
      updatecheck();

      int count = CountTo(hi);

      if (count == 0)
        return;

      stackcheck();
      CircularQueue<T> wasRemoved = (ActiveEvents & EventTypeEnum.Removed) != 0 ? new CircularQueue<T>() : null;

      for (int i = 0; i < count; i++)
      {
        T item = deleteMin();
        if (wasRemoved != null)
          wasRemoved.Enqueue(item);
      }
      if (wasRemoved != null)
        raiseForRemoveAll(wasRemoved);
      else if ((ActiveEvents & EventTypeEnum.Changed) != 0)
        raiseCollectionChanged();
    }

    #endregion

    #region IPersistent<T> Members
#if NCP
    int maxsnapid { get { return snapList == null ? -1 : findLastLiveSnapShot(); } }

    int findLastLiveSnapShot()
    {
      if (snapList == null)
        return -1;
      SnapRef lastLiveSnapRef = snapList.Prev;
      object _snapshot = null;
      while (lastLiveSnapRef != null && (_snapshot = lastLiveSnapRef.Tree.Target) == null)
        lastLiveSnapRef = lastLiveSnapRef.Prev;
      if (lastLiveSnapRef == null)
      {
        snapList = null;
        return -1;
      }
      if (snapList.Prev != lastLiveSnapRef)
      {
        snapList.Prev = lastLiveSnapRef;
        lastLiveSnapRef.Next = snapList;
      }
      return ((TreeSet<T>)_snapshot).generation;
    }

    [Serializable]
    class SnapRef
    {
      public SnapRef Prev, Next;
      public WeakReference Tree;
      public SnapRef(TreeSet<T> tree) { Tree = new WeakReference(tree); }
      public void Dispose()
      {
        Next.Prev = Prev;
        if (Prev != null)
          Prev.Next = Next;
        Next = Prev = null;
      }
    }
#endif

    /// <summary>
    /// If this tree is a snapshot, remove registration in base tree
    /// </summary>
    [Tested]
    public void Dispose()
    {
#if NCP
      if (!isValid)
        return;
      if (isSnapShot)
      {
        snapList.Dispose();
        snapDispose();
      }
      else
      {
        if (snapList != null)
        {
          SnapRef someSnapRef = snapList.Prev;
          while (someSnapRef != null)
          {
            TreeSet<T> lastsnap;
            if ((lastsnap = someSnapRef.Tree.Target as TreeSet<T>) != null)
              lastsnap.snapDispose();
            someSnapRef = someSnapRef.Prev;
          }
        }
        snapList = null;
        Clear();
      }
#else
      Clear();
#endif
    }

    private void snapDispose()
    {
      root = null;
      dirs = null;
      path = null;
      comparer = null;
      isValid = false;
      snapList = null;
    }

    /// <summary>
    /// Make a (read-only) snapshot of this collection.
    /// </summary>
    /// <returns>The snapshot.</returns>
    [Tested]
    public ISorted<T> Snapshot()
    {
#if NCP
      if (isSnapShot)
        throw new InvalidOperationException("Cannot snapshot a snapshot");

      TreeSet<T> res = (TreeSet<T>)MemberwiseClone();
      SnapRef newSnapRef = new SnapRef(res);
      res.isReadOnlyBase = true;
      res.isSnapShot = true;
      res.snapList = newSnapRef;

      findLastLiveSnapShot();
      if (snapList == null)
        snapList = new SnapRef(this);
      SnapRef lastLiveSnapRef = snapList.Prev;

      newSnapRef.Prev = lastLiveSnapRef;
      if (lastLiveSnapRef != null)
        lastLiveSnapRef.Next = newSnapRef;
      newSnapRef.Next = snapList;
      snapList.Prev = newSnapRef;

      generation++;

      return res;
#endif
    }

    #endregion

    #region TreeSet.Range nested class

    internal class Range : DirectedCollectionValueBase<T>, IDirectedCollectionValue<T>
    {
      //We actually need exclusive upper and lower bounds, and flags to 
      //indicate whether the bound is present (we canot rely on default(T))
      private int stamp, size;

      private TreeSet<T> basis;

      private T lowend, highend;

      private bool haslowend, hashighend;

      EnumerationDirection direction;


      [Tested]
      public Range(TreeSet<T> basis, bool haslowend, T lowend, bool hashighend, T highend, EnumerationDirection direction)
      {
        this.basis = basis;
        stamp = basis.stamp;

        //lowind will be const; should we cache highind?
        this.lowend = lowend; //Inclusive
        this.highend = highend;//Exclusive
        this.haslowend = haslowend;
        this.hashighend = hashighend;
        this.direction = direction;
        if (!basis.isSnapShot)
          size = haslowend ?
              (hashighend ? basis.CountFromTo(lowend, highend) : basis.CountFrom(lowend)) :
              (hashighend ? basis.CountTo(highend) : basis.Count);
      }

      #region IEnumerable<T> Members


      #region TreeSet.Range.Enumerator nested class

      internal class Enumerator : SCG.IEnumerator<T>
      {
        #region Private Fields
        private bool valid = false, ready = true;

        private SCG.IComparer<T> comparer;

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
        /// <exception cref="CollectionModifiedException"/> if underlying tree was modified.
        /// </summary>
        /// <returns>True if enumerator is valid now</returns>
        [Tested]
        public bool MoveNext()
        {
          range.basis.modifycheck(range.stamp);
          if (!ready)
            return false;
#if BAG
          if (--togo > 0)
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

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
          get { return Current; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
          return MoveNext();
        }

        void System.Collections.IEnumerator.Reset()
        {
          throw new Exception("The method or operation is not implemented.");
        }

        #endregion
      }

      #endregion


      public override T Choose()
      {
        if (size == 0) throw new NoSuchItemException();
        return lowend;
      }

      [Tested]
      public override SCG.IEnumerator<T> GetEnumerator() { return new Enumerator(this); }


      [Tested]
      public override EnumerationDirection Direction { [Tested]get { return direction; } }


      #endregion

      #region Utility

      bool inside(T item)
      {
        return (!haslowend || basis.comparer.Compare(item, lowend) >= 0) && (!hashighend || basis.comparer.Compare(item, highend) < 0);
      }


      void checkstamp()
      {
        if (stamp < basis.stamp)
          throw new CollectionModifiedException();
      }


      void syncstamp() { stamp = basis.stamp; }

      #endregion

      [Tested]
      public override IDirectedCollectionValue<T> Backwards()
      {
        Range b = (Range)MemberwiseClone();

        b.direction = direction == EnumerationDirection.Forwards ? EnumerationDirection.Backwards : EnumerationDirection.Forwards;
        return b;
      }


      [Tested]
      IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }


      public override bool IsEmpty { get { return size == 0; } }

      [Tested]
      public override int Count { [Tested] get { return size; } }

      //TODO: check that this is correct
      public override Speed CountSpeed { get { return Speed.Constant; } }

    }

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
        Console.WriteLine(String.Format("{0} {4} (size={1}, items={8}, h={2}, gen={3}, id={6}){7}", space + n.item,
#if MAINTAIN_SIZE
 n.size,
#else
				0,
#endif
 0,
#if NCP
 n.generation,
#endif
 n.red ? "RED" : "BLACK",
 0,
 0,
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
      0
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
      0
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
        0
        , m);

      return b;
    }


    bool rbminicheck(Node n, bool redp, System.IO.TextWriter o, out T min, out T max, out int blackheight)
    {//Red-Black invariant
      bool res = true;

      res = massert(!(n.red && redp), n, "RED parent of RED node", o) && res;
      res = massert(n.left == null || n.right != null || n.left.red, n, "Left child black, but right child empty", o) && res;
      res = massert(n.right == null || n.left != null || n.right.red, n, "Right child black, but left child empty", o) && res;
#if BAG
      bool sb = n.size == (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + n.items;

      res = massert(sb, n, "Bad size", o) && res;
#elif MAINTAIN_SIZE
      bool sb = n.size == (n.left == null ? 0 : n.left.size) + (n.right == null ? 0 : n.right.size) + 1;

      res = massert(sb, n, "Bad size", o) && res;
#endif
      min = max = n.item;

      T otherext;
      int lbh = 0, rbh = 0;

      if (n.left != null)
      {
        res = rbminicheck(n.left, n.red, o, out min, out otherext, out lbh) && res;
        res = massert(comparer.Compare(n.item, otherext) > 0, n, "Value not > all left children", o) && res;
      }

      if (n.right != null)
      {
        res = rbminicheck(n.right, n.red, o, out otherext, out max, out rbh) && res;
        res = massert(comparer.Compare(n.item, otherext) < 0, n, "Value not < all right children", o) && res;
      }

      res = massert(rbh == lbh, n, "Different blackheights of children", o) && res;
      blackheight = n.red ? rbh : rbh + 1;
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
      if (!isValid)
        return true;
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
        bool res = rbminicheck(root, false, o, out min, out max, out blackheight);
        res = massert(blackheight == blackdepth, root, "bad blackh/d", o) && res;
        res = massert(!root.red, root, "root is red", o) && res;
#if MAINTAIN_SIZE
        res = massert(root.size == size, root, "count!=root.size", o) && res;
#endif
        return !res;
      }
      else
        return false;
    }
    #endregion

    #region ICloneable Members

    /// <summary>
    /// Make a shallow copy of this TreeSet.
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      TreeSet<T> clone = new TreeSet<T>(comparer, EqualityComparer);
      //TODO: make sure the TreeBag AddSorted copies tree bags smartly!!!
      clone.AddSorted(this);
      return clone;
    }

    #endregion

  }
}

