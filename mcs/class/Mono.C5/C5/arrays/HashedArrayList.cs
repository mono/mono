
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

#define HASHINDEX

using System;
using System.Diagnostics;
using SCG = System.Collections.Generic;
namespace C5
{
  /// <summary>
  /// A list collection based on a plain dynamic array data structure.
  /// Expansion of the internal array is performed by doubling on demand. 
  /// The internal array is only shrinked by the Clear method. 
  ///
  /// <i>When the FIFO property is set to false this class works fine as a stack of T.
  /// When the FIFO property is set to true the class will function as a (FIFO) queue
  /// but very inefficiently, use a LinkedList (<see cref="T:C5.LinkedList`1"/>) instead.</i>
  /// </summary>
  [Serializable]
  public class HashedArrayList<T> : ArrayBase<T>, IList<T>, SCG.IList<T>
#if HASHINDEX
#else
, IStack<T>, IQueue<T>
#endif
  {
    #region Fields

    /// <summary>
    /// Has this list or view not been invalidated by some operation (by someone calling Dispose())
    /// </summary>
    bool isValid = true;

    //TODO: wonder if we should save some memory on none-view situations by 
    //      putting these three fields into a single ref field?
    /// <summary>
    /// The underlying list if we are a view, null else.
    /// </summary>
    HashedArrayList<T> underlying;
    WeakViewList<HashedArrayList<T>> views;
    WeakViewList<HashedArrayList<T>>.Node myWeakReference;

    /// <summary>
    /// The size of the underlying list.
    /// </summary>
    int underlyingsize { get { return (underlying ?? this).size; } }

    /// <summary>
    /// The underlying field of the FIFO property
    /// </summary>
    bool fIFO = false;

#if HASHINDEX
    HashSet<KeyValuePair<T, int>> itemIndex;
#endif
    #endregion
    #region Events

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public override EventTypeEnum ListenableEvents { get { return underlying == null ? EventTypeEnum.All : EventTypeEnum.None; } }

    /*
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event CollectionChangedHandler<T> CollectionChanged
        {
          add
          {
            if (underlying == null)
              base.CollectionChanged += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.CollectionChanged -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event CollectionClearedHandler<T> CollectionCleared
        {
          add
          {
            if (underlying == null)
              base.CollectionCleared += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.CollectionCleared -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event ItemsAddedHandler<T> ItemsAdded
        {
          add
          {
            if (underlying == null)
              base.ItemsAdded += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.ItemsAdded -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event ItemInsertedHandler<T> ItemInserted
        {
          add
          {
            if (underlying == null)
              base.ItemInserted += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.ItemInserted -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event ItemsRemovedHandler<T> ItemsRemoved
        {
          add
          {
            if (underlying == null)
              base.ItemsRemoved += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.ItemsRemoved -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public override event ItemRemovedAtHandler<T> ItemRemovedAt
        {
          add
          {
            if (underlying == null)
              base.ItemRemovedAt += value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
          remove
          {
            if (underlying == null)
              base.ItemRemovedAt -= value;
            else
              throw new UnlistenableEventException("Can't listen to a view");
          }
        }

          */

    #endregion
    #region Util

    bool equals(T i1, T i2) { return itemequalityComparer.Equals(i1, i2); }

    /// <summary>
    /// Increment or decrement the private size fields
    /// </summary>
    /// <param name="delta">Increment (with sign)</param>
    void addtosize(int delta)
    {
      size += delta;
      if (underlying != null)
        underlying.size += delta;
    }

    #region Array handling
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
      if (underlying != null)
        underlying.expand(newcapacity, newsize);
      else
      {
        base.expand(newcapacity, newsize);
        if (views != null)
          foreach (HashedArrayList<T> v in views)
            v.array = array;
      }
    }

    #endregion

    #region Checks
    /// <summary>
    /// Check if it is valid to perform updates and increment stamp if so.
    /// </summary>
    /// <exception cref="ViewDisposedException"> If check fails by this list being a disposed view.</exception>
    /// <exception cref="ReadOnlyCollectionException"> If check fails by this being a read only list.</exception>
    protected override void updatecheck()
    {
      validitycheck();
      base.updatecheck();
      if (underlying != null)
        underlying.stamp++;
    }


    /// <summary>
    /// Check if we are a view that the underlying list has only been updated through us.
    /// <para>This method should be called from enumerators etc to guard against 
    /// modification of the base collection.</para>
    /// </summary>
    /// <exception cref="ViewDisposedException"> if check fails.</exception>
    void validitycheck()
    {
      if (!isValid)
        throw new ViewDisposedException();
    }


    /// <summary>
    /// Check that the list has not been updated since a particular time.
    /// <para>To be used by enumerators and range </para>
    /// </summary>
    /// <exception cref="ViewDisposedException"> If check fails by this list being a disposed view.</exception>
    /// <exception cref="CollectionModifiedException">If the list *has* beeen updated since that  time..</exception>
    /// <param name="stamp">The stamp indicating the time.</param>
    protected override void modifycheck(int stamp)
    {
      validitycheck();
      if (this.stamp != stamp)
        throw new CollectionModifiedException();
    }

    #endregion

    #region Searching

    /// <summary>
    /// Internal version of IndexOf without modification checks.
    /// </summary>
    /// <param name="item">Item to look for</param>
    /// <returns>The index of first occurrence</returns>
    int indexOf(T item)
    {
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>(item);
      if (itemIndex.Find(ref p) && p.Value >= offset && p.Value < offset + size)
        return p.Value - offset;
#else
      for (int i = 0; i < size; i++)
        if (equals(item, array[offset + i]))
          return i;
#endif
      return ~size;
    }

    /// <summary>
    /// Internal version of LastIndexOf without modification checks.
    /// </summary>
    /// <param name="item">Item to look for</param>
    /// <returns>The index of last occurrence</returns>
    int lastIndexOf(T item)
    {
#if HASHINDEX
      return indexOf(item);
#else
      for (int i = size - 1; i >= 0; i--)
        if (equals(item, array[offset + i]))
          return i;
      return ~size;
#endif
    }
    #endregion

    #region Inserting

#if HASHINDEX
    /// <summary>
    /// Internal version of Insert with no modification checks.
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException"> if item already in list.</exception>
    /// <param name="i">Index to insert at</param>
    /// <param name="item">Item to insert</param>
#else
    /// <summary>
    /// Internal version of Insert with no modification checks.
    /// </summary>
    /// <param name="i">Index to insert at</param>
    /// <param name="item">Item to insert</param>
#endif
    protected override void insert(int i, T item)
    {
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>(item, offset + i);
      if (itemIndex.FindOrAdd(ref p))
        throw new DuplicateNotAllowedException("Item already in indexed list: " + item);
#endif
      baseinsert(i, item);
#if HASHINDEX
      reindex(i + offset + 1);
#endif
    }

    private void baseinsert(int i, T item)
    {
      if (underlyingsize == array.Length)
        expand();
      i += offset;
      if (i < underlyingsize)
        Array.Copy(array, i, array, i + 1, underlyingsize - i);
      array[i] = item;
      addtosize(1);
      fixViewsAfterInsert(1, i);
    }
    #endregion

    #region Removing

