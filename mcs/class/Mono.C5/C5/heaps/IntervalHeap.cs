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

using System;
using System.Diagnostics;
using SCG = System.Collections.Generic;

namespace C5
{
  /// <summary>
  /// A priority queue class based on an interval heap data structure.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  [Serializable]
  public class IntervalHeap<T> : CollectionValueBase<T>, IPriorityQueue<T>
  {
    #region Events

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public override EventTypeEnum ListenableEvents { get { return EventTypeEnum.Basic; } }

    #endregion

    #region Fields
    [Serializable]
    struct Interval
    {
      internal T first, last; internal Handle firsthandle, lasthandle;


      public override string ToString() { return String.Format("[{0}; {1}]", first, last); }
    }

    int stamp;

    SCG.IComparer<T> comparer;
    SCG.IEqualityComparer<T> itemequalityComparer;

    Interval[] heap;

    int size;
    #endregion

    #region Util
    bool heapifyMin(int i)
    {
      bool swappedroot = false;
      int cell = i, currentmin = cell;
      T currentitem = heap[cell].first;
      Handle currenthandle = heap[cell].firsthandle;

      // bug20080222.txt
      {
        T other = heap[cell].last;
        if (2 * cell + 1 < size && comparer.Compare(currentitem, other) > 0)
        {
          swappedroot = true;
          Handle otherhandle = heap[cell].lasthandle;
          updateLast(cell, currentitem, currenthandle);
          currentitem = other;
          currenthandle = otherhandle;
        }
      }

      T minitem = currentitem;
      Handle minhandle = currenthandle;

      while (true)
      {
        int l = 2 * cell + 1, r = l + 1;
        T lv, rv;

        if (2 * l < size && comparer.Compare(lv = heap[l].first, minitem) < 0)
        { currentmin = l; minitem = lv; }

        if (2 * r < size && comparer.Compare(rv = heap[r].first, minitem) < 0)
        { currentmin = r; minitem = rv; }

        if (currentmin == cell)
          break;

        minhandle = heap[currentmin].firsthandle;
        updateFirst(cell, minitem, minhandle);
        cell = currentmin;

        //Maybe swap first and last
        T other = heap[cell].last;
        if (2 * currentmin + 1 < size && comparer.Compare(currentitem, other) > 0)
        {
          Handle otherhandle = heap[cell].lasthandle;
          updateLast(cell, currentitem, currenthandle);
          currentitem = other;
          currenthandle = otherhandle;
        }


        minitem = currentitem;
        minhandle = currenthandle;
      }

      if (cell != i || swappedroot)
        updateFirst(cell, minitem, minhandle);
      return swappedroot;
    }


    bool heapifyMax(int i)
    {
      bool swappedroot = false;
      int cell = i, currentmax = cell;
      T currentitem = heap[cell].last;
      Handle currenthandle = heap[cell].lasthandle;

      // bug20080222.txt
      {
        T other = heap[cell].first;
        if (2 * cell + 1 < size && comparer.Compare(currentitem, other) < 0)
        {
          swappedroot = true;
          Handle otherhandle = heap[cell].firsthandle;
          updateFirst(cell, currentitem, currenthandle);
          currentitem = other;
          currenthandle = otherhandle;
        }
      }

      T maxitem = currentitem;
      Handle maxhandle = currenthandle;

      while (true)
      {
        int l = 2 * cell + 1, r = l + 1;
        T lv, rv;

        if (2 * l + 1 < size && comparer.Compare(lv = heap[l].last, maxitem) > 0)
        { currentmax = l; maxitem = lv; }

        if (2 * r + 1 < size && comparer.Compare(rv = heap[r].last, maxitem) > 0)
        { currentmax = r; maxitem = rv; }

        if (currentmax == cell)
          break;

        maxhandle = heap[currentmax].lasthandle;
        updateLast(cell, maxitem, maxhandle);
        cell = currentmax;

        //Maybe swap first and last
        T other = heap[cell].first;
        if (comparer.Compare(currentitem, other) < 0)
        {
          Handle otherhandle = heap[cell].firsthandle;
          updateFirst(cell, currentitem, currenthandle);
          currentitem = other;
          currenthandle = otherhandle;
        }

        maxitem = currentitem;
        maxhandle = currenthandle;
      }

      if (cell != i || swappedroot) //Check could be better?
        updateLast(cell, maxitem, maxhandle);
      return swappedroot;
    }


