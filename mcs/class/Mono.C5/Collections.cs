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
	/// Direction of enumeration order relative to original collection.
	/// </summary>
	public enum EnumerationDirection { 
		/// <summary>
		/// Same direction
		/// </summary>
		Forwards, 
		/// <summary>
		/// Opposite direction
		/// </summary>
		Backwards 
	}

	#region int stuff
	class IC: IComparer<int>
	{
		[Tested]
		public int Compare(int a, int b) { return a > b ? 1 : a < b ? -1 : 0; }
	}



	/// <summary>
	/// A hasher for int32
	/// </summary>
	public class IntHasher: IHasher<int>
	{
		/// <summary>
		/// Get the hash code of this integer, i.e. itself
		/// </summary>
		/// <param name="item">The integer</param>
		/// <returns>The same</returns>
		[Tested]
		public int GetHashCode(int item) { return item; }


		/// <summary>
		/// Check if two integers are equal
		/// </summary>
		/// <param name="i1">first integer</param>
		/// <param name="i2">second integer</param>
		/// <returns>True if equal</returns>
		[Tested]
		public bool Equals(int i1, int i2) { return i1 == i2; }
	}


	#endregion

	#region Natural Comparers


	/// <summary>
	/// A natural generic IComparer for an IComparable&lt;T&gt; item type
	/// </summary>
	public class NaturalComparer<T>: IComparer<T>
		where T: IComparable<T>
	{
		/// <summary>
		/// Compare two items
		/// </summary>
		/// <param name="a">First item</param>
		/// <param name="b">Second item</param>
		/// <returns>a &lt;=&gt; b</returns>
		[Tested]
		public int Compare(T a, T b) { return a.CompareTo(b); }
	}



	/// <summary>
	/// A natural generic IComparer for a System.IComparable item type
	/// </summary>
	public class NaturalComparerO<T>: IComparer<T>
		where T: System.IComparable
	{
		/// <summary>
		/// Compare two items
		/// </summary>
		/// <param name="a">First item</param>
		/// <param name="b">Second item</param>
		/// <returns>a &lt;=&gt; b</returns>
		[Tested]
		public int Compare(T a, T b) { return a.CompareTo(b); }
	}



	#endregion

	#region Hashers
	/// <summary>
	/// The default item hasher for a reference type. A trivial wrapper for calling 
	/// the GetHashCode and Equals methods inherited from object.
	///
	/// <p>Should only be instantiated with a reference type as generic type parameter. 
	/// This is asserted at instatiation time in Debug builds.</p>
	/// </summary>
	public sealed class DefaultReferenceTypeHasher<T>: IHasher<T>
	{
		static DefaultReferenceTypeHasher()
		{
			Debug.Assert(!typeof(T).IsValueType, "DefaultReferenceTypeHasher instantiated with value type: " + typeof(T));
		}
		
		/// <summary>
		/// Get the hash code with respect to this item hasher
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The hash code</returns>
		[Tested]
		public int GetHashCode(T item) { return item.GetHashCode(); }


		/// <summary>
		/// Check if two items are equal with respect to this item hasher
		/// </summary>
		/// <param name="i1">first item</param>
		/// <param name="i2">second item</param>
		/// <returns>True if equal</returns>
		[Tested]
		public bool Equals(T i1, T i2)
		{
			//For reference types, the (object) cast should be jitted as a noop. 
			return (object)i1 == null ? (object)i2 == null : i1.Equals(i2);
		}
	}

	/// <summary>
	/// The default item hasher for a value type. A trivial wrapper for calling 
	/// the GetHashCode and Equals methods inherited from object.
	///
	/// <p>Should only be instantiated with a value type as generic type parameter. 
	/// This is asserted at instatiation time in Debug builds.</p>
	/// <p>We cannot add the constraint "where T : struct" to get a compile time check
	/// because we need to instantiate this class in C5.HasherBuilder.ByPrototype[T].Examine()
	/// with a T that is only known at runtime to be a value type!</p>
	/// </summary>
	
    //Note: we could (now) add a constraint "where T : struct" to get a compile time check,
    //but 
	public sealed class DefaultValueTypeHasher<T>: IHasher<T>
	{
		static DefaultValueTypeHasher()
		{
			Debug.Assert(typeof(T).IsValueType, "DefaultValueTypeHasher instantiated with reference type: " + typeof(T));
		}
		/// <summary>
		/// Get the hash code with respect to this item hasher
		/// </summary>
		/// <param name="item">The item</param>
		/// <returns>The hash code</returns>
		[Tested]
		public int GetHashCode(T item) { return item.GetHashCode(); }


		/// <summary>
		/// Check if two items are equal with respect to this item hasher
		/// </summary>
		/// <param name="i1">first item</param>
		/// <param name="i2">second item</param>
		/// <returns>True if equal</returns>
		[Tested]
		public bool Equals(T i1, T i2) { return i1.Equals(i2); }
	}

	#endregion

	#region Bases

	/// <summary>
	/// A base class for implementing an IEnumerable&lt;T&gt;
	/// </summary>
	public abstract class EnumerableBase<T>: MSG.IEnumerable<T>
	{
		/// <summary>
		/// Create an enumerator for this collection.
		/// </summary>
		/// <returns>The enumerator</returns>
		public abstract MSG.IEnumerator<T> GetEnumerator();

		/// <summary>
		/// Count the number of items in an enumerable by enumeration
		/// </summary>
		/// <param name="items">The enumerable to count</param>
		/// <returns>The size of the enumerable</returns>
		protected static int countItems(MSG.IEnumerable<T> items)
		{
			ICollectionValue<T> jtems = items as ICollectionValue<T>;

			if (jtems != null)
				return jtems.Count;

			int count = 0;

			using (MSG.IEnumerator<T> e = items.GetEnumerator())
				while (e.MoveNext()) count++;

			return count;
		}
	}


	/// <summary>
	/// Base class for classes implementing ICollectionValue[T]
	/// </summary>
	public abstract class CollectionValueBase<T>: EnumerableBase<T>, ICollectionValue<T>
	{
		//This forces our subclasses to make Count virtual!
		/// <summary>
		/// The number of items in this collection.
		/// </summary>
		/// <value></value>
		public abstract int Count { get;}

        /// <summary>
        /// The value is symbolic indicating the type of asymptotic complexity
        /// in terms of the size of this collection (worst-case or amortized as
        /// relevant).
        /// </summary>
        /// <value>A characterization of the speed of the 
        /// <code>Count</code> property in this collection.</value>
        public abstract Speed CountSpeed { get; }


        /// <summary>
		/// Copy the items of this collection to part of an array.
		/// <exception cref="ArgumentOutOfRangeException"/> if i is negative.
		/// <exception cref="ArgumentException"/> if the array does not have room for the items.
		/// </summary>
		/// <param name="a">The array to copy to</param>
		/// <param name="i">The starting index.</param>
		[Tested]
		public virtual void CopyTo(T[] a, int i)
		{
			if (i < 0)
				throw new ArgumentOutOfRangeException();

			if (i + Count > a.Length)
				throw new ArgumentException();

			foreach (T item in this) a[i++] = item;
		}

        /// <summary>
        /// Create an array with the items of this collection (in the same order as an
        /// enumerator would output them).
        /// </summary>
        /// <returns>The array</returns>
        //[Tested]
        public virtual T[] ToArray()
        {
            T[] res = new T[Count];
            int i = 0;

            foreach (T item in this) res[i++] = item;

            return res;
        }

        /// <summary>
        /// Apply an Applier&lt;T&gt; to this enumerable
        /// </summary>
        /// <param name="a">The applier delegate</param>
        [Tested]
        public void Apply(Applier<T> a)
        {
            foreach (T item in this)
                a(item);
        }


        /// <summary>
        /// Check if there exists an item  that satisfies a
        /// specific predicate in this collection.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True is such an item exists</returns>
        [Tested]
        public bool Exists(Filter<T> filter)
        {
            foreach (T item in this)
                if (filter(item))
                    return true;

            return false;
        }


        /// <summary>
        /// Check if all items in this collection satisfies a specific predicate.
        /// </summary>
        /// <param name="filter">A filter delegate 
        /// (<see cref="T:C5.Filter!1"/>) defining the predicate</param>
        /// <returns>True if all items satisfies the predicate</returns>
        [Tested]
        public bool All(Filter<T> filter)
        {
            foreach (T item in this)
                if (!filter(item))
                    return false;

            return true;
        }

        /// <summary>
        /// Create an enumerator for this collection.
		/// </summary>
		/// <returns>The enumerator</returns>
		public override abstract MSG.IEnumerator<T> GetEnumerator();
	}


	/// <summary>
	/// Base class (abstract) for ICollection implementations.
	/// </summary>
	public abstract class CollectionBase<T>: CollectionValueBase<T>
	{
		#region Fields

		object syncroot = new object();

		/// <summary>
		/// The underlying field of the ReadOnly property
		/// </summary>
		protected bool isReadOnly = false;

		/// <summary>
		/// The current stamp value
		/// </summary>
		protected int stamp;

		/// <summary>
		/// The number of items in the collection
		/// </summary>
		protected int size;

		/// <summary>
		/// The item hasher of the collection
		/// </summary>
		protected IHasher<T> itemhasher;

		int iUnSequencedHashCode, iUnSequencedHashCodeStamp = -1;

		#endregion
		
		#region Util

		//protected bool equals(T i1, T i2) { return itemhasher.Equals(i1, i2); }
		/// <summary>
		/// Utility method for range checking.
		/// <exception cref="ArgumentOutOfRangeException"/> if the start or count is negative
		/// <exception cref="ArgumentException"/> if the range does not fit within collection size.
		/// </summary>
		/// <param name="start">start of range</param>
		/// <param name="count">size of range</param>
		[Tested]
		protected void checkRange(int start, int count)
		{
			if (start < 0 || count < 0)
				throw new ArgumentOutOfRangeException();

			if (start + count > size)
				throw new ArgumentException();
		}


		/// <summary>
		/// Compute the unsequenced hash code of a collection
		/// </summary>
		/// <param name="items">The collection to compute hash code for</param>
		/// <param name="itemhasher">The item hasher</param>
		/// <returns>The hash code</returns>
		[Tested]
		public static int ComputeHashCode(ICollectionValue<T> items, IHasher<T> itemhasher)
		{
			int h = 0;

			foreach (T item in items)
				h ^= itemhasher.GetHashCode(item);

			return (items.Count << 16) + h;
		}


		/// <summary>
		/// Examine if tit and tat are equal as unsequenced collections
		/// using the specified item hasher (assumed compatible with the two collections).
		/// </summary>
		/// <param name="tit">The first collection</param>
		/// <param name="tat">The second collection</param>
		/// <param name="itemhasher">The item hasher to use for comparison</param>
		/// <returns>True if equal</returns>
		[Tested]
		public static bool StaticEquals(ICollection<T> tit, ICollection<T> tat, IHasher<T> itemhasher)
		{
			if (tat == null)
				return tit == null;

			if (tit.Count != tat.Count)
				return false;

			//This way we might run through both enumerations twice, but
			//probably not (if the hash codes are good)
			if (tit.GetHashCode() != tat.GetHashCode())
				return false;

            if (!tit.AllowsDuplicates && (tat.AllowsDuplicates || tat.ContainsSpeed >= tit.ContainsSpeed))
            {
                //TODO: use foreach construction
				using (MSG.IEnumerator<T> dit = tit.GetEnumerator())
				{
					while (dit.MoveNext())
						if (!tat.Contains(dit.Current))
							return false;
				}
			}
            else if (!tat.AllowsDuplicates)
            {
				using (MSG.IEnumerator<T> dat = tat.GetEnumerator())
				{
					while (dat.MoveNext())
						if (!tit.Contains(dat.Current))
							return false;
				}
			}
			else
			{//both are bags, we only have a slow one
				//unless the bags are based on a fast T->int dictinary (tree or hash) 
				using (MSG.IEnumerator<T> dat = tat.GetEnumerator())
				{
					while (dat.MoveNext())
					{
						T item = dat.Current;

						if (tit.ContainsCount(item) != tat.ContainsCount(item))
							return false;
					}
				}
				//That was O(n^3) - completely unacceptable.
				//If we use an auxiliary bool[] we can do the comparison in O(n^2)
			}

			return true;
		}


		/// <summary>
		/// Get the unsequenced collection hash code of this collection: from the cached 
		/// value if present and up to date, else (re)compute.
		/// </summary>
		/// <returns>The hash code</returns>
		protected int unsequencedhashcode()
		{
			if (iUnSequencedHashCodeStamp == stamp)
				return iUnSequencedHashCode;

			iUnSequencedHashCode = ComputeHashCode(this, itemhasher);
			iUnSequencedHashCodeStamp = stamp;
			return iUnSequencedHashCode;
		}


		/// <summary>
		/// Check if the contents of that is equal to the contents of this
		/// in the unsequenced sense. Using the item hasher of this collection.
		/// </summary>
		/// <param name="that">The collection to compare to.</param>
		/// <returns>True if  equal</returns>
		protected bool unsequencedequals(ICollection<T> that)
		{
			return StaticEquals((ICollection<T>)this, that, itemhasher);
		}


		/// <summary>
		/// <exception cref="InvalidOperationException"/> if this collection has been updated 
		/// since a target time
		/// </summary>
		/// <param name="thestamp">The stamp identifying the target time</param>
		protected virtual void modifycheck(int thestamp)
		{
			if (this.stamp != thestamp)
				throw new InvalidOperationException("Collection was modified");
		}


		/// <summary>
		/// Check if it is valid to perform update operations, and if so increment stamp
		/// </summary>
		protected virtual void updatecheck()
		{
			if (isReadOnly)
				throw new InvalidOperationException("Collection cannot be modified through this guard object");

			stamp++;
		}

		#endregion

		#region IEditableCollection<T> members

		/// <summary>
		/// 
		/// </summary>
		/// <value>True if this collection is read only</value>
		[Tested]
		public bool IsReadOnly { [Tested]get { return isReadOnly; } }

		#endregion

		#region ICollection<T> members
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


		#endregion

		#region ISink<T> members
		/// <summary>
		/// 
		/// </summary>
		/// <value>A distinguished object to use for locking to synchronize multithreaded access</value>
		[Tested]
		public object SyncRoot { get { return syncroot; } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>True is this collection is empty</value>
		[Tested]
		public bool IsEmpty { [Tested]get { return size == 0; } }
		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Create an enumerator for this collection.
		/// </summary>
		/// <returns>The enumerator</returns>
		public override abstract MSG.IEnumerator<T> GetEnumerator();
		#endregion
	}


	/// <summary>
	/// Base class (abstract) for sequenced collection implementations.
	/// </summary>
	public abstract class SequencedBase<T>: CollectionBase<T>
	{
		#region Fields

		int iSequencedHashCode, iSequencedHashCodeStamp = -1;

		#endregion

		#region Util

		/// <summary>
		/// Compute the unsequenced hash code of a collection
		/// </summary>
		/// <param name="items">The collection to compute hash code for</param>
		/// <param name="itemhasher">The item hasher</param>
		/// <returns>The hash code</returns>
		[Tested]
		public static int ComputeHashCode(ISequenced<T> items, IHasher<T> itemhasher)
		{
			//NOTE: It must be possible to devise a much stronger combined hashcode, 
			//but unfortunately, it has to be universal. OR we could use a (strong)
			//family and initialise its parameter randomly at load time of this class!
			//(We would not want to have yet a flag to check for invalidation?!)
			int iIndexedHashCode = 0;

			foreach (T item in items)
				iIndexedHashCode = iIndexedHashCode * 31 + itemhasher.GetHashCode(item);

			return iIndexedHashCode;
		}


		/// <summary>
		/// Examine if tit and tat are equal as sequenced collections
		/// using the specified item hasher (assumed compatible with the two collections).
		/// </summary>
		/// <param name="tit">The first collection</param>
		/// <param name="tat">The second collection</param>
		/// <param name="itemhasher">The item hasher to use for comparison</param>
		/// <returns>True if equal</returns>
		[Tested]
		public static bool StaticEquals(ISequenced<T> tit, ISequenced<T> tat, IHasher<T> itemhasher)
		{
			if (tat == null)
				return tit == null;

			if (tit.Count != tat.Count)
				return false;

			//This way we might run through both enumerations twice, but
			//probably not (if the hash codes are good)
			if (tit.GetHashCode() != tat.GetHashCode())
				return false;

			using (MSG.IEnumerator<T> dat = tat.GetEnumerator(), dit = tit.GetEnumerator())
			{
				while (dit.MoveNext())
				{
					dat.MoveNext();
					if (!itemhasher.Equals(dit.Current, dat.Current))
						return false;
				}
			}

			return true;
		}


		/// <summary>
		/// Get the sequenced collection hash code of this collection: from the cached 
		/// value if present and up to date, else (re)compute.
		/// </summary>
		/// <returns>The hash code</returns>
		protected int sequencedhashcode()
		{
			if (iSequencedHashCodeStamp == stamp)
				return iSequencedHashCode;

			iSequencedHashCode = ComputeHashCode((ISequenced<T>)this, itemhasher);
			iSequencedHashCodeStamp = stamp;
			return iSequencedHashCode;
		}


		/// <summary>
		/// Check if the contents of that is equal to the contents of this
		/// in the sequenced sense. Using the item hasher of this collection.
		/// </summary>
		/// <param name="that">The collection to compare to.</param>
		/// <returns>True if  equal</returns>
		protected bool sequencedequals(ISequenced<T> that)
		{
			return StaticEquals((ISequenced<T>)this, that, itemhasher);
		}


		#endregion

		/// <summary>
		/// Create an enumerator for this collection.
		/// </summary>
		/// <returns>The enumerator</returns>
		public override abstract MSG.IEnumerator<T> GetEnumerator();


		/// <summary>
		/// <code>Forwards</code> if same, else <code>Backwards</code>
		/// </summary>
		/// <value>The enumeration direction relative to the original collection.</value>
		[Tested]
		public EnumerationDirection Direction { [Tested]get { return EnumerationDirection.Forwards; } }
	}


	/// <summary>
	/// Base class for collection classes of dynamic array type implementations.
	/// </summary>
	public class ArrayBase<T>: SequencedBase<T>
	{
		#region Fields
		/// <summary>
		/// The actual internal array container. Will be extended on demand.
		/// </summary>
		protected T[] array;

		/// <summary>
		/// The offset into the internal array container of the first item. The offset is 0 for a 
		/// base dynamic array and may be positive for an updatable view into a base dynamic array.
		/// </summary>
		protected int offset;
		#endregion

		#region Util
		/// <summary>
		/// Double the size of the internal array.
		/// </summary>
		protected virtual void expand()
		{
			expand(2 * array.Length, size);
		}


		/// <summary>
		/// Expand the internal array container.
		/// </summary>
		/// <param name="newcapacity">The new size of the internal array - 
		/// will be rounded upwards to a power of 2.</param>
		/// <param name="newsize">The (new) size of the (base) collection.</param>
		protected virtual void expand(int newcapacity, int newsize)
		{
			Debug.Assert(newcapacity >= newsize);

			int newlength = array.Length;

			while (newlength < newcapacity) newlength *= 2;

			T[] newarray = new T[newlength];

			Array.Copy(array, newarray, newsize);
			array = newarray;
		}


		/// <summary>
		/// Insert an item at a specific index, moving items to the right
		/// upwards and expanding the array if necessary.
		/// </summary>
		/// <param name="i">The index at which to insert.</param>
		/// <param name="item">The item to insert.</param>
		protected virtual void insert(int i, T item)
		{
			if (size == array.Length)
				expand();

			if (i < size)
				Array.Copy(array, i, array, i + 1, size - i);

			array[i] = item;
			size++;
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create an empty ArrayBase object.
		/// </summary>
		/// <param name="capacity">The initial capacity of the internal array container.
		/// Will be rounded upwards to the nearest power of 2 greater than or equal to 8.</param>
		/// <param name="hasher">The item hasher to use, primarily for item equality</param>
		public ArrayBase(int capacity, IHasher<T> hasher)
		{
			int newlength = 8;

			while (newlength < capacity) newlength *= 2;

			array = new T[newlength];
			itemhasher = hasher;
		}

		#endregion

		#region IIndexed members

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
				checkRange(start, count);
				return new Range(this, start, count, true);
			}
		}

		#endregion

		#region IEditableCollection members
		/// <summary>
		/// Remove all items and reset size of internal array container.
		/// </summary>
		[Tested]
		public virtual void Clear()
		{
			updatecheck();
			array = new T[8];
			size = 0;
		}


		/// <summary>
		/// Create an array containing (copies) of the items of this collection in enumeration order.
		/// </summary>
		/// <returns>The new array</returns>
		[Tested]
		public override T[] ToArray()
		{
			T[] res = new T[size];

			Array.Copy(array, res, size);
			return res;
		}


		/// <summary>
		/// Perform an internal consistency (invariant) test on the array base.
		/// </summary>
		/// <returns>True if test succeeds.</returns>
		[Tested]
		public virtual bool Check()
		{
			bool retval = true;

			if (size > array.Length)
			{
				Console.WriteLine("Bad size ({0}) > array.Length ({1})", size, array.Length);
				return false;
			}

			for (int i = 0; i < size; i++)
			{
				if ((object)(array[i]) == null)
				{
					Console.WriteLine("Bad element: null at index {0}", i);
					return false;
				}
			}

			return retval;
		}

		#endregion

		#region IDirectedCollection<T> Members

		/// <summary>
		/// Create a directed collection with the same contents as this one, but 
		/// opposite enumeration sequence.
		/// </summary>
		/// <returns>The mirrored collection.</returns>
		[Tested]
		public IDirectedCollectionValue<T> Backwards() { return this[0, size].Backwards(); }

		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Create an enumerator for this array based collection.
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<T> GetEnumerator()
		{
			int thestamp = stamp, theend = size + offset, thestart = offset;

			for (int i = thestart; i < theend; i++)
			{
				modifycheck(thestamp);
				yield return array[i];
			}
		}
		#endregion

		#region Range nested class
		/// <summary>
		/// A helper class for defining results of interval queries on array based collections.
		/// </summary>
		protected class Range: CollectionValueBase<T>, IDirectedCollectionValue<T>
		{
			int start, count, delta, stamp;

			ArrayBase<T> thebase;


			internal Range(ArrayBase<T> thebase, int start, int count, bool forwards)
			{
				this.thebase = thebase;  stamp = thebase.stamp;
				delta = forwards ? 1 : -1;
				this.start = start + thebase.offset; this.count = count;
			}


			/// <summary>
			/// 
			/// </summary>
			/// <value>The number of items in the range</value>
			[Tested]
			public override int Count { [Tested]get { thebase.modifycheck(stamp); return count; } }

            /// <summary>
            /// The value is symbolic indicating the type of asymptotic complexity
            /// in terms of the size of this collection (worst-case or amortized as
            /// relevant).
            /// </summary>
            /// <value>A characterization of the speed of the 
            /// <code>Count</code> property in this collection.</value>
            public override Speed CountSpeed { get { thebase.modifycheck(stamp); return Speed.Constant; } }

            /// <summary>
			/// Create an enumerator for this range of an array based collection.
			/// </summary>
			/// <returns>The enumerator</returns>
			[Tested]
			public override MSG.IEnumerator<T> GetEnumerator()
			{
				for (int i = 0; i < count; i++)
				{
					thebase.modifycheck(stamp);
					yield return thebase.array[start + delta * i];
				}
			}


			/// <summary>
			/// Create a araay collection range with the same contents as this one, but 
			/// opposite enumeration sequence.
			/// </summary>
			/// <returns>The mirrored collection.</returns>
			[Tested]
			public IDirectedCollectionValue<T> Backwards()
			{
				thebase.modifycheck(stamp);

				Range res = (Range)MemberwiseClone();

				res.delta = -delta;
				res.start = start + (count - 1) * delta;
				return res;
			}


			IDirectedEnumerable<T> C5.IDirectedEnumerable<T>.Backwards()
			{
				return Backwards();
			}


			/// <summary>
			/// <code>Forwards</code> if same, else <code>Backwards</code>
			/// </summary>
			/// <value>The enumeration direction relative to the original collection.</value>
			[Tested]
			public EnumerationDirection Direction
			{
				[Tested]
				get
				{
					thebase.modifycheck(stamp);
					return delta > 0 ? EnumerationDirection.Forwards : EnumerationDirection.Backwards;
				}
			}
		}
		#endregion
	}

	#endregion

	#region Sorting
	/// <summary>
	/// A utility class with functions for sorting arrays with respect to an IComparer&lt;T&gt;
	/// </summary>
	public class Sorting
	{
		/// <summary>
		/// Sort part of array in place using IntroSort
		/// </summary>
		/// <param name="a">Array to sort</param>
		/// <param name="f">Index of first position to sort</param>
		/// <param name="b">Index of first position beyond the part to sort</param>
		/// <param name="c">IComparer&lt;T&gt; to sort by</param>
		[Tested]
		public static void IntroSort<T>(T[] a, int f, int b, IComparer<T> c)
		{
			new Sorter<T>(a, c).IntroSort(f, b);
		}


		/// <summary>
		/// Sort part of array in place using Insertion Sort
		/// </summary>
		/// <param name="a">Array to sort</param>
		/// <param name="f">Index of first position to sort</param>
		/// <param name="b">Index of first position beyond the part to sort</param>
		/// <param name="c">IComparer&lt;T&gt; to sort by</param>
		[Tested]
		public static void InsertionSort<T>(T[] a, int f, int b, IComparer<T> c)
		{
			new Sorter<T>(a, c).InsertionSort(f, b);
		}


		/// <summary>
		/// Sort part of array in place using Heap Sort
		/// </summary>
		/// <param name="a">Array to sort</param>
		/// <param name="f">Index of first position to sort</param>
		/// <param name="b">Index of first position beyond the part to sort</param>
		/// <param name="c">IComparer&lt;T&gt; to sort by</param>
		[Tested]
		public static void HeapSort<T>(T[] a, int f, int b, IComparer<T> c)
		{
			new Sorter<T>(a, c).HeapSort(f, b);
		}


		class Sorter<T>
		{
			T[] a;

			IComparer<T> c;


			internal Sorter(T[] a, IComparer<T> c) { this.a = a; this.c = c; }


			internal void IntroSort(int f, int b)
			{
				if (b - f > 31)
				{
					int depth_limit = (int)Math.Floor(2.5 * Math.Log(b - f, 2));

					introSort(f, b, depth_limit);
				}
				else
					InsertionSort(f, b);
			}


			private void introSort(int f, int b, int depth_limit)
			{
				const int size_threshold = 14;//24;

				if (depth_limit-- == 0)
					HeapSort(f, b);
				else if (b - f <= size_threshold)
					InsertionSort(f, b);
				else
				{
					int p = partition(f, b);

					introSort(f, p, depth_limit);
					introSort(p, b, depth_limit);
				}
			}


			private int compare(T i1, T i2) { return c.Compare(i1, i2); }


			private int partition(int f, int b)
			{
				int bot = f, mid = (b + f) / 2, top = b - 1;
				T abot = a[bot], amid = a[mid], atop = a[top];

				if (compare(abot, amid) < 0)
				{
					if (compare(atop, abot) < 0)//atop<abot<amid
						{ a[top] = amid; amid = a[mid] = abot; a[bot] = atop; }
					else if (compare(atop, amid) < 0) //abot<=atop<amid
						{ a[top] = amid; amid = a[mid] = atop; }
					//else abot<amid<=atop
				}
				else
				{
					if (compare(amid, atop) > 0) //atop<amid<=abot
						{ a[bot] = atop; a[top] = abot; }
					else if (compare(abot, atop) > 0) //amid<=atop<abot
						{ a[bot] = amid; amid = a[mid] = atop; a[top] = abot; }
					else //amid<=abot<=atop
						{ a[bot] = amid; amid = a[mid] = abot; }
				}

				int i = bot, j = top;

				while (true)
				{
					while (compare(a[++i], amid) < 0);

					while (compare(amid, a[--j]) < 0);

					if (i < j)
					{
						T tmp = a[i]; a[i] = a[j]; a[j] = tmp;
					}
					else
						return i;
				}
			}


			internal void InsertionSort(int f, int b)
			{
				for (int j = f + 1; j < b; j++)
				{
					T key = a[j], other;
					int i = j - 1;

					if (c.Compare(other = a[i], key) > 0)
					{
						a[j] = other;
						while (i > f && c.Compare(other = a[i - 1], key) > 0) { a[i--] = other; }

						a[i] = key;
					}
				}
			}


			internal void HeapSort(int f, int b)
			{
				for (int i = (b + f) / 2; i >= f; i--) heapify(f, b, i);

				for (int i = b - 1; i > f; i--)
				{
					T tmp = a[f]; a[f] = a[i]; a[i] = tmp;
					heapify(f, i, f);
				}
			}


			private void heapify(int f, int b, int i)
			{
				T pv = a[i], lv, rv, max = pv;
				int j = i, maxpt = j;

				while (true)
				{
					int l = 2 * j - f + 1, r = l + 1;

					if (l < b && compare(lv = a[l], max) > 0) { maxpt = l; max = lv; }

					if (r < b && compare(rv = a[r], max) > 0) { maxpt = r; max = rv; }

					if (maxpt == j)
						break;

					a[j] = max;
					max = pv;
					j = maxpt;
				}

				if (j > i)
					a[j] = pv;
			}
		}
	}

	#endregion

	#region Random
	/// <summary>
	/// A modern random number generator based on (whatever)
	/// </summary>
	public class C5Random : Random
	{
		private uint[] Q = new uint[16];

		private uint c = 362436, i = 15;


		private uint Cmwc()
		{
			ulong t, a = 487198574UL;
			uint x, r = 0xfffffffe;

			i = (i + 1) & 15;
			t = a * Q[i] + c;
			c = (uint)(t >> 32);
			x = (uint)(t + c);
			if (x < c)
			{
				x++;
				c++;
			}

			return Q[i] = r - x;
		}


		/// <summary>
		/// Get a new random System.Double value
		/// </summary>
		/// <returns>The random double</returns>
		public override double NextDouble()
		{
			return Cmwc() / 4294967296.0;
		}


		/// <summary>
		/// Get a new random System.Double value
		/// </summary>
		/// <returns>The random double</returns>
		protected override double Sample()
		{
			return NextDouble();
		}


		/// <summary>
		/// Get a new random System.Int32 value
		/// </summary>
		/// <returns>The random int</returns>
		public override int Next()
		{
			return (int)Cmwc();
		}
		

		/// <summary>
		/// Get a random non-negative integer less than a given upper bound
		/// </summary>
		/// <param name="max">The upper bound (exclusive)</param>
		/// <returns></returns>
		public override int Next(int max)
		{
			if (max < 0)
				throw new ApplicationException("max must be non-negative");

			return (int)(Cmwc() / 4294967296.0 * max);
		}


		/// <summary>
		/// Get a random integer between two given bounds
		/// </summary>
		/// <param name="min">The lower bound (inclusive)</param>
		/// <param name="max">The upper bound (exclusive)</param>
		/// <returns></returns>
		public override int Next(int min, int max)
		{
			if (min > max)
				throw new ApplicationException("min must be less than or equal to max");

			return min + (int)(Cmwc() / 4294967296.0 * max);
		}

		/// <summary>
		/// Fill a array of byte with random bytes
		/// </summary>
		/// <param name="buffer">The array to fill</param>
		public override void NextBytes(byte[] buffer)
		{
			for (int i = 0, length = buffer.Length; i < length; i++)
				buffer[i] = (byte)Cmwc();
		}


		/// <summary>
		/// Create a random number generator seed by system time.
		/// </summary>
		public C5Random() : this(DateTime.Now.Ticks)
		{
		}


		/// <summary>
		/// Create a random number generator with a given seed
		/// </summary>
		/// <param name="seed">The seed</param>
		public C5Random(long seed)
		{
			if (seed == 0)
				throw new ApplicationException("Seed must be non-zero");

			uint j = (uint)(seed & 0xFFFFFFFF);

			for (int i = 0; i < 16; i++)
			{
				j ^= j << 13;
				j ^= j >>17;
				j ^= j << 5;
				Q[i] = j;
			}

			Q[15] = (uint)(seed ^ (seed >> 32));
		}
	}

	#endregion

	#region Custom code attributes

	/// <summary>
	/// A custom attribute to mark methods and properties as being tested 
	/// sufficiently in the regression test suite.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class TestedAttribute: Attribute
	{

		/// <summary>
		/// Optional reference to test case
		/// </summary>
		[Tested]
		public string via;


		/// <summary>
		/// Pretty print attribute value
		/// </summary>
		/// <returns>"Tested via " + via</returns>
		[Tested]
		public override string ToString() { return "Tested via " + via; }
	}

	#endregion
}