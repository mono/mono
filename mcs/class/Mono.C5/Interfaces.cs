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
using MSG = System.Collections.Generic;
namespace C5
{
	/*************************************************************************/
    //TODO: use the MS defs fro m MSG if any?
	/// <summary>
	/// A generic delegate that when invoked performs some operation
	/// on it T argument.
	/// </summary>
	public delegate void Applier<T>(T t);



	/// <summary>
	/// A generic delegate whose invocation constitutes a map from T to V.
	/// </summary>
	public delegate V Mapper<T,V>(T item);



	/// <summary>
	/// A generic delegate that when invoked on a T item returns a boolean
	/// value -- i.e. a T predicate.
	/// </summary> 
	public delegate bool Filter<T>(T item);



	/************************************************************************
	/// <summary>
	/// A generic collection that may be enumerated. This is the coarsest interface
	/// for main stream generic collection classes (as opposed to priority queues).
	/// It can also be the result of a query operation on another collection 
	/// (where the result size is not easily computable, in which case the result
	/// could have been an <code>ICollectionValue&lt;T&gt;</code>).
	/// </summary>
	public interface noIEnumerable<T>
	{
		/// <summary>
		/// Create an enumerator for the collection
		/// </summary>
		/// <returns>The enumerator(SIC)</returns>
		MSG.IEnumerator<T> GetEnumerator();
	}
    */


	/// <summary>
	/// A generic collection, that can be enumerated backwards.
	/// </summary>
	public interface IDirectedEnumerable<T>: MSG.IEnumerable<T>
	{
		/// <summary>
		/// Create a collection containing the same items as this collection, but
		/// whose enumerator will enumerate the items backwards. The new collection
		/// will become invalid if the original is modified. Method typicaly used as in
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
	public interface ICollectionValue<T>: MSG.IEnumerable<T>
	{
		/// <summary>
		/// 
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
		/// <param name="a">The array to copy to</param>
		/// <param name="i">The index at which to copy the first item</param>
		void CopyTo(T[] a, int i);

        /// <summary>
        /// Create an array with the items of this collection (in the same order as an
        /// enumerator would output them).
        /// </summary>
        /// <returns>The array</returns>
        T[] ToArray();

        /// <summary>
        /// Apply a delegate to all items of this collection.
        /// </summary>
        /// <param name="a">The delegate to apply</param>
        void Apply(Applier<T> a);


        /// <summary>
        /// Check if there exists an item  that satisfies a
        /// specific predicate in this collection.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True is such an item exists</returns>
        bool Exists(Filter<T> filter);


        /// <summary>
        /// Check if all items in this collection satisfies a specific predicate.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True if all items satisfies the predicate</returns>
        bool All(Filter<T> filter);
    }



	/// <summary>
	/// A sized generic collection, that can be enumerated backwards.
	/// </summary>
	public interface IDirectedCollectionValue<T>: ICollectionValue<T>, IDirectedEnumerable<T>
	{
		/// <summary>
		/// Create a collection containing the same items as this collection, but
		/// whose enumerator will enumerate the items backwards. The new collection
		/// will become invalid if the original is modified. Method typicaly used as in
		/// <code>foreach (T x in coll.Backwards()) {...}</code>
		/// </summary>
		/// <returns>The backwards collection.</returns>
		new IDirectedCollectionValue<T> Backwards();
	}



	/// <summary>
	/// A generic collection to which one may add items. This is just the intersection
	/// of the main stream generic collection interfaces and the priority queue interface,
	/// <see cref="T:C5.ICollection!1"/> and <see cref="T:C5.IPriorityQueue!1"/>.
	/// </summary>
	public interface IExtensible<T> : ICollectionValue<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <value>False if this collection has set semantics, true if bag semantics.</value>
		bool AllowsDuplicates { get;}


		/// <summary>
		/// 
		/// </summary>
		/// <value>An object to be used for locking to enable multi threaded code
		/// to acces this collection safely.</value>
		object SyncRoot { get;}


