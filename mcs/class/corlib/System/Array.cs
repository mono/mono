//
// System.Array.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Collections;
using System.Runtime.CompilerServices;

namespace System
{

	public abstract class Array : ICloneable, ICollection
	{
		// Constructor		
		protected Array ()
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

		// InternalCall Methods
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int GetRank ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int GetLength (int dimension);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int GetLowerBound (int dimension);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern object GetValue (int[] idxs);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void SetValue (object value, int[] idxs);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static Array CreateInstance(Type elementType, int[] lengths, int [] bounds);

		// Methods Implementations

		public int Count {
			get {
				return Length;
			}
		}

		[MonoTODO]
		public bool IsSynchronized {
			get {
				// FIXME?
				return false;
			}
		}

		[MonoTODO]
		public object SyncRoot {
			get {
				// FIXME
				return null;
			}
		}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator ()
		{
			// FIXME
			return null;
		}

		public int GetUpperBound (int dimension)
		{
			return GetLowerBound (dimension) +
				GetLength (dimension);
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
			
			return CreateInstance (elementType, lengths, bounds);
		}
		
		public static Array CreateInstance(Type elementType, int l1, int l2)
		{
			int[] lengths = new int [2];
			int[] bounds = null;
			
			lengths [0] = l1;
			lengths [1] = l2;
			
			return CreateInstance (elementType, lengths, bounds);
		}

		public static Array CreateInstance(Type elementType, int l1, int l2, int l3)
		{
			int[] lengths = new int [3];
			int[] bounds = null;
			
			lengths [0] = l1;
			lengths [1] = l2;
			lengths [2] = l3;
		
			return CreateInstance (elementType, lengths, bounds);
		}

		public static Array CreateInstance(Type elementType, int[] lengths)
		{
			int[] bounds = null;
			
			return CreateInstance (elementType, lengths, bounds);
		}

		
		public static int BinarySearch (Array array, object value)
		{
			return BinarySearch (array, array.GetLowerBound (0), array.GetLength (0),
					     value, null);
		}

		public static int BinarySearch (Array array, object value, IComparer comparer)
		{
			return BinarySearch (array, array.GetLowerBound (0), array.GetLength (0),
					     value, comparer);
		}

		public static int BinarySearch (Array array, int index, int length, object value)
		{
			return BinarySearch (array, index, length, value, null);
		}

		public static int BinarySearch (Array array, int index,
						int length, object value,
						IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.GetLowerBound (0) || length < 0)
				throw new ArgumentOutOfRangeException ();

			if (index + length > array.GetUpperBound (0))
				throw new ArgumentException ();

			if (comparer == null && !(value is IComparable))
				throw new ArgumentException ();

			// FIXME: Throw an ArgumentException if comparer
			// is null and value is not of the same type as the
			// elements of array.

			for (int i = 0; i < length; i++) 
			{
				int result;

				if (comparer == null && !(array.GetValue(index + i) is IComparable))
					throw new ArgumentException ();

				if (comparer == null)
					result = (value as IComparable).CompareTo(array.GetValue(index + i));
				else
					result = comparer.Compare(value, array.GetValue(index + i));

				if (result == 0)
					return index + i;
				else if (result < 0)
					return ~(index + i);
			}

			return ~(index + length);
		}

