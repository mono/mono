//
// System.Array.cs
//
// Authors:
//   Joe Shaw (joe@ximian.com)
//   Martin Baulig (martin@gnome.org)
//   Dietmar Maurer (dietmar@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001-2003 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
#if NET_2_0
using System.Collections.Generic;
#endif
namespace System
{
	[Serializable]
	[MonoTODO ("We are doing way to many double/triple exception checks for the overloaded functions")]
	[MonoTODO ("Sort overloads parameter checks are VERY inconsistent")]
	public abstract class Array : ICloneable, ICollection, IList, IEnumerable
	{
		// Constructor
		private Array ()
		{
		}

		// Properties
		public int Length {
			get {
				int length = this.GetLength (0);

				for (int i = 1; i < this.Rank; i++) {
					length *= this.GetLength (i); 
				}
				return length;
			}
		}

#if NET_1_1
		[ComVisible (false)]
		public long LongLength {
			get { return Length; }
		}
#endif

		public int Rank {
			get {
				return this.GetRank ();
			}
		}

		// IList interface
		object IList.this [int index] {
			get {
				if (unchecked ((uint) index) >= unchecked ((uint) Length))
					throw new ArgumentOutOfRangeException ("index");
				return GetValueImpl (index);
			} 
			set {
				if (unchecked ((uint) index) >= unchecked ((uint) Length))
					throw new ArgumentOutOfRangeException ("index");
				SetValueImpl (value, index);
			}
		}

