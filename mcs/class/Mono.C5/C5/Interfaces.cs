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
using SCG = System.Collections.Generic;
namespace C5
{
  /// <summary>
  /// A generic collection, that can be enumerated backwards.
  /// </summary>
  public interface IDirectedEnumerable<T> : SCG.IEnumerable<T>
  {
    /// <summary>
    /// Create a collection containing the same items as this collection, but
    /// whose enumerator will enumerate the items backwards. The new collection
    /// will become invalid if the original is modified. Method typically used as in
    /// <code>foreach (T x in coll.Backwards()) {...}</code>
    /// </summary>
    /// <returns>The backwards collection.</returns>
    IDirectedEnumerable<T> Backwards();


    /// <summary>
    /// <code>Forwards</code> if same, else <code>Backwards</code>
    /// </summary>
    /// <value>The enumeration direction relative to the original collection.</value>
    EnumerationDirection Direction { get;}
  }

  /// <summary>
  /// A generic collection that may be enumerated and can answer
  /// efficiently how many items it contains. Like <code>IEnumerable&lt;T&gt;</code>,
  /// this interface does not prescribe any operations to initialize or update the 
  /// collection. The main usage for this interface is to be the return type of 
  /// query operations on generic collection.
  /// </summary>
  public interface ICollectionValue<T> : SCG.IEnumerable<T>, IShowable
  {
    /// <summary>
    /// A flag bitmap of the events subscribable to by this collection.
    /// </summary>
    /// <value></value>
    EventTypeEnum ListenableEvents { get;}

    /// <summary>
    /// A flag bitmap of the events currently subscribed to by this collection.
    /// </summary>
    /// <value></value>
    EventTypeEnum ActiveEvents { get;}

    /// <summary>
    /// The change event. Will be raised for every change operation on the collection.
    /// </summary>
    event CollectionChangedHandler<T> CollectionChanged;

    /// <summary>
    /// The change event. Will be raised for every clear operation on the collection.
    /// </summary>
    event CollectionClearedHandler<T> CollectionCleared;

    /// <summary>
    /// The item added  event. Will be raised for every individual addition to the collection.
    /// </summary>
    event ItemsAddedHandler<T> ItemsAdded;

    /// <summary>
    /// The item inserted  event. Will be raised for every individual insertion to the collection.
    /// </summary>
    event ItemInsertedHandler<T> ItemInserted;

    /// <summary>
    /// The item removed event. Will be raised for every individual removal from the collection.
    /// </summary>
    event ItemsRemovedHandler<T> ItemsRemoved;

    /// <summary>
    /// The item removed at event. Will be raised for every individual removal at from the collection.
    /// </summary>
    event ItemRemovedAtHandler<T> ItemRemovedAt;

    /// <summary>
    /// 
    /// </summary>
    /// <value>True if this collection is empty.</value>
    bool IsEmpty { get;}

    /// <summary>
    /// </summary>
    /// <value>The number of items in this collection</value>
    int Count { get;}

    /// <summary>
    /// The value is symbolic indicating the type of asymptotic complexity
    /// in terms of the size of this collection (worst-case or amortized as
    /// relevant).
    /// </summary>
    /// <value>A characterization of the speed of the 
    /// <code>Count</code> property in this collection.</value>
    Speed CountSpeed { get;}

    /// <summary>
    /// Copy the items of this collection to a contiguous part of an array.
    /// </summary>
    /// <param name="array">The array to copy to</param>
    /// <param name="index">The index at which to copy the first item</param>
    void CopyTo(T[] array, int index);

    /// <summary>
    /// Create an array with the items of this collection (in the same order as an
    /// enumerator would output them).
    /// </summary>
    /// <returns>The array</returns>
    T[] ToArray();

    /// <summary>
    /// Apply a delegate to all items of this collection.
    /// </summary>
    /// <param name="action">The delegate to apply</param>
    void Apply(Act<T> action);


    /// <summary>
    /// Check if there exists an item  that satisfies a
    /// specific predicate in this collection.
    /// </summary>
    /// <param name="predicate">A  delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <returns>True is such an item exists</returns>
    bool Exists(Fun<T, bool> predicate);

    /// <summary>
    /// Check if there exists an item  that satisfies a
    /// specific predicate in this collection and return the first one in enumeration order.
    /// </summary>
    /// <param name="predicate">A delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <param name="item"></param>
    /// <returns>True is such an item exists</returns>
    bool Find(Fun<T, bool> predicate, out T item);


    /// <summary>
    /// Check if all items in this collection satisfies a specific predicate.
    /// </summary>
    /// <param name="predicate">A delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <returns>True if all items satisfies the predicate</returns>
    bool All(Fun<T, bool> predicate);

    /// <summary>
    /// Choose some item of this collection. 
    /// <para>Implementations must assure that the item 
    /// returned may be efficiently removed.</para>
    /// <para>Implementors may decide to implement this method in a way such that repeated
    /// calls do not necessarily give the same result, i.e. so that the result of the following 
    /// test is undetermined:
    /// <code>coll.Choose() == coll.Choose()</code></para>
    /// </summary>
    /// <exception cref="NoSuchItemException">if collection is empty.</exception>
    /// <returns></returns>
    T Choose();

    /// <summary>
    /// Create an enumerable, enumerating the items of this collection that satisfies 
    /// a certain condition.
    /// </summary>
    /// <param name="filter">The T->bool filter delegate defining the condition</param>
    /// <returns>The filtered enumerable</returns>
    SCG.IEnumerable<T> Filter(Fun<T, bool> filter);
  }



  /// <summary>
  /// A sized generic collection, that can be enumerated backwards.
  /// </summary>
  public interface IDirectedCollectionValue<T> : ICollectionValue<T>, IDirectedEnumerable<T>
  {
    /// <summary>
    /// Create a collection containing the same items as this collection, but
    /// whose enumerator will enumerate the items backwards. The new collection
    /// will become invalid if the original is modified. Method typically used as in
    /// <code>foreach (T x in coll.Backwards()) {...}</code>
    /// </summary>
    /// <returns>The backwards collection.</returns>
    new IDirectedCollectionValue<T> Backwards();

    /// <summary>
    /// Check if there exists an item  that satisfies a
    /// specific predicate in this collection and return the first one in enumeration order.
    /// </summary>
    /// <param name="predicate">A delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <param name="item"></param>
    /// <returns>True is such an item exists</returns>
    bool FindLast(Fun<T, bool> predicate, out T item);
  }


  /// <summary>
  /// A generic collection to which one may add items. This is just the intersection
  /// of the main stream generic collection interfaces and the priority queue interface,
  /// <see cref="T:C5.ICollection`1"/> and <see cref="T:C5.IPriorityQueue`1"/>.
  /// </summary>
  public interface IExtensible<T> : ICollectionValue<T>, ICloneable
  {
    /// <summary>
    /// If true any call of an updating operation will throw an
    /// <code>ReadOnlyCollectionException</code>
    /// </summary>
    /// <value>True if this collection is read-only.</value>
    bool IsReadOnly { get;}

    //TODO: wonder where the right position of this is
    /// <summary>
    /// 
    /// </summary>
    /// <value>False if this collection has set semantics, true if bag semantics.</value>
    bool AllowsDuplicates { get;}

    //TODO: wonder where the right position of this is. And the semantics.
    /// <summary>
    /// (Here should be a discussion of the role of equalityComparers. Any ). 
    /// </summary>
    /// <value>The equalityComparer used by this collection to check equality of items. 
    /// Or null (????) if collection does not check equality at all or uses a comparer.</value>
    SCG.IEqualityComparer<T> EqualityComparer { get;}

    //ItemEqualityTypeEnum ItemEqualityType {get ;}

    //TODO: find a good name

    /// <summary>
    /// By convention this is true for any collection with set semantics.
    /// </summary>
    /// <value>True if only one representative of a group of equal items 
    /// is kept in the collection together with the total count.</value>
    bool DuplicatesByCounting { get;}