    void bubbleUpMin(int i)
    {
      if (i > 0)
      {
        T min = heap[i].first, iv = min;
        Handle minhandle = heap[i].firsthandle;
        int p = (i + 1) / 2 - 1;

        while (i > 0)
        {
          if (comparer.Compare(iv, min = heap[p = (i + 1) / 2 - 1].first) < 0)
          {
            updateFirst(i, min, heap[p].firsthandle);
            min = iv;
            i = p;
          }
          else
            break;
        }

        updateFirst(i, iv, minhandle);
      }
    }


    void bubbleUpMax(int i)
    {
      if (i > 0)
      {
        T max = heap[i].last, iv = max;
        Handle maxhandle = heap[i].lasthandle;
        int p = (i + 1) / 2 - 1;

        while (i > 0)
        {
          if (comparer.Compare(iv, max = heap[p = (i + 1) / 2 - 1].last) > 0)
          {
            updateLast(i, max, heap[p].lasthandle);
            max = iv;
            i = p;
          }
          else
            break;
        }

        updateLast(i, iv, maxhandle);

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
    /// <param name="comparer">The external comparer</param>
    public IntervalHeap(SCG.IComparer<T> comparer) : this(16, comparer) { }


    //TODO: maybe remove
    /// <summary>
    /// Create an interval heap with natural item comparer and prescribed initial capacity
    /// </summary>
    /// <param name="capacity">The initial capacity</param>
    public IntervalHeap(int capacity) : this(capacity, Comparer<T>.Default, EqualityComparer<T>.Default) { }


    /// <summary>
    /// Create an interval heap with external item comparer and prescribed initial capacity
    /// </summary>
    /// <param name="comparer">The external comparer</param>
    /// <param name="capacity">The initial capacity</param>
    public IntervalHeap(int capacity, SCG.IComparer<T> comparer) : this(capacity, comparer, new ComparerZeroHashCodeEqualityComparer<T>(comparer)) { }

    IntervalHeap(int capacity, SCG.IComparer<T> comparer, SCG.IEqualityComparer<T> itemequalityComparer)
    {
      if (comparer == null)
        throw new NullReferenceException("Item comparer cannot be null");
      if (itemequalityComparer == null)
        throw new NullReferenceException("Item equality comparer cannot be null");
      this.comparer = comparer;
      this.itemequalityComparer = itemequalityComparer;
      int length = 1;
      while (length < capacity) length <<= 1;
      heap = new Interval[length];
    }

    #endregion

    #region IPriorityQueue<T> Members

    /// <summary>
    /// Find the current least item of this priority queue.
    /// <exception cref="NoSuchItemException"/> if queue is empty
    /// </summary>
    /// <returns>The least item.</returns>
    [Tested]
    public T FindMin()
    {
      if (size == 0)
        throw new NoSuchItemException();

      return heap[0].first;
    }


    /// <summary>
    /// Remove the least item from this  priority queue.
    /// <exception cref="NoSuchItemException"/> if queue is empty
    /// </summary>
    /// <returns>The removed item.</returns>
    [Tested]
    public T DeleteMin()
    {
      IPriorityQueueHandle<T> handle = null;
      return DeleteMin(out handle);
    }


    /// <summary>
    /// Find the current largest item of this priority queue.
    /// <exception cref="NoSuchItemException"/> if queue is empty
    /// </summary>
    /// <returns>The largest item.</returns>
    [Tested]
    public T FindMax()
    {
      if (size == 0)
        throw new NoSuchItemException("Heap is empty");
      else if (size == 1)
        return heap[0].first;
      else
        return heap[0].last;
    }


    /// <summary>
    /// Remove the largest item from this  priority queue.
    /// <exception cref="NoSuchItemException"/> if queue is empty
    /// </summary>
    /// <returns>The removed item.</returns>
    [Tested]
    public T DeleteMax()
    {
      IPriorityQueueHandle<T> handle = null;
      return DeleteMax(out handle);
    }


    /// <summary>
    /// The comparer object supplied at creation time for this collection
    /// </summary>
    /// <value>The comparer</value>
    public SCG.IComparer<T> Comparer { get { return comparer; } }

    #endregion

    #region IExtensible<T> Members

    /// <summary>
    /// If true any call of an updating operation will throw an
    /// <code>ReadOnlyCollectionException</code>
    /// </summary>
    /// <value>True if this collection is read-only.</value>
    public bool IsReadOnly { get { return false; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value>True since this collection has bag semantics</value>
    [Tested]
    public bool AllowsDuplicates { [Tested]get { return true; } }

    /// <summary>
    /// Value is null since this collection has no equality concept for its items. 
    /// </summary>
    /// <value></value>
    public virtual SCG.IEqualityComparer<T> EqualityComparer { get { return itemequalityComparer; } }

    /// <summary>
    /// By convention this is true for any collection with set semantics.
    /// </summary>
    /// <value>True if only one representative of a group of equal items 
    /// is kept in the collection together with the total count.</value>
    public virtual bool DuplicatesByCounting { get { return false; } }



    /// <summary>
    /// Add an item to this priority queue.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True</returns>
    [Tested]
    public bool Add(T item)
    {
      stamp++;
      if (add(null, item))
      {
        raiseItemsAdded(item, 1);
        raiseCollectionChanged();
        return true;
      }
      return false;
    }

    private bool add(Handle itemhandle, T item)
    {
      if (size == 0)
      {
        size = 1;
        updateFirst(0, item, itemhandle);
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
        T tmp = heap[p].last;

        if (comparer.Compare(item, tmp) > 0)
        {
          updateFirst(i, tmp, heap[p].lasthandle);
          updateLast(p, item, itemhandle);
          bubbleUpMax(p);
        }
        else
        {
          updateFirst(i, item, itemhandle);

          if (comparer.Compare(item, heap[p].first) < 0)
            bubbleUpMin(i);
        }
      }
      else
      {
        int i = size / 2;
        T other = heap[i].first;

        if (comparer.Compare(item, other) < 0)
        {
          updateLast(i, other, heap[i].firsthandle);
          updateFirst(i, item, itemhandle);
          bubbleUpMin(i);
        }
        else
        {
          updateLast(i, item, itemhandle);
          bubbleUpMax(i);
        }
      }
      size++;

      return true;
    }

    private void updateLast(int cell, T item, Handle handle)
    {
      heap[cell].last = item;
      if (handle != null)
        handle.index = 2 * cell + 1;
      heap[cell].lasthandle = handle;
    }

    private void updateFirst(int cell, T item, Handle handle)
    {
      heap[cell].first = item;
      if (handle != null)
        handle.index = 2 * cell;
      heap[cell].firsthandle = handle;
    }


    /// <summary>
    /// Add the elements from another collection with a more specialized item type 
    /// to this collection. 
    /// </summary>
    /// <typeparam name="U">The type of items to add</typeparam>
    /// <param name="items">The items to add</param>
    [Tested]
    public void AddAll<U>(SCG.IEnumerable<U> items) where U : T
    {
      stamp++;
      int oldsize = size;
      foreach (T item in items)
        add(null, item);
      if (size != oldsize)
      {
        if ((ActiveEvents & EventTypeEnum.Added) != 0)
          foreach (T item in items)
            raiseItemsAdded(item, 1);
        raiseCollectionChanged();
      }
    }

    #endregion

    #region ICollection<T> members

    /// <summary>
    /// 
    /// </summary>
    /// <value>True if this collection is empty.</value>
    [Tested]
    public override bool IsEmpty { [Tested]get { return size == 0; } }

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
    /// Choose some item of this collection. 
    /// </summary>
    /// <exception cref="NoSuchItemException">if collection is empty.</exception>
    /// <returns></returns>
    public override T Choose()
    {
      if (size == 0)
        throw new NoSuchItemException("Collection is empty");
      return heap[0].first;
    }


    /// <summary>
    /// Create an enumerator for the collection
    /// <para>Note: the enumerator does *not* enumerate the items in sorted order, 
    /// but in the internal table order.</para>
    /// </summary>
    /// <returns>The enumerator(SIC)</returns>
    [Tested]
    public override SCG.IEnumerator<T> GetEnumerator()
    {
      int mystamp = stamp;
      for (int i = 0; i < size; i++)
      {
        if (mystamp != stamp) throw new CollectionModifiedException();
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
        if (interval.firsthandle != null && interval.firsthandle.index != 2 * i)
        {
          Console.WriteLine("Cell {0}: firsthandle.index({1}) != 2*cell({2})  [size={3}]", i, interval.firsthandle.index, 2 * i, size);
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
        if (interval.firsthandle != null && interval.firsthandle.index != 2 * i)
        {
          Console.WriteLine("Cell {0}: firsthandle.index({1}) != 2*cell({2})  [size={3}]", i, interval.firsthandle.index, 2 * i, size);
          retval = false;
        }
        if (interval.lasthandle != null && interval.lasthandle.index != 2 * i + 1)
        {
          Console.WriteLine("Cell {0}: lasthandle.index({1}) != 2*cell+1({2})  [size={3}]", i, interval.lasthandle.index, 2 * i + 1, size);
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

    #region IPriorityQueue<T> Members

    [Serializable]
    class Handle : IPriorityQueueHandle<T>
    {
      /// <summary>
      /// To save space, the index is 2*cell for heap[cell].first, and 2*cell+1 for heap[cell].last
      /// </summary>
      internal int index = -1;

      public override string ToString()
      {
        return String.Format("[{0}]", index);
      }

    }

    /// <summary>
    /// Get or set the item corresponding to a handle. 
    /// </summary>
    /// <exception cref="InvalidPriorityQueueHandleException">if the handle is invalid for this queue</exception>
    /// <param name="handle">The reference into the heap</param>
    /// <returns></returns>
    [Tested]
    public T this[IPriorityQueueHandle<T> handle]
    {
      get
      {
        int cell;
        bool isfirst;
        checkHandle(handle, out cell, out isfirst);

        return isfirst ? heap[cell].first : heap[cell].last;
      }
      set
      {
        Replace(handle, value);
      }
    }


    /// <summary>
    /// Check safely if a handle is valid for this queue and if so, report the corresponding queue item.
    /// </summary>
    /// <param name="handle">The handle to check</param>
    /// <param name="item">If the handle is valid this will contain the corresponding item on output.</param>
    /// <returns>True if the handle is valid.</returns>
    public bool Find(IPriorityQueueHandle<T> handle, out T item)
    {
      Handle myhandle = handle as Handle;
      if (myhandle == null)
      {
        item = default(T);
        return false;
      }
      int toremove = myhandle.index;
      int cell = toremove / 2;
      bool isfirst = toremove % 2 == 0;
      {
        if (toremove == -1 || toremove >= size)
        {
          item = default(T);
          return false;
        }
        Handle actualhandle = isfirst ? heap[cell].firsthandle : heap[cell].lasthandle;
        if (actualhandle != myhandle)
        {
          item = default(T);
          return false;
        }
      }
      item = isfirst ? heap[cell].first : heap[cell].last;
      return true;
    }


    /// <summary>
    /// Add an item to the priority queue, receiving a 
    /// handle for the item in the queue, 
    /// or reusing an already existing handle.
    /// </summary>
    /// <param name="handle">On output: a handle for the added item. 
    /// On input: null for allocating a new handle, an invalid handle for reuse. 
    /// A handle for reuse must be compatible with this priority queue, 
    /// by being created by a priority queue of the same runtime type, but not 
    /// necessarily the same priority queue object.</param>
    /// <param name="item">The item to add.</param>
    /// <returns>True since item will always be added unless the call throws an exception.</returns>
    [Tested]
    public bool Add(ref IPriorityQueueHandle<T> handle, T item)
    {
      stamp++;
      Handle myhandle = (Handle)handle;
      if (myhandle == null)
        handle = myhandle = new Handle();
      else
        if (myhandle.index != -1)
          throw new InvalidPriorityQueueHandleException("Handle not valid for reuse");
      if (add(myhandle, item))
      {
        raiseItemsAdded(item, 1);
        raiseCollectionChanged();
        return true;
      }
      return false;
    }

    /// <summary>
    /// Delete an item with a handle from a priority queue.
    /// </summary>
    /// <exception cref="InvalidPriorityQueueHandleException">if the handle is invalid</exception>
    /// <param name="handle">The handle for the item. The handle will be invalidated, but reusable.</param>
    /// <returns>The deleted item</returns>
    [Tested]
    public T Delete(IPriorityQueueHandle<T> handle)
    {
      stamp++;
      int cell;
      bool isfirst;
      Handle myhandle = checkHandle(handle, out cell, out isfirst);

      T retval;
      myhandle.index = -1;
      int lastcell = (size - 1) / 2;

      if (cell == lastcell)
      {
        if (isfirst)
        {
          retval = heap[cell].first;
          if (size % 2 == 0)
          {
            updateFirst(cell, heap[cell].last, heap[cell].lasthandle);
            heap[cell].last = default(T);
            heap[cell].lasthandle = null;
          }
          else
          {
            heap[cell].first = default(T);
            heap[cell].firsthandle = null;
          }
        }
        else
        {
          retval = heap[cell].last;
          heap[cell].last = default(T);
          heap[cell].lasthandle = null;
        }
        size--;
      }
      else if (isfirst)
      {
        retval = heap[cell].first;

        if (size % 2 == 0)
        {
          updateFirst(cell, heap[lastcell].last, heap[lastcell].lasthandle);
          heap[lastcell].last = default(T);
          heap[lastcell].lasthandle = null;
        }
        else
        {
          updateFirst(cell, heap[lastcell].first, heap[lastcell].firsthandle);
          heap[lastcell].first = default(T);
          heap[lastcell].firsthandle = null;
        }

        size--;
        if (heapifyMin(cell))
          bubbleUpMax(cell);
        else
          bubbleUpMin(cell);
      }
      else
      {
        retval = heap[cell].last;

        if (size % 2 == 0)
        {
          updateLast(cell, heap[lastcell].last, heap[lastcell].lasthandle);
          heap[lastcell].last = default(T);
          heap[lastcell].lasthandle = null;
        }
        else
        {
          updateLast(cell, heap[lastcell].first, heap[lastcell].firsthandle);
          heap[lastcell].first = default(T);
          heap[lastcell].firsthandle = null;
        }

        size--;
        if (heapifyMax(cell))
          bubbleUpMin(cell);
        else
          bubbleUpMax(cell);
      }

      raiseItemsRemoved(retval, 1);
      raiseCollectionChanged();

      return retval;
    }

    private Handle checkHandle(IPriorityQueueHandle<T> handle, out int cell, out bool isfirst)
    {
      Handle myhandle = (Handle)handle;
      int toremove = myhandle.index;
      cell = toremove / 2;
      isfirst = toremove % 2 == 0;
      {
        if (toremove == -1 || toremove >= size)
          throw new InvalidPriorityQueueHandleException("Invalid handle, index out of range");
        Handle actualhandle = isfirst ? heap[cell].firsthandle : heap[cell].lasthandle;
        if (actualhandle != myhandle)
          throw new InvalidPriorityQueueHandleException("Invalid handle, doesn't match queue");
      }
      return myhandle;
    }


    /// <summary>
    /// Replace an item with a handle in a priority queue with a new item. 
    /// Typically used for changing the priority of some queued object.
    /// </summary>
    /// <param name="handle">The handle for the old item</param>
    /// <param name="item">The new item</param>
    /// <returns>The old item</returns>
    [Tested]
    public T Replace(IPriorityQueueHandle<T> handle, T item)
    {
      stamp++;
      int cell;
      bool isfirst;
      checkHandle(handle, out cell, out isfirst);
      if (size == 0)
        throw new NoSuchItemException();

      T retval;

      if (isfirst)
      {
        retval = heap[cell].first;
        heap[cell].first = item;
        if (size == 1)
        {
        }
        else if (size == 2 * cell + 1) // cell == lastcell
        {
          int p = (cell + 1) / 2 - 1;
          if (comparer.Compare(item, heap[p].last) > 0)
          {
            Handle thehandle = heap[cell].firsthandle;
            updateFirst(cell, heap[p].last, heap[p].lasthandle);
            updateLast(p, item, thehandle);
            bubbleUpMax(p);
          }
          else
            bubbleUpMin(cell);
        }
        else if (heapifyMin(cell))
          bubbleUpMax(cell);
        else
          bubbleUpMin(cell);
      }
      else
      {
        retval = heap[cell].last;
        heap[cell].last = item;
        if (heapifyMax(cell))
          bubbleUpMin(cell);
        else
          bubbleUpMax(cell);
      }

      raiseItemsRemoved(retval, 1);
      raiseItemsAdded(item, 1);
      raiseCollectionChanged();

      return retval;
    }

    /// <summary>
    /// Find the current least item of this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the item.</param>
    /// <returns>The least item.</returns>
    public T FindMin(out IPriorityQueueHandle<T> handle)
    {
      if (size == 0)
        throw new NoSuchItemException();
      handle = heap[0].firsthandle;

      return heap[0].first;
    }

    /// <summary>
    /// Find the current largest item of this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the item.</param>
    /// <returns>The largest item.</returns>
    public T FindMax(out IPriorityQueueHandle<T> handle)
    {
      if (size == 0)
        throw new NoSuchItemException();
      else if (size == 1)
      {
        handle = heap[0].firsthandle;
        return heap[0].first;
      }
      else
      {
        handle = heap[0].lasthandle;
        return heap[0].last;
      }
    }

    /// <summary>
    /// Remove the least item from this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the removed item.</param>
    /// <returns>The removed item.</returns>
    public T DeleteMin(out IPriorityQueueHandle<T> handle)
    {
      stamp++;
      if (size == 0)
        throw new NoSuchItemException();

      T retval = heap[0].first;
      Handle myhandle = heap[0].firsthandle;
      handle = myhandle;
      if (myhandle != null)
        myhandle.index = -1;

      if (size == 1)
      {
        size = 0;
        heap[0].first = default(T);
        heap[0].firsthandle = null;
      }
      else
      {
        int lastcell = (size - 1) / 2;

        if (size % 2 == 0)
        {
          updateFirst(0, heap[lastcell].last, heap[lastcell].lasthandle);
          heap[lastcell].last = default(T);
          heap[lastcell].lasthandle = null;
        }
        else
        {
          updateFirst(0, heap[lastcell].first, heap[lastcell].firsthandle);
          heap[lastcell].first = default(T);
          heap[lastcell].firsthandle = null;
        }

        size--;
        heapifyMin(0);
      }

      raiseItemsRemoved(retval, 1);
      raiseCollectionChanged();
      return retval;

    }

    /// <summary>
    /// Remove the largest item from this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the removed item.</param>
    /// <returns>The removed item.</returns>
    public T DeleteMax(out IPriorityQueueHandle<T> handle)
    {
      stamp++;
      if (size == 0)
        throw new NoSuchItemException();

      T retval;
      Handle myhandle;

      if (size == 1)
      {
        size = 0;
        retval = heap[0].first;
        myhandle = heap[0].firsthandle;
        if (myhandle != null)
          myhandle.index = -1;
        heap[0].first = default(T);
        heap[0].firsthandle = null;
      }
      else
      {
        retval = heap[0].last;
        myhandle = heap[0].lasthandle;
        if (myhandle != null)
          myhandle.index = -1;

        int lastcell = (size - 1) / 2;

        if (size % 2 == 0)
        {
          updateLast(0, heap[lastcell].last, heap[lastcell].lasthandle);
          heap[lastcell].last = default(T);
          heap[lastcell].lasthandle = null;
        }
        else
        {
          updateLast(0, heap[lastcell].first, heap[lastcell].firsthandle);
          heap[lastcell].first = default(T);
          heap[lastcell].firsthandle = null;
        }

        size--;
        heapifyMax(0);
      }
      raiseItemsRemoved(retval, 1);
      raiseCollectionChanged();
      handle = myhandle;
      return retval;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Make a shallow copy of this IntervalHeap.
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      IntervalHeap<T> clone = new IntervalHeap<T>(size, comparer, itemequalityComparer);
      clone.AddAll(this);
      return clone;
    }

    #endregion

  }

}