		int IList.Add (object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Clear ()
		{
			Array.Clear (this, this.GetLowerBound (0), this.Length);
		}

		bool IList.Contains (object value)
		{
			if (this.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			int length = this.Length;
			for (int i = 0; i < length; i++) {
				if (Object.Equals (value, this.GetValueImpl (i)))
					return true;
			}
			return false;
		}

		int IList.IndexOf (object value)
		{
			if (this.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

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

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Remove (object value)
		{
			throw new NotSupportedException ();
		}

		void IList.RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}

		// InternalCall Methods
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int GetRank ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLength (int dimension);

#if NET_1_1
		[ComVisible (false)]
		public long GetLongLength (int dimension)
		{
			return GetLength (dimension);
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int GetLowerBound (int dimension);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern object GetValue (int[] indices);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern void SetValue (object value, int[] indices);

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

		// Properties
		int ICollection.Count {
			get {
				return Length;
			}
		}

		public virtual bool IsSynchronized {
			get {
				return false;
			}
		}

		public virtual object SyncRoot {
			get {
				return this;
			}
		}

		public virtual bool IsFixedSize {
			get {
				return true;
			}
		}

		public virtual bool IsReadOnly {
			get {
				return false;
			}
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return new SimpleEnumerator (this);
		}

		public int GetUpperBound (int dimension)
		{
			return GetLowerBound (dimension) + GetLength (dimension) - 1;
		}

		public object GetValue (int index)
		{
			if (Rank != 1)
				throw new ArgumentException (Locale.GetText ("Array was not a one-dimensional array."));
			if (index < GetLowerBound (0) || index > GetUpperBound (0))
				throw new IndexOutOfRangeException (Locale.GetText (
					"Index has to be between upper and lower bound of the array."));

			return GetValueImpl (index - GetLowerBound (0));
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

#if NET_1_1
		[ComVisible (false)]
		public object GetValue (long index)
		{
			if (index < 0 || index > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			return GetValue ((int) index);
		}

		[ComVisible (false)]
		public object GetValue (long index1, long index2)
		{
			if (index1 < 0 || index1 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index1", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index2 < 0 || index2 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index2", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			return GetValue ((int) index1, (int) index2);
		}

		[ComVisible (false)]
		public object GetValue (long index1, long index2, long index3)
		{
			if (index1 < 0 || index1 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index1", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index2 < 0 || index2 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index2", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index3 < 0 || index3 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index3", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			return GetValue ((int) index1, (int) index2, (int) index3);
		}

		[ComVisible (false)]
		public void SetValue (object value, long index)
		{
			if (index < 0 || index > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			SetValue (value, (int) index);
		}

		[ComVisible (false)]
		public void SetValue (object value, long index1, long index2)
		{
			if (index1 < 0 || index1 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index1", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index2 < 0 || index2 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index2", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			int[] ind = {(int) index1, (int) index2};
			SetValue (value, ind);
		}

		[ComVisible (false)]
		public void SetValue (object value, long index1, long index2, long index3)
		{
			if (index1 < 0 || index1 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index1", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index2 < 0 || index2 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index2", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			if (index3 < 0 || index3 > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index3", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			int[] ind = {(int) index1, (int) index2, (int) index3};
			SetValue (value, ind);
		}
#endif

//		// This function is currently unused, but just in case we need it later on ... */
//		internal int IndexToPos (int[] idxs)
//		{
//			if (idxs == null)
//				throw new ArgumentNullException ("idxs");
//
//			if ((idxs.Rank != 1) || (idxs.Length != Rank))
//				throw new ArgumentException ();
//
//			if ((idxs [0] < GetLowerBound (0)) || (idxs [0] > GetUpperBound (0)))
//				throw new IndexOutOfRangeException();
//
//			int pos = idxs [0] - GetLowerBound (0);
//			for (int i = 1; i < Rank; i++) {
//				if ((idxs [i] < GetLowerBound (i)) || (idxs [i] > GetUpperBound (i)))
//					throw new IndexOutOfRangeException();
//
//				pos *= GetLength (i);
//				pos += idxs [i] - GetLowerBound (i);
//			}
//
//			return pos;
//		}

		public void SetValue (object value, int index)
		{
			if (Rank != 1)
				throw new ArgumentException (Locale.GetText ("Array was not a one-dimensional array."));
			if (index < GetLowerBound (0) || index > GetUpperBound (0))
				throw new IndexOutOfRangeException (Locale.GetText (
					"Index has to be >= lower bound and <= upper bound of the array."));

			SetValueImpl (value, index - GetLowerBound (0));
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

		public static Array CreateInstance (Type elementType, int length)
		{
			int[] lengths = {length};
			int[] bounds = null;

			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance (Type elementType, int length1, int length2)
		{
			int[] lengths = {length1, length2};
			int[] bounds = null;

			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance (Type elementType, int length1, int length2, int length3)
		{
			int[] lengths = {length1, length2, length3};
			int[] bounds = null;

			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance (Type elementType, int[] lengths)
		{
			if (elementType == null)
				throw new ArgumentNullException ("elementType");
			if (lengths == null)
				throw new ArgumentNullException ("lengths");

			if (lengths.Length > 255)
				throw new TypeLoadException ();

			int[] bounds = null;
			
			return CreateInstanceImpl (elementType, lengths, bounds);
		}

		public static Array CreateInstance (Type elementType, int[] lengths, int [] bounds)
		{
			if (elementType == null)
				throw new ArgumentNullException ("elementType");
			if (lengths == null)
				throw new ArgumentNullException ("lengths");
			if (bounds == null)
				throw new ArgumentNullException ("bounds");

			if (lengths.Length < 1)
				throw new ArgumentException (Locale.GetText ("Arrays must contain >= 1 elements."));

			if (lengths.Length != bounds.Length)
				throw new ArgumentException (Locale.GetText ("Arrays must be of same size."));

			for (int j = 0; j < bounds.Length; j ++) {
				if (lengths [j] < 0)
					throw new ArgumentOutOfRangeException ("lengths", Locale.GetText (
						"Each value has to be >= 0."));
				if ((long)bounds [j] + (long)lengths [j] > (long)Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("lengths", Locale.GetText (
						"Length + bound must not exceed Int32.MaxValue."));
			}

			if (lengths.Length > 255)
				throw new TypeLoadException ();

			return CreateInstanceImpl (elementType, lengths, bounds);
		}

#if NET_1_1
		static int [] GetIntArray (long [] values)
		{
			int len = values.Length;
			int [] ints = new int [len];
			for (int i = 0; i < len; i++) {
				long current = values [i];
				if (current < 0 || current > (long) Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("values", Locale.GetText (
						"Each value has to be >= 0 and <= Int32.MaxValue."));

				ints [i] = (int) current;
			}
			return ints;
		}

		public static Array CreateInstance (Type elementType, params long [] lengths)
		{
			if (lengths == null) {
				// LAMESPEC: Docs say we should throw a ArgumentNull, but .NET
				// 1.1 actually throws a NullReference.
				throw new NullReferenceException (Locale.GetText ("'lengths' cannot be null."));
			}
			return CreateInstance (elementType, GetIntArray (lengths));
		}

		[ComVisible (false)]
		public object GetValue (long [] indices)
		{
			if (indices == null) {
				// LAMESPEC: Docs say we should throw a ArgumentNull, but .NET
				// 1.1 actually throws a NullReference.
				throw new NullReferenceException (Locale.GetText ("'indices' cannot be null."));
			}
			return GetValue (GetIntArray (indices));
		}

		[ComVisible (false)]
		public void SetValue (object value, long [] indices)
		{
			if (indices == null) {
				// LAMESPEC: Docs say we should throw a ArgumentNull, but .NET
				// 1.1 actually throws a NullReference.
				throw new NullReferenceException (Locale.GetText ("'indices' cannot be null."));
			}
			SetValue (value, GetIntArray (indices));
		}
#endif

		public static int BinarySearch (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (value == null)
				return -1;

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if (!(value is IComparable))
				throw new ArgumentException (Locale.GetText ("value does not support IComparable."));

			return DoBinarySearch (array, array.GetLowerBound (0), array.GetLength (0), value, null);
		}

		public static int BinarySearch (Array array, object value, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if ((comparer == null) && (value != null) && !(value is IComparable))
				throw new ArgumentException (Locale.GetText (
					"comparer is null and value does not support IComparable."));

			return DoBinarySearch (array, array.GetLowerBound (0), array.GetLength (0), value, comparer);
		}

		public static int BinarySearch (Array array, int index, int length, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if (index < array.GetLowerBound (0))
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"index is less than the lower bound of array."));
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));
			// re-ordered to avoid possible integer overflow
			if (index > array.GetLowerBound (0) + array.GetLength (0) - length)
				throw new ArgumentException (Locale.GetText (
					"index and length do not specify a valid range in array."));
			if ((value != null) && (!(value is IComparable)))
				throw new ArgumentException (Locale.GetText (
					"value does not support IComparable"));

			return DoBinarySearch (array, index, length, value, null);
		}

		public static int BinarySearch (Array array, int index, int length, object value, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if (index < array.GetLowerBound (0))
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"index is less than the lower bound of array."));
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));
			// re-ordered to avoid possible integer overflow
			if (index > array.GetLowerBound (0) + array.GetLength (0) - length)
				throw new ArgumentException (Locale.GetText (
					"index and length do not specify a valid range in array."));

			if ((comparer == null) && (value != null) && !(value is IComparable))
				throw new ArgumentException (Locale.GetText (
					"comparer is null and value does not support IComparable."));

			return DoBinarySearch (array, index, length, value, comparer);
		}

		static int DoBinarySearch (Array array, int index, int length, object value, IComparer comparer)
		{
			// cache this in case we need it
			if (comparer == null)
				comparer = Comparer.Default;

			int iMin = index;
			// Comment from Tum (tum@veridicus.com):
			// *Must* start at index + length - 1 to pass rotor test co2460binarysearch_iioi
			int iMax = index + length - 1;
			int iCmp = 0;
			try {
				while (iMin <= iMax) {
					int iMid = (iMin + iMax) / 2;
					object elt = array.GetValueImpl (iMid);

					iCmp = comparer.Compare (value, elt);

					if (iCmp == 0)
						return iMid;
					else if (iCmp < 0)
						iMax = iMid - 1;
					else
						iMin = iMid + 1; // compensate for the rounding down
				}
			}
			catch (Exception e) {
				throw new InvalidOperationException (Locale.GetText ("Comparer threw an exception."), e);
			}

			return ~iMin;
		}

		public static void Clear (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length < 0");

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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public virtual extern object Clone ();

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

		public static void Copy (Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length)
		{
			if (sourceArray == null)
				throw new ArgumentNullException ("sourceArray");

			if (destinationArray == null)
				throw new ArgumentNullException ("destinationArray");

			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));;

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

			// re-ordered to avoid possible integer overflow
			if (source_pos > sourceArray.Length - length || dest_pos > destinationArray.Length - length)
				throw new ArgumentException ("length");

			if (sourceArray.Rank != destinationArray.Rank)
				throw new RankException (Locale.GetText ("Arrays must be of same size."));

			Type src_type = sourceArray.GetType ().GetElementType ();
			Type dst_type = destinationArray.GetType ().GetElementType ();

			if (!Object.ReferenceEquals (sourceArray, destinationArray) || source_pos > dest_pos) {
				for (int i = 0; i < length; i++) {
					Object srcval = sourceArray.GetValueImpl (source_pos + i);

					try {
						destinationArray.SetValueImpl (srcval, dest_pos + i);
					} catch {
						if ((dst_type.IsValueType || dst_type.Equals (typeof (String))) &&
							(src_type.Equals (typeof (Object))))
							throw new InvalidCastException ();
						else
							throw new ArrayTypeMismatchException (String.Format (Locale.GetText (
								"(Types: source={0};  target={1})"), src_type.FullName, dst_type.FullName));
					}
				}
			}
			else {
				for (int i = length - 1; i >= 0; i--) {
					Object srcval = sourceArray.GetValueImpl (source_pos + i);

					try {
						destinationArray.SetValueImpl (srcval, dest_pos + i);
					} catch {
						if ((dst_type.IsValueType || dst_type.Equals (typeof (String))) &&
							(src_type.Equals (typeof (Object))))
							throw new InvalidCastException ();
						else
							throw new ArrayTypeMismatchException (String.Format (Locale.GetText (
								"(Types: source={0};  target={1})"), src_type.FullName, dst_type.FullName));
					}
				}
			}
		}

#if NET_1_1
		public static void Copy (Array sourceArray, long sourceIndex, Array destinationArray,
		                         long destinationIndex, long length)
		{
			if (sourceArray == null)
				throw new ArgumentNullException ("sourceArray");

			if (destinationArray == null)
				throw new ArgumentNullException ("destinationArray");

			if (sourceIndex < Int32.MinValue || sourceIndex > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("sourceIndex",
					Locale.GetText ("Must be in the Int32 range."));

			if (destinationIndex < Int32.MinValue || destinationIndex > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("destinationIndex",
					Locale.GetText ("Must be in the Int32 range."));

			if (length < 0 || length > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			Copy (sourceArray, (int) sourceIndex, destinationArray, (int) destinationIndex, (int) length);
		}

		public static void Copy (Array sourceArray, Array destinationArray, long length)
		{
			if (length < 0 || length > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			Copy (sourceArray, destinationArray, (int) length);
		}
#endif

		public static int IndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
	
			return IndexOf (array, value, 0, array.Length);
		}

		public static int IndexOf (Array array, object value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return IndexOf (array, value, startIndex, array.Length - startIndex);
		}

		public static int IndexOf (Array array, object value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			// re-ordered to avoid possible integer overflow
			if (count < 0 || startIndex < array.GetLowerBound (0) || startIndex - 1 > array.GetUpperBound (0) - count)
				throw new ArgumentOutOfRangeException ();

			int max = startIndex + count;
			for (int i = startIndex; i < max; i++) {
				if (Object.Equals (array.GetValueImpl (i), value))
					return i;
			}

			return array.GetLowerBound (0) - 1;
		}

		[MonoTODO]
		public void Initialize()
		{
			//FIXME: We would like to find a compiler that uses
			// this method. It looks like this method do nothing
			// in C# so no exception is trown by the moment.
		}

		public static int LastIndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, array.Length - 1);
		}

		public static int LastIndexOf (Array array, object value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, startIndex, startIndex - array.GetLowerBound (0) + 1);
		}
		
		public static int LastIndexOf (Array array, object value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
	
			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if (count < 0 || startIndex < array.GetLowerBound (0) ||
				startIndex > array.GetUpperBound (0) ||	startIndex - count + 1 < array.GetLowerBound (0))
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i >= startIndex - count + 1; i--) {
				if (Object.Equals (array.GetValueImpl (i), value))
					return i;
			}

			return array.GetLowerBound (0) - 1;
		}

		public static void Reverse (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Reverse (array, array.GetLowerBound (0), array.GetLength (0));
		}

		public static void Reverse (Array array, int index, int length)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));

			if (index < array.GetLowerBound (0) || length < 0)
				throw new ArgumentOutOfRangeException ();

			// re-ordered to avoid possible integer overflow
			if (index > array.GetUpperBound (0) + 1 - length)
				throw new ArgumentException ();

			for (int i = 0; i < length / 2; i++)
			{
				object tmp;

				tmp = array.GetValueImpl (index + i);
				array.SetValueImpl (array.GetValueImpl (index + length - i - 1), index + i);
				array.SetValueImpl (tmp, index + length - i - 1);
			}
		}

		public static void Sort (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), null);
		}

		public static void Sort (Array keys, Array items)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), null);
		}

