//
// System.Array.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Jeffrey Stedfast (fejj@novell.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2001-2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.ConstrainedExecution;

namespace System
{
	public abstract partial class Array
	{
		// Constructor
		private Array ()
		{
		}
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

		// adapted to the Mono array layout
		[StructLayout(LayoutKind.Sequential)]
		private class RawData
		{
			public IntPtr Bounds;
			public IntPtr Count;
			public byte Data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref byte GetRawSzArrayData()
		{
			return ref Unsafe.As<RawData>(this).Data;
		}

		internal IEnumerator<T> InternalArray__IEnumerable_GetEnumerator<T> ()
		{
			if (Length == 0)
				return EmptyInternalEnumerator<T>.Value;
			else
				return new InternalEnumerator<T> (this);
		}

		internal void InternalArray__ICollection_Clear ()
		{
			throw new NotSupportedException ("Collection is read-only");
		}

		internal void InternalArray__ICollection_Add<T> (T item)
		{
			throw new NotSupportedException ("Collection is of a fixed size");
		}

		internal bool InternalArray__ICollection_Remove<T> (T item)
		{
			throw new NotSupportedException ("Collection is of a fixed size");
		}

		internal bool InternalArray__ICollection_Contains<T> (T item)
		{
			if (this.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			int length = this.Length;
			for (int i = 0; i < length; i++) {
				T value;
				GetGenericValueImpl (i, out value);
				if (item == null){
					if (value == null) {
						return true;
					}

					continue;
				}

				if (item.Equals (value)) {
					return true;
				}
			}

			return false;
		}

		internal void InternalArray__ICollection_CopyTo<T> (T[] array, int arrayIndex)
		{
			Copy (this, GetLowerBound (0), array, arrayIndex, Length);
		}

		internal T InternalArray__IReadOnlyList_get_Item<T> (int index)
		{
			if (unchecked ((uint) index) >= unchecked ((uint) Length))
				throw new ArgumentOutOfRangeException ("index");

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
			throw new NotSupportedException ("Collection is of a fixed size");
		}

		internal void InternalArray__RemoveAt (int index)
		{
			throw new NotSupportedException ("Collection is of a fixed size");
		}

		internal int InternalArray__IndexOf<T> (T item)
		{
			if (this.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			int length = this.Length;
			for (int i = 0; i < length; i++) {
				T value;
				GetGenericValueImpl (i, out value);
				if (item == null){
					if (value == null)
						return i + this.GetLowerBound (0);

					continue;
				}
				if (value.Equals (item))
					// array index may not be zero-based.
					// use lower bound
					return i + this.GetLowerBound (0);
			}

			unchecked {
				// lower bound may be MinValue
				return this.GetLowerBound (0) - 1;
			}
		}

		internal T InternalArray__get_Item<T> (int index)
		{
			if (unchecked ((uint) index) >= unchecked ((uint) Length))
				throw new ArgumentOutOfRangeException ("index");

			T value;
			GetGenericValueImpl (index, out value);
			return value;
		}

		internal void InternalArray__set_Item<T> (int index, T item)
		{
			if (unchecked ((uint) index) >= unchecked ((uint) Length))
				throw new ArgumentOutOfRangeException ("index");

			object[] oarray = this as object [];
			if (oarray != null) {
				oarray [index] = (object)item;
				return;
			}
			SetGenericValueImpl (index, ref item);
		}

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern void GetGenericValueImpl<T> (int pos, out T value);

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern void SetGenericValueImpl<T> (int pos, ref T value);

		internal struct InternalEnumerator<T> : IEnumerator<T>
		{
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			readonly Array array;
			int idx;

			internal InternalEnumerator (Array array)
			{
				this.array = array;
				idx = NOT_STARTED;
			}

			public void Dispose ()
			{
			}

			public bool MoveNext ()
			{
				if (idx == NOT_STARTED)
					idx = array.Length;

				return idx != FINISHED && -- idx != FINISHED;
			}

			public T Current {
				get {
					if (idx == NOT_STARTED)
						throw new InvalidOperationException ("Enumeration has not started. Call MoveNext");
					if (idx == FINISHED)
						throw new InvalidOperationException ("Enumeration already finished");

					return array.InternalArray__get_Item<T> (array.Length - 1 - idx);
				}
			}

			void IEnumerator.Reset ()
			{
				idx = NOT_STARTED;
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}
		}

		// Properties
		public int Length {
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
			get {
				int length = this.GetLength (0);

				for (int i = 1; i < this.Rank; i++) {
					length *= this.GetLength (i); 
				}
				return length;
			}
		}

		public int Rank {
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
			get {
				return this.GetRank ();
			}
		}

		internal class EmptyInternalEnumerator<T> : IEnumerator<T>
		{
			public static readonly EmptyInternalEnumerator<T> Value = new EmptyInternalEnumerator<T> ();

			public void Dispose ()
			{
				return;
			}

			public bool MoveNext ()
			{
				return false;
			}

			public T Current {
				get {
					throw new InvalidOperationException ("Enumeration has not started. Call MoveNext");
				}
			}

			object IEnumerator.Current {
				get {
					return Current;
				}
			}

			void IEnumerator.Reset ()
			{
				return;
			}
		}

		// InternalCall Methods
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern int GetRank ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLength (int dimension);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLowerBound (int dimension);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern object GetValue (params int[] indices);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern void SetValue (object value, params int[] indices);

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern object GetValueImpl (int pos);

		// CAUTION! No bounds checking!
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern void SetValueImpl (object value, int pos);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool FastCopy (Array source, int source_idx, Array dest, int dest_idx, int length);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static Array CreateInstanceImpl (Type elementType, int[] lengths, int[] bounds);

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public int GetUpperBound (int dimension)
		{
			return GetLowerBound (dimension) + GetLength (dimension) - 1;
		}

		public object GetValue (int index)
		{
			if (Rank != 1)
				throw new ArgumentException (SR.Arg_RankMultiDimNotSupported);

			var lb  = GetLowerBound (0);
			if (index < lb || index > GetUpperBound (0))
				throw new IndexOutOfRangeException (Locale.GetText (
					"Index has to be between upper and lower bound of the array."));

			if (GetType ().GetElementType ().IsPointer)
				throw new NotSupportedException ("Type is not supported.");

			return GetValueImpl (index - lb);
		}

		public object GetValue (int index1, int index2)
		{
			int[] ind = {index1, index2};
			return GetValue (ind);
		}

		public object GetValue (int index1, int index2, int index3)
		{
			int[] ind = {index1, index2, index3};
			return GetValue (ind);
		}

		public void SetValue (object value, int index)
		{
			if (Rank != 1)
				throw new ArgumentException (SR.Arg_RankMultiDimNotSupported);

			var lb  = GetLowerBound (0);
			if (index < lb || index > GetUpperBound (0))
				throw new IndexOutOfRangeException (Locale.GetText (
					"Index has to be >= lower bound and <= upper bound of the array."));

			if (GetType ().GetElementType ().IsPointer)
				throw new NotSupportedException ("Type is not supported.");

			SetValueImpl (value, index - lb);
		}

		public void SetValue (object value, int index1, int index2)
		{
			int[] ind = {index1, index2};
			SetValue (value, ind);
		}

		public void SetValue (object value, int index1, int index2, int index3)
		{
			int[] ind = {index1, index2, index3};
			SetValue (value, ind);
		}

		internal static Array UnsafeCreateInstance(Type elementType, int[] lengths, int[] lowerBounds)
		{
			return CreateInstance(elementType, lengths, lowerBounds);
		}

		internal static Array UnsafeCreateInstance (Type elementType, int length1, int length2)
		{
			return CreateInstance (elementType, length1, length2);
		}

		internal static Array UnsafeCreateInstance (Type elementType, params int[] lengths)
		{
			return CreateInstance(elementType, lengths);
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
				throw new ArgumentException (Locale.GetText ("Arrays must contain >= 1 elements."));

			if (lengths.Length != lowerBounds.Length)
				throw new ArgumentException (Locale.GetText ("Arrays must be of same size."));

			for (int j = 0; j < lowerBounds.Length; j ++) {
				if (lengths [j] < 0)
					throw new ArgumentOutOfRangeException ("lengths", Locale.GetText (
						"Each value has to be >= 0."));
				if ((long)lowerBounds [j] + (long)lengths [j] > (long)Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("lengths", Locale.GetText (
						"Length + bound must not exceed Int32.MaxValue."));
			}

			if (lengths.Length > 255)
				throw new TypeLoadException ();

			return CreateInstanceImpl (elementType, lengths, lowerBounds);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static void Clear (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (length < 0)
				throw new IndexOutOfRangeException ("length < 0");

			int low = array.GetLowerBound (0);
			if (index < low)
				throw new IndexOutOfRangeException ("index < lower bound");
			index = index - low;

			// re-ordered to avoid possible integer overflow
			if (index > array.Length - length)
				throw new IndexOutOfRangeException ("index + length > size");

			ClearInternal (array, index, length);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void ClearInternal (Array a, int index, int count);

		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy (Array sourceArray, Array destinationArray, int length)
		{
			// need these checks here because we are going to use
			// GetLowerBound() on source and dest.
			if (sourceArray == null)
				throw new ArgumentNullException ("sourceArray");

			if (destinationArray == null)
				throw new ArgumentNullException ("destinationArray");

			Copy (sourceArray, sourceArray.GetLowerBound (0), destinationArray,
				destinationArray.GetLowerBound (0), length);
		}

		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, Cer.MayFail)]
		public static void Copy (Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
		{
			if (sourceArray == null)
				throw new ArgumentNullException ("sourceArray");

			if (destinationArray == null)
				throw new ArgumentNullException ("destinationArray");

			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));;

			if (sourceArray.Rank != destinationArray.Rank)
				throw new RankException(SR.Rank_MultiDimNotSupported);

			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException ("sourceIndex", Locale.GetText (
					"Value has to be >= 0."));;

			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException ("destinationIndex", Locale.GetText (
					"Value has to be >= 0."));;

			if (FastCopy (sourceArray, sourceIndex, destinationArray, destinationIndex, length))
				return;

			int source_pos = sourceIndex - sourceArray.GetLowerBound (0);
			int dest_pos = destinationIndex - destinationArray.GetLowerBound (0);

			if (dest_pos < 0)
				throw new ArgumentOutOfRangeException ("destinationIndex", "Index was less than the array's lower bound in the first dimension.");

			// re-ordered to avoid possible integer overflow
			if (source_pos > sourceArray.Length - length)
				throw new ArgumentException ("length");

			if (dest_pos > destinationArray.Length - length) {
				throw new ArgumentException ("Destination array was not long enough. Check destIndex and length, and the array's lower bounds", nameof (destinationArray));
			}

			Type src_type = sourceArray.GetType ().GetElementType ();
			Type dst_type = destinationArray.GetType ().GetElementType ();
			var dst_type_vt = dst_type.IsValueType;

			if (!Object.ReferenceEquals (sourceArray, destinationArray) || source_pos > dest_pos) {
				for (int i = 0; i < length; i++) {
					Object srcval = sourceArray.GetValueImpl (source_pos + i);

					if (srcval == null && dst_type_vt)
						throw new InvalidCastException ();

					try {
						destinationArray.SetValueImpl (srcval, dest_pos + i);
					} catch (ArgumentException) {
						throw CreateArrayTypeMismatchException ();
					} catch (InvalidCastException) {
						if (CanAssignArrayElement (src_type, dst_type))
							throw;
						throw CreateArrayTypeMismatchException ();
					}
				}
			}
			else {
				for (int i = length - 1; i >= 0; i--) {
					Object srcval = sourceArray.GetValueImpl (source_pos + i);

					try {
						destinationArray.SetValueImpl (srcval, dest_pos + i);
					} catch (ArgumentException) {
						throw CreateArrayTypeMismatchException ();
					} catch {
						if (CanAssignArrayElement (src_type, dst_type))
							throw;

						throw CreateArrayTypeMismatchException ();
					}
				}
			}
		}

		static ArrayTypeMismatchException CreateArrayTypeMismatchException ()
		{
			return new ArrayTypeMismatchException ();
		}

		static bool CanAssignArrayElement (Type source, Type target)
		{
			if (source.IsValueType)
				return source.IsAssignableFrom (target);

			if (source.IsInterface)
				return !target.IsValueType;

			if (target.IsInterface)
				return !source.IsValueType;

			return source.IsAssignableFrom (target) || target.IsAssignableFrom (source);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		//
		// The constrained copy should guarantee that if there is an exception thrown
		// during the copy, the destination array remains unchanged.
		// This is related to System.Runtime.Reliability.CER
		public static void ConstrainedCopy (Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
		{
			Copy (sourceArray, sourceIndex, destinationArray, destinationIndex, length);
		}

		public static T[] Empty<T>()
		{
			return EmptyArray<T>.Value;
		}

		public void Initialize()
		{
			return;
		}

		static int IndexOfImpl<T>(T[] array, T value, int startIndex, int count)
		{
			return EqualityComparer<T>.Default.IndexOf (array, value, startIndex, count);
		}

		static int LastIndexOfImpl<T>(T[] array, T value, int startIndex, int count)
		{
			return EqualityComparer<T>.Default.LastIndexOf (array, value, startIndex, count);
		}

		static void SortImpl (Array keys, Array items, int index, int length, IComparer comparer)
		{
			Object[] objKeys = keys as Object[];
			Object[] objItems = null;
			if (objKeys != null)
				objItems = items as Object[];

			if (objKeys != null && (items == null || objItems != null)) {
				SorterObjectArray sorter = new SorterObjectArray(objKeys, objItems, comparer);
				sorter.Sort(index, length);
			} else {
				SorterGenericArray sorter = new SorterGenericArray(keys, items, comparer);
				sorter.Sort(index, length);
			}
		}

		#region Unsafe array operations

		//
		// Loads array index with no safety checks (JIT intristics)
		//
		internal static T UnsafeLoad<T> (T[] array, int index) {
			return array [index];
		}

		//
		// Stores values at specified array index with no safety checks (JIT intristics)
		//
		internal static void UnsafeStore<T> (T[] array, int index, T value) {
			array [index] = value;
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
		internal static R UnsafeMov<S,R> (S instance) {
			return (R)(object) instance;
		}

		#endregion

		internal sealed class FunctorComparer<T> : IComparer<T> {
			Comparison<T> comparison;

			public FunctorComparer(Comparison<T> comparison) {
				this.comparison = comparison;
			}

			public int Compare(T x, T y) {
				return comparison(x, y);
			}
		}

		partial class ArrayEnumerator
		{
			public Object Current {
				get {
					if (_index < 0) throw new InvalidOperationException (SR.InvalidOperation_EnumNotStarted);
					if (_index >= _endIndex) throw new InvalidOperationException (SR.InvalidOperation_EnumEnded);
					if (_index == 0 && _array.GetType ().GetElementType ().IsPointer) throw new NotSupportedException ("Type is not supported.");
					return _array.GetValueImpl(_index);
				}
			}
		}
	}
}
