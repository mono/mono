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
	/// A read-only wrapper class for a generic enumerator
	/// </summary>
	public class GuardedEnumerator<T>: MSG.IEnumerator<T>
	{
		#region Fields

		MSG.IEnumerator<T> enumerator;

		#endregion

		#region Constructor

		/// <summary>
		/// Create a wrapper around a generic enumerator
		/// </summary>
		/// <param name="enumerator">The enumerator to wrap</param>
		public GuardedEnumerator(MSG.IEnumerator<T> enumerator)
		{ this.enumerator = enumerator; }

		#endregion

		#region IEnumerator<T> Members

		/// <summary>
		/// Move wrapped enumerator to next item, or the first item if
		/// this is the first call to MoveNext. 
		/// </summary>
		/// <returns>True if enumerator is valid now</returns>
		public bool MoveNext() { return enumerator.MoveNext(); }


		/// <summary>
		/// Undefined if enumerator is not valid (MoveNext hash been called returning true)
		/// </summary>
		/// <value>The current item of the wrapped enumerator.</value>
		public T Current { get { return enumerator.Current; } }

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose wrapped enumerator
		/// </summary>
		public void Dispose() { enumerator.Dispose(); }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper class for a generic enumerable
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedEnumerable<T>: MSG.IEnumerable<T>
	{
		#region Fields

		MSG.IEnumerable<T> enumerable;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap an enumerable in a read-only wrapper
		/// </summary>
		/// <param name="enumerable">The enumerable to wrap</param>
		public GuardedEnumerable(MSG.IEnumerable<T> enumerable)
		{ this.enumerable = enumerable; }

		#endregion

		#region MSG.IEnumerable<T> Members

		/// <summary>
		/// Get an enumerator from the wrapped enumerable
		/// </summary>
		/// <returns>The enumerator (itself wrapped)</returns>
		public MSG.IEnumerator<T> GetEnumerator()
		{ return new GuardedEnumerator<T>(enumerable.GetEnumerator()); }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for a generic directed enumerable
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedDirectedEnumerable<T>: GuardedEnumerable<T>, IDirectedEnumerable<T>
	{
		#region Fields

		IDirectedEnumerable<T> directedenumerable;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a directed enumerable in a read-only wrapper
		/// </summary>
		/// <param name="directedenumerable">the collection to wrap</param>
		public GuardedDirectedEnumerable(IDirectedEnumerable<T> directedenumerable)
			: base(directedenumerable)
		{ this.directedenumerable = directedenumerable; }

		#endregion

		#region IDirectedEnumerable<T> Members

		/// <summary>
		/// Get a enumerable that enumerates the wrapped collection in the opposite direction
		/// </summary>
		/// <returns>The mirrored enumerable</returns>
		public IDirectedEnumerable<T> Backwards()
		{ return new GuardedDirectedEnumerable<T>(directedenumerable.Backwards()); }


		/// <summary>
		/// <code>Forwards</code> if same, else <code>Backwards</code>
		/// </summary>
		/// <value>The enumeration direction relative to the original collection.</value>
		public EnumerationDirection Direction
		{ get { return directedenumerable.Direction; } }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for an ICollectionValue&lt;T&gt;
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedCollectionValue<T>: GuardedEnumerable<T>, ICollectionValue<T>
	{
		#region Fields

		ICollectionValue<T> collection;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a ICollectionValue&lt;T&gt; in a read-only wrapper
		/// </summary>
		/// <param name="collection">the collection to wrap</param>
		public GuardedCollectionValue(ICollectionValue<T> collection) : base(collection)
		{ this.collection = collection; }

		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// Get the size of the wrapped collection
		/// </summary>
		/// <value>The size</value>
		public int Count { get { return collection.Count; } }

        /// <summary>
        /// The value is symbolic indicating the type of asymptotic complexity
        /// in terms of the size of this collection (worst-case or amortized as
        /// relevant).
        /// </summary>
        /// <value>A characterization of the speed of the 
        /// <code>Count</code> property in this collection.</value>
        public Speed CountSpeed { get { return collection.CountSpeed; } }

        /// <summary>
		/// Copy the items of the wrapped collection to an array
		/// </summary>
		/// <param name="a">The array</param>
		/// <param name="i">Starting offset</param>
		public void CopyTo(T[] a, int i) { collection.CopyTo(a, i); }

        /// <summary>
        /// Create an array from the items of the wrapped collection
        /// </summary>
        /// <returns>The array</returns>
        public T[] ToArray() { return collection.ToArray(); }

        /// <summary>
        /// Apply a delegate to all items of the wrapped enumerable.
        /// </summary>
        /// <param name="a">The delegate to apply</param>
        //TODO: change this to throw an exception?
        public void Apply(Applier<T> a) { collection.Apply(a); }


        /// <summary>
        /// Check if there exists an item  that satisfies a
        /// specific predicate in the wrapped enumerable.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True is such an item exists</returns>
        public bool Exists(Filter<T> filter) { return collection.Exists(filter); }


        /// <summary>
        /// Check if all items in the wrapped enumerable satisfies a specific predicate.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True if all items satisfies the predicate</returns>
        public bool All(Filter<T> filter) { return collection.All(filter); }
        #endregion
    }



	/// <summary>
	/// A read-only wrapper for a directed collection
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedDirectedCollectionValue<T>: GuardedCollectionValue<T>, IDirectedCollectionValue<T>
	{
		#region Fields

		IDirectedCollectionValue<T> directedcollection;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a directed collection in a read-only wrapper
		/// </summary>
		/// <param name="directedcollection">the collection to wrap</param>
		public GuardedDirectedCollectionValue(IDirectedCollectionValue<T> directedcollection) : 
			base(directedcollection)
		{ this.directedcollection = directedcollection; }

		#endregion

		#region IDirectedCollection<T> Members

		/// <summary>
		/// Get a collection that enumerates the wrapped collection in the opposite direction
		/// </summary>
		/// <returns>The mirrored collection</returns>
		public IDirectedCollectionValue<T> Backwards()
		{ return new GuardedDirectedCollectionValue<T>(directedcollection.Backwards()); }

		#endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
		{ return Backwards(); }


		/// <summary>
		/// <code>Forwards</code> if same, else <code>Backwards</code>
		/// </summary>
		/// <value>The enumeration direction relative to the original collection.</value>
		public EnumerationDirection Direction
		{ get { return directedcollection.Direction; } }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for an ICollection&lt;T&gt;.
	/// <see cref="T:C5.ICollection!1"/>
	///
	/// <p>Suitable for wrapping hash tables, <see cref="T:C5.HashSet!1"/>
	/// and <see cref="T:C5.HashBag!1"/>  </p>
	/// </summary>
	public class GuardedCollection<T>: GuardedCollectionValue<T>, ICollection<T>
	{
		#region Fields

		ICollection<T> collection;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap an ICollection&lt;T&gt; in a read-only wrapper
		/// </summary>
		/// <param name="collection">the collection to wrap</param>
		public GuardedCollection(ICollection<T> collection)
			:base(collection)
		{ this.collection = collection; }

		#endregion

		#region ICollection<T> Members

		/// <summary>
		/// (This is a read-only wrapper)
		/// </summary>
		/// <value>True</value>
		public bool IsReadOnly { get { return true; } }


		/// <summary> </summary>
		/// <value>Speed of wrapped collection</value>
		public Speed ContainsSpeed { get { return collection.ContainsSpeed; } }


		int ICollection<T>.GetHashCode()
		{ return ((ICollection<T>)collection).GetHashCode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return ((ICollection<T>)collection).Equals(that); }


		/// <summary>
		/// Check if an item is in the wrapped collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>True if found</returns>
		public bool Contains(T item) { return collection.Contains(item); }


		/// <summary>
		/// Count the number of times an item appears in the wrapped collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The number of copies</returns>
		public int ContainsCount(T item) { return collection.ContainsCount(item); }


		/// <summary>
		/// Check if all items in the argument is in the wrapped collection
		/// </summary>
		/// <param name="items">The items</param>
		/// <returns>True if so</returns>
		public bool ContainsAll(MSG.IEnumerable<T> items) { return collection.ContainsAll(items); }


		/// <summary>
		/// Search for an item in the wrapped collection
		/// </summary>
		/// <param name="item">On entry the item to look for, on exit the equivalent item found (if any)</param>
		/// <returns></returns>
		public bool Find(ref T item) { return collection.Find(ref item); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool FindOrAdd(ref T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Update(T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool UpdateOrAdd(T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool RemoveWithReturn(ref T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		public void RemoveAllCopies(T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="items"></param>
		public void RemoveAll(MSG.IEnumerable<T> items)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		public void Clear()
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="items"></param>
		public void RetainAll(MSG.IEnumerable<T> items)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// Check  wrapped collection for internal consistency
		/// </summary>
		/// <returns>True if check passed</returns>
		public bool Check() { return collection.Check(); }

		#endregion

		#region ISink<T> Members

		/// <summary> </summary>
		/// <value>False if wrapped collection has set semantics</value>
        public bool AllowsDuplicates { get { return collection.AllowsDuplicates; } }


        /// <summary> </summary>
		/// <value>The sync root of the wrapped collection</value>
		public object SyncRoot { get { return collection.SyncRoot; } }


		/// <summary> </summary>
		/// <value>True if wrapped collection is empty</value>
		public bool IsEmpty { get { return collection.IsEmpty; } }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Add(T item)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


        /// <summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// </summary>
        /// <param name="items"></param>
        public void AddAll(MSG.IEnumerable<T> items)
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

        /// <summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// </summary>
        /// <param name="items"></param>
        /*public*/ void C5.IExtensible<T>.AddAll<U>(MSG.IEnumerable<U> items) //where U : T
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

        #endregion
    }


	/// <summary>
	/// A read-only wrapper for a sequenced collection
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedSequenced<T>: GuardedCollection<T>, ISequenced<T>
	{
		#region Fields

		ISequenced<T> sequenced;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a sequenced collection in a read-only wrapper
		/// </summary>
		/// <param name="sorted"></param>
		public GuardedSequenced(ISequenced<T> sorted):base(sorted) { this.sequenced = sorted; }

		#endregion

		#region ISequenced<T> Members

		int ISequenced<T>.GetHashCode()
		{ return ((ISequenced<T>)sequenced).GetHashCode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ return ((ISequenced<T>)sequenced).Equals(that); }

		#endregion

		#region IEditableCollection<T> Members

		int ICollection<T>.GetHashCode()
		{ return ((ICollection<T>)sequenced).GetHashCode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return ((ICollection<T>)sequenced).Equals(that); }

		#endregion

		#region IDirectedCollection<T> Members

		/// <summary>
		/// Get a collection that enumerates the wrapped collection in the opposite direction
		/// </summary>
		/// <returns>The mirrored collection</returns>
		public IDirectedCollectionValue<T> Backwards()
		{ return new GuardedDirectedCollectionValue<T>(sequenced.Backwards()); }

		#endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
		{ return Backwards(); }



		/// <summary>
		/// <code>Forwards</code> if same, else <code>Backwards</code>
		/// </summary>
		/// <value>The enumeration direction relative to the original collection.</value>
		public EnumerationDirection Direction
		{ get { return EnumerationDirection.Forwards; } }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for a sorted collection
	///
	/// <p>This is mainly interesting as a base of other guard classes</p>
	/// </summary>
	public class GuardedSorted<T>: GuardedSequenced<T>, ISorted<T>
	{
		#region Fields

		ISorted<T> sorted;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a sorted collection in a read-only wrapper
		/// </summary>
		/// <param name="sorted"></param>
		public GuardedSorted(ISorted<T> sorted):base(sorted) { this.sorted = sorted; }

		#endregion

        #region IEditableCollection Members

		int ICollection<T>.GetHashCode()
		{ return ((ICollection<T>)sorted).GetHashCode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return ((ICollection<T>)sorted).Equals(that); }

		#endregion

		#region ISequenced<T> Members

		int ISequenced<T>.GetHashCode()
		{ return ((ISequenced<T>)sorted).GetHashCode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ return ((ISequenced<T>)sorted).Equals(that); }


		#endregion

		#region ISorted<T> Members

		/// <summary>
		/// Find the predecessor of the item in the wrapped sorted collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The predecessor</returns>
		public T Predecessor(T item) { return sorted.Predecessor(item); }


		/// <summary>
		/// Find the Successor of the item in the wrapped sorted collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The Successor</returns>
		public T Successor(T item) { return sorted.Successor(item); }


		/// <summary>
		/// Find the weak predecessor of the item in the wrapped sorted collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The weak predecessor</returns>
		public T WeakPredecessor(T item) { return sorted.WeakPredecessor(item); }


		/// <summary>
		/// Find the weak Successor of the item in the wrapped sorted collection
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The weak Successor</returns>
		public T WeakSuccessor(T item) { return sorted.WeakSuccessor(item); }


		/// <summary>
		/// Run Cut on the wrapped sorted collection
		/// </summary>
		/// <param name="c"></param>
		/// <param name="low"></param>
		/// <param name="lval"></param>
		/// <param name="high"></param>
		/// <param name="hval"></param>
		/// <returns></returns>
		public bool Cut(IComparable<T> c, out T low, out bool lval, out T high, out bool hval)
		{ return sorted.Cut(c, out low, out lval, out high, out hval); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="bot"></param>
		/// <returns></returns>
		public IDirectedEnumerable<T> RangeFrom(T bot) { return sorted.RangeFrom(bot); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="bot"></param>
		/// <param name="top"></param>
		/// <returns></returns>
		public IDirectedEnumerable<T> RangeFromTo(T bot, T top)
		{ return sorted.RangeFromTo(bot, top); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		public IDirectedEnumerable<T> RangeTo(T top) { return sorted.RangeTo(top); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <returns></returns>
		public IDirectedCollectionValue<T> RangeAll() { return sorted.RangeAll(); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="items"></param>
		public void AddSorted(MSG.IEnumerable<T> items)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="low"></param>
		public void RemoveRangeFrom(T low)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="low"></param>
		/// <param name="hi"></param>
		public void RemoveRangeFromTo(T low, T hi)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="hi"></param>
		public void RemoveRangeTo(T hi)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

		#endregion

		#region IPriorityQueue<T> Members

		/// <summary>
		/// Find the minimum of the wrapped collection
		/// </summary>
		/// <returns>The minimum</returns>
		public T FindMin() { return sorted.FindMin(); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <returns></returns>
		public T DeleteMin()
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// Find the maximum of the wrapped collection
		/// </summary>
		/// <returns>The maximum</returns>
		public T FindMax() { return sorted.FindMax(); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <returns></returns>
		public T DeleteMax()
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

        /// <summary>
        /// The comparer object supplied at creation time for the underlying collection
        /// </summary>
        /// <value>The comparer</value>
        public IComparer<T> Comparer { get { return sorted.Comparer; } }
        #endregion


		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
		{ return Backwards(); }

		#endregion
	}



	/// <summary>
	/// Read-only wrapper for indexed sorted collections
	///
	/// <p>Suitable for wrapping TreeSet, TreeBag and SortedArray</p>
	/// </summary>
	public class GuardedIndexedSorted<T>: GuardedSorted<T>, IIndexedSorted<T>
	{
		#region Fields

		IIndexedSorted<T> indexedsorted;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap an indexed sorted collection in a read-only wrapper
		/// </summary>
		/// <param name="list">the indexed sorted collection</param>
		public GuardedIndexedSorted(IIndexedSorted<T> list):base(list)
		{ this.indexedsorted = list; }

		#endregion

		#region IIndexedSorted<T> Members

		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="bot"></param>
		/// <returns></returns>
		public new IDirectedCollectionValue<T> RangeFrom(T bot)
		{ return indexedsorted.RangeFrom(bot); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="bot"></param>
		/// <param name="top"></param>
		/// <returns></returns>
		public new IDirectedCollectionValue<T> RangeFromTo(T bot, T top)
		{ return indexedsorted.RangeFromTo(bot, top); }


		/// <summary>
		/// Get the specified range from the wrapped collection. 
		/// (The current implementation erroneously does not wrap the result.)
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		public new IDirectedCollectionValue<T> RangeTo(T top)
		{ return indexedsorted.RangeTo(top); }


		/// <summary>
		/// Report the number of items in the specified range of the wrapped collection
		/// </summary>
		/// <param name="bot"></param>
		/// <returns></returns>
		public int CountFrom(T bot) { return indexedsorted.CountFrom(bot); }


		/// <summary>
		/// Report the number of items in the specified range of the wrapped collection
		/// </summary>
		/// <param name="bot"></param>
		/// <param name="top"></param>
		/// <returns></returns>
		public int CountFromTo(T bot, T top) { return indexedsorted.CountFromTo(bot, top); }


		/// <summary>
		/// Report the number of items in the specified range of the wrapped collection
		/// </summary>
		/// <param name="top"></param>
		/// <returns></returns>
		public int CountTo(T top) { return indexedsorted.CountTo(top); }


		/// <summary>
		/// Run FindAll on the wrapped collection with the indicated filter.
		/// The result will <b>not</b> be read-only.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public IIndexedSorted<T> FindAll(Filter<T> f)
		{ return indexedsorted.FindAll(f); }


		/// <summary>
		/// Run Map on the wrapped collection with the indicated mapper.
		/// The result will <b>not</b> be read-only.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="c">The comparer to use in the result</param>
		/// <returns></returns>
		public IIndexedSorted<V> Map<V>(Mapper<T,V> m, IComparer<V> c)
		{ return indexedsorted.Map(m, c); }

		#endregion

		#region IIndexed<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>The i'th item of the wrapped sorted collection</value>
		public T this[int i] { get { return indexedsorted[i]; } }


		/// <summary> </summary>
		/// <value>A directed collection of the items in the indicated interval of the wrapped collection</value>
		public IDirectedCollectionValue<T> this[int start, int end]
		{ get { return new GuardedDirectedCollectionValue<T>(indexedsorted[start, end]); } }


		/// <summary>
		/// Find the (first) index of an item in the wrapped collection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item) { return indexedsorted.IndexOf(item); }


		/// <summary>
		/// Find the last index of an item in the wrapped collection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int LastIndexOf(T item) { return indexedsorted.LastIndexOf(item); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public T RemoveAt(int i)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		public void RemoveInterval(int start, int count)
		{ throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

		#endregion

		#region ISequenced<T> Members

		int ISequenced<T>.GetHashCode()
		{ return ((ISequenced<T>)indexedsorted).GetHashCode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ return ((ISequenced<T>)indexedsorted).Equals(that); }

		#endregion

		#region IEditableCollection<T> Members

		int ICollection<T>.GetHashCode()
		{ return ((ICollection<T>)indexedsorted).GetHashCode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return ((ICollection<T>)indexedsorted).Equals(that); }

		#endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
		{ return Backwards(); }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for a generic list collection
	/// <p>Suitable as a wrapper for LinkedList, HashedLinkedList, ArrayList and HashedArray.
	/// <see cref="T:C5.LinkedList!1"/>, 
	/// <see cref="T:C5.HashedLinkedList!1"/>, 
	/// <see cref="T:C5.ArrayList!1"/> or
	/// <see cref="T:C5.HashedArray!1"/>.
	/// </p>
	/// </summary>
	public class GuardedList<T>: GuardedSequenced<T>, IList<T>
	{
		#region Fields

		IList<T> list;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a list in a read-only wrapper
		/// </summary>
		/// <param name="list">The list</param>
		public GuardedList(IList<T> list) : base (list) { this.list = list; }

		#endregion

		#region IList<T> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>The first item of the wrapped list</value>
		public T First { get { return list.First; } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>The last item of the wrapped list</value>
		public T Last { get { return list.Last; } }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if used as setter
		/// </summary>
		/// <value>True if wrapped list has FIFO semantics for the Add(T item) and Remove() methods</value>
		public bool FIFO
		{
			get { return list.FIFO; }
			set { throw new InvalidOperationException("List is read only"); }
		}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if used as setter
		/// </summary>
		/// <value>The i'th item of the wrapped list</value>
		public T this[int i]
		{
			get { return list[i]; }
			set { throw new InvalidOperationException("List is read only"); }
		}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="i"></param>
		/// <param name="item"></param>
		public void Insert(int i, T item)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		public void InsertFirst(T item)
		{ throw new InvalidOperationException("List is read only"); }

		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		public void InsertLast(T item)
		{ throw new InvalidOperationException("List is read only"); }

		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		public void InsertBefore(T item, T target)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		public void InsertAfter(T item, T target)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="i"></param>
		/// <param name="items"></param>
		public void InsertAll(int i, MSG.IEnumerable<T> items)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// Perform FindAll on the wrapped list. The result is <b>not</b> necessarily read-only.
		/// </summary>
		/// <param name="filter">The filter to use</param>
		/// <returns></returns>
		public IList<T> FindAll(Filter<T> filter) { return list.FindAll(filter); }


		/// <summary>
		/// Perform Map on the wrapped list. The result is <b>not</b> necessarily read-only.
		/// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The mapper to use.</param>
        /// <returns>The mapped list</returns>
        public IList<V> Map<V>(Mapper<T, V> mapper) { return list.Map(mapper); }

        /// <summary>
        /// Perform Map on the wrapped list. The result is <b>not</b> necessarily read-only.
        /// </summary>
        /// <typeparam name="V">The type of items of the new list</typeparam>
        /// <param name="mapper">The delegate defining the map.</param>
        /// <param name="hasher">The hasher to use for the new list</param>
        /// <returns>The new list.</returns>
        public IList<V> Map<V>(Mapper<T, V> mapper, IHasher<V> hasher) { return list.Map(mapper, hasher); }

        /// <summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <returns></returns>
		public T Remove() { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <returns></returns>
		public T RemoveFirst() { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <returns></returns>
		public T RemoveLast() { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// Create the indicated view on the wrapped list and wrap it read-only.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public IList<T> View(int start, int count)
		{
			return new GuardedList<T>(list.View(start, count));
		}


		//TODO: This is wrong!
		/// <summary>
		/// (This is wrong functionality)
		/// </summary>
        /// <value>The wrapped underlying list of the wrapped view </value>
        public IList<T> Underlying { get { return new GuardedList<T>(list.Underlying); } }


        /// <summary>
		/// 
		/// </summary>
		/// <value>The offset of the wrapped list as a view.</value>
		public int Offset { get { return list.Offset; } }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="offset"></param>
		public void Slide(int offset) { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		public void Slide(int offset, int size) { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		public void Reverse() { throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		public void Reverse(int start, int count)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// Check if wrapped list is sorted
		/// </summary>
		/// <param name="c">The sorting order to use</param>
		/// <returns>True if sorted</returns>
		public bool IsSorted(IComparer<T> c) { return list.IsSorted(c); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="c"></param>
		public void Sort(IComparer<T> c)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		public void Shuffle()
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="rnd"></param>
		public void Shuffle(Random rnd)
		{ throw new InvalidOperationException("List is read only"); }

		#endregion

		#region IIndexed<T> Members

		/// <summary> </summary>
		/// <value>A directed collection of the items in the indicated interval of the wrapped collection</value>
		public IDirectedCollectionValue<T> this[int start, int end]
		{ get { return new GuardedDirectedCollectionValue<T>(list[start, end]); } }


		/// <summary>
		/// Find the (first) index of an item in the wrapped collection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item) { return list.IndexOf(item); }


		/// <summary>
		/// Find the last index of an item in the wrapped collection
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int LastIndexOf(T item) { return list.LastIndexOf(item); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public T RemoveAt(int i)
		{ throw new InvalidOperationException("List is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="start"></param>
		/// <param name="count"></param>
		public void RemoveInterval(int start, int count)
		{ throw new InvalidOperationException("List is read only"); }

		#endregion

		#region ISequenced<T> Members

		int ISequenced<T>.GetHashCode()
		{ return ((ISequenced<T>)list).GetHashCode(); }


		bool ISequenced<T>.Equals(ISequenced<T> that)
		{ return ((ISequenced<T>)list).Equals(that); }

		#endregion

		#region IEditableCollection<T> Members

		int ICollection<T>.GetHashCode()
		{ return ((ICollection<T>)list).GetHashCode(); }


		bool ICollection<T>.Equals(ICollection<T> that)
		{ return ((ICollection<T>)list).Equals(that); }

		#endregion

		#region IDirectedEnumerable<T> Members

		IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
		{ return Backwards(); }

		#endregion

        #region IStack<T> Members


        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// <returns>-</returns>
        public void Push(T item)
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// <returns>-</returns>
        public T Pop()
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

                #endregion

        #region IQueue<T> Members

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// <returns>-</returns>
        public void EnQueue(T item)
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
        /// <returns>-</returns>
        public T DeQueue()
        { throw new InvalidOperationException("Collection cannot be modified through this guard object"); }

                #endregion

    }



	/// <summary>
	/// A read-only wrapper for a dictionary.
	///
	/// <p>Suitable for wrapping a HashDictionary. <see cref="T:C5.HashDictionary!2"/></p>
	/// </summary>
	public class GuardedDictionary<K,V>: GuardedEnumerable<KeyValuePair<K,V>>, IDictionary<K,V>
	{
		#region Fields

		IDictionary<K,V> dict;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a dictionary in a read-only wrapper
		/// </summary>
		/// <param name="dict">the dictionary</param>
		public GuardedDictionary(IDictionary<K,V> dict) : base(dict) { this.dict = dict; }

		#endregion

		#region IDictionary<K,V> Members

		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a
		/// read-only wrappper if used as a setter
		/// </summary>
		/// <value>Get the value corresponding to a key in the wrapped dictionary</value>
		public V this[K key]
		{
			get { return dict[key]; }
			set { throw new InvalidOperationException("Dictionary is read only"); }
		}


		/// <summary> </summary>
		/// <value>The size of the wrapped dictionary</value>
		public int Count { get { return dict.Count; } }


		/// <summary>
		/// (This is a read-only wrapper)
		/// </summary>
		/// <value>True</value>
		public bool IsReadOnly { get { return true; } }


		/// <summary> </summary>
		/// <value>The sync root of the wrapped dictionary</value>
		public object SyncRoot { get { return dict.SyncRoot; } }


		//TODO: guard with a read-only wrapper? Probably so!
		/// <summary> </summary>
		/// <value>The collection of keys of the wrapped dictionary</value>
		public ICollectionValue<K> Keys
		{ get { return dict.Keys; } }


		/// <summary> </summary>
		/// <value>The collection of values of the wrapped dictionary</value>
		public ICollectionValue<V> Values { get { return dict.Values; } }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		public void Add(K key, V val)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Remove(K key)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool Remove(K key, out V val)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		public void Clear()
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// Check if the wrapped dictionary contains a specific key
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>True if it does</returns>
		public bool Contains(K key) { return dict.Contains(key); }


		/// <summary>
		/// Search for a key in the wrapped dictionary, reporting the value if found
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="val">On exit: the value if found</param>
		/// <returns>True if found</returns>
		public bool Find(K key, out V val) { return dict.Find(key, out val); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool Update(K key, V val)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool FindOrAdd(K key, ref V val)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// <exception cref="InvalidOperationException"/> since this is a read-only wrappper
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public bool UpdateOrAdd(K key, V val)
		{ throw new InvalidOperationException("Dictionary is read only"); }


		/// <summary>
		/// Check the internal consistency of the wrapped dictionary
		/// </summary>
		/// <returns>True if check passed</returns>
		public bool Check() { return dict.Check(); }

		#endregion
	}



	/// <summary>
	/// A read-only wrapper for a sorted dictionary.
	///
	/// <p>Suitable for wrapping a Dictionary. <see cref="T:C5.Dictionary!2"/></p>
	/// </summary>
	public class GuardedSortedDictionary<K,V>: GuardedDictionary<K,V>, ISortedDictionary<K,V>
	{
		#region Fields

		ISortedDictionary<K,V> sorteddict;

		#endregion

		#region Constructor

		/// <summary>
		/// Wrap a sorted dictionary in a read-only wrapper
		/// </summary>
		/// <param name="sorteddict">the dictionary</param>
		public GuardedSortedDictionary(ISortedDictionary<K,V> sorteddict) :base(sorteddict)
		{ this.sorteddict = sorteddict; }

		#endregion

		#region ISortedDictionary<K,V> Members

		/// <summary>
		/// Get the entry in the wrapped dictionary whose key is the
		/// predecessor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		public KeyValuePair<K,V> Predecessor(K key)
		{ return sorteddict.Predecessor(key); }


		/// <summary>
		/// Get the entry in the wrapped dictionary whose key is the
		/// successor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		public KeyValuePair<K,V> Successor(K key)
		{ return sorteddict.Successor(key); }


		/// <summary>
		/// Get the entry in the wrapped dictionary whose key is the
		/// weak predecessor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		public KeyValuePair<K,V> WeakPredecessor(K key)
		{ return sorteddict.WeakPredecessor(key); }


		/// <summary>
		/// Get the entry in the wrapped dictionary whose key is the
		/// weak successor of a specified key.
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>The entry</returns>
		public KeyValuePair<K,V> WeakSuccessor(K key)
		{ return sorteddict.WeakSuccessor(key); }

		#endregion
	}

}
#endif