    /// <summary>
    /// Internal version of RemoveAt with no modification checks.
    /// </summary>
    /// <param name="i">Index to remove at</param>
    /// <returns>The removed item</returns>
    T removeAt(int i)
    {
      i += offset;
      fixViewsBeforeSingleRemove(i);
      T retval = array[i];
      addtosize(-1);
      if (underlyingsize > i)
        Array.Copy(array, i + 1, array, i, underlyingsize - i);
      array[underlyingsize] = default(T);
#if HASHINDEX
      itemIndex.Remove(new KeyValuePair<T, int>(retval));
      reindex(i);
#endif
      return retval;
    }
    #endregion

    #region Indexing

#if HASHINDEX
    private void reindex(int start) { reindex(start, underlyingsize); }

    private void reindex(int start, int end)
    {
      for (int j = start; j < end; j++)
        itemIndex.UpdateOrAdd(new KeyValuePair<T, int>(array[j], j));
    }
#endif
    #endregion

    #region fixView utilities

    /// <summary>
    /// 
    /// </summary>
    /// <param name="added">The actual number of inserted nodes</param>
    /// <param name="realInsertionIndex"></param>
    void fixViewsAfterInsert(int added, int realInsertionIndex)
    {
      if (views != null)
        foreach (HashedArrayList<T> view in views)
        {
          if (view != this)
          {
            if (view.offset < realInsertionIndex && view.offset + view.size > realInsertionIndex)
              view.size += added;
            if (view.offset > realInsertionIndex || (view.offset == realInsertionIndex && view.size > 0))
              view.offset += added;
          }
        }
    }

    void fixViewsBeforeSingleRemove(int realRemovalIndex)
    {
      if (views != null)
        foreach (HashedArrayList<T> view in views)
        {
          if (view != this)
          {
            if (view.offset <= realRemovalIndex && view.offset + view.size > realRemovalIndex)
              view.size--;
            if (view.offset > realRemovalIndex)
              view.offset--;
          }
        }
    }

