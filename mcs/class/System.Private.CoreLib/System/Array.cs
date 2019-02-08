using Internal.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	partial class Array
	{
		[StructLayout(LayoutKind.Sequential)]
		private class RawData
		{
			public IntPtr Bounds;
			public IntPtr Count;
			public byte Data;
		}

		public int Length {
			get {
				int length = GetLength (0);

				for (int i = 1; i < Rank; i++) {
					length *= GetLength (i);
				}
				return length;
			}
		}

		public long LongLength {
			get {
				long length = GetLength (0);

				for (int i = 1; i < Rank; i++) {
					length *= GetLength (i);
				}
				return length;
			}
		}

		public int Rank {
			get {
				return GetRank ();
			}
		}

		public static void Clear (Array array, int index, int length)
		{
			if (array == null)
				ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
			if (length < 0)
				ThrowHelper.ThrowIndexOutOfRangeException();

			int low = array.GetLowerBound (0);
			if (index < low)
				ThrowHelper.ThrowIndexOutOfRangeException();

			index = index - low;

			// re-ordered to avoid possible integer overflow
			if (index > array.Length - length)
				ThrowHelper.ThrowIndexOutOfRangeException();

			ClearInternal (array, index, length);
		}

		public static Array CreateInstance (Type elementType, int length)
		{
			int[] lengths = {length};

			return CreateInstance (elementType, lengths);
		}

		public static Array CreateInstance (Type elementType, int length1, int length2)
		{
			int[] lengths = {length1, length2};

			return CreateInstance (elementType, lengths);
		}

		public static Array CreateInstance (Type elementType, int length1, int length2, int length3)
		{
			int[] lengths = {length1, length2, length3};

			return CreateInstance (elementType, lengths);
		}

		public static Array CreateInstance (Type elementType, params int[] lengths)
		{
			if (elementType == null)
				throw new ArgumentNullException ("elementType");
			if (lengths == null)
				throw new ArgumentNullException ("lengths");

			if (lengths.Length > 255)
				throw new TypeLoadException ();

			int[] bounds = null;

			elementType = elementType.UnderlyingSystemType as RuntimeType;
			if (elementType == null)
				throw new ArgumentException ("Type must be a type provided by the runtime.", "elementType");
			if (elementType.Equals (typeof (void)))
				throw new NotSupportedException ("Array type can not be void");
			if (elementType.ContainsGenericParameters)
				throw new NotSupportedException ("Array type can not be an open generic type");
			
			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance (Type elementType, int[] lengths, int [] lowerBounds)
		{
			if (elementType == null)
				throw new ArgumentNullException ("elementType");
			if (lengths == null)
				throw new ArgumentNullException ("lengths");
			if (lowerBounds == null)
				throw new ArgumentNullException ("lowerBounds");

			elementType = elementType.UnderlyingSystemType as RuntimeType;
			if (elementType == null)
				throw new ArgumentException ("Type must be a type provided by the runtime.", "elementType");
			if (elementType.Equals (typeof (void)))
				throw new NotSupportedException ("Array type can not be void");
			if (elementType.ContainsGenericParameters)
				throw new NotSupportedException ("Array type can not be an open generic type");

			if (lengths.Length < 1)
				throw new ArgumentException ("Arrays must contain >= 1 elements.");

			if (lengths.Length != lowerBounds.Length)
				throw new ArgumentException ("Arrays must be of same size.");

			for (int j = 0; j < lowerBounds.Length; j ++) {
				if (lengths [j] < 0)
					throw new ArgumentOutOfRangeException ("lengths", "Each value has to be >= 0.");
				if ((long)lowerBounds [j] + (long)lengths [j] > (long)Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("lengths", "Length + bound must not exceed Int32.MaxValue.");
			}

			if (lengths.Length > 255)
				throw new TypeLoadException ();

			return CreateInstanceImpl (elementType, lengths, lowerBounds);
		}

		static int IndexOfImpl<T>(T[] array, T value, int startIndex, int count)
		{
			throw new NotImplementedException ();
		}

		static int LastIndexOfImpl<T>(T[] array, T value, int startIndex, int count)
		{
			throw new NotImplementedException ();
		}

		static void SortImpl (Array keys, Array items, int index, int length, IComparer comparer)
		{
			throw new NotImplementedException ();
		}

		public int GetUpperBound (int dimension)
		{
			return GetLowerBound (dimension) + GetLength (dimension) - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref byte GetRawSzArrayData()
		{
			return ref Unsafe.As<RawData>(this).Data;
		}

		internal ref byte GetRawArrayData ()
		{
			throw new NotImplementedException ();
		}

		//
		// Moved value from instance into target of different type with no checks (JIT intristics)
		//
		// Restrictions:
		//
		// S and R must either:
		// 	 both be blitable valuetypes
		// 	 both be reference types (IOW, an unsafe cast)
		// S and R cannot be float or double
		// S and R must either:
		//	 both be a struct
		// 	 both be a scalar
		// S and R must either:
		// 	 be of same size
		// 	 both be a scalar of size <= 4
		//
		internal static R UnsafeMov<S,R> (S instance)
		{
			return (R)(object) instance;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void ClearInternal (Array a, int index, int count);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Array CreateInstanceImpl (Type elementType, int[] lengths, int[] bounds);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool FastCopy (Array source, int source_idx, Array dest, int dest_idx, int length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern int GetRank ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLength (int dimension);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLowerBound (int dimension);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern object GetValue (params int[] indices);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern void SetValue (object value, params int[] indices);

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void GetGenericValueImpl<T> (int pos, out T value);

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void SetGenericValueImpl<T> (int pos, ref T value);

		/*
		 * These methods are used to implement the implicit generic interfaces 
		 * implemented by arrays in NET 2.0.
		 * Only make those methods generic which really need it, to avoid
		 * creating useless instantiations.
		 */
		internal int InternalArray__ICollection_get_Count ()
		{
			return Length;
		}

		internal bool InternalArray__ICollection_get_IsReadOnly ()
		{
			return true;
		}

		internal IEnumerator<T> InternalArray__IEnumerable_GetEnumerator<T> ()
		{
			throw new NotImplementedException ();
		}

		internal void InternalArray__ICollection_Clear ()
		{
			ThrowHelper.ThrowNotSupportedException (ExceptionResource.NotSupported_ReadOnlyCollection);
		}

		internal void InternalArray__ICollection_Add<T> (T item)
		{
			ThrowHelper.ThrowNotSupportedException (ExceptionResource.NotSupported_FixedSizeCollection);
		}

		internal bool InternalArray__ICollection_Remove<T> (T item)
		{
			ThrowHelper.ThrowNotSupportedException (ExceptionResource.NotSupported_FixedSizeCollection);
			return default;
		}

		internal bool InternalArray__ICollection_Contains<T> (T item)
		{
			return IndexOf (this, item, 0, Length) >= 0;
		}

		internal void InternalArray__ICollection_CopyTo<T> (T[] array, int arrayIndex)
		{
			Copy (this, GetLowerBound (0), array, arrayIndex, Length);
		}

		internal T InternalArray__IReadOnlyList_get_Item<T> (int index)
		{
			if ((uint)index >= (uint)Length)
				ThrowHelper.ThrowArgumentOutOfRange_IndexException ();

			T value;
			GetGenericValueImpl (index, out value);
			return value;
		}

		internal int InternalArray__IReadOnlyCollection_get_Count ()
		{
			return Length;
		}

		internal void InternalArray__Insert<T> (int index, T item)
		{
			ThrowHelper.ThrowNotSupportedException (ExceptionResource.NotSupported_FixedSizeCollection);
		}

		internal void InternalArray__RemoveAt (int index)
		{
			ThrowHelper.ThrowNotSupportedException (ExceptionResource.NotSupported_FixedSizeCollection);
		}

		internal int InternalArray__IndexOf<T> (T item)
		{
			return IndexOf (this, item, 0, Length);
		}

		internal T InternalArray__get_Item<T> (int index)
		{
			if ((uint)index >= (uint)Length)
				ThrowHelper.ThrowArgumentOutOfRange_IndexException ();

			T value;
			GetGenericValueImpl (index, out value);
			return value;
		}

		internal void InternalArray__set_Item<T> (int index, T item)
		{
			if ((uint)index >= (uint)Length)
				ThrowHelper.ThrowArgumentOutOfRange_IndexException();

			object[] oarray = this as object [];
			if (oarray != null) {
				oarray [index] = (object)item;
				return;
			}
			SetGenericValueImpl (index, ref item);
		}
	}
}