    /// <summary>
    /// Add an item to this collection if possible. If this collection has set
    /// semantics, the item will be added if not already in the collection. If
    /// bag semantics, the item will always be added.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if item was added.</returns>
    bool Add(T item);

    /// <summary>
    /// Add the elements from another collection with a more specialized item type 
    /// to this collection. If this
    /// collection has set semantics, only items not already in the collection
    /// will be added.
    /// </summary>
    /// <typeparam name="U">The type of items to add</typeparam>
    /// <param name="items">The items to add</param>
    void AddAll<U>(SCG.IEnumerable<U> items) where U : T;

    //void Clear(); // for priority queue
    //int Count why not?
    /// <summary>
    /// Check the integrity of the internal data structures of this collection.
    /// <i>This is only relevant for developers of the library</i>
    /// </summary>
    /// <returns>True if check was passed.</returns>
    bool Check();
  }

  /// <summary>
  /// The simplest interface of a main stream generic collection
  /// with lookup, insertion and removal operations. 
  /// </summary>
  public interface ICollection<T> : IExtensible<T>, SCG.ICollection<T>
  {
    //This is somewhat similar to the RandomAccess marker itf in java
    /// <summary>
    /// The value is symbolic indicating the type of asymptotic complexity
    /// in terms of the size of this collection (worst-case or amortized as
    /// relevant). 
    /// <para>See <see cref="T:C5.Speed"/> for the set of symbols.</para>
    /// </summary>
    /// <value>A characterization of the speed of lookup operations
    /// (<code>Contains()</code> etc.) of the implementation of this collection.</value>
    Speed ContainsSpeed { get;}

    /// <summary>
    /// </summary>
    /// <value>The number of items in this collection</value>
    new int Count { get; }

    /// <summary>
    /// If true any call of an updating operation will throw an
    /// <code>ReadOnlyCollectionException</code>
    /// </summary>
    /// <value>True if this collection is read-only.</value>
    new bool IsReadOnly { get; }

    /// <summary>
    /// Add an item to this collection if possible. If this collection has set
    /// semantics, the item will be added if not already in the collection. If
    /// bag semantics, the item will always be added.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>True if item was added.</returns>
    new bool Add(T item);

    /// <summary>
    /// Copy the items of this collection to a contiguous part of an array.
    /// </summary>
    /// <param name="array">The array to copy to</param>
    /// <param name="index">The index at which to copy the first item</param>
    new void CopyTo(T[] array, int index);

    /// <summary>
    /// The unordered collection hashcode is defined as the sum of 
    /// <code>h(hashcode(item))</code> over the items
    /// of the collection, where the function <code>h</code> is a function from 
    /// int to int of the form <code> t -> (a0*t+b0)^(a1*t+b1)^(a2*t+b2)</code>, where 
    /// the ax and bx are the same for all collection classes. 
    /// <para>The current implementation uses fixed values for the ax and bx, 
    /// specified as constants in the code.</para>
    /// </summary>
    /// <returns>The unordered hashcode of this collection.</returns>
    int GetUnsequencedHashCode();


    /// <summary>
    /// Compare the contents of this collection to another one without regards to
    /// the sequence order. The comparison will use this collection's itemequalityComparer
    /// to compare individual items.
    /// </summary>
    /// <param name="otherCollection">The collection to compare to.</param>
    /// <returns>True if this collection and that contains the same items.</returns>
    bool UnsequencedEquals(ICollection<T> otherCollection);


    /// <summary>
    /// Check if this collection contains (an item equivalent to according to the
    /// itemequalityComparer) a particular value.
    /// </summary>
    /// <param name="item">The value to check for.</param>
    /// <returns>True if the items is in this collection.</returns>
    new bool Contains(T item);


    /// <summary>
    /// Count the number of items of the collection equal to a particular value.
    /// Returns 0 if and only if the value is not in the collection.
    /// </summary>
    /// <param name="item">The value to count.</param>
    /// <returns>The number of copies found.</returns>
    int ContainsCount(T item);


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ICollectionValue<T> UniqueItems();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ICollectionValue<KeyValuePair<T, int>> ItemMultiplicities();

