
// System.Array.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Collections;
using System.Runtime.CompilerServices;

namespace System
{

	[Serializable]
	public abstract class Array : ICloneable, ICollection, IList, IEnumerable
	{
		// Constructor		
		private Array ()
		{
			/* empty */
		}
		
		// Properties
		public int Length 
		{
			get 
			{
				int length = this.GetLength (0);

				for (int i = 1; i < this.Rank; i++) {
					length *= this.GetLength (i); 
				}
				
				return length;
			}
		}

		public int Rank 
		{
			get
			{
				return this.GetRank ();
			}
		}

		// IList interface
		object IList.this [int index] {
			get {
				return GetValueImpl (index);
			} 
			set {
				SetValueImpl (value, index);
			}
		}

		int IList.Add (object value) {
			throw new NotSupportedException ();
		}

		void IList.Clear () {
			Array.Clear (this, this.GetLowerBound(0), this.Length);
		}

		bool IList.Contains (object value) {
			if (this.Rank > 1)
				throw new RankException ("Only single dimension arrays are supported.");

			int length = this.Length;
			for (int i = 0; i < length; i++) {
				if (Object.Equals (value, this.GetValueImpl (i)))
					return true;
			}
			return false;
		}

		int IList.IndexOf (object value) {
			if (this.Rank > 1)
				throw new RankException ();

			int length = this.Length;
			for (int i = 0; i < length; i++) {
				if (Object.Equals (value, this.GetValueImpl (i)))
					// array index may not be zero-based.
					// use lower bound
					return i + this.GetLowerBound (0);
			}

			int retVal;
			unchecked {
				// lower bound may be MinValue
				retVal = this.GetLowerBound (0) - 1;
			}

			return retVal;
		}

		void IList.Insert (int index, object value) {
			throw new NotSupportedException ();
		}

		void IList.Remove (object value) {
			throw new NotSupportedException ();
		}

		void IList.RemoveAt (int index) {
			throw new NotSupportedException ();
		}

