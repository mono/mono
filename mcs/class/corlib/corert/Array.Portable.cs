// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
#if MONO
using System.Diagnostics.Private;
#endif

namespace System
{
    public abstract partial class Array : ICollection, IEnumerable, IList, IStructuralComparable, IStructuralEquatable, ICloneable
    {
        public static ReadOnlyCollection<T> AsReadOnly<T>(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // T[] implements IList<T>.
            return new ReadOnlyCollection<T>(array);
        }

        public static void Resize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException(nameof(newSize), SR.ArgumentOutOfRange_NeedNonNegNum);

            T[] larray = array;
            if (larray == null)
            {
                array = new T[newSize];
                return;
            }

            if (larray.Length != newSize)
            {
                T[] newArray = new T[newSize];
                Copy(larray, 0, newArray, 0, larray.Length > newSize ? newSize : larray.Length);
                array = newArray;
            }
        }

        // Number of elements in the Array.
        int ICollection.Count
        { get { return Length; } }

        // Is this Array read-only?
        bool IList.IsReadOnly
        { get { return false; } }

        Object IList.this[int index]
        {
            get
            {
                return GetValue(index);
            }

            set
            {
                SetValue(value, index);
            }
        }

        int IList.Add(Object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        bool IList.Contains(Object value)
        {
            return Array.IndexOf(this, value) >= 0;
        }

        void IList.Clear()
        {
            Array.Clear(this, 0, this.Length);
        }

        void IList.Insert(int index, Object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        void IList.Remove(Object value)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
        }

        // CopyTo copies a collection into an Array, starting at a particular
        // index into the array.
        // 
        // This method is to support the ICollection interface, and calls
        // Array.Copy internally.  If you aren't using ICollection explicitly,
        // call Array.Copy to avoid an extra indirection.
        // 
        public void CopyTo(Array array, int index)
        {
            // Note: Array.Copy throws a RankException and we want a consistent ArgumentException for all the IList CopyTo methods.
            if (array != null && array.Rank != 1)
                throw new ArgumentException(SR.Arg_RankMultiDimNotSupported);

            Array.Copy(this, 0, array, index, Length);
        }

        // Make a new array which is a deep copy of the original array.
        // 
        public Object Clone()
        {
            return MemberwiseClone();
        }

        Int32 IStructuralComparable.CompareTo(Object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            Array o = other as Array;

            if (o == null || this.Length != o.Length)
            {
                throw new ArgumentException(SR.ArgumentException_OtherNotArrayOfCorrectLength, nameof(other));
            }

            int i = 0;
            int c = 0;

            while (i < o.Length && c == 0)
            {
                object left = GetValue(i);
                object right = o.GetValue(i);

                c = comparer.Compare(left, right);
                i++;
            }

            return c;
        }

        Boolean IStructuralEquatable.Equals(Object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            Array o = other as Array;

            if (o == null || o.Length != this.Length)
            {
                return false;
            }

            int i = 0;
            while (i < o.Length)
            {
                object left = GetValue(i);
                object right = o.GetValue(i);

                if (!comparer.Equals(left, right))
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        // From System.Web.Util.HashCodeCombiner
        internal static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            int ret = 0;

            for (int i = (this.Length >= 8 ? this.Length - 8 : 0); i < this.Length; i++)
            {
                ret = CombineHashCodes(ret, comparer.GetHashCode(GetValue(i)));
            }

            return ret;
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the
        // IComparable interface, which must be implemented by all elements
        // of the array and the given search value. This method assumes that the
        // array is already sorted according to the IComparable interface;
        // if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        // 
        public static int BinarySearch(Array array, Object value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return BinarySearch(array, 0, array.Length, value, null);
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            Contract.Ensures(Contract.Result<TOutput[]>() != null);
            Contract.Ensures(Contract.Result<TOutput[]>().Length == array.Length);
            Contract.EndContractBlock();

            TOutput[] newArray = new TOutput[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = converter(array[i]);
            }
            return newArray;
        }

        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            if (length > Int32.MaxValue || length < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_HugeArrayNotSupported);

            Array.Copy(sourceArray, destinationArray, (int)length);
        }

        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            if (sourceIndex > Int32.MaxValue || sourceIndex < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(sourceIndex), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            if (destinationIndex > Int32.MaxValue || destinationIndex < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(destinationIndex), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            if (length > Int32.MaxValue || length < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_HugeArrayNotSupported);

            Array.Copy(sourceArray, (int)sourceIndex, destinationArray, (int)destinationIndex, (int)length);
        }

        public void CopyTo(Array array, long index)
        {
            if (index > Int32.MaxValue || index < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            Contract.EndContractBlock();

            this.CopyTo(array, (int)index);
        }

        public static void ForEach<T>(T[] array, Action<T> action)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Contract.EndContractBlock();

            for (int i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }
        }

        public long LongLength
        {
            get
            {
                long ret = GetLength(0);

                for (int i = 1; i < Rank; ++i)
                {
                    ret = ret * GetLength(i);
                }

                return ret;
            }
        }

        public long GetLongLength(int dimension)
        {
            // This method does throw an IndexOutOfRangeException for compat if dimension < 0 or >= Rank
            // by calling GetUpperBound
            return GetLength(dimension);
        }

        public Object GetValue(long index)
        {
            if (index > Int32.MaxValue || index < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            Contract.EndContractBlock();

            return this.GetValue((int)index);
        }

        public Object GetValue(long index1, long index2)
        {
            if (index1 > Int32.MaxValue || index1 < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index1), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 > Int32.MaxValue || index2 < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index2), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            Contract.EndContractBlock();

            return this.GetValue((int)index1, (int)index2);
        }

        public Object GetValue(long index1, long index2, long index3)
        {
            if (index1 > Int32.MaxValue || index1 < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index1), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index2 > Int32.MaxValue || index2 < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index2), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            if (index3 > Int32.MaxValue || index3 < Int32.MinValue)
                throw new ArgumentOutOfRangeException(nameof(index3), SR.ArgumentOutOfRange_HugeArrayNotSupported);
            Contract.EndContractBlock();

            return this.GetValue((int)index1, (int)index2, (int)index3);
        }

        public Object GetValue(params long[] indices)
        {
            if (indices == null)
                throw new ArgumentNullException(nameof(indices));
            if (Rank != indices.Length)
                throw new ArgumentException(SR.Arg_RankIndices);
            Contract.EndContractBlock();

            int[] intIndices = new int[indices.Length];

            for (int i = 0; i < indices.Length; ++i)
            {
                long index = indices[i];
                if (index > Int32.MaxValue || index < Int32.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(index), SR.ArgumentOutOfRange_HugeArrayNotSupported);
                intIndices[i] = (int)index;
            }

            return this.GetValue(intIndices);
        }

        public void Initialize()
        {
            // Project N port note: On the desktop, this api is a nop unless the array element type is a value type with
            // an explicit nullary constructor. Such a type cannot be expressed in C# so Project N does not support this.
            // The ILC toolchain fails the build if it encounters such a type.
            return;
        }        

        public bool IsFixedSize { get { return true; } }

        // Is this Array synchronized (i.e., thread-safe)?  If you want a synchronized
        // collection, you can use SyncRoot as an object to synchronize your 
        // collection with.  You could also call GetSynchronized() 
        // to get a synchronized wrapper around the Array.
        public bool IsSynchronized { get { return false; } }

        // Returns an object appropriate for synchronizing access to this 
        // Array.
        public Object SyncRoot { get { return this; } }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the IComparable interface, which must be implemented by all
        // elements of the array and the given search value. This method assumes
        // that the array is already sorted according to the IComparable
        // interface; if this is not the case, the result will be incorrect.
        //
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        // 
        public static int BinarySearch(Array array, int index, int length, Object value)
        {
            return BinarySearch(array, index, length, value, null);
        }

        // Searches an array for a given element using a binary search algorithm.
        // Elements of the array are compared to the search value using the given
        // IComparer interface. If comparer is null, elements of the
        // array are compared to the search value using the IComparable
        // interface, which in that case must be implemented by all elements of the
        // array and the given search value. This method assumes that the array is
        // already sorted; if this is not the case, the result will be incorrect.
        // 
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        // 
        public static int BinarySearch(Array array, Object value, IComparer comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return BinarySearch(array, 0, array.Length, value, comparer);
        }

        // Searches a section of an array for a given element using a binary search
        // algorithm. Elements of the array are compared to the search value using
        // the given IComparer interface. If comparer is null,
        // elements of the array are compared to the search value using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array and the given search value. This method
        // assumes that the array is already sorted; if this is not the case, the
        // result will be incorrect.
        // 
        // The method returns the index of the given value in the array. If the
        // array does not contain the given value, the method returns a negative
        // integer. The bitwise complement operator (~) can be applied to a
        // negative result to produce the index of the first element (if any) that
        // is larger than the given search value.
        // 
        public static int BinarySearch(Array array, int index, int length, Object value, IComparer comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(length)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - index < length)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (array.Rank != 1)
                throw new RankException(SR.Rank_MultiDimNotSupported);

            if (comparer == null) comparer = LowLevelComparer.Default;

            int lo = index;
            int hi = index + length - 1;
            Object[] objArray = array as Object[];
            if (objArray != null)
            {
                while (lo <= hi)
                {
                    // i might overflow if lo and hi are both large positive numbers. 
                    int i = GetMedian(lo, hi);

                    int c;
                    try
                    {
                        c = comparer.Compare(objArray[i], value);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                    }
                    if (c == 0) return i;
                    if (c < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
            }
            else
            {
                while (lo <= hi)
                {
                    int i = GetMedian(lo, hi);

                    int c;
                    try
                    {
                        c = comparer.Compare(array.GetValue(i), value);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                    }
                    if (c == 0) return i;
                    if (c < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
            }
            return ~lo;
        }

        private static int GetMedian(int low, int hi)
        {
            // Note both may be negative, if we are dealing with arrays w/ negative lower bounds.
            return low + ((hi - low) >> 1);
        }

        public static int BinarySearch<T>(T[] array, T value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return BinarySearch<T>(array, 0, array.Length, value, null);
        }

        public static int BinarySearch<T>(T[] array, T value, System.Collections.Generic.IComparer<T> comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            return BinarySearch<T>(array, 0, array.Length, value, comparer);
        }

        public static int BinarySearch<T>(T[] array, int index, int length, T value)
        {
            return BinarySearch<T>(array, index, length, value, null);
        }

        public static int BinarySearch<T>(T[] array, int index, int length, T value, System.Collections.Generic.IComparer<T> comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(length)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - index < length)
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            return ArraySortHelper<T>.BinarySearch(array, index, length, value, comparer);
        }

        // Returns the index of the first occurrence of a given value in an array.
        // The array is searched forwards, and the elements of the array are
        // compared to the given value using the Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return IndexOf(array, value, 0, array.Length);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and ending at the last element of the array. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return IndexOf(array, value, startIndex, array.Length - startIndex);
        }

        // Returns the index of the first occurrence of a given value in a range of
        // an array. The array is searched forwards, starting at index
        // startIndex and upto count elements. The
        // elements of the array are compared to the given value using the
        // Object.Equals method.
        // 
        public static int IndexOf(Array array, Object value, int startIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (array.Rank != 1)
                throw new RankException(SR.Rank_MultiDimNotSupported);
            if (startIndex < 0 || startIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            if (count < 0 || count > array.Length - startIndex)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);

            Object[] objArray = array as Object[];
            int endIndex = startIndex + count;
            if (objArray != null)
            {
                if (value == null)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (objArray[i] == null) return i;
                    }
                }
                else
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        Object obj = objArray[i];
                        if (obj != null && obj.Equals(value)) return i;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    Object obj = array.GetValue(i);
                    if (obj == null)
                    {
                        if (value == null) return i;
                    }
                    else
                    {
                        if (obj.Equals(value)) return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// This version is called from Array<T>.IndexOf and Contains<T>, so it's in every unique array instance due to array interface implementation.
        /// Do not call into IndexOf<T>(Array array, Object value, int startIndex, int count) for size and space reasons.
        /// Otherwise there will be two IndexOf methods for each unique array instance, and extra parameter checking which are not needed for the common case.
        /// </summary>
        public static int IndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // See comment above Array.GetComparerForReferenceTypesOnly for details
            EqualityComparer<T> comparer = GetComparerForReferenceTypesOnly<T>();

            if (comparer != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (comparer.Equals(array[i], value))
                        return i;
                }
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (StructOnlyEquals<T>(array[i], value))
                        return i;
                }
            }

            return -1;
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return IndexOf(array, value, startIndex, array.Length - startIndex);
        }

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (startIndex < 0 || startIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            if (count < 0 || count > array.Length - startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            int endIndex = startIndex + count;

            // See comment above Array.GetComparerForReferenceTypesOnly for details
            EqualityComparer<T> comparer = GetComparerForReferenceTypesOnly<T>();

            if (comparer != null)
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    if (comparer.Equals(array[i], value))
                        return i;
                }
            }
            else
            {
                for (int i = startIndex; i < endIndex; i++)
                {
                    if (StructOnlyEquals<T>(array[i], value))
                        return i;
                }
            }

            return -1;
        }