    /// <summary>
    /// Check whether this collection contains all the values in another collection.
    /// If this collection has bag semantics (<code>AllowsDuplicates==true</code>)
    /// the check is made with respect to multiplicities, else multiplicities
    /// are not taken into account.
    /// </summary>
    /// <param name="items">The </param>
    /// <typeparam name="U"></typeparam>
    /// <returns>True if all values in <code>items</code>is in this collection.</returns>
    bool ContainsAll<U>(SCG.IEnumerable<U> items) where U : T;


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, return in the ref argument (a
    /// binary copy of) the actual value found.
    /// </summary>
    /// <param name="item">The value to look for.</param>
    /// <returns>True if the items is in this collection.</returns>
    bool Find(ref T item);


    //This should probably just be bool Add(ref T item); !!!
    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, return in the ref argument (a
    /// binary copy of) the actual value found. Else, add the item to the collection.
    /// </summary>
    /// <param name="item">The value to look for.</param>
    /// <returns>True if the item was found (hence not added).</returns>
    bool FindOrAdd(ref T item);


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// with a (binary copy of) the supplied value. If the collection has bag semantics,
    /// it depends on the value of DuplicatesByCounting if this updates all equivalent copies in
    /// the collection or just one.
    /// </summary>
    /// <param name="item">Value to update.</param>
    /// <returns>True if the item was found and hence updated.</returns>
    bool Update(T item);

    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// with a (binary copy of) the supplied value. If the collection has bag semantics,
    /// it depends on the value of DuplicatesByCounting if this updates all equivalent copies in
    /// the collection or just one.
    /// </summary>
    /// <param name="item">Value to update.</param>
    /// <param name="olditem">On output the olditem, if found.</param>
    /// <returns>True if the item was found and hence updated.</returns>
    bool Update(T item, out T olditem);


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// to with a binary copy of the supplied value; else add the value to the collection. 
    /// </summary>
    /// <param name="item">Value to add or update.</param>
    /// <returns>True if the item was found and updated (hence not added).</returns>
    bool UpdateOrAdd(T item);


    /// <summary>
    /// Check if this collection contains an item equivalent according to the
    /// itemequalityComparer to a particular value. If so, update the item in the collection 
    /// to with a binary copy of the supplied value; else add the value to the collection. 
    /// </summary>
    /// <param name="item">Value to add or update.</param>
    /// <param name="olditem">On output the olditem, if found.</param>
    /// <returns>True if the item was found and updated (hence not added).</returns>
    bool UpdateOrAdd(T item, out T olditem);

    /// <summary>
    /// Remove a particular item from this collection. If the collection has bag
    /// semantics only one copy equivalent to the supplied item is removed. 
    /// </summary>
    /// <param name="item">The value to remove.</param>
    /// <returns>True if the item was found (and removed).</returns>
    new bool Remove(T item);


    /// <summary>
    /// Remove a particular item from this collection if found. If the collection
    /// has bag semantics only one copy equivalent to the supplied item is removed,
    /// which one is implementation dependent. 
    /// If an item was removed, report a binary copy of the actual item removed in 
    /// the argument.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    /// <param name="removeditem">The value removed if any.</param>
    /// <returns>True if the item was found (and removed).</returns>
    bool Remove(T item, out T removeditem);


    /// <summary>
    /// Remove all items equivalent to a given value.
    /// </summary>
    /// <param name="item">The value to remove.</param>
    void RemoveAllCopies(T item);


    /// <summary>
    /// Remove all items in another collection from this one. If this collection
    /// has bag semantics, take multiplicities into account.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to remove.</param>
    void RemoveAll<U>(SCG.IEnumerable<U> items) where U : T;

    //void RemoveAll(Fun<T, bool> predicate);

    /// <summary>
    /// Remove all items from this collection.
    /// </summary>
    new void Clear();


    /// <summary>
    /// Remove all items not in some other collection from this one. If this collection
    /// has bag semantics, take multiplicities into account.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items">The items to retain.</param>
    void RetainAll<U>(SCG.IEnumerable<U> items) where U : T;

    //void RetainAll(Fun<T, bool> predicate);
    //IDictionary<T> UniqueItems()
  }



  /// <summary>
  /// An editable collection maintaining a definite sequence order of the items.
  ///
  /// <i>Implementations of this interface must compute the hash code and 
  /// equality exactly as prescribed in the method definitions in order to
  /// be consistent with other collection classes implementing this interface.</i>
  /// <i>This interface is usually implemented by explicit interface implementation,
  /// not as ordinary virtual methods.</i>
  /// </summary>
  public interface ISequenced<T> : ICollection<T>, IDirectedCollectionValue<T>
  {
    /// <summary>
    /// The hashcode is defined as <code>h(...h(h(h(x1),x2),x3),...,xn)</code> for
    /// <code>h(a,b)=CONSTANT*a+b</code> and the x's the hash codes of the items of 
    /// this collection.
    /// </summary>
    /// <returns>The sequence order hashcode of this collection.</returns>
    int GetSequencedHashCode();


    /// <summary>
    /// Compare this sequenced collection to another one in sequence order.
    /// </summary>
    /// <param name="otherCollection">The sequenced collection to compare to.</param>
    /// <returns>True if this collection and that contains equal (according to
    /// this collection's itemequalityComparer) in the same sequence order.</returns>
    bool SequencedEquals(ISequenced<T> otherCollection);
  }



  /// <summary>
  /// A sequenced collection, where indices of items in the order are maintained
  /// </summary>
  public interface IIndexed<T> : ISequenced<T>
  {
    /// <summary>
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if <code>index</code> is negative or
    /// &gt;= the size of the collection.</exception>
    /// <value>The <code>index</code>'th item of this list.</value>
    /// <param name="index">the index to lookup</param>
    T this[int index] { get;}

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    Speed IndexingSpeed { get;}

    /// <summary>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <value>The directed collection of items in a specific index interval.</value>
    /// <param name="start">The low index of the interval (inclusive).</param>
    /// <param name="count">The size of the range.</param>
    IDirectedCollectionValue<T> this[int start, int count] { get;}


    /// <summary>
    /// Searches for an item in the list going forwards from the start. 
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of item from start. A negative number if item not found, 
    /// namely the one's complement of the index at which the Add operation would put the item.</returns>
    int IndexOf(T item);


    /// <summary>
    /// Searches for an item in the list going backwards from the end.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of of item from the end. A negative number if item not found, 
    /// namely the two-complement of the index at which the Add operation would put the item.</returns>
    int LastIndexOf(T item);

    /// <summary>
    /// Check if there exists an item  that satisfies a
    /// specific predicate in this collection and return the index of the first one.
    /// </summary>
    /// <param name="predicate">A delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <returns>the index, if found, a negative value else</returns>
    int FindIndex(Fun<T, bool> predicate);

    /// <summary>
    /// Check if there exists an item  that satisfies a
    /// specific predicate in this collection and return the index of the last one.
    /// </summary>
    /// <param name="predicate">A delegate 
    /// (<see cref="T:C5.Fun`2"/> with <code>R == bool</code>) defining the predicate</param>
    /// <returns>the index, if found, a negative value else</returns>
    int FindLastIndex(Fun<T, bool> predicate);


    /// <summary>
    /// Remove the item at a specific position of the list.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if <code>index</code> is negative or
    /// &gt;= the size of the collection.</exception>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>The removed item.</returns>
    T RemoveAt(int index);


    /// <summary>
    /// Remove all items in an index interval.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> if start or count 
    /// is negative or start+count &gt; the size of the collection.</exception>
    /// <param name="start">The index of the first item to remove.</param>
    /// <param name="count">The number of items to remove.</param>
    void RemoveInterval(int start, int count);
  }

  //TODO: decide if this should extend ICollection
  /// <summary>
  /// The interface describing the operations of a LIFO stack data structure.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  public interface IStack<T> : IDirectedCollectionValue<T>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    bool AllowsDuplicates { get;}
    /// <summary>
    /// Get the <code>index</code>'th element of the stack.  The bottom of the stack has index 0.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    T this[int index] { get;}
    /// <summary>
    /// Push an item to the top of the stack.
    /// </summary>
    /// <param name="item">The item</param>
    void Push(T item);
    /// <summary>
    /// Pop the item at the top of the stack from the stack.
    /// </summary>
    /// <returns>The popped item.</returns>
    T Pop();
  }

  /// <summary>
  /// The interface describing the operations of a FIFO queue data structure.
  /// </summary>
  /// <typeparam name="T">The item type</typeparam>
  public interface IQueue<T> : IDirectedCollectionValue<T>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    bool AllowsDuplicates { get;}
    /// <summary>
    /// Get the <code>index</code>'th element of the queue.  The front of the queue has index 0.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    T this[int index] { get;}
    /// <summary>
    /// Enqueue an item at the back of the queue. 
    /// </summary>
    /// <param name="item">The item</param>
    void Enqueue(T item);
    /// <summary>
    /// Dequeue an item from the front of the queue.
    /// </summary>
    /// <returns>The item</returns>
    T Dequeue();
  }


  /// <summary>
  /// This is an indexed collection, where the item order is chosen by 
  /// the user at insertion time.
  ///
  /// NBNBNB: we need a description of the view functionality here!
  /// </summary>
  public interface IList<T> : IIndexed<T>, IDisposable, SCG.IList<T>, System.Collections.IList
  {
    /// <summary>
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <value>The first item in this list.</value>
    T First { get;}

    /// <summary>
    /// </summary>
    /// <exception cref="NoSuchItemException"> if this list is empty.</exception>
    /// <value>The last item in this list.</value>
    T Last { get;}

    /// <summary>
    /// Since <code>Add(T item)</code> always add at the end of the list,
    /// this describes if list has FIFO or LIFO semantics.
    /// </summary>
    /// <value>True if the <code>Remove()</code> operation removes from the
    /// start of the list, false if it removes from the end.</value>
    bool FIFO { get; set;}

    /// <summary>
    /// 
    /// </summary>
    bool IsFixedSize { get; }

    /// <summary>
    /// On this list, this indexer is read/write.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if index is negative or
    /// &gt;= the size of the collection.</exception>
    /// <value>The index'th item of this list.</value>
    /// <param name="index">The index of the item to fetch or store.</param>
    new T this[int index] { get; set;}

    #region Ambiguous calls when extending SCG.IList<T>