		// InternalCall Methods
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern int GetRank ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int GetLength (int dimension);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int GetLowerBound (int dimension);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern object GetValue (int[] idxs);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void SetValue (object value, int[] idxs);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern object GetValueImpl (int pos);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern void SetValueImpl (object value, int pos);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void FastCopy (Array source, int source_idx, Array dest, int dest_idx, int length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static Array CreateInstanceImpl(Type elementType, int[] lengths, int [] bounds);

		// Properties
		int ICollection.Count {
			get {
				return Length;
			}
		}

		[MonoTODO]
		public virtual bool IsSynchronized {
			get {
				// FIXME?
				return false;
			}
		}

		[MonoTODO]
		public virtual object SyncRoot {
			get {
				// FIXME
				return null;
			}
		}

		public virtual bool IsFixedSize 
		{
			get {
				return true;
			}
		}

		public virtual bool IsReadOnly 
		{
			get{
				return false;
			}
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return new SimpleEnumerator(this);
		}

		public int GetUpperBound (int dimension)
		{
			return GetLowerBound (dimension) +
				GetLength (dimension) - 1;
		}

		public object GetValue (int idx)
		{
			int[] ind = new int [1];

			ind [0] = idx;

			return GetValue (ind);
		}

		public object GetValue (int idx1, int idx2)
		{
			int[] ind = new int [2];

			ind [0] = idx1;
			ind [1] = idx2;

			return GetValue (ind);
		}

		public object GetValue (int idx1, int idx2, int idx3)
		{
			int[] ind = new int [3];

			ind [0] = idx1;
			ind [1] = idx2;
			ind [2] = idx3;

			return GetValue (ind);
		}

		// This function is currently unused, but just in case we need it later on ... */
		internal int IndexToPos (int[] idxs)
		{
			if (idxs == null)
				throw new ArgumentNullException ();

			if ((idxs.Rank != 1) || (idxs.Length != Rank))
				throw new ArgumentException ();

			if ((idxs [0] < GetLowerBound (0)) || (idxs [0] > GetUpperBound (0)))
				throw new IndexOutOfRangeException();

			int pos = idxs [0] - GetLowerBound (0);
			for (int i = 1; i < Rank; i++) {
				if ((idxs [i] < GetLowerBound (i)) || (idxs [i] > GetUpperBound (i)))
					throw new IndexOutOfRangeException();

				pos *= GetLength (i);
				pos += idxs [i] - GetLowerBound (i);
			}

			return pos;
		}

		public void SetValue (object value, int idx)
		{
			int[] ind = new int [1];

			ind [0] = idx;

			SetValue (value, ind);
		}
		
		public void SetValue (object value, int idx1, int idx2)
		{
			int[] ind = new int [2];

			ind [0] = idx1;
			ind [1] = idx2;

			SetValue (value, ind);
		}

		public void SetValue (object value, int idx1, int idx2, int idx3)
		{
			int[] ind = new int [3];

			ind [0] = idx1;
			ind [1] = idx2;
			ind [2] = idx3;

			SetValue (value, ind);
		}

		public static Array CreateInstance(Type elementType, int length)
		{
			int[] lengths = new int [1];
			int[] bounds = null;
			
			lengths [0] = length;
			
			return CreateInstanceImpl (elementType, lengths, bounds);
		}
		
		public static Array CreateInstance(Type elementType, int l1, int l2)
		{
			int[] lengths = new int [2];
			int[] bounds = null;
			
			lengths [0] = l1;
			lengths [1] = l2;
			
			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance(Type elementType, int l1, int l2, int l3)
		{
			int[] lengths = new int [3];
			int[] bounds = null;
			
			lengths [0] = l1;
			lengths [1] = l2;
			lengths [2] = l3;
		
			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance(Type elementType, int[] lengths)
		{
			int[] bounds = null;
			
			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance(Type elementType, int[] lengths, int [] bounds)
		{
			if (bounds == null)
				throw new ArgumentNullException("bounds");

			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		
		public static int BinarySearch (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Rank > 1)
				throw new RankException();

			if (!(value is IComparable))
				throw new ArgumentException("value does not support IComparable");

			return BinarySearch (array, array.GetLowerBound (0), array.GetLength (0),
					     value, null);
		}

		public static int BinarySearch (Array array, object value, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Rank > 1)
				throw new RankException();

			if ((comparer == null) && !(value is IComparable))
				throw new ArgumentException("comparer is null and value does not support IComparable");

			return BinarySearch (array, array.GetLowerBound (0), array.GetLength (0),
					     value, comparer);
		}

		public static int BinarySearch (Array array, int index, int length, object value)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Rank > 1)
				throw new RankException();

			if (index < array.GetLowerBound (0))
				throw new ArgumentOutOfRangeException("index is less than the lower bound of array.");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length is less than zero.");

			if (index + length > array.GetLowerBound (0) + array.GetLength (0))
				throw new ArgumentException("index and length do not specify a valid range in array.");
			if (!(value is IComparable))
				throw new ArgumentException("value does not support IComparable");

			return BinarySearch (array, index, length, value, null);
		}

		public static int BinarySearch (Array array, int index,
						int length, object value,
						IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.GetLowerBound (0))
				throw new ArgumentOutOfRangeException("index is less than the lower bound of array.");
			if (length < 0)
				throw new ArgumentOutOfRangeException("length is less than zero.");

			if (index + length > array.GetLowerBound (0) + array.GetLength (0))
				throw new ArgumentException("index and length do not specify a valid range in array.");

			if ((comparer == null) && !(value is IComparable))
				throw new ArgumentException("comparer is null and value does not support IComparable");

			// cache this in case we need it
			IComparable valueCompare = value as IComparable;

			int iMin = index;
			int iMax = index + length;
			int iCmp = 0;
			try
			{
				// there's a subtle balance here between
				// 1) the while condition
				// 2) the rounding down of the '/ 2'
				// 3) the asymetrical recursion
				// 4) the fact that iMax starts beyond the end of the array
				while (iMin < iMax)
				{
					int iMid = (iMin + iMax) / 2;
					object elt = array.GetValueImpl (iMid);

					// this order is from MSDN
					if (comparer != null)
						iCmp = comparer.Compare (value, elt);
					else
					{
						IComparable eltCompare = elt as IComparable;
						if (eltCompare != null)
							iCmp = -eltCompare.CompareTo (value);
						else
							iCmp = valueCompare.CompareTo (elt);
					}

					if (iCmp == 0)
						return iMid;
					else if (iCmp < 0)
						iMax = iMid;
					else
						iMin = iMid + 1;	// compensate for the rounding down
				}
			}
			catch (InvalidCastException e)
			{
				throw new ArgumentException ("array", e);
			}

			if (iCmp > 0)
				return ~iMax;
			else
				return ~iMin;
		}

