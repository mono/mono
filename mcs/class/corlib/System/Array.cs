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
using System.Runtime.ConstrainedExecution;
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
#if NET_2_0
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
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
#if NET_2_0
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
			get { return Length; }
		}
#endif

		public int Rank {
#if NET_2_0
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
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

#if NET_2_0
	[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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

					iCmp = comparer.Compare (elt, value);

					if (iCmp == 0)
						return iMid;
					else if (iCmp > 0)
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
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
#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Copy (Array sourceArray, Array destinationArray, long length)
		{
			if (length < 0 || length > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value must be >= 0 and <= Int32.MaxValue."));

			Copy (sourceArray, destinationArray, (int) length);
		}
#endif

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
		public static int IndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
	
			return IndexOf (array, value, 0, array.Length);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
		public static int IndexOf (Array array, object value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return IndexOf (array, value, startIndex, array.Length - startIndex);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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
				if (Object.Equals (value, array.GetValueImpl (i)))
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

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
		public static int LastIndexOf (Array array, object value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, array.Length - 1);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
		public static int LastIndexOf (Array array, object value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, startIndex, startIndex - array.GetLowerBound (0) + 1);
		}
		
#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
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
				if (Object.Equals (value, array.GetValueImpl (i)))
					return i;
			}

			return array.GetLowerBound (0) - 1;
		}

#if !BOOTSTRAP_WITH_OLDLIB && !NET_2_0
		/* delegate used to swap array elements */
		delegate void Swapper (int i, int j);
#endif

		static Swapper get_swapper (Array array)
		{
			if (array is int[])
				return new Swapper (array.int_swapper);
			if (array is double[])
				return new Swapper (array.double_swapper);
			if (array is object[]) {
				return new Swapper (array.obj_swapper);
			}
			return new Swapper (array.slow_swapper);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Reverse (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Reverse (array, array.GetLowerBound (0), array.GetLength (0));
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
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

			int end = index + length - 1;
			object[] oarray = array as object[];
			if (oarray != null) {
				while (index < end) {
					object tmp = oarray [index];
					oarray [index] = oarray [end];
					oarray [end] = tmp;
					++index;
					--end;
				}
				return;
			}
			int[] iarray = array as int[];
			if (iarray != null) {
				while (index < end) {
					int tmp = iarray [index];
					iarray [index] = iarray [end];
					iarray [end] = tmp;
					++index;
					--end;
				}
				return;
			}
			double[] darray = array as double[];
			if (darray != null) {
				while (index < end) {
					double tmp = darray [index];
					darray [index] = darray [end];
					darray [end] = tmp;
					++index;
					--end;
				}
				return;
			}
			// fallback
			Swapper swapper = get_swapper (array);
			while (index < end) {
				swapper (index, end);
				++index;
				--end;
			}
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), null);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array keys, Array items)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), null);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array array, IComparer comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			Sort (array, null, array.GetLowerBound (0), array.GetLength (0), comparer);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array array, int index, int length)
		{
			Sort (array, null, index, length, null);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array keys, Array items, IComparer comparer)
		{
			if (keys == null)
				throw new ArgumentNullException ("keys");

			Sort (keys, items, keys.GetLowerBound (0), keys.GetLength (0), comparer);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array keys, Array items, int index, int length)
		{
			Sort (keys, items, index, length, null);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif
		public static void Sort (Array array, int index, int length, IComparer comparer)
		{
			Sort (array, null, index, length, comparer);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.MayCorruptInstance, CER.MayFail)]