    #region SCG.ICollection<T>
    /// <summary>
    /// 
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// 
    /// </summary>
    new bool IsReadOnly { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    new bool Add(T item);

    /// <summary>
    /// 
    /// </summary>
    new void Clear();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    new bool Contains(T item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    new void CopyTo(T[] array, int index);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    new bool Remove(T item);

    #endregion

    #region SCG.IList<T> proper

    /// <summary>
    /// Searches for an item in the list going forwards from the start. 
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>Index of item from start. A negative number if item not found, 
    /// namely the one's complement of the index at which the Add operation would put the item.</returns>
    new int IndexOf(T item);

    /// <summary>
    /// Remove the item at a specific position of the list.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if <code>index</code> is negative or
    /// &gt;= the size of the collection.</exception>
    /// <param name="index">The index of the item to remove.</param>
    /// <returns>The removed item.</returns>
    new T RemoveAt(int index);

    #endregion

    #endregion

    /*/// <summary>
    /// Insert an item at a specific index location in this list. 
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if <code>index</code> is negative or
    /// &gt; the size of the collection.</exception>
    /// <exception cref="DuplicateNotAllowedException"> if the list has
    /// <code>AllowsDuplicates==false</code> and the item is 
    /// already in the list.</exception>
    /// <param name="index">The index at which to insert.</param>
    /// <param name="item">The item to insert.</param>
    void Insert(int index, T item);*/

    /// <summary>
    /// Insert an item at the end of a compatible view, used as a pointer.
    /// <para>The <code>pointer</code> must be a view on the same list as
    /// <code>this</code> and the endpoitn of <code>pointer</code> must be
    /// a valid insertion point of <code>this</code></para>
    /// </summary>
    /// <exception cref="IncompatibleViewException">If <code>pointer</code> 
    /// is not a view on the same list as <code>this</code></exception>
    /// <exception cref="IndexOutOfRangeException"><b>??????</b> if the endpoint of 
    ///  <code>pointer</code> is not inside <code>this</code></exception>
    /// <exception cref="DuplicateNotAllowedException"> if the list has
    /// <code>AllowsDuplicates==false</code> and the item is 
    /// already in the list.</exception>
    /// <param name="pointer"></param>
    /// <param name="item"></param>
    void Insert(IList<T> pointer, T item);

    /// <summary>
    /// Insert an item at the front of this list.
    /// <exception cref="DuplicateNotAllowedException"/> if the list has
    /// <code>AllowsDuplicates==false</code> and the item is 
    /// already in the list.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    void InsertFirst(T item);

    /// <summary>
    /// Insert an item at the back of this list.
    /// <exception cref="DuplicateNotAllowedException"/> if the list has
    /// <code>AllowsDuplicates==false</code> and the item is 
    /// already in the list.
    /// </summary>
    /// <param name="item">The item to insert.</param>
    void InsertLast(T item);

    /// <summary>
    /// Insert into this list all items from an enumerable collection starting 
    /// at a particular index.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"> if <code>index</code> is negative or
    /// &gt; the size of the collection.</exception>
    /// <exception cref="DuplicateNotAllowedException"> if the list has 
    /// <code>AllowsDuplicates==false</code> and one of the items to insert is
    /// already in the list.</exception>
    /// <param name="index">Index to start inserting at</param>
    /// <param name="items">Items to insert</param>
    /// <typeparam name="U"></typeparam>
    void InsertAll<U>(int index, SCG.IEnumerable<U> items) where U : T;

    /// <summary>
    /// Create a new list consisting of the items of this list satisfying a 
    /// certain predicate.
    /// </summary>
    /// <param name="filter">The filter delegate defining the predicate.</param>
    /// <returns>The new list.</returns>
    IList<T> FindAll(Fun<T, bool> filter);

    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use the default equalityComparer for the item type V.
    /// </summary>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <returns>The new list.</returns>
    IList<V> Map<V>(Fun<T, V> mapper);

    /// <summary>
    /// Create a new list consisting of the results of mapping all items of this
    /// list. The new list will use a specified equalityComparer for the item type.
    /// </summary>
    /// <typeparam name="V">The type of items of the new list</typeparam>
    /// <param name="mapper">The delegate defining the map.</param>
    /// <param name="equalityComparer">The equalityComparer to use for the new list</param>
    /// <returns>The new list.</returns>
    IList<V> Map<V>(Fun<T, V> mapper, SCG.IEqualityComparer<V> equalityComparer);

    /// <summary>
    /// Remove one item from the list: from the front if <code>FIFO</code>
    /// is true, else from the back.
    /// <exception cref="NoSuchItemException"/> if this list is empty.
    /// </summary>
    /// <returns>The removed item.</returns>
    T Remove();

    /// <summary>
    /// Remove one item from the front of the list.
    /// <exception cref="NoSuchItemException"/> if this list is empty.
    /// </summary>
    /// <returns>The removed item.</returns>
    T RemoveFirst();

    /// <summary>
    /// Remove one item from the back of the list.
    /// <exception cref="NoSuchItemException"/> if this list is empty.
    /// </summary>
    /// <returns>The removed item.</returns>
    T RemoveLast();

    /// <summary>
    /// Create a list view on this list. 
    /// <exception cref="ArgumentOutOfRangeException"/> if the view would not fit into
    /// this list.
    /// </summary>
    /// <param name="start">The index in this list of the start of the view.</param>
    /// <param name="count">The size of the view.</param>
    /// <returns>The new list view.</returns>
    IList<T> View(int start, int count);

    /// <summary>
    /// Create a list view on this list containing the (first) occurrence of a particular item. 
    /// <exception cref="NoSuchItemException"/> if the item is not in this list.
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The new list view.</returns>
    IList<T> ViewOf(T item);

    /// <summary>
    /// Create a list view on this list containing the last occurrence of a particular item. 
    /// <exception cref="NoSuchItemException"/> if the item is not in this list.
    /// </summary>
    /// <param name="item">The item to find.</param>
    /// <returns>The new list view.</returns>
    IList<T> LastViewOf(T item);

    /// <summary>
    /// Null if this list is not a view.
    /// </summary>
    /// <value>Underlying list for view.</value>
    IList<T> Underlying { get;}

    /// <summary>
    /// </summary>
    /// <value>Offset for this list view or 0 for an underlying list.</value>
    int Offset { get;}

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    bool IsValid { get;}

    /// <summary>
    /// Slide this list view along the underlying list.
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <exception cref="ArgumentOutOfRangeException"> if the operation
    /// would bring either end of the view outside the underlying list.</exception>
    /// <param name="offset">The signed amount to slide: positive to slide
    /// towards the end.</param>
    IList<T> Slide(int offset);

    /// <summary>
    /// Slide this list view along the underlying list, changing its size.
    /// 
    /// </summary>
    /// <exception cref="NotAViewException"> if this list is not a view.</exception>
    /// <exception cref="ArgumentOutOfRangeException"> if the operation
    /// would bring either end of the view outside the underlying list.</exception>
    /// <param name="offset">The signed amount to slide: positive to slide
    /// towards the end.</param>
    /// <param name="size">The new size of the view.</param>
    IList<T> Slide(int offset, int size);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    bool TrySlide(int offset);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    bool TrySlide(int offset, int size);

    /// <summary>
    /// 
    /// <para>Returns null if <code>otherView</code> is strictly to the left of this view</para>
    /// </summary>
    /// <param name="otherView"></param>
    /// <exception cref="IncompatibleViewException">If otherView does not have the same underlying list as this</exception>
    /// <exception cref="ArgumentOutOfRangeException">If <code>otherView</code> is strictly to the left of this view</exception>
    /// <returns></returns>
    IList<T> Span(IList<T> otherView);

    /// <summary>
    /// Reverse the list so the items are in the opposite sequence order.
    /// </summary>
    void Reverse();

    /// <summary>
    /// Check if this list is sorted according to the default sorting order
    /// for the item type T, as defined by the <see cref="T:C5.Comparer`1"/> class 
    /// </summary>
    /// <exception cref="NotComparableException">if T is not comparable</exception>
    /// <returns>True if the list is sorted, else false.</returns>
    bool IsSorted();

    /// <summary>
    /// Check if this list is sorted according to a specific sorting order.
    /// </summary>
    /// <param name="comparer">The comparer defining the sorting order.</param>
    /// <returns>True if the list is sorted, else false.</returns>
    bool IsSorted(SCG.IComparer<T> comparer);

    /// <summary>
    /// Sort the items of the list according to the default sorting order
    /// for the item type T, as defined by the <see cref="T:C5.Comparer`1"/> class 
    /// </summary>
    /// <exception cref="NotComparableException">if T is not comparable</exception>
    void Sort();

    /// <summary>
    /// Sort the items of the list according to a specified sorting order.
    /// <para>The sorting does not perform duplicate elimination or identify items
    /// according to the comparer or itemequalityComparer. I.e. the list as an 
    /// unsequenced collection with binary equality, will not change.
    /// </para>
    /// </summary>
    /// <param name="comparer">The comparer defining the sorting order.</param>
    void Sort(SCG.IComparer<T> comparer);


    /// <summary>
    /// Randomly shuffle the items of this list. 
    /// </summary>
    void Shuffle();


    /// <summary>
    /// Shuffle the items of this list according to a specific random source.
    /// </summary>
    /// <param name="rnd">The random source.</param>
    void Shuffle(Random rnd);
  }


  /// <summary>
  /// The base type of a priority queue handle
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IPriorityQueueHandle<T>
  {
    //TODO: make abstract and prepare for double dispatch:
    //public virtual bool Delete(IPriorityQueue<T> q) { throw new InvalidFooException();}
    //bool Replace(T item);
  }


  /// <summary>
  /// A generic collection of items prioritized by a comparison (order) relation.
  /// Supports adding items and reporting or removing extremal elements. 
  /// <para>
  /// 
  /// </para>
  /// When adding an item, the user may choose to have a handle allocated for this item in the queue. 
  /// The resulting handle may be used for deleting the item even if not extremal, and for replacing the item.
  /// A priority queue typically only holds numeric priorities associated with some objects
  /// maintained separately in other collection objects.
  /// </summary>
  public interface IPriorityQueue<T> : IExtensible<T>
  {
    /// <summary>
    /// Find the current least item of this priority queue.
    /// </summary>
    /// <returns>The least item.</returns>
    T FindMin();


    /// <summary>
    /// Remove the least item from this  priority queue.
    /// </summary>
    /// <returns>The removed item.</returns>
    T DeleteMin();


    /// <summary>
    /// Find the current largest item of this priority queue.
    /// </summary>
    /// <returns>The largest item.</returns>
    T FindMax();


    /// <summary>
    /// Remove the largest item from this priority queue.
    /// </summary>
    /// <returns>The removed item.</returns>
    T DeleteMax();

    /// <summary>
    /// The comparer object supplied at creation time for this collection
    /// </summary>
    /// <value>The comparer</value>
    SCG.IComparer<T> Comparer { get;}
    /// <summary>
    /// Get or set the item corresponding to a handle. Throws exceptions on 
    /// invalid handles.
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    T this[IPriorityQueueHandle<T> handle] { get; set;}

    /// <summary>
    /// Check if the entry corresponding to a handle is in the priority queue.
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    bool Find(IPriorityQueueHandle<T> handle, out T item);

    /// <summary>
    /// Add an item to the priority queue, receiving a 
    /// handle for the item in the queue, 
    /// or reusing an existing unused handle.
    /// </summary>
    /// <param name="handle">On output: a handle for the added item. 
    /// On input: null for allocating a new handle, or a currently unused handle for reuse. 
    /// A handle for reuse must be compatible with this priority queue, 
    /// by being created by a priority queue of the same runtime type, but not 
    /// necessarily the same priority queue object.</param>
    /// <param name="item"></param>
    /// <returns></returns>
    bool Add(ref IPriorityQueueHandle<T> handle, T item);

    /// <summary>
    /// Delete an item with a handle from a priority queue
    /// </summary>
    /// <param name="handle">The handle for the item. The handle will be invalidated, but reusable.</param>
    /// <returns>The deleted item</returns>
    T Delete(IPriorityQueueHandle<T> handle);

    /// <summary>
    /// Replace an item with a handle in a priority queue with a new item. 
    /// Typically used for changing the priority of some queued object.
    /// </summary>
    /// <param name="handle">The handle for the old item</param>
    /// <param name="item">The new item</param>
    /// <returns>The old item</returns>
    T Replace(IPriorityQueueHandle<T> handle, T item);

    /// <summary>
    /// Find the current least item of this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the item.</param>
    /// <returns>The least item.</returns>
    T FindMin(out IPriorityQueueHandle<T> handle);

    /// <summary>
    /// Find the current largest item of this priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the item.</param>
    /// <returns>The largest item.</returns>

    T FindMax(out IPriorityQueueHandle<T> handle);

    /// <summary>
    /// Remove the least item from this  priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the removed item.</param>
    /// <returns>The removed item.</returns>

    T DeleteMin(out IPriorityQueueHandle<T> handle);

    /// <summary>
    /// Remove the largest item from this  priority queue.
    /// </summary>
    /// <param name="handle">On return: the handle of the removed item.</param>
    /// <returns>The removed item.</returns>
    T DeleteMax(out IPriorityQueueHandle<T> handle);
  }



  /// <summary>
  /// A sorted collection, i.e. a collection where items are maintained and can be searched for in sorted order.
  /// Thus the sequence order is given as a sorting order.
  /// 
  /// <para>The sorting order is defined by a comparer, an object of type IComparer&lt;T&gt; 
  /// (<see cref="T:C5.IComparer`1"/>). Implementors of this interface will normally let the user 
  /// define the comparer as an argument to a constructor. 
  /// Usually there will also be constructors without a comparer argument, in which case the 
  /// comparer should be the defalt comparer for the item type, <see cref="P:C5.Comparer`1.Default"/>.</para>
  /// 
  /// <para>The comparer of the sorted collection is available as the <code>Comparer</code> property 
  /// (<see cref="P:C5.ISorted`1.Comparer"/>).</para>
  /// 
  /// <para>The methods are grouped according to
  /// <list>
  /// <item>Extrema: report or report and delete an extremal item. This is reminiscent of simplified priority queues.</item>
  /// <item>Nearest neighbor: report predecessor or successor in the collection of an item. Cut belongs to this group.</item>
  /// <item>Range: report a view of a range of elements or remove all elements in a range.</item>
  /// <item>AddSorted: add a collection of items known to be sorted in the same order (should be faster) (to be removed?)</item>
  /// </list>
  /// </para>
  /// 
  /// <para>Since this interface extends ISequenced&lt;T&gt;, sorted collections will also have an 
  /// item equalityComparer (<see cref="P:C5.IExtensible`1.EqualityComparer"/>). This equalityComparer will not be used in connection with 
  /// the inner workings of the sorted collection, but will be used if the sorted collection is used as 
  /// an item in a collection of unsequenced or sequenced collections, 
  /// (<see cref="T:C5.ICollection`1"/> and <see cref="T:C5.ISequenced`1"/>)</para>
  /// 
  /// <para>Note that code may check if two sorted collections has the same sorting order 
  /// by checking if the Comparer properties are equal. This is done a few places in this library
  /// for optimization purposes.</para>
  /// </summary>
  public interface ISorted<T> : ISequenced<T>
  {
    /// <summary>
    /// Find the current least item of this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The least item.</returns>
    T FindMin();


    /// <summary>
    /// Remove the least item from this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The removed item.</returns>
    T DeleteMin();


    /// <summary>
    /// Find the current largest item of this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The largest item.</returns>
    T FindMax();


    /// <summary>
    /// Remove the largest item from this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The removed item.</returns>
    T DeleteMax();

    /// <summary>
    /// The comparer object supplied at creation time for this sorted collection.
    /// </summary>
    /// <value>The comparer</value>
    SCG.IComparer<T> Comparer { get; }

    /// <summary>
    /// Find the strict predecessor of item in the sorted collection,
    /// that is, the greatest item in the collection smaller than the item.
    /// </summary>
    /// <param name="item">The item to find the predecessor for.</param>
    /// <param name="res">The predecessor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a predecessor; otherwise false.</returns>
    bool TryPredecessor(T item, out T res);


    /// <summary>
    /// Find the strict successor of item in the sorted collection,
    /// that is, the least item in the collection greater than the supplied value.
    /// </summary>
    /// <param name="item">The item to find the successor for.</param>
    /// <param name="res">The successor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a successor; otherwise false.</returns>
    bool TrySuccessor(T item, out T res);


    /// <summary>
    /// Find the weak predecessor of item in the sorted collection,
    /// that is, the greatest item in the collection smaller than or equal to the item.
    /// </summary>
    /// <param name="item">The item to find the weak predecessor for.</param>
    /// <param name="res">The weak predecessor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a weak predecessor; otherwise false.</returns>
    bool TryWeakPredecessor(T item, out T res);


    /// <summary>
    /// Find the weak successor of item in the sorted collection,
    /// that is, the least item in the collection greater than or equal to the supplied value.
    /// </summary>
    /// <param name="item">The item to find the weak successor for.</param>
    /// <param name="res">The weak successor, if any; otherwise the default value for T.</param>
    /// <returns>True if item has a weak successor; otherwise false.</returns>
    bool TryWeakSuccessor(T item, out T res);


    /// <summary>
    /// Find the strict predecessor in the sorted collection of a particular value,
    /// that is, the largest item in the collection less than the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is less than or equal to the minimum of this collection.)</exception>
    /// <param name="item">The item to find the predecessor for.</param>
    /// <returns>The predecessor.</returns>
    T Predecessor(T item);


    /// <summary>
    /// Find the strict successor in the sorted collection of a particular value,
    /// that is, the least item in the collection greater than the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is greater than or equal to the maximum of this collection.)</exception>
    /// <param name="item">The item to find the successor for.</param>
    /// <returns>The successor.</returns>
    T Successor(T item);


    /// <summary>
    /// Find the weak predecessor in the sorted collection of a particular value,
    /// that is, the largest item in the collection less than or equal to the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is less than the minimum of this collection.)</exception>
    /// <param name="item">The item to find the weak predecessor for.</param>
    /// <returns>The weak predecessor.</returns>
    T WeakPredecessor(T item);


    /// <summary>
    /// Find the weak successor in the sorted collection of a particular value,
    /// that is, the least item in the collection greater than or equal to the supplied value.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no such element exists (the
    /// supplied  value is greater than the maximum of this collection.)</exception>
    ///<param name="item">The item to find the weak successor for.</param>
    /// <returns>The weak successor.</returns>
    T WeakSuccessor(T item);


    /// <summary>
    /// Given a "cut" function from the items of the sorted collection to <code>int</code>
    /// whose only sign changes when going through items in increasing order
    /// can be 
    /// <list>
    /// <item>from positive to zero</item>
    /// <item>from positive to negative</item>
    /// <item>from zero to negative</item>
    /// </list>
    /// The "cut" function is supplied as the <code>CompareTo</code> method 
    /// of an object <code>c</code> implementing 
    /// <code>IComparable&lt;T&gt;</code>. 
    /// A typical example is the case where <code>T</code> is comparable and 
    /// <code>cutFunction</code> is itself of type <code>T</code>.
    /// <para>This method performs a search in the sorted collection for the ranges in which the
    /// "cut" function is negative, zero respectively positive. If <code>T</code> is comparable
    /// and <code>c</code> is of type <code>T</code>, this is a safe way (no exceptions thrown) 
    /// to find predecessor and successor of <code>c</code>.
    /// </para>
    /// <para> If the supplied cut function does not satisfy the sign-change condition, 
    /// the result of this call is undefined.
    /// </para>
    /// 
    /// </summary>
    /// <param name="cutFunction">The cut function <code>T</code> to <code>int</code>, given
    /// by the <code>CompareTo</code> method of an object implementing 
    /// <code>IComparable&lt;T&gt;</code>.</param>
    /// <param name="low">Returns the largest item in the collection, where the
    /// cut function is positive (if any).</param>
    /// <param name="lowIsValid">Returns true if the cut function is positive somewhere
    /// on this collection.</param>
    /// <param name="high">Returns the least item in the collection, where the
    /// cut function is negative (if any).</param>
    /// <param name="highIsValid">Returns true if the cut function is negative somewhere
    /// on this collection.</param>
    /// <returns>True if the cut function is zero somewhere
    /// on this collection.</returns>
    bool Cut(IComparable<T> cutFunction, out T low, out bool lowIsValid, out T high, out bool highIsValid);


    /// <summary>
    /// Query this sorted collection for items greater than or equal to a supplied value.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<T> RangeFrom(T bot);


    /// <summary>
    /// Query this sorted collection for items between two supplied values.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<T> RangeFromTo(T bot, T top);


    /// <summary>
    /// Query this sorted collection for items less than a supplied value.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<T> RangeTo(T top);


    /// <summary>
    /// Create a directed collection with the same items as this collection.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <returns>The result directed collection.</returns>
    IDirectedCollectionValue<T> RangeAll();


    //TODO: remove now that we assume that we can check the sorting order?
    /// <summary>
    /// Add all the items from another collection with an enumeration order that 
    /// is increasing in the items.
    /// </summary>
    /// <exception cref="ArgumentException"> if the enumerated items turns out
    /// not to be in increasing order.</exception>
    /// <param name="items">The collection to add.</param>
    /// <typeparam name="U"></typeparam>
    void AddSorted<U>(SCG.IEnumerable<U> items) where U : T;


    /// <summary>
    /// Remove all items of this collection above or at a supplied threshold.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    void RemoveRangeFrom(T low);


    /// <summary>
    /// Remove all items of this collection between two supplied thresholds.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    /// <param name="hi">The upper threshold (exclusive).</param>
    void RemoveRangeFromTo(T low, T hi);


    /// <summary>
    /// Remove all items of this collection below a supplied threshold.
    /// </summary>
    /// <param name="hi">The upper threshold (exclusive).</param>
    void RemoveRangeTo(T hi);
  }



  /// <summary>
  /// A collection where items are maintained in sorted order together
  /// with their indexes in that order.
  /// </summary>
  public interface IIndexedSorted<T> : ISorted<T>, IIndexed<T>
  {
    /// <summary>
    /// Determine the number of items at or above a supplied threshold.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive)</param>
    /// <returns>The number of matcing items.</returns>
    int CountFrom(T bot);


    /// <summary>
    /// Determine the number of items between two supplied thresholds.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive)</param>
    /// <param name="top">The upper bound (exclusive)</param>
    /// <returns>The number of matcing items.</returns>
    int CountFromTo(T bot, T top);


    /// <summary>
    /// Determine the number of items below a supplied threshold.
    /// </summary>
    /// <param name="top">The upper bound (exclusive)</param>
    /// <returns>The number of matcing items.</returns>
    int CountTo(T top);


    /// <summary>
    /// Query this sorted collection for items greater than or equal to a supplied value.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <returns>The result directed collection.</returns>
    new IDirectedCollectionValue<T> RangeFrom(T bot);


    /// <summary>
    /// Query this sorted collection for items between two supplied values.
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    new IDirectedCollectionValue<T> RangeFromTo(T bot, T top);


    /// <summary>
    /// Query this sorted collection for items less than a supplied value.
    /// </summary>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    new IDirectedCollectionValue<T> RangeTo(T top);


    /// <summary>
    /// Create a new indexed sorted collection consisting of the items of this
    /// indexed sorted collection satisfying a certain predicate.
    /// </summary>
    /// <param name="predicate">The filter delegate defining the predicate.</param>
    /// <returns>The new indexed sorted collection.</returns>
    IIndexedSorted<T> FindAll(Fun<T, bool> predicate);


    /// <summary>
    /// Create a new indexed sorted collection consisting of the results of
    /// mapping all items of this list.
    /// <exception cref="ArgumentException"/> if the map is not increasing over 
    /// the items of this collection (with respect to the two given comparison 
    /// relations).
    /// </summary>
    /// <param name="mapper">The delegate definging the map.</param>
    /// <param name="comparer">The comparion relation to use for the result.</param>
    /// <returns>The new sorted collection.</returns>
    IIndexedSorted<V> Map<V>(Fun<T, V> mapper, SCG.IComparer<V> comparer);
  }



  /// <summary>
  /// The type of a sorted collection with persistence
  /// </summary>
  public interface IPersistentSorted<T> : ISorted<T>, IDisposable
  {
    /// <summary>
    /// Make a (read-only) snap shot of this collection.
    /// </summary>
    /// <returns>The snap shot.</returns>
    ISorted<T> Snapshot();
  }



  /*************************************************************************/
  /// <summary>
  /// A dictionary with keys of type K and values of type V. Equivalent to a
  /// finite partial map from K to V.
  /// </summary>
  public interface IDictionary<K, V> : ICollectionValue<KeyValuePair<K, V>>, ICloneable
  {
    /// <summary>
    /// The key equalityComparer.
    /// </summary>
    /// <value></value>
    SCG.IEqualityComparer<K> EqualityComparer { get;}

    /// <summary>
    /// Indexer for dictionary.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if no entry is found. </exception>
    /// <value>The value corresponding to the key</value>
    V this[K key] { get; set;}


    /// <summary>
    /// 
    /// </summary>
    /// <value>True if dictionary is read-only</value>
    bool IsReadOnly { get;}


    /// <summary>
    /// 
    /// </summary>
    /// <value>A collection containg the all the keys of the dictionary</value>
    ICollectionValue<K> Keys { get;}


    /// <summary>
    /// 
    /// </summary>
    /// <value>A collection containing all the values of the dictionary</value>
    ICollectionValue<V> Values { get;}

    /// <summary>
    /// 
    /// </summary>
    /// <value>A delegate of type <see cref="T:C5.Fun`2"/> defining the partial function from K to V give by the dictionary.</value>
    Fun<K, V> Fun { get; }


    //TODO: resolve inconsistency: Add thows exception if key already there, AddAll ignores keys already There?
    /// <summary>
    /// Add a new (key, value) pair (a mapping) to the dictionary.
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException"> if there already is an entry with the same key. </exception>>
    /// <param name="key">Key to add</param>
    /// <param name="val">Value to add</param>
    void Add(K key, V val);

    /// <summary>
    /// Add the entries from a collection of <see cref="T:C5.KeyValuePair`2"/> pairs to this dictionary.
    /// </summary>
    /// <exception cref="DuplicateNotAllowedException"> 
    /// If the input contains duplicate keys or a key already present in this dictionary.</exception>
    /// <param name="entries"></param>
    void AddAll<U, W>(SCG.IEnumerable<KeyValuePair<U, W>> entries)
        where U : K
        where W : V
      ;

    /// <summary>
    /// The value is symbolic indicating the type of asymptotic complexity
    /// in terms of the size of this collection (worst-case or amortized as
    /// relevant). 
    /// <para>See <see cref="T:C5.Speed"/> for the set of symbols.</para>
    /// </summary>
    /// <value>A characterization of the speed of lookup operations
    /// (<code>Contains()</code> etc.) of the implementation of this dictionary.</value>
    Speed ContainsSpeed { get;}

    /// <summary>
    /// Check whether this collection contains all the values in another collection.
    /// If this collection has bag semantics (<code>AllowsDuplicates==true</code>)
    /// the check is made with respect to multiplicities, else multiplicities
    /// are not taken into account.
    /// </summary>
    /// <param name="items">The </param>
    /// <returns>True if all values in <code>items</code>is in this collection.</returns>
      bool ContainsAll<H>(SCG.IEnumerable<H> items) where H : K;

    /// <summary>
    /// Remove an entry with a given key from the dictionary
    /// </summary>
    /// <param name="key">The key of the entry to remove</param>
    /// <returns>True if an entry was found (and removed)</returns>
    bool Remove(K key);


    /// <summary>
    /// Remove an entry with a given key from the dictionary and report its value.
    /// </summary>
    /// <param name="key">The key of the entry to remove</param>
    /// <param name="val">On exit, the value of the removed entry</param>
    /// <returns>True if an entry was found (and removed)</returns>
    bool Remove(K key, out V val);


    /// <summary>
    /// Remove all entries from the dictionary
    /// </summary>
    void Clear();


    /// <summary>
    /// Check if there is an entry with a specified key
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <returns>True if key was found</returns>
    bool Contains(K key);


    /// <summary>
    /// Check if there is an entry with a specified key and report the corresponding
    /// value if found. This can be seen as a safe form of "val = this[key]".
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">On exit, the value of the entry</param>
    /// <returns>True if key was found</returns>
    bool Find(K key, out V val);

    /// <summary>
    /// Check if there is an entry with a specified key and report the corresponding
    /// value if found. This can be seen as a safe form of "val = this[key]".
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">On exit, the value of the entry</param>
    /// <returns>True if key was found</returns>
    bool Find(ref K key, out V val);


    /// <summary>
    /// Look for a specific key in the dictionary and if found replace the value with a new one.
    /// This can be seen as a non-adding version of "this[key] = val".
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">The new value</param>
    /// <returns>True if key was found</returns>
    bool Update(K key, V val);          //no-adding				    	


    /// <summary>
    /// Look for a specific key in the dictionary and if found replace the value with a new one.
    /// This can be seen as a non-adding version of "this[key] = val" reporting the old value.
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">The new value</param>
    /// <param name="oldval">The old value if any</param>
    /// <returns>True if key was found</returns>
    bool Update(K key, V val, out V oldval);          //no-adding				    	

    /// <summary>
    /// Look for a specific key in the dictionary. If found, report the corresponding value,
    /// else add an entry with the key and the supplied value.
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">On entry the value to add if the key is not found.
    /// On exit the value found if any.</param>
    /// <returns>True if key was found</returns>
    bool FindOrAdd(K key, ref V val);   //mixture


    /// <summary>
    /// Update value in dictionary corresponding to key if found, else add new entry.
    /// More general than "this[key] = val;" by reporting if key was found.
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">The value to add or replace with.</param>
    /// <returns>True if key was found and value updated.</returns>
    bool UpdateOrAdd(K key, V val);


    /// <summary>
    /// Update value in dictionary corresponding to key if found, else add new entry.
    /// More general than "this[key] = val;" by reporting if key was found.
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="val">The value to add or replace with.</param>
    /// <param name="oldval">The old value if any</param>
    /// <returns>True if key was found and value updated.</returns>
    bool UpdateOrAdd(K key, V val, out V oldval);


    /// <summary>
    /// Check the integrity of the internal data structures of this dictionary.
    /// Only avaliable in DEBUG builds???
    /// </summary>
    /// <returns>True if check does not fail.</returns>
    bool Check();
  }



  /// <summary>
  /// A dictionary with sorted keys.
  /// </summary>
  public interface ISortedDictionary<K, V> : IDictionary<K, V>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    new ISorted<K> Keys { get;}

    /// <summary>
    /// Find the current least item of this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The least item.</returns>
    KeyValuePair<K, V> FindMin();


    /// <summary>
    /// Remove the least item from this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The removed item.</returns>
    KeyValuePair<K, V> DeleteMin();


    /// <summary>
    /// Find the current largest item of this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The largest item.</returns>
    KeyValuePair<K, V> FindMax();


    /// <summary>
    /// Remove the largest item from this sorted collection.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if the collection is empty.</exception>
    /// <returns>The removed item.</returns>
    KeyValuePair<K, V> DeleteMax();

    /// <summary>
    /// The key comparer used by this dictionary.
    /// </summary>
    /// <value></value>
    SCG.IComparer<K> Comparer { get;}

    /// <summary>
    /// Find the entry in the dictionary whose key is the
    /// predecessor of the specified key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="res">The predecessor, if any</param>
    /// <returns>True if key has a predecessor</returns>
    bool TryPredecessor(K key, out KeyValuePair<K, V> res);

    /// <summary>
    /// Find the entry in the dictionary whose key is the
    /// successor of the specified key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="res">The successor, if any</param>
    /// <returns>True if the key has a successor</returns>
    bool TrySuccessor(K key, out KeyValuePair<K, V> res);

    /// <summary>
    /// Find the entry in the dictionary whose key is the
    /// weak predecessor of the specified key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="res">The predecessor, if any</param>
    /// <returns>True if key has a weak predecessor</returns>
    bool TryWeakPredecessor(K key, out KeyValuePair<K, V> res);

    /// <summary>
    /// Find the entry in the dictionary whose key is the
    /// weak successor of the specified key.
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="res">The weak successor, if any</param>
    /// <returns>True if the key has a weak successor</returns>
    bool TryWeakSuccessor(K key, out KeyValuePair<K, V> res);

    /// <summary>
    /// Find the entry with the largest key less than a given key.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if there is no such entry. </exception>
    /// <param name="key">The key to compare to</param>
    /// <returns>The entry</returns>
    KeyValuePair<K, V> Predecessor(K key);


    /// <summary>
    /// Find the entry with the least key greater than a given key.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if there is no such entry. </exception>
    /// <param name="key">The key to compare to</param>
    /// <returns>The entry</returns>
    KeyValuePair<K, V> Successor(K key);


    /// <summary>
    /// Find the entry with the largest key less than or equal to a given key.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if there is no such entry. </exception>
    /// <param name="key">The key to compare to</param>
    /// <returns>The entry</returns>
    KeyValuePair<K, V> WeakPredecessor(K key);


    /// <summary>
    /// Find the entry with the least key greater than or equal to a given key.
    /// </summary>
    /// <exception cref="NoSuchItemException"> if there is no such entry. </exception>
    /// <param name="key">The key to compare to</param>
    /// <returns>The entry</returns>
    KeyValuePair<K, V> WeakSuccessor(K key);

    /// <summary>
    /// Given a "cut" function from the items of the sorted collection to <code>int</code>
    /// whose only sign changes when going through items in increasing order
    /// can be 
    /// <list>
    /// <item>from positive to zero</item>
    /// <item>from positive to negative</item>
    /// <item>from zero to negative</item>
    /// </list>
    /// The "cut" function is supplied as the <code>CompareTo</code> method 
    /// of an object <code>c</code> implementing 
    /// <code>IComparable&lt;K&gt;</code>. 
    /// A typical example is the case where <code>K</code> is comparable and 
    /// <code>c</code> is itself of type <code>K</code>.
    /// <para>This method performs a search in the sorted collection for the ranges in which the
    /// "cut" function is negative, zero respectively positive. If <code>K</code> is comparable
    /// and <code>c</code> is of type <code>K</code>, this is a safe way (no exceptions thrown) 
    /// to find predecessor and successor of <code>c</code>.
    /// </para>
    /// <para> If the supplied cut function does not satisfy the sign-change condition, 
    /// the result of this call is undefined.
    /// </para>
    /// 
    /// </summary>
    /// <param name="cutFunction">The cut function <code>K</code> to <code>int</code>, given
    /// by the <code>CompareTo</code> method of an object implementing 
    /// <code>IComparable&lt;K&gt;</code>.</param>
    /// <param name="lowEntry">Returns the largest item in the collection, where the
    /// cut function is positive (if any).</param>
    /// <param name="lowIsValid">Returns true if the cut function is positive somewhere
    /// on this collection.</param>
    /// <param name="highEntry">Returns the least item in the collection, where the
    /// cut function is negative (if any).</param>
    /// <param name="highIsValid">Returns true if the cut function is negative somewhere
    /// on this collection.</param>
    /// <returns>True if the cut function is zero somewhere
    /// on this collection.</returns>
    bool Cut(IComparable<K> cutFunction, out KeyValuePair<K, V> lowEntry, out bool lowIsValid, out KeyValuePair<K, V> highEntry, out bool highIsValid);

    /// <summary>
    /// Query this sorted collection for items greater than or equal to a supplied value.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="bot">The lower bound (inclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<KeyValuePair<K, V>> RangeFrom(K bot);


    /// <summary>
    /// Query this sorted collection for items between two supplied values.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="lowerBound">The lower bound (inclusive).</param>
    /// <param name="upperBound">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<KeyValuePair<K, V>> RangeFromTo(K lowerBound, K upperBound);


    /// <summary>
    /// Query this sorted collection for items less than a supplied value.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <param name="top">The upper bound (exclusive).</param>
    /// <returns>The result directed collection.</returns>
    IDirectedEnumerable<KeyValuePair<K, V>> RangeTo(K top);


    /// <summary>
    /// Create a directed collection with the same items as this collection.
    /// <para>The returned collection is not a copy but a view into the collection.</para>
    /// <para>The view is fragile in the sense that changes to the underlying collection will 
    /// invalidate the view so that further operations on the view throws InvalidView exceptions.</para>
    /// </summary>
    /// <returns>The result directed collection.</returns>
    IDirectedCollectionValue<KeyValuePair<K, V>> RangeAll();


    //TODO: remove now that we assume that we can check the sorting order?
    /// <summary>
    /// Add all the items from another collection with an enumeration order that 
    /// is increasing in the items.
    /// </summary>
    /// <exception cref="ArgumentException"> if the enumerated items turns out
    /// not to be in increasing order.</exception>
    /// <param name="items">The collection to add.</param>
    void AddSorted(SCG.IEnumerable<KeyValuePair<K, V>> items);


    /// <summary>
    /// Remove all items of this collection above or at a supplied threshold.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    void RemoveRangeFrom(K low);


    /// <summary>
    /// Remove all items of this collection between two supplied thresholds.
    /// </summary>
    /// <param name="low">The lower threshold (inclusive).</param>
    /// <param name="hi">The upper threshold (exclusive).</param>
    void RemoveRangeFromTo(K low, K hi);


    /// <summary>
    /// Remove all items of this collection below a supplied threshold.
    /// </summary>
    /// <param name="hi">The upper threshold (exclusive).</param>
    void RemoveRangeTo(K hi);
  }