		/// <summary>
		/// 
		/// </summary>
		/// <value>True if this collection is empty.</value>
		bool IsEmpty { get;}


		/// <summary>
		/// Add an item to this collection if possible. If this collection has set
		/// semantics, the item will be added if not already in the collection. If
		/// bag semantics, the item will always be added.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True if item was added.</returns>
		bool Add(T item);


		/// <summary>
		/// Add the elements from another collection to this collection. If this
		/// collection has set semantics, only items not already in the collection
		/// will be added.
		/// </summary>
		/// <param name="items">The items to add.</param>
		void AddAll(MSG.IEnumerable<T> items);

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. If this
        /// collection has set semantics, only items not already in the collection
        /// will be added.
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        void AddAll<U>(MSG.IEnumerable<U> items) where U : T;

		//void Clear(); // for priority queue
		//int Count why not?
		/// <summary>
		/// Check the integrity of the internal data structures of this collection.
		/// <p>This is only relevant for developers of the library</p>
		/// </summary>
		/// <returns>True if check was passed.</returns>
		bool Check();
	}



	/// <summary>
	/// The symbolic characterization of the speed of lookups for a collection.
	/// The values may refer to worst-case, amortized and/or expected asymtotic 
	/// complexity wrt. the collection size.
	/// </summary>
	public enum Speed: short
	{
        /// <summary>
        /// Counting the collection with the <code>Count property</code> may not return
        /// (for a synthetic and potentially infinite collection).
        /// </summary>
        PotentiallyInfinite = 1,
        /// <summary>
        /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
        /// property may take time O(n),
        /// where n is the size of the collection.
        /// </summary>
        Linear = 2,
        /// <summary>
        /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
        /// property  takes time O(log n),
        /// where n is the size of the collection.
		/// </summary>
		Log = 3,
		/// <summary>
        /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
        /// property  takes time O(1),
        /// where n is the size of the collection.
		/// </summary>
		Constant = 4
	}



	//TODO: add ItemHasher to interface by making it an IHasher<T>? No, add Comparer property!
	/// <summary>
	/// The simplest interface of a main stream generic collection
	/// with lookup, insertion and removal operations. 
	/// </summary>
	public interface ICollection<T>: IExtensible<T>
	{
		/// <summary>
		/// If true any call of an updating operation will throw an
		/// <code>InvalidOperationException</code>
		/// </summary>
		/// <value>True if this collection is read only.</value>
		bool IsReadOnly { get;}


		//This is somewhat similar to the RandomAccess marker itf in java
		/// <summary>
		/// The value is symbolic indicating the type of asymptotic complexity
		/// in terms of the size of this collection (worst-case or amortized as
		/// relevant).
		/// </summary>
		/// <value>A characterization of the speed of lookup operations
		/// (<code>Contains()</code> etc.) of the implementation of this list.</value>
		Speed ContainsSpeed { get;}


		/// <summary>
		/// The hashcode is defined as the sum of <code>h(item)</code> over the items
		/// of the collection, where the function <code>h</code> is??? 
		/// </summary>
		/// <returns>The unordered hashcode of this collection.</returns>
		int GetHashCode();


		/// <summary>
		/// Compare the contents of this collection to another one without regards to
		/// the sequence order. The comparison will use this collection's itemhasher
		/// to compare individual items.
		/// </summary>
		/// <param name="that">The collection to compare to.</param>
		/// <returns>True if this collection and that contains the same items.</returns>
		bool Equals(ICollection<T> that);


		/// <summary>
		/// Check if this collection contains (an item equivalent to according to the
		/// itemhasher) a particular value.
		/// </summary>
		/// <param name="item">The value to check for.</param>
		/// <returns>True if the items is in this collection.</returns>
		bool Contains(T item);


		/// <summary>
		/// Count the number of items of the collection equal to a particular value.
		/// Returns 0 if and only if the value is not in the collection.
		/// </summary>
		/// <param name="item">The value to count.</param>
		/// <returns>The number of copies found.</returns>
		int ContainsCount(T item);