		public static void Sort (Array array, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), comparer);
		}

		public static void Sort (Array array, int index, int length)
		{
			Sort (array, null, index, length, null);
		}

		public static void Sort (Array keys, Array items, IComparer comparer)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

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
			if (keys == null)
				throw new ArgumentNullException ("keys");

			if (keys.Rank > 1 || (items != null && items.Rank > 1))
				throw new RankException ();

			if (items != null && keys.GetLowerBound (0) != items.GetLowerBound (0))
				throw new ArgumentException ();

			if (index < keys.GetLowerBound (0))
				throw new ArgumentOutOfRangeException ("index");

			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));

			if (keys.Length - (index + keys.GetLowerBound (0)) < length
				|| (items != null && index > items.Length - length))
				throw new ArgumentException ();

			try {
				int low0 = index;
				int high0 = index + length - 1;
				qsort (keys, items, low0, high0, comparer);
			}
			catch (Exception e) {
				throw new InvalidOperationException (Locale.GetText ("The comparer threw an exception."), e);
			}
		}

		private static void qsort (Array keys, Array items, int low0, int high0, IComparer comparer)
		{
			if (low0 >= high0)
				return;

			int low = low0;
			int high = high0;

			object objPivot = keys.GetValueImpl ((low + high) / 2);

			while (low <= high) {
				// Move the walls in
				while (low < high0 && compare (keys.GetValueImpl (low), objPivot, comparer) < 0)
					++low;
				while (high > low0 && compare (objPivot, keys.GetValueImpl (high), comparer) < 0)
					--high;

				if (low <= high) {
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

			if (items != null) {
				tmp = items.GetValueImpl (i);
				items.SetValueImpl (items.GetValueImpl (j), i);
				items.SetValueImpl (tmp, j);
			}
		}

		private static int compare (object value1, object value2, IComparer comparer)
		{
			if (value1 == null)
				return value2 == null ? 0 : -1;
			else if (value2 == null)
				return 1;
			else if (comparer == null)
				return ((IComparable) value1).CompareTo (value2);
			else
				return comparer.Compare (value1, value2);
		}
	
		public virtual void CopyTo (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			// The order of these exception checks may look strange,
			// but that's how the microsoft runtime does it.
			if (this.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));
			if (index + this.GetLength (0) > array.GetLowerBound (0) + array.GetLength (0))
				throw new ArgumentException ();
			if (array.Rank > 1)
				throw new RankException (Locale.GetText ("Only single dimension arrays are supported."));
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"Value has to be >= 0."));

			Copy (this, this.GetLowerBound (0), array, index, this.GetLength (0));
		}