    /// <summary>
    /// Fix offsets and sizes of other views before removing an interval from this 
    /// </summary>
    /// <param name="start">the start of the interval relative to the array/underlying</param>
    /// <param name="count"></param>
    void fixViewsBeforeRemove(int start, int count)
    {
      int clearend = start + count - 1;
      if (views != null)
        foreach (HashedArrayList<T> view in views)
        {
          if (view == this)
            continue;
          int viewoffset = view.offset, viewend = viewoffset + view.size - 1;
          if (start < viewoffset)
          {
            if (clearend < viewoffset)
              view.offset = viewoffset - count;
            else
            {
              view.offset = start;
              view.size = clearend < viewend ? viewend - clearend : 0;
            }
          }
          else if (start <= viewend)
            view.size = clearend <= viewend ? view.size - count : start - viewoffset;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="otherOffset"></param>
    /// <param name="otherSize"></param>
    /// <returns>The position of View(otherOffset, otherSize) wrt. this view</returns>
    MutualViewPosition viewPosition(int otherOffset, int otherSize)
    {
      int end = offset + size, otherEnd = otherOffset + otherSize;
      if (otherOffset >= end || otherEnd <= offset)
        return MutualViewPosition.NonOverlapping;
      if (size == 0 || (otherOffset <= offset && end <= otherEnd))
        return MutualViewPosition.Contains;
      if (otherSize == 0 || (offset <= otherOffset && otherEnd <= end))
        return MutualViewPosition.ContainedIn;
      return MutualViewPosition.Overlapping;
    }

    //TODO: make version that fits the new, more forgiving rules for disposing
    void disposeOverlappingViews(bool reverse)
    {
      if (views != null)
        foreach (HashedArrayList<T> view in views)
        {
          if (view != this)
          {
            switch (viewPosition(view.offset, view.size))
            {
              case MutualViewPosition.ContainedIn:
                if (reverse)
                  view.offset = 2 * offset + size - view.size - view.offset;
                else
                  view.Dispose();
                break;
              case MutualViewPosition.Overlapping:
                view.Dispose();
                break;
              case MutualViewPosition.Contains:
              case MutualViewPosition.NonOverlapping:
                break;
            }
          }
        }
    }
    #endregion

    #endregion

    #region Position, PositionComparer and ViewHandler nested types
    class PositionComparer : SCG.IComparer<Position>
    {
      public int Compare(Position a, Position b)
      {
        return a.index.CompareTo(b.index);
      }
    }
    /// <summary>
    /// During RemoveAll, we need to cache the original endpoint indices of views (??? also for HashedArrayList?)
    /// </summary>
    struct Position
    {
      public readonly HashedArrayList<T> view;
      public readonly int index;
      public Position(HashedArrayList<T> view, bool left)
      {
        this.view = view;
        index = left ? view.offset : view.offset + view.size - 1;
      }
      public Position(int index) { this.index = index; view = null; }
    }

    /// <summary>
    /// Handle the update of (other) views during a multi-remove operation.
    /// </summary>
    struct ViewHandler
    {
      HashedArrayList<Position> leftEnds;
      HashedArrayList<Position> rightEnds;
      int leftEndIndex, rightEndIndex;
      internal readonly int viewCount;
      internal ViewHandler(HashedArrayList<T> list)
      {
        leftEndIndex = rightEndIndex = viewCount = 0;
        leftEnds = rightEnds = null;
        if (list.views != null)
          foreach (HashedArrayList<T> v in list.views)
            if (v != list)
            {
              if (leftEnds == null)
              {
                leftEnds = new HashedArrayList<Position>();
                rightEnds = new HashedArrayList<Position>();
              }
              leftEnds.Add(new Position(v, true));
              rightEnds.Add(new Position(v, false));
            }
        if (leftEnds == null)
          return;
        viewCount = leftEnds.Count;
        leftEnds.Sort(new PositionComparer());
        rightEnds.Sort(new PositionComparer());
      }
      /// <summary>
      /// This is to be called with realindex pointing to the first node to be removed after a (stretch of) node that was not removed
      /// </summary>
      /// <param name="removed"></param>
      /// <param name="realindex"></param>
      internal void skipEndpoints(int removed, int realindex)
      {
        if (viewCount > 0)
        {
          Position endpoint;
          while (leftEndIndex < viewCount && (endpoint = leftEnds[leftEndIndex]).index <= realindex)
          {
            HashedArrayList<T> view = endpoint.view;
            view.offset = view.offset - removed;
            view.size += removed;
            leftEndIndex++;
          }
          while (rightEndIndex < viewCount && (endpoint = rightEnds[rightEndIndex]).index < realindex)
          {
            endpoint.view.size -= removed;
            rightEndIndex++;
          }
        }
      }
      internal void updateViewSizesAndCounts(int removed, int realindex)
      {
        if (viewCount > 0)
        {
          Position endpoint;
          while (leftEndIndex < viewCount && (endpoint = leftEnds[leftEndIndex]).index <= realindex)
          {
            HashedArrayList<T> view = endpoint.view;
            view.offset = view.Offset - removed;
            view.size += removed;
            leftEndIndex++;
          }
          while (rightEndIndex < viewCount && (endpoint = rightEnds[rightEndIndex]).index < realindex)
          {
            endpoint.view.size -= removed;
            rightEndIndex++;
          }
        }
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Create an array list with default item equalityComparer and initial capacity 8 items.
    /// </summary>
    public HashedArrayList() : this(8) { }


    /// <summary>
    /// Create an array list with external item equalityComparer and initial capacity 8 items.
    /// </summary>
    /// <param name="itemequalityComparer">The external item equalityComparer</param>
    public HashedArrayList(SCG.IEqualityComparer<T> itemequalityComparer) : this(8, itemequalityComparer) { }


    /// <summary>
    /// Create an array list with default item equalityComparer and prescribed initial capacity.
    /// </summary>
    /// <param name="capacity">The prescribed capacity</param>
    public HashedArrayList(int capacity) : this(capacity, EqualityComparer<T>.Default) { }


    /// <summary>
    /// Create an array list with external item equalityComparer and prescribed initial capacity.
    /// </summary>
    /// <param name="capacity">The prescribed capacity</param>
    /// <param name="itemequalityComparer">The external item equalityComparer</param>
    public HashedArrayList(int capacity, SCG.IEqualityComparer<T> itemequalityComparer)
      : base(capacity, itemequalityComparer)
    {
#if HASHINDEX
      itemIndex = new HashSet<KeyValuePair<T, int>>(new KeyValuePairEqualityComparer<T, int>(itemequalityComparer));
#endif
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <value>The first item in this list.</value>
    [Tested]
    public virtual T First
    {
      [Tested]
      get
      {
        validitycheck();
        if (size == 0)
          throw new NoSuchItemException();

        return array[offset];
      }
    }


    /// <summary>
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <value>The last item in this list.</value>
    [Tested]
    public virtual T Last
    {
      [Tested]
      get
      {
        validitycheck();
        if (size == 0)
          throw new NoSuchItemException();

        return array[offset + size - 1];
      }
    }


    /// <summary>
    /// Since <code>Add(T item)</code> always add at the end of the list,
    /// this describes if list has FIFO or LIFO semantics.
    /// </summary>
    /// <value>True if the <code>Remove()</code> operation removes from the
    /// start of the list, false if it removes from the end. The default for a new array list is false.</value>
    [Tested]
    public virtual bool FIFO
    {
      [Tested]
      get { validitycheck(); return fIFO; }
      [Tested]
      set { updatecheck(); fIFO = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsFixedSize
    {
      get { validitycheck(); return false; }
    }


#if HASHINDEX
    /// <summary>
    /// On this list, this indexer is read/write.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt;= the size of the collection.</exception>
    /// <exception cref="DuplicateNotAllowedException"> By the get operation
    /// if the item already is present somewhere else in the list.</exception>
    /// <value>The index'th item of this list.</value>
    /// <param name="index">The index of the item to fetch or store.</param>
#else
    /// <summary>
    /// On this list, this indexer is read/write.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt;= the size of the collection.</exception>
    /// <value>The index'th item of this list.</value>
    /// <param name="index">The index of the item to fetch or store.</param>
#endif
    [Tested]
    public virtual T this[int index]
    {
      [Tested]
      get
      {
        validitycheck();
        if (index < 0 || index >= size)
          throw new IndexOutOfRangeException();

        return array[offset + index];
      }
      [Tested]
      set
      {
        updatecheck();
        if (index < 0 || index >= size)
          throw new IndexOutOfRangeException();
        index += offset;
        T item = array[index];
#if HASHINDEX
        KeyValuePair<T, int> p = new KeyValuePair<T, int>(value, index);
        if (itemequalityComparer.Equals(value, item))
        {
          array[index] = value;
          itemIndex.Update(p);
        }
        else if (!itemIndex.FindOrAdd(ref p))
        {
          itemIndex.Remove(new KeyValuePair<T, int>(item));
          array[index] = value;
        }
        else
          throw new DuplicateNotAllowedException("Item already in indexed list");
#else
        array[index] = value;
#endif
        (underlying ?? this).raiseForSetThis(index, value, item);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public virtual Speed IndexingSpeed { get { return Speed.Constant; } }

#if HASHINDEX
    /// <summary>
    /// Insert an item at a specific index location in this list. 
    ///</summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt; the size of the collection. </exception>
    /// <exception cref="DuplicateNotAllowedException"> 
    /// If the item is already present in the list.</exception>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
#else
    /// <summary>
    /// Insert an item at a specific index location in this list. 
    ///</summary>
    /// <exception cref="IndexOutOfRangeException"> if i is negative or
    /// &gt; the size of the collection. </exception>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
#endif
    [Tested]
    public virtual void Insert(int index, T item)
    {
      updatecheck();
      if (index < 0 || index > size)
        throw new IndexOutOfRangeException();

      insert(index, item);
      (underlying ?? this).raiseForInsert(index + offset, item);
    }

    /// <summary>
    /// Insert an item at the end of a compatible view, used as a pointer.
    /// <para>The <code>pointer</code> must be a view on the same list as
    /// <code>this</code> and the endpoitn of <code>pointer</code> must be
    /// a valid insertion point of <code>this</code></para>
    /// </summary>
    /// <exception cref="IncompatibleViewException">If <code>pointer</code> 
    /// is not a view on or the same list as <code>this</code></exception>
    /// <exception cref="IndexOutOfRangeException"><b>??????</b> if the endpoint of 
    ///  <code>pointer</code> is not inside <code>this</code></exception>
    /// <exception cref="DuplicateNotAllowedException"> if the list has
    /// <code>AllowsDuplicates==false</code> and the item is 
    /// already in the list.</exception>
    /// <param name="pointer"></param>
    /// <param name="item"></param>
    public void Insert(IList<T> pointer, T item)
    {
      if ((pointer == null) || ((pointer.Underlying ?? pointer) != (underlying ?? this)))
        throw new IncompatibleViewException();
      Insert(pointer.Offset + pointer.Count - Offset, item);
    }

#if HASHINDEX
    /// <summary>
    /// Insert into this list all items from an enumerable collection starting 
    /// at a particular index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt; the size of the collection.</exception>
    /// <exception cref="DuplicateNotAllowedException"> If <code>items</code> 
    /// contains duplicates or some item already  present in the list.</exception>
    /// <param name="index">Index to start inserting at</param>
    /// <param name="items">Items to insert</param>
#else
    /// <summary>
    /// Insert into this list all items from an enumerable collection starting 
    /// at a particular index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt; the size of the collection.</exception>
    /// <param name="index">Index to start inserting at</param>
    /// <param name="items">Items to insert</param>
    /// <typeparam name="U"></typeparam>
#endif
    [Tested]
    public virtual void InsertAll<U>(int index, SCG.IEnumerable<U> items) where U : T
    {
      updatecheck();
      if (index < 0 || index > size)
        throw new IndexOutOfRangeException();
      index += offset;
      int toadd = EnumerableBase<U>.countItems(items);
      if (toadd == 0)
        return;
      if (toadd + underlyingsize > array.Length)
        expand(toadd + underlyingsize, underlyingsize);
      if (underlyingsize > index)
        Array.Copy(array, index, array, index + toadd, underlyingsize - index);
      int i = index;
      try
      {

        foreach (T item in items)
        {
#if HASHINDEX
          KeyValuePair<T, int> p = new KeyValuePair<T, int>(item, i);
          if (itemIndex.FindOrAdd(ref p))
            throw new DuplicateNotAllowedException("Item already in indexed list");
#endif
          array[i++] = item;
        }
      }
      finally
      {
        int added = i - index;
        if (added < toadd)
        {
          Array.Copy(array, index + toadd, array, i, underlyingsize - index);
          Array.Clear(array, underlyingsize + added, toadd - added);
        }
        if (added > 0)
        {
          addtosize(added);
#if HASHINDEX
          reindex(i);
#endif
          fixViewsAfterInsert(added, index);
          (underlying ?? this).raiseForInsertAll(index, added);
        }
      }
    }
    private void raiseForInsertAll(int index, int added)
    {
      if (ActiveEvents != 0)
      {
        if ((ActiveEvents & (EventTypeEnum.Added | EventTypeEnum.Inserted)) != 0)
          for (int j = index; j < index + added; j++)
          {
            raiseItemInserted(array[j], j);
            raiseItemsAdded(array[j], 1);
          }
        raiseCollectionChanged();
      }
    }

#if HASHINDEX
    /// <summary>
    /// Insert an item at the front of this list;
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException">If the item is already in the list</exception>
    /// <param name="item">The item to insert.</param>
#else
    /// <summary>
    /// Insert an item at the front of this list;
    /// </summary>
    /// <param name="item">The item to insert.</param>
#endif
    [Tested]
    public virtual void InsertFirst(T item)
    {
      updatecheck();
      insert(0, item);
      (underlying ?? this).raiseForInsert(offset, item);
    }


#if HASHINDEX
    /// <summary>
    /// Insert an item at the back of this list.
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException">If the item is already in the list</exception>
    /// <param name="item">The item to insert.</param>
#else
    /// <summary>
    /// Insert an item at the back of this list.
    /// </summary>
    /// <param name="item">The item to insert.</param>
#endif
    [Tested]
    public virtual void InsertLast(T item)
    {
      updatecheck();
      insert(size, item);
      (underlying ?? this).raiseForInsert(size - 1 + offset, item);
    }


    //NOTE: if the filter throws an exception, no result will be returned.
    /// <summary>
    /// Create a new list consisting of the items of this list satisfying a 
    /// certain predicate.
    /// <para>The new list will be of type HashedArrayList</para>
    /// </summary>
    /// <param name="filter">The filter delegate defining the predicate.</param>
    /// <returns>The new list.</returns>
    [Tested]
    public virtual IList<T> FindAll(Fun<T, bool> filter)
    {
      validitycheck();
      int stamp = this.stamp;
      HashedArrayList<T> res = new HashedArrayList<T>(itemequalityComparer);
      int j = 0, rescap = res.array.Length;
      for (int i = 0; i < size; i++)
      {
        T a = array[offset + i];
        bool found = filter(a);
        modifycheck(stamp);
        if (found)
        {
          if (j == rescap) res.expand(rescap = 2 * rescap, j);
          res.array[j++] = a;
        }
      }
      res.size = j;
#if HASHINDEX
      res.reindex(0);
#endif
      return res;
    }


#if HASHINDEX
    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use the default item equalityComparer for the item type V.
    /// <para>The new list will be of type HashedArrayList</para>
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException">If <code>mapper</code>
    /// creates duplicates</exception>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <returns>The new list.</returns>
#else
    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use the default item equalityComparer for the item type V.
    /// <para>The new list will be of type HashedArrayList</para>
    /// </summary>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <returns>The new list.</returns>
#endif
    [Tested]
    public virtual IList<V> Map<V>(Fun<T, V> mapper)
    {
      validitycheck();

      HashedArrayList<V> res = new HashedArrayList<V>(size);

      return map<V>(mapper, res);
    }

#if HASHINDEX
    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use a specified item equalityComparer for the item type.
    /// <para>The new list will be of type HashedArrayList</para>
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException">If <code>mapper</code>
    /// creates duplicates</exception>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <param name="itemequalityComparer">The item equalityComparer to use for the new list</param>
    /// <returns>The new list.</returns>
#else
    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use a specified item equalityComparer for the item type.
    /// <para>The new list will be of type HashedArrayList</para>
    /// </summary>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <param name="itemequalityComparer">The item equalityComparer to use for the new list</param>
    /// <returns>The new list.</returns>
#endif
    public virtual IList<V> Map<V>(Fun<T, V> mapper, SCG.IEqualityComparer<V> itemequalityComparer)
    {
      validitycheck();

      HashedArrayList<V> res = new HashedArrayList<V>(size, itemequalityComparer);

      return map<V>(mapper, res);
    }

    private IList<V> map<V>(Fun<T, V> mapper, HashedArrayList<V> res)
    {
      int stamp = this.stamp;
      if (size > 0)
        for (int i = 0; i < size; i++)
        {
          V mappeditem = mapper(array[offset + i]);
          modifycheck(stamp);
#if HASHINDEX
          KeyValuePair<V, int> p = new KeyValuePair<V, int>(mappeditem, i);
          if (res.itemIndex.FindOrAdd(ref p))
            throw new ArgumentException("Mapped item already in indexed list");
#endif
          res.array[i] = mappeditem;
        }
      res.size = size;
      return res;
    }

    /// <summary>
    /// Remove one item from the list: from the front if <code>FIFO</code>
    /// is true, else from the back.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <returns>The removed item.</returns>
    [Tested]
    public virtual T Remove()
    {
      updatecheck();
      if (size == 0)
        throw new NoSuchItemException("List is empty");

      T item = removeAt(fIFO ? 0 : size - 1);
      (underlying ?? this).raiseForRemove(item);
      return item;
    }

    /// <summary>
    /// Remove one item from the fromnt of the list.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <returns>The removed item.</returns>
    [Tested]
    public virtual T RemoveFirst()
    {
      updatecheck();
      if (size == 0)
        throw new NoSuchItemException("List is empty");

      T item = removeAt(0);
      (underlying ?? this).raiseForRemoveAt(offset, item);
      return item;
    }


    /// <summary>
    /// Remove one item from the back of the list.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <returns>The removed item.</returns>
    [Tested]
    public virtual T RemoveLast()
    {
      updatecheck();
      if (size == 0)
        throw new NoSuchItemException("List is empty");

      T item = removeAt(size - 1);
      (underlying ?? this).raiseForRemoveAt(size + offset, item);
      return item;
    }

    /// <summary>
    /// Create a list view on this list. 
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> if the start or count is negative
    /// or the range does not fit within list.</exception>
    /// <param name="start">The index in this list of the start of the view.</param>
    /// <param name="count">The size of the view.</param>
    /// <returns>The new list view.</returns>
    [Tested]
    public virtual IList<T> View(int start, int count)
    {
      validitycheck();
      checkRange(start, count);
      if (views == null)
        views = new WeakViewList<HashedArrayList<T>>();
      HashedArrayList<T> retval = (HashedArrayList<T>)MemberwiseClone();


      retval.underlying = underlying != null ? underlying : this;
      retval.offset = start + offset;
      retval.size = count;
      retval.myWeakReference = views.Add(retval);
      return retval;
    }

    /// <summary>
    /// Create a list view on this list containing the (first) occurrence of a particular item.
    /// <para>Returns <code>null</code> if the item is not in this list.</para>
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The new list view.</returns>
    [Tested]
    public virtual IList<T> ViewOf(T item)
    {
      int index = indexOf(item);
      if (index < 0)
        return null;
      return View(index, 1);
    }


    /// <summary>
    /// Create a list view on this list containing the last occurrence of a particular item. 
    /// <para>Returns <code>null</code> if the item is not in this list.</para>
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The new list view.</returns>
    [Tested]
    public virtual IList<T> LastViewOf(T item)
    {
      int index = lastIndexOf(item);
      if (index < 0)
        return null;
      return View(index, 1);
    }

    /// <summary>
    /// Null if this list is not a view.
    /// </summary>
    /// <value>Underlying list for view.</value>
    [Tested]
    public virtual IList<T> Underlying { [Tested]get { return underlying; } }


    /// <summary>
    /// </summary>
    /// <value>Offset for this list view or 0 for an underlying list.</value>
    [Tested]
    public virtual int Offset { [Tested]get { return offset; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public virtual bool IsValid { get { return isValid; } }

    /// <summary>
    /// Slide this list view along the underlying list.
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <exception cref="ArgumentOutOfRangeException"> if the operation
    /// would bring either end of the view outside the underlying list.</exception>
    /// <param name="offset">The signed amount to slide: positive to slide
    /// towards the end.</param>
    [Tested]
    public virtual IList<T> Slide(int offset)
    {
      if (!TrySlide(offset, size))
        throw new ArgumentOutOfRangeException();
      return this;
    }


    /// <summary>
    /// Slide this list view along the underlying list, changing its size.
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <exception cref="ArgumentOutOfRangeException"> if the operation
    /// would bring either end of the view outside the underlying list.</exception>
    /// <param name="offset">The signed amount to slide: positive to slide
    /// towards the end.</param>
    /// <param name="size">The new size of the view.</param>
    [Tested]
    public virtual IList<T> Slide(int offset, int size)
    {
      if (!TrySlide(offset, size))
        throw new ArgumentOutOfRangeException();
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <param name="offset"></param>
    /// <returns></returns>
    [Tested]
    public virtual bool TrySlide(int offset)
    {
      return TrySlide(offset, size);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [Tested]
    public virtual bool TrySlide(int offset, int size)
    {
      updatecheck();
      if (underlying == null)
        throw new NotAViewException("Not a view");

      int newoffset = this.offset + offset;
      int newsize = size;

      if (newoffset < 0 || newsize < 0 || newoffset + newsize > underlyingsize)
        return false;

      this.offset = newoffset;
      this.size = newsize;
      return true;
    }

    /// <summary>
    /// 
    /// <para>Returns null if <code>otherView</code> is strictly to the left of this view</para>
    /// </summary>
    /// <param name="otherView"></param>
    /// <exception cref="IncompatibleViewException">If otherView does not have the same underlying list as this</exception>
    /// <returns></returns>
    public virtual IList<T> Span(IList<T> otherView)
    {
      if ((otherView == null) || ((otherView.Underlying ?? otherView) != (underlying ?? this)))
        throw new IncompatibleViewException();
      if (otherView.Offset + otherView.Count - Offset < 0)
        return null;
      return (underlying ?? this).View(Offset, otherView.Offset + otherView.Count - Offset);
    }

    /// <summary>
    /// Reverst the list so the items are in the opposite sequence order.
    /// </summary>
    [Tested]
    public virtual void Reverse()
    {
      updatecheck();
      if (size == 0)
        return;
      for (int i = 0, length = size / 2, end = offset + size - 1; i < length; i++)
      {
        T swap = array[offset + i];

        array[offset + i] = array[end - i];
        array[end - i] = swap;
      }
#if HASHINDEX
      reindex(offset, offset + size);
#endif
      //TODO: be more forgiving wrt. disposing
      disposeOverlappingViews(true);
      (underlying ?? this).raiseCollectionChanged();
    }

    /// <summary>
    /// Check if this list is sorted according to the default sorting order
    /// for the item type T, as defined by the <see cref="T:C5.Comparer`1"/> class 
    /// </summary>
    /// <exception cref="NotComparableException">if T is not comparable</exception>
    /// <returns>True if the list is sorted, else false.</returns>
    [Tested]
    public bool IsSorted() { return IsSorted(Comparer<T>.Default); }

    /// <summary>
    /// Check if this list is sorted according to a specific sorting order.
    /// </summary>
    /// <param name="c">The comparer defining the sorting order.</param>
    /// <returns>True if the list is sorted, else false.</returns>
    [Tested]
    public virtual bool IsSorted(SCG.IComparer<T> c)
    {
      validitycheck();
      for (int i = offset + 1, end = offset + size; i < end; i++)
        if (c.Compare(array[i - 1], array[i]) > 0)
          return false;

      return true;
    }

    /// <summary>
    /// Sort the items of the list according to the default sorting order
    /// for the item type T, as defined by the Comparer[T] class 
    /// (<see cref="T:C5.Comparer`1"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">if T is not comparable</exception>
    public virtual void Sort()
    {
      Sort(Comparer<T>.Default);
    }


    /// <summary>
    /// Sort the items of the list according to a specific sorting order.
    /// </summary>
    /// <param name="comparer">The comparer defining the sorting order.</param>
    [Tested]
    public virtual void Sort(SCG.IComparer<T> comparer)
    {
      updatecheck();
      if (size == 0)
        return;
      Sorting.IntroSort<T>(array, offset, size, comparer);
      disposeOverlappingViews(false);
#if HASHINDEX
      reindex(offset, offset + size);
#endif
      (underlying ?? this).raiseCollectionChanged();
    }


    /// <summary>
    /// Randomly shuffle the items of this list. 
    /// </summary>
    public virtual void Shuffle() { Shuffle(new C5Random()); }


    /// <summary>
    /// Shuffle the items of this list according to a specific random source.
    /// </summary>
    /// <param name="rnd">The random source.</param>
    public virtual void Shuffle(Random rnd)
    {
      updatecheck();
      if (size == 0)
        return;
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
      disposeOverlappingViews(false);
#if HASHINDEX
      reindex(offset, offset + size);
#endif
      (underlying ?? this).raiseCollectionChanged();
    }
    #endregion

    #region IIndexed<T> Members

    /// <summary>
    /// Search for an item in the list going forwrds from the start.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of item from start.</returns>
    [Tested]
    public virtual int IndexOf(T item) { validitycheck(); return indexOf(item); }


    /// <summary>
    /// Search for an item in the list going backwords from the end.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of item from the end.</returns>
    [Tested]
    public virtual int LastIndexOf(T item) { validitycheck(); return lastIndexOf(item); }


    /// <summary>
    /// Remove the item at a specific position of the list.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt;= the size of the collection.</exception>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>The removed item.</returns>
    [Tested]
    public virtual T RemoveAt(int index)
    {
      updatecheck();
      if (index < 0 || index >= size)
        throw new IndexOutOfRangeException("Index out of range for sequenced collection");

      T item = removeAt(index);
      (underlying ?? this).raiseForRemoveAt(offset + index, item);
      return item;
    }


    /// <summary>
    /// Remove all items in an index interval.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If <code>start</code>
    /// and <code>count</code> does not describe a valid interval in the list</exception> 
    /// <param name="start">The index of the first item to remove.</param>
    /// <param name="count">The number of items to remove.</param>
    [Tested]
    public virtual void RemoveInterval(int start, int count)
    {
      updatecheck();
      if (count == 0)
        return;
      checkRange(start, count);
      start += offset;
      fixViewsBeforeRemove(start, count);
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>();
      for (int i = start, end = start + count; i < end; i++)
      {
        p.Key = array[i];
        itemIndex.Remove(p);
      }
#endif
      Array.Copy(array, start + count, array, start, underlyingsize - start - count);
      addtosize(-count);
      Array.Clear(array, underlyingsize, count);
#if HASHINDEX
      reindex(start);
#endif
      (underlying ?? this).raiseForRemoveInterval(start, count);
    }
    void raiseForRemoveInterval(int start, int count)
    {
      if (ActiveEvents != 0)
      {
        raiseCollectionCleared(size == 0, count, start);
        raiseCollectionChanged();
      }
    }
    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// The value is symbolic indicating the type of asymptotic complexity
    /// in terms of the size of this collection (worst-case or amortized as
    /// relevant).
    /// </summary>
    /// <value>Speed.Linear</value>
    [Tested]
    public virtual Speed ContainsSpeed
    {
      [Tested]
      get
      {
#if HASHINDEX
        return Speed.Constant;
#else
        return Speed.Linear;
#endif
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Tested]
    public override int GetUnsequencedHashCode()
    { validitycheck(); return base.GetUnsequencedHashCode(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    [Tested]
    public override bool UnsequencedEquals(ICollection<T> that)
    { validitycheck(); return base.UnsequencedEquals(that); }

    /// <summary>
    /// Check if this collection contains (an item equivalent to according to the
    /// itemequalityComparer) a particular value.
    /// </summary>
    /// <param name="item">The value to check for.</param>
    /// <returns>True if the items is in this collection.</returns>
    [Tested]
    public virtual bool Contains(T item)
    { validitycheck(); return indexOf(item) >= 0; }


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, return in the ref argument (a
    /// binary copy of) the actual value found.
    /// </summary>
    /// <param name="item">The value to look for.</param>
    /// <returns>True if the items is in this collection.</returns>
    [Tested]
    public virtual bool Find(ref T item)
    {
      validitycheck();

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
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// to with a binary copy of the supplied value. This will only update the first 
    /// mathching item.
    /// </summary>
    /// <param name="item">Value to update.</param>
    /// <returns>True if the item was found and hence updated.</returns>
    [Tested]
    public virtual bool Update(T item)
    {
      T olditem;
      return Update(item, out olditem);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public virtual bool Update(T item, out T olditem)
    {
      updatecheck();
      int i;

      if ((i = indexOf(item)) >= 0)
      {
        olditem = array[offset + i];
        array[offset + i] = item;
#if HASHINDEX
        itemIndex.Update(new KeyValuePair<T, int>(item, offset + i));
#endif
        (underlying ?? this).raiseForUpdate(item, olditem);
        return true;
      }

      olditem = default(T);
      return false;
    }

    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, return in the ref argument (a
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
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
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
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public virtual bool UpdateOrAdd(T item, out T olditem)
    {
      updatecheck();
      if (Update(item, out olditem))
        return true;

      Add(item);
      olditem = default(T);
      return false;
    }

    /// <summary>
    /// Remove a particular item from this list. The item will be searched 
    /// for from the end of the list if <code>FIFO == false</code> (the default), 
    /// else from the start.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    /// <returns>True if the item was found (and removed).</returns>
    [Tested]
    public virtual bool Remove(T item)
    {
      updatecheck();

      int i = fIFO ? indexOf(item) : lastIndexOf(item);

      if (i < 0)
        return false;

      T removeditem = removeAt(i);
      (underlying ?? this).raiseForRemove(removeditem);
      return true;
    }


    /// <summary>
    /// Remove the first copy of a particular item from this collection if found.
    /// If an item was removed, report a binary copy of the actual item removed in 
    /// the argument. The item will be searched 
    /// for from the end of the list if <code>FIFO == false</code> (the default), 
    /// else from the start.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    /// <param name="removeditem">The removed value.</param>
    /// <returns>True if the item was found (and removed).</returns>
    [Tested]
    public virtual bool Remove(T item, out T removeditem)
    {
      updatecheck();

      int i = fIFO ? indexOf(item) : lastIndexOf(item);

      if (i < 0)
      {
        removeditem = default(T);
        return false;
      }

      removeditem = removeAt(i);
      (underlying ?? this).raiseForRemove(removeditem);
      return true;
    }


    //TODO: remove from end or according to FIFO?
    /// <summary>
    /// Remove all items in another collection from this one, taking multiplicities into account.
    /// Matching items will be removed from the front. Current implementation is not optimal.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to remove.</param>
    [Tested]
    public virtual void RemoveAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      updatecheck();
      if (size == 0)
        return;
      //TODO: reactivate the old code for small sizes
      HashBag<T> toremove = new HashBag<T>(itemequalityComparer);
      toremove.AddAll(items);
      if (toremove.Count == 0)
        return;
      RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler(underlying ?? this);
      bool mustFire = raiseHandler.MustFire;
      ViewHandler viewHandler = new ViewHandler(this);
      int j = offset;
      int removed = 0;
      int i = offset, end = offset + size;
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>();
#endif
      while (i < end)
      {
        T item;
        //pass by a stretch of nodes
        while (i < end && !toremove.Contains(item = array[i]))
        {
#if HASHINDEX
          if (j < i)
          {
            p.Key = item;
            p.Value = j;
            itemIndex.Update(p);
          }
#endif
          //if (j<i)
          array[j] = item;
          i++; j++;
        }
        viewHandler.skipEndpoints(removed, i);
        //Remove a stretch of nodes
        while (i < end && toremove.Remove(item = array[i]))
        {
#if HASHINDEX
          p.Key = item;
          itemIndex.Remove(p);
#endif
          if (mustFire)
            raiseHandler.Remove(item);
          removed++;
          i++;
          viewHandler.updateViewSizesAndCounts(removed, i);
        }
      }
      if (removed == 0)
        return;
      viewHandler.updateViewSizesAndCounts(removed, underlyingsize);
      Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
      addtosize(-removed);
      Array.Clear(array, underlyingsize, removed);
#if HASHINDEX
      reindex(j);
#endif
      if (mustFire)
        raiseHandler.Raise();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    void RemoveAll(Fun<T, bool> predicate)
    {
      updatecheck();
      if (size == 0)
        return;
      RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler(underlying ?? this);
      bool mustFire = raiseHandler.MustFire;
      ViewHandler viewHandler = new ViewHandler(this);
      int j = offset;
      int removed = 0;
      int i = offset, end = offset + size;
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>();
#endif
      while (i < end)
      {
        T item;
        //pass by a stretch of nodes
        while (i < end && !predicate(item = array[i]))
        {
          updatecheck();
#if HASHINDEX
          if (j < i)
          {
            p.Key = item;
            p.Value = j;
            itemIndex.Update(p);
          }
#endif
          //if (j<i)
          array[j] = item;
          i++; j++;
        }
        updatecheck();
        viewHandler.skipEndpoints(removed, i);
        //Remove a stretch of nodes
        while (i < end && predicate(item = array[i]))
        {
          updatecheck();
#if HASHINDEX
          p.Key = item;
          itemIndex.Remove(p);
#endif
          if (mustFire)
            raiseHandler.Remove(item);
          removed++;
          i++;
          viewHandler.updateViewSizesAndCounts(removed, i);
        }
        updatecheck();
      }
      if (removed == 0)
        return;
      viewHandler.updateViewSizesAndCounts(removed, underlyingsize);
      Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
      addtosize(-removed);
      Array.Clear(array, underlyingsize, removed);
#if HASHINDEX
      reindex(j);
#endif
      if (mustFire)
        raiseHandler.Raise();
    }

    /// <summary>
    /// Remove all items from this collection, resetting internal array size.
    /// </summary>
    [Tested]
    public override void Clear()
    {
      if (underlying == null)
      {
        updatecheck();
        if (size == 0)
          return;
        int oldsize = size;
        fixViewsBeforeRemove(0, size);
#if HASHINDEX
        itemIndex.Clear();
#endif
        array = new T[8];
        size = 0;
        (underlying ?? this).raiseForRemoveInterval(offset, oldsize);
      }
      else
        RemoveInterval(0, size);
    }

    /// <summary>
    /// Remove all items not in some other collection from this one, taking multiplicities into account.
    /// Items are retained front first.  
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to retain.</param>
    [Tested]
    public virtual void RetainAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      updatecheck();
      if (size == 0)
        return;
      HashBag<T> toretain = new HashBag<T>(itemequalityComparer);
      toretain.AddAll(items);
      if (toretain.Count == 0)
      {
        Clear();
        return;
      }
      RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler(underlying ?? this);
      bool mustFire = raiseHandler.MustFire;
      ViewHandler viewHandler = new ViewHandler(this);
      int j = offset;
      int removed = 0;
      int i = offset, end = offset + size;
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>();
#endif
      while (i < end)
      {
        T item;
        //pass by a stretch of nodes
        while (i < end && toretain.Remove(item = array[i]))
        {
#if HASHINDEX
          if (j < i)
          {
            p.Key = item;
            p.Value = j;
            itemIndex.Update(p);
          }
#endif
          //if (j<i)
          array[j] = item;
          i++; j++;
        }
        viewHandler.skipEndpoints(removed, i);
        //Remove a stretch of nodes
        while (i < end && !toretain.Contains(item = array[i]))
        {
#if HASHINDEX
          p.Key = item;
          itemIndex.Remove(p);
#endif
          if (mustFire)
            raiseHandler.Remove(item);
          removed++;
          i++;
          viewHandler.updateViewSizesAndCounts(removed, i);
        }
      }
      if (removed == 0)
        return;
      viewHandler.updateViewSizesAndCounts(removed, underlyingsize);
      Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
      addtosize(-removed);
      Array.Clear(array, underlyingsize, removed);
#if HASHINDEX
      reindex(j);
#endif
      raiseHandler.Raise();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    void RetainAll(Fun<T, bool> predicate)
    {
      updatecheck();
      if (size == 0)
        return;
      RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler(underlying ?? this);
      bool mustFire = raiseHandler.MustFire;
      ViewHandler viewHandler = new ViewHandler(this);
      int j = offset;
      int removed = 0;
      int i = offset, end = offset + size;
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>();
#endif
      while (i < end)
      {
        T item;
        //pass by a stretch of nodes
        while (i < end && predicate(item = array[i]))
        {
          updatecheck();
#if HASHINDEX
          if (j < i)
          {
            p.Key = item;
            p.Value = j;
            itemIndex.Update(p);
          }
#endif
          //if (j<i)
          array[j] = item;
          i++; j++;
        }
        updatecheck();
        viewHandler.skipEndpoints(removed, i);
        //Remove a stretch of nodes
        while (i < end && !predicate(item = array[i]))
        {
          updatecheck();
#if HASHINDEX
          p.Key = item;
          itemIndex.Remove(p);
#endif
          if (mustFire)
            raiseHandler.Remove(item);
          removed++;
          i++;
          viewHandler.updateViewSizesAndCounts(removed, i);
        }
        updatecheck();
      }
      if (removed == 0)
        return;
      viewHandler.updateViewSizesAndCounts(removed, underlyingsize);
      Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
      addtosize(-removed);
      Array.Clear(array, underlyingsize, removed);
#if HASHINDEX
      reindex(j);
#endif
      raiseHandler.Raise();
    }


    /// <summary>
    /// Check if this collection contains all the values in another collection,
    /// taking multiplicities into account.
    /// Current implementation is not optimal.
    /// </summary>
    /// <param name="items">The </param>
    /// <typeparam name="U"></typeparam>
    /// <returns>True if all values in <code>items</code>is in this collection.</returns>
    [Tested]
    public virtual bool ContainsAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      validitycheck();
#if HASHINDEX
      foreach (T item in items)
        if (indexOf(item) < 0)
          return false;

      return true;
#else
      //TODO: use aux hash bag to obtain linear time procedure
      HashBag<T> tomatch = new HashBag<T>(itemequalityComparer);
      tomatch.AddAll(items);
      if (tomatch.Count == 0)
        return true;
      for (int i = offset, end = offset + size; i < end; i++)
      {
        tomatch.Remove(array[i]);
        if (tomatch.Count == 0)
          return true;
      }
      return false;
#endif
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
      validitycheck();
#if HASHINDEX
      return indexOf(item) >= 0 ? 1 : 0;
#else
      int count = 0;
      for (int i = 0; i < size; i++)
        if (equals(item, array[offset + i]))
          count++;
      return count;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual ICollectionValue<T> UniqueItems()
    {
#if HASHINDEX
      return this;
#else
      HashBag<T> hashbag = new HashBag<T>(itemequalityComparer);
      hashbag.AddAll(this);
      return hashbag.UniqueItems();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual ICollectionValue<KeyValuePair<T, int>> ItemMultiplicities()
    {
#if HASHINDEX
      return new MultiplicityOne<T>(this);
#else
      HashBag<T> hashbag = new HashBag<T>(itemequalityComparer);
      hashbag.AddAll(this);
      return hashbag.ItemMultiplicities();
#endif
    }





    /// <summary>
    /// Remove all items equal to a given one.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    [Tested]
    public virtual void RemoveAllCopies(T item)
    {
#if HASHINDEX
      Remove(item);
#else
      updatecheck();
      if (size == 0)
        return;
      RaiseForRemoveAllHandler raiseHandler = new RaiseForRemoveAllHandler(underlying ?? this);
      bool mustFire = raiseHandler.MustFire;
      ViewHandler viewHandler = new ViewHandler(this);
      int j = offset;
      int removed = 0;
      int i = offset, end = offset + size;
      while (i < end)
      {
        //pass by a stretch of nodes
        while (i < end && !equals(item, array[i]))
          array[j++] = array[i++];
        viewHandler.skipEndpoints(removed, i);
        //Remove a stretch of nodes
        while (i < end && equals(item, array[i]))
        {
          if (mustFire)
            raiseHandler.Remove(array[i]);
          removed++;
          i++;
          viewHandler.updateViewSizesAndCounts(removed, i);
        }
      }
      if (removed == 0)
        return;
      viewHandler.updateViewSizesAndCounts(removed, underlyingsize);
      Array.Copy(array, offset + size, array, j, underlyingsize - offset - size);
      addtosize(-removed);
      Array.Clear(array, underlyingsize, removed);
      raiseHandler.Raise();
#endif
    }


    //TODO: check views
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

      {
        HashedArrayList<T> u = underlying ?? this;
        if (u.views != null)
          foreach (HashedArrayList<T> v in u.views)
          {
            if (u.array != v.array)
            {
              Console.WriteLine("View from {0} of length has different base array than the underlying list", v.offset, v.size);
              retval = false;
            }
          }
      }


#if HASHINDEX
      if (underlyingsize != itemIndex.Count)
      {
        Console.WriteLine("size ({0})!= index.Count ({1})", size, itemIndex.Count);
        retval = false;
      }

      for (int i = 0; i < underlyingsize; i++)
      {
        KeyValuePair<T, int> p = new KeyValuePair<T, int>(array[i]);

        if (!itemIndex.Find(ref p))
        {
          Console.WriteLine("Item {1} at {0} not in hashindex", i, array[i]);
          retval = false;
        }

        if (p.Value != i)
        {
          Console.WriteLine("Item {1} at {0} has hashindex {2}", i, array[i], p.Value);
          retval = false;
        }
      }
#endif
      return retval;
    }

    #endregion

    #region IExtensible<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <value>True, indicating array list has bag semantics.</value>
    [Tested]
    public virtual bool AllowsDuplicates
    {
      [Tested]
      get
      {
#if HASHINDEX
        return false;
#else
        return true;
#endif
      }
    }

    /// <summary>
    /// By convention this is true for any collection with set semantics.
    /// </summary>
    /// <value>True if only one representative of a group of equal items 
    /// is kept in the collection together with the total count.</value>
    public virtual bool DuplicatesByCounting
    {
      get
      {
#if HASHINDEX
        return true;
#else
        return false;
#endif
      }
    }

    /// <summary>
    /// Add an item to end of this list.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True</returns>
    [Tested]
    public virtual bool Add(T item)
    {
      updatecheck();
#if HASHINDEX
      KeyValuePair<T, int> p = new KeyValuePair<T, int>(item, size + offset);
      if (itemIndex.FindOrAdd(ref p))
        return false;
#endif
      baseinsert(size, item);
#if HASHINDEX
      reindex(size + offset);
#endif
      (underlying ?? this).raiseForAdd(item);
      return true;
    }


    /// <summary>
    /// Add the elements from another collection to this collection.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items"></param>
    [Tested]
    public virtual void AddAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      updatecheck();
      int toadd = EnumerableBase<U>.countItems(items);
      if (toadd == 0)
        return;

      if (toadd + underlyingsize > array.Length)
        expand(toadd + underlyingsize, underlyingsize);

      int i = size + offset;
      if (underlyingsize > i)
        Array.Copy(array, i, array, i + toadd, underlyingsize - i);
      try
      {
        foreach (T item in items)
        {
#if HASHINDEX
          KeyValuePair<T, int> p = new KeyValuePair<T, int>(item, i);
          if (itemIndex.FindOrAdd(ref p))
            continue;
#endif
          array[i++] = item;
        }
      }
      finally
      {
        int added = i - size - offset;
        if (added < toadd)
        {
          Array.Copy(array, size + offset + toadd, array, i, underlyingsize - size - offset);
          Array.Clear(array, underlyingsize + added, toadd - added);
        }
        if (added > 0)
        {
          addtosize(added);
#if HASHINDEX
          reindex(i);
#endif
          fixViewsAfterInsert(added, i - added);
          (underlying ?? this).raiseForAddAll(i - added, added);
        }
      }
    }
    private void raiseForAddAll(int start, int added)
    {
      if ((ActiveEvents & EventTypeEnum.Added) != 0)
        for (int i = start, end = start + added; i < end; i++)
          raiseItemsAdded(array[i], 1);
      raiseCollectionChanged();
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

    #region ICollectionValue<T> Members
    /// <summary>
    /// 
    /// </summary>
    /// <value>The number of items in this collection</value>
    [Tested]
    public override int Count { [Tested]get { validitycheck(); return size; } }
    #endregion

    #region IEnumerable<T> Members
    //TODO: make tests of all calls on a disposed view throws the right exception! (Which should be C5.InvalidListViewException)
    /// <summary>
    /// Create an enumerator for the collection
    /// </summary>
    /// <returns>The enumerator</returns>
    [Tested]
    public override SCG.IEnumerator<T> GetEnumerator()
    {
      validitycheck();
      return base.GetEnumerator();
    }
    #endregion

#if HASHINDEX
#else
    #region IStack<T> Members

    /// <summary>
    /// Push an item to the top of the stack.
    /// </summary>
    /// <param name="item">The item</param>
    [Tested]
    public virtual void Push(T item)
    {
      InsertLast(item);
    }

    /// <summary>
    /// Pop the item at the top of the stack from the stack.
    /// </summary>
    /// <returns>The popped item.</returns>
    [Tested]
    public virtual T Pop()
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
    public virtual void Enqueue(T item)
    {
      InsertLast(item);
    }

    /// <summary>
    /// Dequeue an item from the front of the queue.
    /// </summary>
    /// <returns>The item</returns>
    [Tested]
    public virtual T Dequeue()
    {
      return RemoveFirst();
    }

    #endregion
#endif
    #region IDisposable Members

    /// <summary>
    /// Invalidate this list. If a view, just invalidate the view. 
    /// If not a view, invalidate the list and all views on it.
    /// </summary>
    public virtual void Dispose()
    {
      Dispose(false);
    }

    void Dispose(bool disposingUnderlying)
    {
      if (isValid)
      {
        if (underlying != null)
        {
          isValid = false;
          if (!disposingUnderlying && views != null)
            views.Remove(myWeakReference);
          underlying = null;
          views = null;
          myWeakReference = null;
        }
        else
        {
          //isValid = false;
          if (views != null)
            foreach (HashedArrayList<T> view in views)
              view.Dispose(true);
          Clear();
        }
      }
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Make a shallow copy of this HashedArrayList.
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      HashedArrayList<T> clone = new HashedArrayList<T>(size, itemequalityComparer);
      clone.AddAll(this);
      return clone;
    }

    #endregion

    #region ISerializable Members
    /*
    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public HashedArrayList(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) :
      this(info.GetInt32("sz"),(SCG.IEqualityComparer<T>)(info.GetValue("eq",typeof(SCG.IEqualityComparer<T>))))
    {
      size = info.GetInt32("sz");
      for (int i = 0; i < size; i++)
      {
        array[i] = (T)(info.GetValue("elem" + i,typeof(T)));
      }
#if HASHINDEX
      reindex(0);
#endif      
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
      info.AddValue("sz", size);
      info.AddValue("eq", EqualityComparer);
      for (int i = 0; i < size; i++)
			{
        info.AddValue("elem" + i, array[i + offset]);
      }
    }
*/
    #endregion

    #region System.Collections.Generic.IList<T> Members

    void System.Collections.Generic.IList<T>.RemoveAt(int index)
    {
      RemoveAt(index);
    }

    void System.Collections.Generic.ICollection<T>.Add(T item)
    {
      Add(item);
    }

    #endregion

    #region System.Collections.ICollection Members

    bool System.Collections.ICollection.IsSynchronized
    {
      get { return false; }
    }

    [Obsolete]
    Object System.Collections.ICollection.SyncRoot
    {
      get { return underlying != null ? ((System.Collections.ICollection)underlying).SyncRoot : array; }
    }

    void System.Collections.ICollection.CopyTo(Array arr, int index)
    {
      if (index < 0 || index + Count > arr.Length)
        throw new ArgumentOutOfRangeException();

      foreach (T item in this)
        arr.SetValue(item, index++);
    }

    #endregion

    #region System.Collections.IList Members

    Object System.Collections.IList.this[int index]
    {
      get { return this[index]; }
      set { this[index] = (T)value; }
    }

    int System.Collections.IList.Add(Object o)
    {
      bool added = Add((T)o);
      // What position to report if item not added? SC.IList.Add doesn't say
      return added ? Count - 1 : -1;
    }

    bool System.Collections.IList.Contains(Object o)
    {
      return Contains((T)o);
    }

    int System.Collections.IList.IndexOf(Object o)
    {
      return Math.Max(-1, IndexOf((T)o));
    }

    void System.Collections.IList.Insert(int index, Object o)
    {
      Insert(index, (T)o);
    }

    void System.Collections.IList.Remove(Object o)
    {
      Remove((T)o);
    }

    void System.Collections.IList.RemoveAt(int index)
    {
      RemoveAt(index);
    }

    #endregion
  }
}