		/// <summary>
		/// Check if this collection contains all the values in another collection.
		/// If this collection has bag semantics (<code>NoDuplicates==false</code>)
		/// the check is made with respect to multiplicities, else multiplicities
		/// are not taken into account.
		/// </summary>
		/// <param name="items">The </param>
		/// <returns>True if all values in <code>items</code>is in this collection.</returns>
		bool ContainsAll(MSG.IEnumerable<T> items);


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the items is in this collection.</returns>
		bool Find(ref T item);


		//This should probably just be bool Add(ref T item); !!!
		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, return in the ref argument (a
		/// binary copy of) the actual value found. Else, add the item to the collection.
		/// </summary>
		/// <param name="item">The value to look for.</param>
		/// <returns>True if the item was found (hence not added).</returns>
		bool FindOrAdd(ref T item);


		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value. If the collection has bag semantics,
		/// it is implementation dependent if this updates all equivalent copies in
		/// the collection or just one.
		/// </summary>
		/// <param name="item">Value to update.</param>
		/// <returns>True if the item was found and hence updated.</returns>
		bool Update(T item);


		//Better to call this AddOrUpdate since the return value fits that better
		//OTOH for a bag the update would be better be the default!!!
		/// <summary>
		/// Check if this collection contains an item equivalent according to the
		/// itemhasher to a particular value. If so, update the item in the collection 
		/// to with a binary copy of the supplied value; else add the value to the collection. 
		/// </summary>
		/// <param name="item">Value to add or update.</param>
		/// <returns>True if the item was found and updated (hence not added).</returns>
		bool UpdateOrAdd(T item);


		/// <summary>
		/// Remove a particular item from this collection. If the collection has bag
		/// semantics only one copy equivalent to the supplied item is removed. 
		/// </summary>
		/// <param name="item">The value to remove.</param>
		/// <returns>True if the item was found (and removed).</returns>
		bool Remove(T item);


		//CLR will allow us to let this be bool Remove(ref T item); !!!
		/// <summary>
		/// Remove a particular item from this collection if found. If the collection
		/// has bag semantics only one copy equivalent to the supplied item is removed,
		/// which one is implementation dependent. 
		/// If an item was removed, report a binary copy of the actual item removed in 
		/// the argument.
		/// </summary>
		/// <param name="item">The value to remove on input.</param>
		/// <returns>True if the item was found (and removed).</returns>
		bool RemoveWithReturn(ref T item);


		/// <summary>
		/// Remove all items equivalent to a given value.
		/// </summary>
		/// <param name="item">The value to remove.</param>
		void RemoveAllCopies(T item);


		/// <summary>
		/// Remove all items in another collection from this one. If this collection
		/// has bag semantics, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		void RemoveAll(MSG.IEnumerable<T> items);


		/// <summary>
		/// Remove all items from this collection.
		/// </summary>
		void Clear();


		/// <summary>
		/// Remove all items not in some other collection from this one. If this collection
		/// has bag semantics, take multiplicities into account.
		/// </summary>
		/// <param name="items">The items to retain.</param>
		void RetainAll(MSG.IEnumerable<T> items);

		//IDictionary<T> UniqueItems()
	}



	/// <summary>
	/// An editable collection maintaining a definite sequence order of the items.
	///
	/// <p>Implementations of this interface must compute the hash code and 
	/// equality exactly as prescribed in the method definitions in order to
	/// be consistent with other collection classes implementing this interface.</p>
	/// <p>This interface is usually implemented by explicit interface implementation,
	/// not as ordinary virtual methods.</p>
	/// </summary>
	public interface ISequenced<T>: ICollection<T>, IDirectedCollectionValue<T>
	{
		/// <summary>
		/// The hashcode is defined as <code>h(...h(h(x1),x2)...,xn)</code> for
		/// <code>h(a,b)=31*a+b</code> and the x's the hash codes of 
		/// </summary>
		/// <returns>The sequence order hashcode of this collection.</returns>
		new int GetHashCode();