		public static void Clear (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.GetLowerBound (0) || length < 0 ||
				index + length > array.GetUpperBound (0) + 1)
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++) 
			{
				array.SetValueImpl(null, index + i);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public virtual extern object Clone ();

		public static void Copy (Array source, Array dest, int length)
		{
			// need these checks here because we are going to use
			// GetLowerBound() on source and dest.
			if (source == null)
				throw new ArgumentNullException ("null");

			if (dest == null)
				throw new ArgumentNullException ("dest");

			Copy (source, source.GetLowerBound (0), dest, dest.GetLowerBound (0), length);			
		}

		public static void Copy (Array source, int source_idx, Array dest, int dest_idx, int length)
		{
			if (source == null)
				throw new ArgumentNullException ("null");

			if (dest == null)
				throw new ArgumentNullException ("dest");

			if (length < 0)
				throw new ArgumentOutOfRangeException ("length");

			if (source_idx < 0)
				throw new ArgumentException ("source_idx");

			if (dest_idx < 0)
				throw new ArgumentException ("dest_idx");

			int source_pos = source_idx - source.GetLowerBound (0);
			int dest_pos = dest_idx - dest.GetLowerBound (0);


			if (source_pos + length > source.Length || dest_pos + length > dest.Length)
				throw new ArgumentException ("length");

			if (source.Rank != dest.Rank)
				throw new RankException ();

			Type src_type = source.GetType ().GetElementType ();
			Type dst_type = dest.GetType ().GetElementType ();

			if (src_type == dst_type) {
				FastCopy (source, source_pos, dest, dest_pos, length);
				return;
			}

			if (!Object.ReferenceEquals (source, dest) || source_pos > dest_pos)
			{
				for (int i = 0; i < length; i++) 
				{
					Object srcval = source.GetValueImpl (source_pos + i);

					try {
						dest.SetValueImpl (srcval, dest_pos + i);
					} catch {
						if ((dst_type.IsValueType || dst_type.Equals (typeof (String))) &&
							(src_type.Equals (typeof (Object))))
							throw new InvalidCastException ();
						else
							throw new ArrayTypeMismatchException (
								String.Format ("(Types: source={0};  target={1})", src_type.FullName, dst_type.FullName));
					}
				}
			}
			else
			{
				for (int i = length - 1; i >= 0; i--) 
				{
					Object srcval = source.GetValueImpl (source_pos + i);

					try {
						dest.SetValueImpl (srcval, dest_pos + i);
					} catch {
						if ((dst_type.IsValueType || dst_type.Equals (typeof (String))) &&
							(src_type.Equals (typeof (Object))))
							throw new InvalidCastException ();
						else
							throw new ArrayTypeMismatchException (
								String.Format ("(Types: source={0};  target={1})", src_type.FullName, dst_type.FullName));
					}
				}
			}
		}
		
		public static int IndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			return IndexOf (array, value, 0, array.Length);
		}

		public static int IndexOf (Array array, object value, int index)
		{
			if (array == null)
				throw new ArgumentNullException ();

			return IndexOf (array, value, index, array.Length - index);
		}
		
		public static int IndexOf (Array array, object value, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			if (array.Rank > 1)
				throw new RankException ();

			if (length < 0 || index < array.GetLowerBound (0) ||
			    index+length-1 > array.GetUpperBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++)
			{
				if (Object.Equals (array.GetValueImpl(index + i), value))
					return index + i;
			}

			return array.GetLowerBound (0) - 1;
		}