#endif

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

			if (keys.Length - (index + keys.GetLowerBound (0)) < length || (items != null && index > items.Length - length))
				throw new ArgumentException ();

			if (length <= 1)
				return;

			if (comparer == null) {
				Swapper iswapper;
				if (items == null)
					iswapper = null;
				else 
					iswapper = get_swapper (items);
				if (keys is double[]) {
					combsort (keys as double[], index, length, iswapper);
					return;
				}
				if (keys is int[]) {
					combsort (keys as int[], index, length, iswapper);
					return;
				}
				if (keys is char[]) {
					combsort (keys as char[], index, length, iswapper);
					return;
				}
			}
			try {
				int low0 = index;
				int high0 = index + length - 1;
				qsort (keys, items, low0, high0, comparer);
			}
			catch (Exception e) {
				throw new InvalidOperationException (Locale.GetText ("The comparer threw an exception."), e);
			}
		}

		/* note, these are instance methods */
		void int_swapper (int i, int j) {
			int[] array = this as int[];
			int val = array [i];
			array [i] = array [j];
			array [j] = val;
		}

		void obj_swapper (int i, int j) {
			object[] array = this as object[];
			object val = array [i];
			array [i] = array [j];
			array [j] = val;
		}

		void slow_swapper (int i, int j) {
			object val = GetValueImpl (i);
			SetValueImpl (GetValue (j), i);
			SetValueImpl (val, j);
		}

		void double_swapper (int i, int j) {
			double[] array = this as double[];
			double val = array [i];
			array [i] = array [j];
			array [j] = val;
		}

		static int new_gap (int gap)
		{
			gap = (gap * 10) / 13;
			if (gap == 9 || gap == 10)
				return 11;
			if (gap < 1)
				return 1;
			return gap;
		}

		/* we use combsort because it's fast enough and very small, since we have
		 * several specialized versions here.
		 */
		static void combsort (double[] array, int start, int size, Swapper swap_items)
		{
			int gap = size;
			while (true) {
				gap = new_gap (gap);
				bool swapped = false;
				int end = start + size - gap;
				for (int i = start; i < end; i++) {
					int j = i + gap;
					if (array [i] > array [j]) {
						double val = array [i];
						array [i] = array [j];
						array [j] = val;
						swapped = true;
						if (swap_items != null)
							swap_items (i, j);
					}
				}
				if (gap == 1 && !swapped)
					break;
			}
		}

		static void combsort (int[] array, int start, int size, Swapper swap_items)
		{
			int gap = size;
			while (true) {
				gap = new_gap (gap);
				bool swapped = false;
				int end = start + size - gap;
				for (int i = start; i < end; i++) {
					int j = i + gap;
					if (array [i] > array [j]) {
						int val = array [i];
						array [i] = array [j];
						array [j] = val;
						swapped = true;
						if (swap_items != null)
							swap_items (i, j);
					}
				}
				if (gap == 1 && !swapped)
					break;
			}
		}

		static void combsort (char[] array, int start, int size, Swapper swap_items)
		{
			int gap = size;
			while (true) {
				gap = new_gap (gap);
				bool swapped = false;
				int end = start + size - gap;
				for (int i = start; i < end; i++) {
					int j = i + gap;
					if (array [i] > array [j]) {
						char val = array [i];
						array [i] = array [j];
						array [j] = val;
						swapped = true;
						if (swap_items != null)
							swap_items (i, j);
					}
				}
				if (gap == 1 && !swapped)
					break;
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
		public static void Resize <T> (ref T [] array, int newSize) {
			if (newSize < 0)
				throw new ArgumentOutOfRangeException ();
			
			if (array == null) {
				array = new T [newSize];
				return;
			}
			
			if (array.Length == newSize)
				return;
			
			T [] a = new T [newSize];
			Array.Copy (array, a, Math.Min (newSize, array.Length));
			array = a;
		}
		
		public static bool TrueForAll <T> (T [] array, Predicate <T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (match == null)
				throw new ArgumentNullException ("match");
			
			foreach (T t in array)
				if (! match (t))
					return false;
				
			return true;
		}
		
		public static void ForEach<T> (T [] array, Action <T> action)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (action == null)
				throw new ArgumentNullException ("action");
			
			foreach (T t in array)
				action (t);
		}
		
		public static TOutput[] ConvertAll<TInput, TOutput> (TInput [] array, Converter<TInput, TOutput> converter)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (converter == null)
				throw new ArgumentNullException ("converter");
			
			TOutput [] output = new TOutput [array.Length];
			for (int i = 0; i < array.Length; i ++)
				output [i] = converter (array [i]);
			
			return output;
		}
		
		public static int FindLastIndex<T> (T [] array, Predicate <T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			return FindLastIndex<T> (array, 0, array.Length, match);
		}
		
		public static int FindLastIndex<T> (T [] array, int startIndex, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ();
			
			return FindLastIndex<T> (array, startIndex, array.Length - startIndex, match);
		}
		
		public static int FindLastIndex<T> (T [] array, int startIndex, int count, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (match == null)
				throw new ArgumentNullException ("match");
			
			if (startIndex > array.Length || startIndex + count > array.Length)
				throw new ArgumentOutOfRangeException ();
			
			for (int i = startIndex + count - 1; i >= startIndex; i--)
				if (match (array [i]))
					return i;
				
			return -1;
		}
		
		public static int FindIndex<T> (T [] array, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			return FindIndex<T> (array, 0, array.Length, match);
		}
		
		public static int FindIndex<T> (T [] array, int startIndex, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			return FindIndex<T> (array, startIndex, array.Length - startIndex, match);
		}
		
		public static int FindIndex<T> (T [] array, int startIndex, int count, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			if (match == null)
				throw new ArgumentNullException ("match");
			
			if (startIndex > array.Length || startIndex + count > array.Length)
				throw new ArgumentOutOfRangeException ();
			
			for (int i = startIndex; i < startIndex + count; i ++)
				if (match (array [i]))
					return i;
				
			return -1;
		}
		
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
		public static int BinarySearch<T> (T [] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			return BinarySearch<T> (array, 0, array.Length, value, null);
		}
		
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
		public static int BinarySearch<T> (T [] array, T value, IComparer<T> comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			return BinarySearch<T> (array, 0, array.Length, value, comparer);
		}
		
		public static int BinarySearch<T> (T [] array, int offset, int length, T value)
		{
			return BinarySearch<T> (array, offset, length, value, null);
		}
		
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
		public static int BinarySearch<T> (T [] array, int index, int length, T value, IComparer<T> comparer)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", Locale.GetText (
					"index is less than the lower bound of array."));
			if (length < 0)
				throw new ArgumentOutOfRangeException ("length", Locale.GetText (
					"Value has to be >= 0."));
			// re-ordered to avoid possible integer overflow
			if (index > array.Length - length)
				throw new ArgumentException (Locale.GetText (
					"index and length do not specify a valid range in array."));
			if (comparer == null)
				comparer = Comparer <T>.Default;
			
			int iMin = index;
			int iMax = index + length - 1;
			int iCmp = 0;
			try {
				while (iMin <= iMax) {
					int iMid = (iMin + iMax) / 2;
					iCmp = comparer.Compare (value, array [iMid]);

					if (iCmp == 0)
						return iMid;
					else if (iCmp < 0)
						iMax = iMid - 1;
					else
						iMin = iMid + 1; // compensate for the rounding down
				}
			} catch (Exception e) {
				throw new InvalidOperationException (Locale.GetText ("Comparer threw an exception."), e);
			}

			return ~iMin;
		}
		
		public static int IndexOf<T> (T [] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
	
			return IndexOf (array, value, 0, array.Length);
		}

		public static int IndexOf<T> (T [] array, T value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return IndexOf (array, value, startIndex, array.Length - startIndex);
		}

		public static int IndexOf<T> (T [] array, T value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			// re-ordered to avoid possible integer overflow
			if (count < 0 || startIndex < array.GetLowerBound (0) || startIndex - 1 > array.GetUpperBound (0) - count)
				throw new ArgumentOutOfRangeException ();

			int max = startIndex + count;
			for (int i = startIndex; i < max; i++) {
				if (Object.Equals (value, array [i]))
					return i;
			}

			return -1;
		}
		
		public static int LastIndexOf<T> (T [] array, T value)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, array.Length - 1);
		}

		public static int LastIndexOf<T> (T [] array, T value, int startIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			return LastIndexOf (array, value, startIndex, startIndex + 1);
		}

		public static int LastIndexOf<T> (T [] array, T value, int startIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			
			if (count < 0 || startIndex > array.Length || startIndex - count + 1 < 0)
				throw new ArgumentOutOfRangeException ();

			for (int i = startIndex; i >= startIndex - count + 1; i--) {
				if (Object.Equals (value, array [i]))
					return i;
			}

			return -1;
		}
		
		public static T [] FindAll<T> (T [] array, Predicate <T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (match == null)
				throw new ArgumentNullException ("match");
			
			int pos = 0;
			T [] d = new T [array.Length];
			foreach (T t in array)
				if (match (t))
					d [pos++] = t;
			
			Resize <T> (ref d, pos);
			return d;
		}

		public static bool Exists<T> (T [] array, Predicate <T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (match == null)
				throw new ArgumentNullException ("match");
			
			foreach (T t in array)
				if (match (t))
					return true;
			return false;
		}

		public static IList<T> AsReadOnly<T> (T[] array)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			return new ReadOnlyArray<T> (array);
		}

		[CLSCompliant (false)]
		public static Nullable<T> Find<T> (T [] array, Predicate<T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (match == null)
				throw new ArgumentNullException ("match");
			
			foreach (T t in array)
				if (match (t))
					return new Nullable <T> (t);
				
			return default (Nullable <T>);
		}
		
		[CLSCompliant (false)]
		public static Nullable<T> FindLast<T> (T [] array, Predicate <T> match)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (match == null)
				throw new ArgumentNullException ("match");
			
			for (int i = array.Length - 1; i >= 0; i--)
				if (match (array [i]))
					return new Nullable <T> (array [i]);
				
			return default (Nullable <T>);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]		
		//
		// The constrained copy should guarantee that if there is an exception thrown
		// during the copy, the destination array remains unchanged.
		// This is related to System.Runtime.Reliability.CER
		public static void ConstrainedCopy (Array s, int s_i, Array d, int d_i, int c)
		{
			Copy (s, s_i, d, d_i, c);
		}