		/// <summary>
		/// Compare this sequenced collection to another one in sequence order.
		/// </summary>
		/// <param name="that">The sequenced collection to compare to.</param>
		/// <returns>True if this collection and that contains equal (according to
		/// this collection's itemhasher) in the same sequence order.</returns>
		bool Equals(ISequenced<T> that);
	}



	/// <summary>
	/// A sequenced collection, where indices of items in the order are maintained
	/// </summary>
	public interface IIndexed<T>: ISequenced<T>
	{
		/// <summary>
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <value>The i'th item of this list.</value>
		/// <param name="i">the index to lookup</param>
		T this[int i] { get;}


		/// <summary>
		/// <exception cref="IndexOutOfRangeException"/>.
		/// </summary>
		/// <value>The directed collection of items in a specific index interval.</value>
		/// <param name="start">The low index of the interval (inclusive).</param>
		/// <param name="count">The size of the range.</param>
		IDirectedCollectionValue<T> this[int start, int count] { get;}


		/// <summary>
		/// Searches for an item in the list going forwrds from the start.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of item from start.</returns>
		int IndexOf(T item);


		/// <summary>
		/// Searches for an item in the list going backwords from the end.
		/// </summary>
		/// <param name="item">Item to search for.</param>
		/// <returns>Index of of item from the end.</returns>
		int LastIndexOf(T item);


		/// <summary>
		/// Remove the item at a specific position of the list.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <param name="i">The index of the item to remove.</param>
		/// <returns>The removed item.</returns>
		T RemoveAt(int i);


		/// <summary>
		/// Remove all items in an index interval.
		/// <exception cref="IndexOutOfRangeException"/>???. 
		/// </summary>
		/// <param name="start">The index of the first item to remove.</param>
		/// <param name="count">The number of items to remove.</param>
		void RemoveInterval(int start, int count);
	}