  /*******************************************************************/
  /*/// <summary>
  /// The type of an item comparer
  /// <i>Implementations of this interface must asure that the method is self-consistent
  /// and defines a sorting order on items, or state precise conditions under which this is true.</i>
  /// <i>Implementations <b>must</b> assure that repeated calls of
  /// the method to the same (in reference or binary identity sense) arguments 
  /// will return values with the same sign (-1, 0 or +1), or state precise conditions
  /// under which the user 
  /// can be assured repeated calls will return the same sign.</i>
  /// <i>Implementations of this interface must always return values from the method
  /// and never throw exceptions.</i>
  /// <i>This interface is identical to System.Collections.Generic.IComparer&lt;T&gt;</i>
  /// </summary>
  public interface IComparer<T>
  {
    /// <summary>
    /// Compare two items with respect to this item comparer
    /// </summary>
    /// <param name="item1">First item</param>
    /// <param name="item2">Second item</param>
    /// <returns>Positive if item1 is greater than item2, 0 if they are equal, negative if item1 is less than item2</returns>
    int Compare(T item1, T item2);
  }

  /// <summary>
  /// The type of an item equalityComparer. 
  /// <i>Implementations of this interface <b>must</b> assure that the methods are 
  /// consistent, that is, that whenever two items i1 and i2 satisfies that Equals(i1,i2)
  /// returns true, then GetHashCode returns the same value for i1 and i2.</i>
  /// <i>Implementations of this interface <b>must</b> assure that repeated calls of
  /// the methods to the same (in reference or binary identity sense) arguments 
  /// will return the same values, or state precise conditions under which the user 
  /// can be assured repeated calls will return the same values.</i>
  /// <i>Implementations of this interface must always return values from the methods
  /// and never throw exceptions.</i>
  /// <i>This interface is similar in function to System.IKeyComparer&lt;T&gt;</i>
  /// </summary>
  public interface SCG.IEqualityComparer<T>
  {
    /// <summary>
    /// Get the hash code with respect to this item equalityComparer
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The hash code</returns>
    int GetHashCode(T item);


    /// <summary>
    /// Check if two items are equal with respect to this item equalityComparer
    /// </summary>
    /// <param name="item1">first item</param>
    /// <param name="item2">second item</param>
    /// <returns>True if equal</returns>
    bool Equals(T item1, T item2);
  }*/
}