#endif 
	}

#if NET_2_0

	internal struct ReadOnlyArrayEnumerator <T> : IEnumerator <T> {
		const int NOT_STARTED = -2;
			
		// this MUST be -1, because we depend on it in move next.
		// we just decr the size, so, 0 - 1 == FINISHED
		const int FINISHED = -1;
			
		ReadOnlyArray <T> array;
		int idx;
			
		internal ReadOnlyArrayEnumerator (ReadOnlyArray<T> array)
		{
			this.array = array;
			idx = NOT_STARTED;
		}
			
		public void Dispose ()
		{
			idx = NOT_STARTED;
		}
			
		public bool MoveNext ()
		{
			if (idx == NOT_STARTED)
				idx = array.Count;
				
			return idx != FINISHED && -- idx != FINISHED;
		}
			
		public T Current {
			get {
				if (idx < 0)
					throw new InvalidOperationException ();
					
				return array [array.Count - 1 - idx];
			}
		}

		void IEnumerator.Reset ()
		{
			throw new NotImplementedException ();
		}

		object IEnumerator.Current {
			get {
				return Current;
			}
		}
	}

	internal class ReadOnlyArray <T> : ICollection <T>, IList <T>, IEnumerable <T>
	{
		T[] arr;

		internal ReadOnlyArray (T[] array) {
			arr = array;
		}

		// ICollection<T> interface
		public int Count {
			get {
				return arr.Length;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		public void Add (T item) {
			throw new NotSupportedException ("Collection is read-only");
		}

		public bool Remove (T item) {
			throw new NotSupportedException ("Collection is read-only");
		}

		public void Clear () {
			throw new NotSupportedException ("Collection is read-only");
		}

		public void CopyTo (T[] array, int index) {
			arr.CopyTo (array, index);
		}

		public bool Contains (T item) {
			return Array.IndexOf <T> (arr, item) != -1;
		}

		// IList<T> interface
		public T this [int index] {
			get {
				if (unchecked ((uint) index) >= unchecked ((uint) arr.Length))
					throw new ArgumentOutOfRangeException ("index");
				return arr [index];
			} 
			set {
				if (unchecked ((uint) index) >= unchecked ((uint) arr.Length))
					throw new ArgumentOutOfRangeException ("index");
				arr [index] = value;
			}
		}

		public void Insert (int index, T item) {
			throw new NotSupportedException ("Collection is read-only");
		}

		public void RemoveAt (int index) {
			throw new NotSupportedException ("Collection is read-only");
		}

		public int IndexOf (T item) {
			return Array.IndexOf <T> (arr, item);
		}

		// IEnumerable<T> interface
		public IEnumerator<T> GetEnumerator () {
			return new ReadOnlyArrayEnumerator <T> (this);
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return new ReadOnlyArrayEnumerator <T> (this);
		}
	}

#endif

#if BOOTSTRAP_WITH_OLDLIB || NET_2_0
	/* delegate used to swap array elements, keep defined outside Array */
	delegate void Swapper (int i, int j);
#endif
}