    //TODO: decide if this should extend ICollection
    /// <summary>
    /// The interface describing the operations of a LIFO stack data structure.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public interface IStack<T>
    {
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

    //TODO: decide if this should extend ICollection
    /// <summary>
    /// The interface describing the operations of a FIFO queue data structure.
    /// </summary>
    /// <typeparam name="T">The item type</typeparam>
    public interface IQueue<T>
    {
        /// <summary>
        /// Enqueue an item at the back of the queue. 
        /// </summary>
        /// <param name="item">The item</param>
        void EnQueue(T item);
        /// <summary>
        /// Dequeue an item from the front of the queue.
        /// </summary>
        /// <returns>The item</returns>
        T DeQueue();
    }


	/// <summary>
	/// This is an indexed collection, where the item order is chosen by 
	/// the user at insertion time.
	///
	/// NBNBNB: we neeed a description of the view functionality here!
	/// </summary>
	public interface IList<T>: IIndexed<T>, IStack<T>, IQueue<T>
	{
		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <value>The first item in this list.</value>
		T First { get;}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
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
		/// On this list, this indexer is read/write.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt;= the size of the collection.
		/// </summary>
		/// <value>The i'th item of this list.</value>
		/// <param name="i">The index of the item to fetch or store.</param>
		new T this[int i] { get; set;}


		/// <summary>
		/// Insert an item at a specific index location in this list. 
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.</summary>
		/// <exception cref="InvalidOperationException"/> if the list has
		/// <code>NoDuplicates=true</code> and the item is 
		/// already in the list.
		/// <param name="i">The index at which to insert.</param>
		/// <param name="item">The item to insert.</param>
		void Insert(int i, T item);


		/// <summary>
		/// Insert an item at the front of this list.
		/// <exception cref="InvalidOperationException"/> if the list has
		/// <code>NoDuplicates=true</code> and the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		void InsertFirst(T item);


		/// <summary>
		/// Insert an item at the back of this list.
		/// <exception cref="InvalidOperationException"/> if the list has
		/// <code>NoDuplicates=true</code> and the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		void InsertLast(T item);


		/// <summary>
		/// Insert an item right before the first occurrence of some target item.
		/// <exception cref="InvalidOperationException"/> if target	is not found
		/// or if the list has <code>NoDuplicates=true</code> and the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target before which to insert.</param>
		void InsertBefore(T item, T target);


		/// <summary>
		/// Insert an item right after the last(???) occurrence of some target item.
		/// <exception cref="InvalidOperationException"/> if target	is not found
		/// or if the list has <code>NoDuplicates=true</code> and the item is 
		/// already in the list.
		/// </summary>
		/// <param name="item">The item to insert.</param>
		/// <param name="target">The target after which to insert.</param>
		void InsertAfter(T item, T target);


		/// <summary>
		/// Insert into this list all items from an enumerable collection starting 
		/// at a particular index.
		/// <exception cref="IndexOutOfRangeException"/> if i is negative or
		/// &gt; the size of the collection.
		/// <exception cref="InvalidOperationException"/> if the list has 
		/// <code>NoDuplicates=true</code> and one of the items to insert is
		/// already in the list.
		/// </summary>
		/// <param name="i">Index to start inserting at</param>
		/// <param name="items">Items to insert</param>
		void InsertAll(int i, MSG.IEnumerable<T> items);


		/// <summary>
		/// Create a new list consisting of the items of this list satisfying a 
		/// certain predicate.
		/// </summary>
		/// <param name="filter">The filter delegate defining the predicate.</param>
		/// <returns>The new list.</returns>
		IList<T> FindAll(Filter<T> filter);


		/// <summary>
		/// Create a new list consisting of the results of mapping all items of this
		/// list. The new list will use the default hasher for the item type V.
		/// </summary>
		/// <typeparam name="V">The type of items of the new list</typeparam>
		/// <param name="mapper">The delegate defining the map.</param>
		/// <returns>The new list.</returns>
		IList<V> Map<V>(Mapper<T,V> mapper);

        /// <summary>
        /// Create a new list consisting of the results of mapping all items of this
		/// list. The new list will use a specified hasher for the item type.
        /// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The delegate defining the map.</param>
        /// <param name="hasher">The hasher to use for the new list</param>
        /// <returns>The new list.</returns>
        IList<V> Map<V>(Mapper<T, V> mapper, IHasher<V> hasher);
        
        /// <summary>
        /// Remove one item from the list: from the front if <code>FIFO</code>
		/// is true, else from the back.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		T Remove();


		/// <summary>
		/// Remove one item from the fromnt of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
		/// </summary>
		/// <returns>The removed item.</returns>
		T RemoveFirst();


		/// <summary>
		/// Remove one item from the back of the list.
		/// <exception cref="InvalidOperationException"/> if this list is empty.
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
		/// Null if this list is not a view.
		/// </summary>
        /// <value>Underlying list for view.</value>
        IList<T> Underlying { get;}


		/// <summary>
		/// </summary>
        /// <value>Offset for this list view or 0 for an underlying list.</value>
        int Offset { get;}


		/// <summary>
		/// Slide this list view along the underlying list.
		/// <exception cref="InvalidOperationException"/> if this list is not a view.
		/// <exception cref="ArgumentOutOfRangeException"/> if the operation
		/// would bring either end of the view outside the underlying list.
		/// </summary>
		/// <param name="offset">The signed amount to slide: positive to slide
		/// towards the end.</param>
		void Slide(int offset);


		/// <summary>
		/// Slide this list view along the underlying list, changing its size.
		/// <exception cref="InvalidOperationException"/> if this list is not a view.
		/// <exception cref="ArgumentOutOfRangeException"/> if the operation
		/// would bring either end of the view outside the underlying list.
		/// </summary>
		/// <param name="offset">The signed amount to slide: positive to slide
		/// towards the end.</param>
		/// <param name="size">The new size of the view.</param>
		void Slide(int offset, int size);


		/// <summary>
		/// Reverse the list so the items are in the opposite sequence order.
		/// </summary>
		void Reverse();


		/// <summary>
		/// Reverst part of the list so the items are in the opposite sequence order.
		/// <exception cref="ArgumentException"/> if the count is negative.
		/// <exception cref="ArgumentOutOfRangeException"/> if the part does not fit
		/// into the list.
		/// </summary>
		/// <param name="start">The index of the start of the part to reverse.</param>
		/// <param name="count">The size of the part to reverse.</param>
		void Reverse(int start, int count);


		//NaturalSort for comparable items?
		/// <summary>
		/// Check if this list is sorted according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		/// <returns>True if the list is sorted, else false.</returns>
		bool IsSorted(IComparer<T> c);


		/// <summary>
		/// Sort the items of the list according to a specific sorting order.
		/// </summary>
		/// <param name="c">The comparer defining the sorting order.</param>
		void Sort(IComparer<T> c);


		/// <summary>
		/// Randonmly shuffle the items of this list. 
		/// </summary>
		void Shuffle();


		/// <summary>
		/// Shuffle the items of this list according to a specific random source.
		/// </summary>
		/// <param name="rnd">The random source.</param>
		void Shuffle(Random rnd);
	}


    /// <summary>
	/// A generic collection of items prioritized by a comparison (order) relation.
	/// Supports adding items and reporting or removing extremal elements. 
	/// The priority queue itself exports the used
	/// order relation through its implementation of <code>IComparer&lt;T&gt;</code>
	/// </summary>
	public interface IPriorityQueue<T>: IExtensible<T>
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
		/// Remove the largest item from this  priority queue.
		/// </summary>
		/// <returns>The removed item.</returns>
		T DeleteMax();

        /// <summary>
        /// The comparer object supplied at creation time for this collection
        /// </summary>
        /// <value>The comparer</value>
        IComparer<T> Comparer { get;}
    }