		[MonoTODO]
		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public static int LastIndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			return LastIndexOf (array, value, array.Length-1);
		}

		public static int LastIndexOf (Array array, object value, int index)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			return LastIndexOf (array, value, index, index-array.GetLowerBound(0)+1);
		}
		
		public static int LastIndexOf (Array array, object value, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			if (array.Rank > 1)
				throw new RankException ();

			if (length < 0 || index-length+1 < array.GetLowerBound (0) ||
			    index > array.GetUpperBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = index; i >= index-length+1; i--)
			{
				if (Object.Equals (array.GetValueImpl(i), value))
					return i;
			}

			return array.GetLowerBound (0) - 1;
		}

		public static void Reverse (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ();

			Reverse (array, array.GetLowerBound (0), array.GetLength (0));
		}

		public static void Reverse (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.GetLowerBound (0) || length < 0)
				throw new ArgumentOutOfRangeException ();

			if (index + length > array.GetUpperBound (0) + 1)
				throw new ArgumentException ();

			for (int i = 0; i < length/2; i++)
			{
				object tmp;

				tmp = array.GetValueImpl (index + i);
				array.SetValueImpl(array.GetValueImpl (index + length - i - 1), index + i);
				array.SetValueImpl(tmp, index + length - i - 1);
			}
		}		
		
		public static void Sort (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ();

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), null);
		}

		public static void Sort (Array keys, Array items)
		{
			if (keys == null)
				throw new ArgumentNullException ();

			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), null);
		}

		public static void Sort (Array array, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ();

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), comparer);
		}

		public static void Sort (Array array, int index, int length)
		{
			Sort (array, null, index, length, null);
		}

		public static void Sort (Array keys, Array items, IComparer comparer)
		{
			if (keys == null)
				throw new ArgumentNullException ();

			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), comparer);
		}

		public static void Sort (Array keys, Array items, int index, int length)
		{
			Sort (keys, items, index, length, null);
		}

		public static void Sort (Array array, int index, int length, IComparer comparer)
		{
			Sort (array, null, index, length, comparer);
		}

		public static void Sort (Array keys, Array items, int index, int length, IComparer comparer)
		{
			int low0 = index;
			int high0 = index + length - 1;

			qsort (keys, items, index, index + length - 1, comparer);
		}

		private static void qsort (Array keys, Array items, int low0, int high0, IComparer comparer)
		{
			if (keys == null)
				throw new ArgumentNullException ();

			if (keys.Rank > 1 || (items != null && items.Rank > 1))
				throw new RankException ();

			if (low0 >= high0)
				return;

			int low = low0;
			int high = high0;

			object objPivot = keys.GetValueImpl ((low + high) / 2);

			while (low <= high)
			{
				// Move the walls in
				while (low < high0 && compare (keys.GetValueImpl (low), objPivot, comparer) < 0)
					++low;
				while (high > low0 && compare (objPivot, keys.GetValueImpl (high), comparer) < 0)
					--high;

				if (low <= high)
				{
					swap (keys, items, low, high);
					++low;
					--high;
				}
			}

			if (low0 < high)
				qsort (keys, items, low0, high, comparer);
			if (low < high0)
				qsort (keys, items, low, high0, comparer);
		}

		private static void swap (Array keys, Array items, int i, int j)
		{
			object tmp;

			tmp = keys.GetValueImpl (i);
			keys.SetValueImpl (keys.GetValue (j), i);
			keys.SetValueImpl (tmp, j);

			if (items != null)
			{
				tmp = items.GetValueImpl (i);
				items.SetValueImpl (items.GetValueImpl (j), i);
				items.SetValueImpl (tmp, j);
			}
		}

		private static int compare (object value1, object value2, IComparer comparer)
		{
			if (comparer == null)
				return ((IComparable) value1).CompareTo(value2);
			else
				return comparer.Compare(value1, value2);
		}
	
		public virtual void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ();

			// The order of these exception checks may look strange,
			// but that's how the microsoft runtime does it.
			if (this.Rank > 1)
				throw new RankException ();
			if (index + this.GetLength (0) > array.GetLowerBound (0) + array.GetLength (0))
				throw new ArgumentException ();
			if (array.Rank > 1)
				throw new RankException ();
			if (index < 0)
				throw new ArgumentOutOfRangeException ();

			Copy (this, this.GetLowerBound(0), array, index, this.GetLength (0));
		}

		internal class SimpleEnumerator : IEnumerator {
			Array enumeratee;
			int currentpos;
			int length;

			public SimpleEnumerator (Array arrayToEnumerate) {
				this.enumeratee = arrayToEnumerate;
				this.currentpos = -1;
				this.length = arrayToEnumerate.Length;
			}

			public object Current {
				get {
			 		// Exception messages based on MS implementation
					if (currentpos < 0 ) {
						throw new InvalidOperationException
							("Enumeration has not started");
					}
					if  (currentpos >= length) {
						throw new InvalidOperationException
							("Enumeration has already ended");
					}
					// Current should not increase the position. So no ++ over here.
					return enumeratee.GetValueImpl(currentpos);
				}
			}

			public bool MoveNext() {
				//The docs say Current should throw an exception if last
				//call to MoveNext returned false. This means currentpos
				//should be set to length when returning false.
					if (currentpos < length) {
						currentpos++;
					}
				if(currentpos < length)
					return true;
				else
					return false;
			}

			public void Reset() {
				currentpos= -1;
			}
		}
	}
}