        public static int LastIndexOf(Array array, Object value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            return LastIndexOf(array, value, array.Length - 1, array.Length);
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and ending at index 0. The elements of the array are
        // compared to the given value using the Object.Equals method.
        // 
        public static int LastIndexOf(Array array, Object value, int startIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            return LastIndexOf(array, value, startIndex, startIndex + 1);
        }

        // Returns the index of the last occurrence of a given value in a range of
        // an array. The array is searched backwards, starting at index
        // startIndex and counting uptocount elements. The elements of
        // the array are compared to the given value using the Object.Equals
        // method.
        // 
        public static int LastIndexOf(Array array, Object value, int startIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length == 0)
            {
                return -1;
            }

            if (startIndex < 0 || startIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            if (count > startIndex + 1)
                throw new ArgumentOutOfRangeException("endIndex", SR.ArgumentOutOfRange_EndIndexStartIndex);
            if (array.Rank != 1)
                throw new RankException(SR.Rank_MultiDimNotSupported);

            Object[] objArray = array as Object[];
            int endIndex = startIndex - count + 1;
            if (objArray != null)
            {
                if (value == null)
                {
                    for (int i = startIndex; i >= endIndex; i--)
                    {
                        if (objArray[i] == null) return i;
                    }
                }
                else
                {
                    for (int i = startIndex; i >= endIndex; i--)
                    {
                        Object obj = objArray[i];
                        if (obj != null && obj.Equals(value)) return i;
                    }
                }
            }
            else
            {
                for (int i = startIndex; i >= endIndex; i--)
                {
                    Object obj = array.GetValue(i);
                    if (obj == null)
                    {
                        if (value == null) return i;
                    }
                    else
                    {
                        if (obj.Equals(value)) return i;
                    }
                }
            }
            return -1;  // Return lb-1 for arrays with negative lower bounds.
        }

        public static int LastIndexOf<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return LastIndexOf(array, value, array.Length - 1, array.Length);
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // if array is empty and startIndex is 0, we need to pass 0 as count
            return LastIndexOf(array, value, startIndex, (array.Length == 0) ? 0 : (startIndex + 1));
        }