	/// <summary>
	/// A collection where items are maintained in sorted order.
	/// </summary>
	public interface ISorted<T>: ISequenced<T>, IPriorityQueue<T>
	{
		/// <summary>
		/// Find the strict predecessor in the sorted collection of a particular value,
		/// i.e. the largest item in the collection less than the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is less than or equal to the minimum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the predecessor for.</param>
		/// <returns>The predecessor.</returns>
		T Predecessor(T item);


		/// <summary>
		/// Find the strict successor in the sorted collection of a particular value,
		/// i.e. the least item in the collection greater than the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is greater than or equal to the maximum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the successor for.</param>
		/// <returns>The successor.</returns>
		T Successor(T item);


		/// <summary>
		/// Find the weak predecessor in the sorted collection of a particular value,
		/// i.e. the largest item in the collection less than or equal to the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is less than the minimum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the weak predecessor for.</param>
		/// <returns>The weak predecessor.</returns>
		T WeakPredecessor(T item);


		/// <summary>
		/// Find the weak successor in the sorted collection of a particular value,
		/// i.e. the least item in the collection greater than or equal to the supplied value.
		/// <exception cref="InvalidOperationException"/> if no such element exists (the
		/// supplied  value is greater than the maximum of this collection.)
		/// </summary>
		/// <param name="item">The item to find the weak successor for.</param>
		/// <returns>The weak successor.</returns>
		T WeakSuccessor(T item);


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
		bool Cut(IComparable<T> c, out T low, out bool lowIsValid, out T high, out bool highIsValid);


		/// <summary>
		/// Query this sorted collection for items greater than or equal to a supplied value.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive).</param>
		/// <returns>The result directed collection.</returns>
		IDirectedEnumerable<T> RangeFrom(T bot);


		/// <summary>
		/// Query this sorted collection for items between two supplied values.
		/// </summary>
		/// <param name="bot">The lower bound (inclusive).</param>
		/// <param name="top">The upper bound (exclusive).</param>
		/// <returns>The result directed collection.</returns>
		IDirectedEnumerable<T> RangeFromTo(T bot, T top);


