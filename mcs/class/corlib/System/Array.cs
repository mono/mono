//
// System.Array.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public abstract class Array : ICloneable {
		public int lower_bound = 0;
		private int length;
		private int rank;

		// Properties
		public int Length {
			get {
				return length;
			}
		}

		public int Rank {
			get {
				return rank;
			}
		}

		// Methods
		public static int BinarySearch (Array array, object value)
		{
			return BinarySearch (array, this.lower_bound, this.length, value, null);
		}

		public static int BinarySearch (Array array, object value, IComparer comparer)
		{
			return BinarySearch (array, this.lower_bound, this.length, value, comparer);
		}

		public static int BinarySearch (Array array, int index, int length, object value)
		{
			return BinarySearch (array, index, length, value, null);
		}

		public static int BinarySearch (Array array, int index, int length, object value, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.lower_bound || length < 0)
				throw new ArgumentOutOfRangeException ();

			if (index + length > array.lower_bound + array.Length)
				throw new ArgumentException ();

			if (comparer == null && !(value is IComparable))
				throw new ArgumentException ();

			// FIXME: Throw an ArgumentException if comparer
			// is null and value is not of the same type as the
			// elements of array.

			for (int i = 0; i < length; i++) {
				int result;

				if (comparer == null && !(array.GetValue(index + i) is IComparable))
					throw new ArgumentException ();

				if (comparer == null)
					result = comparer.Compare(value, array.GetValue(index + i));
				else
					result = value.CompareTo(array.GetValue(index + i));

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

			if (index < this.lower_bound || length < 0 ||
			    index + length > this.lower_bound + this.length)
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++) {
				if (array.GetValue(index + i) is bool)
					array.SetValue(index + i, false);
				else if (array.GetValue(index + i) is ValueType)
					array.SetValue(index + i, 0);
				else
					array.SetValue(index + i, null);
			}
		}

		public virtual object Clone ()
		{
			Array a = new Array;

			for (int i = 0; i < this.length; i++) {
				int index = this.lower_bound + i;

				a.SetValue(index, this.GetValue(index));
			}

			return a;
		}

		public static void Copy (Array source, Array dest, int length)
		{
			Copy (source, source.lower_bound, dest, dest.lower_bound, length);
		}

		public static void Copy (Array source, int source_idx, Array dest, int dest_idx, int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException ();

			if (source == null || dest == null)
				throw new ArgumentNullException ();

			if (source_idx < source.lower_bound || source_idx + length > source.lower_bound + source.Length || dest_idx < dest.lower_bound || dest_idx + length > dest.lower_bound + dest.Length)
				throw new ArgumentException ();

			if (source.Rank != dest.Rank)
				throw new RankException ();

			for (int i = 0; i < length; i++) {
				int index = source.lower_bound + i;

				dest.SetValue(index, source.GetValue(index));
			}
		}

		[MethodImplAttribute(InternalCall)]
		private object InternalGetValue (int index);

		public object GetValue (int index)
		{
			if (this.rank > 1)
				throw new ArgumentException ();

			if (index < this.lower_bound ||
			    index > this.lower_bound + this.length)
				throw new ArgumentOutOfRangeException ();
			
			return InternalGetValue (index);
		}

		[MethodImplAttribute(InternalCall)]
		private void InternalSetValue (int index, object value);

		public void SetValue (int index, object value)
		{
			if (this.rank > 1)
				throw new ArgumentException ();

			if (index < this.lower_bound ||
			    index > this.lower_bound + this.length)
				throw new ArgumentOutOfRangeException ();

			InternalSetValue (index, value);
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
	
			if (length < 0 || index < array.lower_bound || index > array.lower_bound + length)
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++) {
				if (array.GetValue(index + i) == value)
					return index + i;
			}

			return array.lower_bound - 1;
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
			int occurrance = array.lower_bound - 1;

			if (array == null)
				throw new ArgumentNullException ();
	
			if (length < 0 || index < array.lower_bound || index > array.lower_bound + length)
				throw new ArgumentOutOfRangeException ();

			for (int i = 0; i < length; i++) {
				if (array.GetValue(index + i) == value)
					occurrance = index + i;
			}

			return occurrance;
		}
	
		public static void Reverse (Array array)
		{
			Reverse (array, array.lower_bound, array.Length);
		}

		public static void Reverse (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ();

			if (array.Rank > 1)
				throw new RankException ();

			if (index < array.lower_bound || length < 0)
				throw new ArgumentOutOfRangeException ();

			if (index + length > array.lower_bound + array.Length)
				throw new ArgumentException ();

			for (int i = 0; i < length/2; i++) {
				object tmp;

				tmp = array.GetValue(index + i);
				array.SetValue(index + i, array.GetValue(index + length - i - 1));
				array.SetValue(index + length - i - 1, tmp);
			}
		}

		public static void Sort (Array array)
		{
			Sort (array, null, array.lower_bound, array.Length, null);
		}

		public static void Sort (Array keys, Array items)
		{
			Sort (keys, items, keys.lower_bound, keys.Length, null);
		}

		public static void Sort (Array array, IComparer comparer)
		{
			Sort (array, null, array.lower_bound, array.Length, comparer);
		}

		public static void Sort (Array array, int index, int length)
		{
			Sort (array, null, index, length, null);
		}

		public static void Sort (Array keys, Array items, IComparer comparer)
		{
			Sort (keys, items, keys.lower_bound, keys.Length, comparer);
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
			// TODO
		}