		public static void Clear (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.GetLowerBound (0) || length < 0 ||
				index + length > array.GetUpperBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++) 
			{
				if (array.GetValue(index + i) is bool)
					array.SetValue(false, index + i);
				else if (array.GetValue(index + i) is ValueType)
					array.SetValue(0, index + i);
				else
					array.SetValue(null, index + i);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public virtual extern object Clone ();

		public static void Copy (Array source, Array dest, int length)
		{
			// I don't know how to handle this ?
			if (source.Rank > 1 || dest.Rank > 1)
				throw new RankException ();

			Copy (source, source.GetLowerBound (0), dest, dest.GetLowerBound (0), length);			
		}

		public static void Copy (Array source, int source_idx, Array dest, int dest_idx, int length)
		{
			// I don't know how to handle this ?
			if (source.Rank > 1 || dest.Rank > 1)
				throw new RankException ();

			if (length < 0)
				throw new ArgumentOutOfRangeException ();

			if (source == null || dest == null)
				throw new ArgumentNullException ();

			if (source_idx < source.GetLowerBound (0) ||
			    source_idx + length > source.GetUpperBound (0) ||
			    dest_idx < dest.GetLowerBound (0) || dest_idx + length > dest.GetUpperBound (0))
				throw new ArgumentException ();

			if (source.Rank != dest.Rank)
				throw new RankException ();

			for (int i = 0; i < length; i++) 
			{
				int index = source.GetLowerBound (0) + i + source_idx;

				dest.SetValue(source.GetValue(index), dest_idx + i + dest.GetLowerBound (0));
			}
			
		}
		
		public static int IndexOf (Array array, object value)
		{
			return IndexOf (array, value, 0, array.Length);
		}

		public static int IndexOf (Array array, object value, int index)
		{
			return IndexOf (array, value, index, array.Length - index);
		}
		
		public static int IndexOf (Array array, object value, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			if (length < 0 || index < array.GetLowerBound (0) ||
			    index > array.GetUpperBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++)
			{
				if (array.GetValue(index + i) == value)
					return index + i;
			}

			return array.GetLowerBound (0) - 1;
		}

		public static int LastIndexOf (Array array, object value)
		{
			return LastIndexOf (array, value, 0, array.Length);
		}

		public static int LastIndexOf (Array array, object value, int index)
		{
			return LastIndexOf (array, value, index, array.Length - index);
		}
		
		public static int LastIndexOf (Array array, object value, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();
	
			if (length < 0 || index < array.GetLowerBound (0) ||
			    index > array.GetUpperBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = length - 1; i >= 0; i--)
			{
				if (array.GetValue(index + i) == value)
					return index + i;
			}

			return array.GetLowerBound (0) - 1;
		}

		public static void Reverse (Array array)
		{
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

			if (index + length > array.GetUpperBound (0))
				throw new ArgumentException ();

			for (int i = 0; i < length/2; i++)
			{
				object tmp;

				tmp = array.GetValue (index + i);
				array.SetValue(array.GetValue (index + length - i - 1), index + i);
				array.SetValue(tmp, index + length - i - 1);
			}
		}		
		
		public static void Sort (Array array)
		{
			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), null);
		}

		public static void Sort (Array keys, Array items)
		{
			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), null);
		}

		public static void Sort (Array array, IComparer comparer)
		{
			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), comparer);
		}

		public static void Sort (Array array, int index, int length)
		{
			Sort (array, null, index, length, null);
		}

		public static void Sort (Array keys, Array items, IComparer comparer)
		{
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
			int pivot;
			int low = low0;
			int high = high0;
			
			if (keys.Rank > 1 || (items != null && items.Rank > 1))
				throw new RankException ();

			if (low >= high)
				return;

			pivot = (low + high) / 2;

			if (compare (keys.GetValue (low), keys.GetValue (pivot), comparer) > 0)
				swap (keys, items, low, pivot);
			
			if (compare (keys.GetValue (pivot), keys.GetValue (high), comparer) > 0)
				swap (keys, items, pivot, high);

			while (low < high)
			{
				// Move the walls in
				while (low < high && compare (keys.GetValue (low), keys.GetValue (pivot), comparer) < 0)
					low++;
				while (low < high && compare (keys.GetValue (pivot), keys.GetValue (high), comparer) < 0)
					high--;

				if (low < high)
				{
					swap (keys, items, low, high);
					low++;
					high--;
				}
			}

			qsort (keys, items, low0, low - 1, comparer);
			qsort (keys, items, high + 1, high0, comparer);
		}

		private static void swap (Array keys, Array items, int i, int j)
		{
			object tmp;

			tmp = keys.GetValue (i);
			keys.SetValue (keys.GetValue (j), i);
			keys.SetValue (tmp, j);

			if (items != null)
			{
				tmp = items.GetValue (i);
				items.SetValue (items.GetValue (j), i);
				items.SetValue (tmp, j);
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
			Copy (this, this.GetLowerBound(0), array, index, this.GetUpperBound (0));
		}
	}
}