		/// <summary>
		/// Query this sorted collection for items less than a supplied value.
		/// </summary>
		/// <param name="top">The upper bound (exclusive).</param>
		/// <returns>The result directed collection.</returns>
		IDirectedEnumerable<T> RangeTo(T top);


		/// <summary>
		/// Create a directed collection with the same items as this collection.
		/// </summary>
		/// <returns>The result directed collection.</returns>
		IDirectedCollectionValue<T> RangeAll();


		/// <summary>
		/// Add all the items from another collection with an enumeration order that 
		/// is increasing in the items.
		/// <exception cref="ArgumentException"/> if the enumerated items turns out
		/// not to be in increasing order.
		/// </summary>
		/// <param name="items">The collection to add.</param>
		void AddSorted(MSG.IEnumerable<T> items);


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
	public interface IIndexedSorted<T>: ISorted<T>, IIndexed<T>
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
		/// <param name="f">The filter delegate defining the predicate.</param>
		/// <returns>The new indexed sorted collection.</returns>
		IIndexedSorted<T> FindAll(Filter<T> f);


		/// <summary>
		/// Create a new indexed sorted collection consisting of the results of
		/// mapping all items of this list.
		/// <exception cref="ArgumentException"/> if the map is not increasing over 
		/// the items of this collection (with respect to the two given comparison 
		/// relations).
		/// </summary>
		/// <param name="m">The delegate definging the map.</param>
		/// <param name="c">The comparion relation to use for the result.</param>
		/// <returns>The new sorted collection.</returns>
		IIndexedSorted<V> Map<V>(Mapper<T,V> m, IComparer<V> c);
	}



	/// <summary>
	/// The type of a sorted collection with persistence
	/// </summary>
	public interface IPersistentSorted<T>: ISorted<T>, IDisposable
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
	public interface IDictionary<K,V>: MSG.IEnumerable<KeyValuePair<K,V>>
	{
		/// <summary>
		/// Indexer for dictionary.
		/// <exception cref="InvalidOperationException"/> if no entry is found. 
		/// </summary>
		/// <value>The value corresponding to the key</value>
		V this[K key] { get; set;}


		/// <summary>
		/// 
		/// </summary>
		/// <value>The number of entrues in the dictionary</value>
		int Count { get; }


		/// <summary>
		/// 
		/// </summary>
		/// <value>True if dictionary is read  only</value>
		bool IsReadOnly { get;}


		/// <summary>
		/// 
		/// </summary>
		/// <value>The distinguished object to use for locking to synchronize multithreaded access</value>
		object SyncRoot { get;}


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
		/// Add a new (key, value) pair (a mapping) to the dictionary.
		/// <exception cref="InvalidOperationException"/> if there already is an entry with the same key. 
		/// </summary>
		/// <param name="key">Key to add</param>
		/// <param name="val">Value to add</param>
		void Add(K key, V val);


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
		/// Look for a specific key in the dictionary and if found replace the value with a new one.
		/// This can be seen as a non-adding version of "this[key] = val".
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <param name="val">The new value</param>
		/// <returns>True if key was found</returns>
		bool Update(K key, V val);          //no-adding				    	


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
		/// Check the integrity of the internal data structures of this dictionary.
		/// Only avaliable in DEBUG builds???
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		bool Check();
	}



	/// <summary>
	/// A dictionary with sorted keys.
	/// </summary>
	public interface ISortedDictionary<K,V>: IDictionary<K,V>
	{
		/// <summary>
		/// Find the entry with the largest key less than a given key.
		/// <exception cref="InvalidOperationException"/> if there is no such entry. 
		/// </summary>
		/// <param name="key">The key to compare to</param>
		/// <returns>The entry</returns>
		KeyValuePair<K,V> Predecessor(K key);


		/// <summary>
		/// Find the entry with the least key greater than a given key.
		/// <exception cref="InvalidOperationException"/> if there is no such entry. 
		/// </summary>
		/// <param name="key">The key to compare to</param>
		/// <returns>The entry</returns>
		KeyValuePair<K,V> Successor(K key);