        public static int LastIndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Length == 0)
            {
                //
                // Special case for 0 length List
                // accept -1 and 0 as valid startIndex for compablility reason.
                //
                if (startIndex != -1 && startIndex != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
                }

                // only 0 is a valid value for count if array is empty
                if (count != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
                }
                return -1;
            }

            // Make sure we're not out of range            
            if (startIndex < 0 || startIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            // See comment above Array.GetComparerForReferenceTypesOnly for details
            EqualityComparer<T> comparer = GetComparerForReferenceTypesOnly<T>();

            int endIndex = startIndex - count + 1;
            if (comparer != null)
            {
                for (int i = startIndex; i >= endIndex; i--)
                {
                    if (comparer.Equals(array[i], value))
                        return i;
                }
            }
            else
            {
                for (int i = startIndex; i >= endIndex; i--)
                {
                    if (StructOnlyEquals<T>(array[i], value))
                        return i;
                }
            }

            return -1;
        }

        // Reverses all elements of the given array. Following a call to this
        // method, an element previously located at index i will now be
        // located at index length - i - 1, where length is the
        // length of the array.
        // 
        public static void Reverse(Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Reverse(array, 0, array.Length);
        }

        // Reverses the elements in a range of an array. Following a call to this
        // method, an element in the range given by index and count
        // which was previously located at index i will now be located at
        // index index + (index + count - i - 1).
        // Reliability note: This may fail because it may have to box objects.
        // 
        public static void Reverse(Array array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            int lowerBound = array.GetLowerBound(0);
            if (index < lowerBound || length < 0)
                throw new ArgumentOutOfRangeException((index < lowerBound ? nameof(index) : nameof(length)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - (index - lowerBound) < length)
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            if (array.Rank != 1)
                throw new RankException(SR.Rank_MultiDimNotSupported);

            int i = index;
            int j = index + length - 1;
            Object[] objArray = array as Object[];
            if (objArray != null)
            {
                while (i < j)
                {
                    Object temp = objArray[i];
                    objArray[i] = objArray[j];
                    objArray[j] = temp;
                    i++;
                    j--;
                }
            }
            else
            {
                while (i < j)
                {
                    Object temp = array.GetValue(i);
                    array.SetValue(array.GetValue(j), i);
                    array.SetValue(temp, j);
                    i++;
                    j--;
                }
            }
        }

        public static void Reverse<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Reverse(array, 0, array.Length);
        }

        public static void Reverse<T>(T[] array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? nameof(index) : nameof(length)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - index < length)
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            int i = index;
            int j = index + length - 1;
            while (i < j)
            {
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
                i++;
                j--;
            }
        }

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the IComparable interface, which must be implemented
        // by all elements of the array.
        // 
        public static void Sort(Array array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Sort(array, null, 0, array.Length, null);
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the IComparable interface, which
        // must be implemented by all elements in the given section of the array.
        // 
        public static void Sort(Array array, int index, int length)
        {
            Sort(array, null, index, length, null);
        }

        // Sorts the elements of an array. The sort compares the elements to each
        // other using the given IComparer interface. If comparer is
        // null, the elements are compared to each other using the
        // IComparable interface, which in that case must be implemented by
        // all elements of the array.
        // 
        public static void Sort(Array array, IComparer comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Sort(array, null, 0, array.Length, comparer);
        }

        // Sorts the elements in a section of an array. The sort compares the
        // elements to each other using the given IComparer interface. If
        // comparer is null, the elements are compared to each other using
        // the IComparable interface, which in that case must be implemented
        // by all elements in the given section of the array.
        // 
        public static void Sort(Array array, int index, int length, IComparer comparer)
        {
            Sort(array, null, index, length, comparer);
        }

        public static void Sort(Array keys, Array items)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            Sort(keys, items, keys.GetLowerBound(0), keys.Length, null);
        }

        public static void Sort(Array keys, Array items, IComparer comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            Sort(keys, items, keys.GetLowerBound(0), keys.Length, comparer);
        }

        public static void Sort(Array keys, Array items, int index, int length)
        {
            Sort(keys, items, index, length, null);
        }

        public static void Sort(Array keys, Array items, int index, int length, IComparer comparer)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (keys.Rank != 1 || (items != null && items.Rank != 1))
                throw new RankException(SR.Rank_MultiDimNotSupported);
            int keysLowerBound = keys.GetLowerBound(0);
            if (items != null && keysLowerBound != items.GetLowerBound(0))
                throw new ArgumentException(SR.Arg_LowerBoundsMustMatch);
            if (index < keysLowerBound || length < 0)
                throw new ArgumentOutOfRangeException((length < 0 ? nameof(length) : nameof(index)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (keys.Length - (index - keysLowerBound) < length || (items != null && (index - keysLowerBound) > items.Length - length))
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            if (length > 1)
            {
#if CORERT
                IComparer<Object> comparerT = new ComparerAsComparerT(comparer);
                Object[] objKeys = keys as Object[];
                Object[] objItems = items as Object[];

                // Unfortunately, on Project N, we don't have the ability to specialize ArraySortHelper<> on demand
                // for value types. Rather than incur a boxing cost on every compare and every swap (and maintain a separate introsort algorithm
                // just for this), box them all, sort them as an Object[] array and unbox them back.

                // Check if either of the arrays need to be copied.
                if (objKeys == null)
                {
                    objKeys = new Object[index + length];
                    Array.CopyImplValueTypeArrayToReferenceArray(keys, index, objKeys, index, length, reliable: false);
                }
                if (objItems == null && items != null)
                {
                    objItems = new Object[index + length];
                    Array.CopyImplValueTypeArrayToReferenceArray(items, index, objItems, index, length, reliable: false);
                }

                Sort<Object, Object>(objKeys, objItems, index, length, comparerT);

                // If either array was copied, copy it back into the original
                if (objKeys != keys)
                {
                    Array.CopyImplReferenceArrayToValueTypeArray(objKeys, index, keys, index, length, reliable: false);
                }
                if (objItems != items)
                {
                    Array.CopyImplReferenceArrayToValueTypeArray(objItems, index, items, index, length, reliable: false);
                }
#else
                Object[] objKeys = keys as Object[];
                Object[] objItems = null;
                if (objKeys != null)
                    objItems = items as Object[];
                if (objKeys != null && (items == null || objItems != null))
                {
                    SorterObjectArray sorter = new SorterObjectArray(objKeys, objItems, comparer);
                    sorter.Sort(index, length);
                }
                else
                {
                	SorterGenericArray sorter = new SorterGenericArray(keys, items, comparer);
                  	sorter.Sort(index, length);
                }
#endif                
            }
        }

        // Wraps an IComparer inside an IComparer<Object>.
        private sealed class ComparerAsComparerT : IComparer<Object>
        {
            public ComparerAsComparerT(IComparer comparer)
            {
                _comparer = (comparer == null) ? LowLevelComparer.Default : comparer;
            }

            public int Compare(Object x, Object y)
            {
                return _comparer.Compare(x, y);
            }

            private IComparer _comparer;
        }

        public static void Sort<T>(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Sort<T>(array, 0, array.Length, null);
        }

        public static void Sort<T>(T[] array, int index, int length)
        {
            Sort<T>(array, index, length, null);
        }

        public static void Sort<T>(T[] array, System.Collections.Generic.IComparer<T> comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            Sort<T>(array, 0, array.Length, comparer);
        }

        public static void Sort<T>(T[] array, int index, int length, System.Collections.Generic.IComparer<T> comparer)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((length < 0 ? nameof(length) : nameof(index)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - index < length)
                throw new ArgumentException(SR.Argument_InvalidOffLen);

            if (length > 1)
                ArraySortHelper<T>.Sort(array, index, length, comparer);
        }

        public static void Sort<T>(T[] array, Comparison<T> comparison)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (comparison == null)
            {
                throw new ArgumentNullException(nameof(comparison));
            }

            ArraySortHelper<T>.Sort(array, 0, array.Length, comparison);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            Contract.EndContractBlock();
            Sort<TKey, TValue>(keys, items, 0, keys.Length, null);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
        {
            Sort<TKey, TValue>(keys, items, index, length, null);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            Contract.EndContractBlock();
            Sort<TKey, TValue>(keys, items, 0, keys.Length, comparer);
        }

        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (index < 0 || length < 0)
                throw new ArgumentOutOfRangeException((length < 0 ? nameof(length) : nameof(index)), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (keys.Length - index < length || (items != null && index > items.Length - length))
                throw new ArgumentException(SR.Argument_InvalidOffLen);
            Contract.EndContractBlock();

            if (length > 1)
            {
                if (items == null)
                {
                    Sort<TKey>(keys, index, length, comparer);
                    return;
                }

                ArraySortHelper<TKey, TValue>.Default.Sort(keys, items, index, length, comparer);
            }
        }

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        public static bool Exists<T>(T[] array, Predicate<T> match)
        {
            return Array.FindIndex(array, match) != -1;
        }

        public static void Fill<T>(T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static void Fill<T>(T[] array, T value, int startIndex, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (startIndex < 0 || startIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            if (count < 0 || startIndex > array.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            for (int i = startIndex; i < startIndex + count; i++)
            {
                array[i] = value;
            }
        }

        public static T Find<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        public static int FindIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return FindIndex(array, 0, array.Length, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return FindIndex(array, startIndex, array.Length - startIndex, match);
        }

        public static int FindIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (startIndex < 0 || startIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
            }

            if (count < 0 || startIndex > array.Length - count)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (match(array[i])) return i;
            }
            return -1;
        }

        public static T FindLast<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (match(array[i]))
                {
                    return array[i];
                }
            }
            return default(T);
        }

        public static int FindLastIndex<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return FindLastIndex(array, array.Length - 1, array.Length, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return FindLastIndex(array, startIndex, startIndex + 1, match);
        }

        public static int FindLastIndex<T>(T[] array, int startIndex, int count, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (array.Length == 0)
            {
                // Special case for 0 length List
                if (startIndex != -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
                }
            }
            else
            {
                // Make sure we're not out of range            
                if (startIndex < 0 || startIndex >= array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), SR.ArgumentOutOfRange_Index);
                }
            }

            // 2nd have of this also catches when startIndex == MAXINT, so MAXINT - 0 + 1 == -1, which is < 0.
            if (count < 0 || startIndex - count + 1 < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_Count);
            }

            int endIndex = startIndex - count;
            for (int i = startIndex; i > endIndex; i--)
            {
                if (match(array[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool TrueForAll<T>(T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (!match(array[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return new ArrayEnumerator(this);
        }

        // These functions look odd, as they are part of a complex series of compiler intrinsics
        // designed to produce very high quality code for equality comparison cases without utilizing
        // reflection like other platforms. The major complication is that the specification of
        // IndexOf is that it is supposed to use IEquatable<T> if possible, but that requirement
        // cannot be expressed in IL directly due to the lack of constraints.
        // Instead, specialization at call time is used within the compiler. 
        // 
        // General Approach
        // - Perform fancy redirection for Array.GetComparerForReferenceTypesOnly<T>(). If T is a reference 
        //   type or UniversalCanon, have this redirect to EqualityComparer<T>.get_Default, Otherwise, use 
        //   the function as is. (will return null in that case)
        // - Change the contents of the IndexOf functions to have a pair of loops. One for if 
        //   GetComparerForReferenceTypesOnly returns null, and one for when it does not. 
        //   - If it does not return null, call the EqualityComparer<T> code.
        //   - If it does return null, use a special function StructOnlyEquals<T>(). 
        //     - Calls to that function result in calls to a pair of helper function in 
        //       EqualityComparerHelpers (StructOnlyEqualsIEquatable, or StructOnlyEqualsNullable) 
        //       depending on whether or not they are the right function to call.
        // - The end result is that in optimized builds, we have the same single function compiled size 
        //   characteristics that the old EqualsOnlyComparer<T>.Equals function had, but we maintain 
        //   correctness as well.
        private static EqualityComparer<T> GetComparerForReferenceTypesOnly<T>()
        {
#if !CORERT
        	return System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T> () ? EqualityComparer<T>.Default : null;
#else
            return EqualityComparer<T>.Default;
#endif
        }

        private static bool StructOnlyEquals<T>(T left, T right)
        {
            return left.Equals(right);
        }

        private sealed class ArrayEnumerator : IEnumerator, ICloneable
        {
            private Array _array;
            private int _index;
            private int _endIndex; // cache array length, since it's a little slow.

            internal ArrayEnumerator(Array array)
            {
                _array = array;
                _index = -1;
                _endIndex = array.Length;
            }

            public bool MoveNext()
            {
                if (_index < _endIndex)
                {
                    _index++;
                    return (_index < _endIndex);
                }
                return false;
            }

            public Object Current
            {
                get
                {
                    if (_index < 0) throw new InvalidOperationException(SR.InvalidOperation_EnumNotStarted);
                    if (_index >= _endIndex) throw new InvalidOperationException(SR.InvalidOperation_EnumEnded);
                    return _array.GetValueWithFlattenedIndex_NoErrorCheck(_index);
                }
            }

            public void Reset()
            {
                _index = -1;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }
        }
    }
}