#if NET_1_1
		[ComVisible (false)]
		public virtual void CopyTo (Array array, long index)
		{
			if (index < 0 || index > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			CopyTo (array, (int) index);
		}
#endif

		internal class SimpleEnumerator : IEnumerator, ICloneable
		{
			Array enumeratee;
			int currentpos;
			int length;

			public SimpleEnumerator (Array arrayToEnumerate)
			{
				this.enumeratee = arrayToEnumerate;
				this.currentpos = -1;
				this.length = arrayToEnumerate.Length;
			}

			public object Current {
				get {
			 		// Exception messages based on MS implementation
					if (currentpos < 0 )
						throw new InvalidOperationException (Locale.GetText (
							"Enumeration has not started."));
					if  (currentpos >= length)
						throw new InvalidOperationException (Locale.GetText (
							"Enumeration has already ended"));
					// Current should not increase the position. So no ++ over here.
					return enumeratee.GetValueImpl (currentpos);
				}
			}

			public bool MoveNext()
			{
				//The docs say Current should throw an exception if last
				//call to MoveNext returned false. This means currentpos
				//should be set to length when returning false.
				if (currentpos < length)
					currentpos++;
				if(currentpos < length)
					return true;
				else
					return false;
			}

			public void Reset()
			{
				currentpos = -1;
			}

			public object Clone ()
			{
				return MemberwiseClone ();
			}
		}
#if NET_2_0
		[CLSCompliant (false)]
		public static void Resize <T> (ref T [] arr, int sz) {
			if (sz < 0)
				throw new ArgumentOutOfRangeException ();
			
			if (arr == null) {
				arr = new T [sz];
				return;
			}
			
			if (arr.Length == sz)
				return;
			
			T [] a = new T [sz];
			Array.Copy (arr, a, Math.Min (sz, arr.Length));
			arr = a;
		}
		
		[CLSCompliant (false)]
		public static bool TrueForAll <T> (T [] array, Predicate <T> p)
		{
			if (array == null || p == null)
				throw new ArgumentNullException ();
			
			foreach (T t in array)
				if (! p (t))
					return false;
				
			return true;
		}
		[CLSCompliant (false)]
		public static void ForEach <T> (T [] array, Action <T> a)
		{
			if (array == null || a == null)
				throw new ArgumentNullException ();
			
			foreach (T t in array)
				a (t);
		}
		
		[CLSCompliant (false)]
		public static U [] ConvertAll <T, U> (T [] s, Converter <T, U> c)
		{
			if (s == null || c == null)
				throw new ArgumentNullException ();
			
			U [] r = new U [s.Length];
			
			for (int i = 0; i < s.Length; i ++)
				r [i] = c (s [i]);
			
			return r;
		}
		
		[CLSCompliant (false)]
		public static int FindLastIndex <T> (T [] a, Predicate <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ();
			
			return FindLastIndex <T> (a, 0, a.Length, c);
		}
		
		[CLSCompliant (false)]
		public static int FindLastIndex <T> (T [] a, int idx, Predicate <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ();
			
			return FindLastIndex <T> (a, idx, a.Length - idx, c);
		}
		
		[CLSCompliant (false)]
		public static int FindLastIndex <T> (T [] a, int idx, int cnt, Predicate <T> c)
		{
			if (a == null || c == null)
				throw new ArgumentNullException ();
			
			if (idx > a.Length || idx + cnt > a.Length)
				throw new ArgumentOutOfRangeException ();
			
			for (int i = idx + cnt - 1; i >= idx; i --)
				if (c (a [i]))
					return i;
				
			return -1;
		}
		
		[CLSCompliant (false)]
		public static int FindIndex <T> (T [] a, Predicate <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ();
			
			return FindIndex <T> (a, 0, a.Length, c);
		}
		
		[CLSCompliant (false)]
		public static int FindIndex <T> (T [] a, int idx, Predicate <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ();
			
			return FindIndex <T> (a, idx, a.Length - idx, c);
		}
		
		[CLSCompliant (false)]
		public static int FindIndex <T> (T [] a, int idx, int cnt, Predicate <T> c)
		{
			if (a == null || c == null)
				throw new ArgumentNullException ();
			
			if (idx > a.Length || idx + cnt > a.Length)
				throw new ArgumentOutOfRangeException ();
			
			for (int i = idx; i < idx + cnt; i ++)
				if (c (a [i]))
					return i;
				
			return -1;
		}
		
		[CLSCompliant (false)]
		public static int BinarySearch <T> (T [] a, T x)
		{
			if (a == null)
				throw new ArgumentNullException ("a");
			
			return BinarySearch <T> (a, 0, a.Length, x, null);
		}
		
		[CLSCompliant (false)]
		public static int BinarySearch <T> (T [] a, T x, IComparer <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ("a");
			
			return BinarySearch <T> (a, 0, a.Length, x, c);
		}
		
		[CLSCompliant (false)]
		public static int BinarySearch <T> (T [] a, int offset, int length, T x)
		{
			return BinarySearch <T> (a, offset, length, x, null);
		}
		
		[CLSCompliant (false)]
		public static int BinarySearch <T> (T [] a, int offset, int length, T x, IComparer <T> c)
		{
			if (a == null)
				throw new ArgumentNullException ("a");
			if (offset > a.Length || (offset + length) > a.Length || offset < 0)
				throw new ArgumentOutOfRangeException ();
			
			if (c == null)
				c = Comparer <T>.Default;
			
			int iMin = offset;
			int iMax = offset + length - 1;
			int iCmp = 0;
			try {
				while (iMin <= iMax) {
					int iMid = (iMin + iMax) / 2;
					iCmp = c.Compare (x, a [iMid]);

					if (iCmp == 0)
						return iMid;
					else if (iCmp < 0)
						iMax = iMid - 1;
					else
						iMin = iMid + 1; // compensate for the rounding down
				}
			}
			catch (Exception e) {
				throw new InvalidOperationException (Locale.GetText ("Comparer threw an exception."), e);
			}

			return ~iMin;
		}
		
		[CLSCompliant (false)]
		public static int IndexOf <T> (T [] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
	
			return IndexOf (array, value, 0, array.Length);
		}

		[CLSCompliant (false)]
		public static int IndexOf <T> (T [] array, T value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return IndexOf (array, value, startIndex, array.Length - startIndex);
		}

		[CLSCompliant (false)]
		public static int IndexOf <T> (T [] array, T value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			// re-ordered to avoid possible integer overflow
			if (count < 0 || startIndex < array.GetLowerBound (0) || startIndex - 1 > array.GetUpperBound (0) - count)
				throw new ArgumentOutOfRangeException ();

			int max = startIndex + count;
			for (int i = startIndex; i < max; i++) {
				if (Object.Equals (array [i], value))
				if (Object.Equals (array [i], value))
					return i;
			}

			return -1;
		}
		
		[CLSCompliant (false)]
		public static int LastIndexOf <T> (T [] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, array.Length - 1);
		}

		[CLSCompliant (false)]
		public static int LastIndexOf <T> (T [] array, T value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, startIndex, startIndex - array.Length + 1);
		}
		
		[CLSCompliant (false)]
		public static int LastIndexOf <T> (T [] array, T value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			if (count < 0 || startIndex > array.Length || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i >= startIndex - count + 1; i--) {
				if (Object.Equals (array [i], value))
					return i;
			}

			return -1;
		}
		
		[CLSCompliant (false)]
		public static T [] FindAll <T> (T [] a, Predicate <T> p)
		{
			if (a == null || p == null)
				throw new ArgumentNullException ();
			
			int pos = 0;
			T [] d = new T [a.Length];
			foreach (T t in a)
				if (p (t))
					d [pos ++] = t;
			
			Resize <T> (ref a, pos);
			return a;
		}

		[CLSCompliant (false)]
		public static bool Exists <T> (T [] a, Predicate <T> p)
		{
			if (a == null || p == null)
				throw new ArgumentNullException ();
			
			foreach (T t in a)
				if (p (t))
					return true;
			return false;
		}

		[CLSCompliant (false)]
		[MonoTODO]
		public static IList<T> AsReadOnly<T> (T[] array)
		{
			throw new NotImplementedException ();
		}
		
#if FIXME
		[CLSCompliant (false)]
		public static Nullable <T> Find <T> (T [] a, Predicate <T> p)
		{
			if (a == null || p == null)
				throw new ArgumentNullException ();
			
			foreach (T t in a)
				if (p (t))
					return new Nullable <T> (t);
				
			return default (Nullable <T>);
		}
		
		[CLSCompliant (false)]
		public static Nullable <T> FindLast <T> (T [] a, Predicate <T> p)
		{
			if (a == null || p == null)
				throw new ArgumentNullException ();
			
			for (int i = a.Length - 1; i >= 0; i--)
				if (p (a [i]))
					return new Nullable <T> (a [i]);
				
			return default (Nullable <T>);
		}
#endif
		
		// Fixme: wtf is constrained about this
		public static void ConstrainedCopy (Array s, int s_i, Array d, int d_i, int c)
		{
			Copy (s, s_i, d, d_i, c);
		}		
#endif
	}
}