		/// <summary>
		/// Find the entry with the largest key less than or equal to a given key.
		/// <exception cref="InvalidOperationException"/> if there is no such entry. 
		/// </summary>
		/// <param name="key">The key to compare to</param>
		/// <returns>The entry</returns>
		KeyValuePair<K,V> WeakPredecessor(K key);


		/// <summary>
		/// Find the entry with the least key greater than or equal to a given key.
		/// <exception cref="InvalidOperationException"/> if there is no such entry. 
		/// </summary>
		/// <param name="key">The key to compare to</param>
		/// <returns>The entry</returns>
		KeyValuePair<K,V> WeakSuccessor(K key);
	}



	/*******************************************************************/
	/// <summary>
	/// The type of an item comparer
	/// <p>Implementations of this interface must asure that the method is self-consistent
	/// and defines a sorting order on items, or state precise conditions under which this is true.</p>
	/// <p>Implementations <b>must</b> assure that repeated calls of
	/// the method to the same (in reference or binary identity sense) arguments 
	/// will return values with the same sign (-1, 0 or +1), or state precise conditions
	/// under which the user 
	/// can be assured repeated calls will return the same sign.</p>
	/// <p>Implementations of this interface must always return values from the method
	/// and never throw exceptions.</p>
	/// <p>This interface is identical to System.Collections.Generic.IComparer&lt;T&gt;</p>
	/// </summary>
	public interface IComparer<T>
	{
		/// <summary>
		/// Compare two items with respect to this item comparer
		/// </summary>
		/// <param name="a">First item</param>
		/// <param name="b">Second item</param>
		/// <returns>Positive if a is greater than b, 0 if they are equal, negative if a is less than b</returns>
		int Compare(T a, T b);
	}


    /*
	/// <summary>
	/// The interface for an item that is generic comparable.
	/// <p>Implementations of this interface <b>must</b> assure that repeated calls of
	/// the method to the same (in reference or binary identity sense) object and argument
	/// will return values with the same sign (-1, 0 or +1), or state precise conditions
	/// under which the user 
	/// can be assured repeated calls will return the same sign.</p>
	/// <p>Implementations of this interface must always return values from the method
	/// and never throw exceptions.</p>
	/// <p>This interface is identical to System.Collections.Generic.IComparable&lt;T&gt;</p>
	/// </summary>
	public interface unIComparable<T> //: System.IComparable<T>
	{
		/// <summary>
		/// Compare this item to another one
		/// </summary>
		/// <param name="that">The other item</param>
		/// <returns>Positive if this is greater than , 0 if equal to, negative if less than that</returns>
		int CompareTo(T that);
	}
    */


	/// <summary>
	/// The type of an item hasher. 
	/// <p>Implementations of this interface <b>must</b> assure that the methods are 
	/// consistent, i.e. that whenever two items i1 and i2 satisfies that Equals(i1,i2)
	/// returns true, then GetHashCode returns the same values for i1 and i2.</p>
	/// <p>Implementations of this interface <b>must</b> assure that repeated calls of
	/// the methods to the same (in reference or binary identity sense) arguments 
	/// will return the same values, or state precise conditions under which the user 
	/// can be assured repeated calls will return the same values.</p>
	/// <p>Implementations of this interface must always return values from the methods
	/// and never throw exceptions.</p>
	/// <p>This interface is similar in function to System.IKeyComparer&lt;T&gt;</p>
	/// </summary>
	public interface IHasher<T>
	{
		/// <summary>
		/// Get the hash code with respect to this item hasher
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The hash code</returns>
		int GetHashCode(T item);


		/// <summary>
		/// Check if two items are equal with respect to this item hasher
		/// </summary>
		/// <param name="i1">first item</param>
		/// <param name="i2">second item</param>
		/// <returns>True if equal</returns>
		bool Equals(T i1, T i2);
	}
}