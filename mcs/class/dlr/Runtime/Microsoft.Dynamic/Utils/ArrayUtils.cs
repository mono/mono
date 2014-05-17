/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Scripting.Utils {
    public static class ArrayUtils {
        internal sealed class FunctorComparer<T> : IComparer<T> {
            private readonly Comparison<T> _comparison;

            public FunctorComparer(Comparison<T> comparison) {
                Assert.NotNull(comparison);
                _comparison = comparison;
            }

            public int Compare(T x, T y) {
                return _comparison(x, y);
            }
        }

        // Emitted:
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly string[] EmptyStrings = new string[0];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
        public static readonly object[] EmptyObjects = new object[0];

        public static IComparer<T> ToComparer<T>(Comparison<T> comparison) {
            return new FunctorComparer<T>(comparison);
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] input, Func<TInput, TOutput> conv) {
            ContractUtils.RequiresNotNull(input, "input");
            ContractUtils.RequiresNotNull(conv, "conv");

            TOutput[] res = new TOutput[input.Length];
            for (int i = 0; i < input.Length; i++) {
                res[i] = conv(input[i]);
            }

            return res;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "1#")] // TODO: fix
        public static void PrintTable(StringBuilder output, string[,] table) {
            ContractUtils.RequiresNotNull(output, "output");
            ContractUtils.RequiresNotNull(table, "table");

            int max_width = 0;
            for (int i = 0; i < table.GetLength(0); i++) {
                if (table[i, 0].Length > max_width) {
                    max_width = table[i, 0].Length;
                }
            }

            for (int i = 0; i < table.GetLength(0); i++) {
                output.Append(" ");
                output.Append(table[i, 0]);

                for (int j = table[i, 0].Length; j < max_width + 1; j++) {
                    output.Append(' ');
                }

                output.AppendLine(table[i, 1]);
            }
        }

        public static T[] Copy<T>(T[] array) {
            return (array.Length > 0) ? (T[])array.Clone() : array;
        }

        /// <summary>
        /// Converts a generic ICollection of T into an array of T.  
        /// 
        /// If the collection is already an  array of T the original collection is returned.
        /// </summary>
        public static T[] ToArray<T>(ICollection<T> list) {
            return (list as T[]) ?? MakeArray(list);
        }

        /// <summary>
        /// Converts a generic ICollection of T into an array of R using a given conversion.  
        /// 
        /// If the collection is already an array of R the original collection is returned.
        /// </summary>
        public static TResult[] ToArray<TElement, TResult>(ICollection<TElement> list, Func<TElement, TResult> convertor) {
            TResult[] res = list as TResult[];
            if (res == null) {
                res = new TResult[list.Count];
                int i = 0;
                foreach (TElement obj in list) {
                    res[i++] = convertor(obj);
                }
            }
            return res;
        }

        public static T[] MakeArray<T>(ICollection<T> list) {
            if (list.Count == 0) {
                return new T[0];
            }

            T[] res = new T[list.Count];
            list.CopyTo(res, 0);
            return res;
        }

        public static T[] MakeArray<T>(ICollection<T> elements, int reservedSlotsBefore, int reservedSlotsAfter) {
            if (reservedSlotsAfter < 0) throw new ArgumentOutOfRangeException("reservedSlotsAfter");
            if (reservedSlotsBefore < 0) throw new ArgumentOutOfRangeException("reservedSlotsBefore");

            if (elements == null) {
                return new T[reservedSlotsBefore + reservedSlotsAfter];
            }

            T[] result = new T[reservedSlotsBefore + elements.Count + reservedSlotsAfter];
            elements.CopyTo(result, reservedSlotsBefore);
            return result;
        }

        public static T[] RotateRight<T>(T[] array, int count) {
            ContractUtils.RequiresNotNull(array, "array");
            if ((count < 0) || (count > array.Length)) throw new ArgumentOutOfRangeException("count");

            T[] result = new T[array.Length];
            // The head of the array is shifted, and the tail will be rotated to the head of the resulting array
            int sizeOfShiftedArray = array.Length - count;
            Array.Copy(array, 0, result, count, sizeOfShiftedArray);
            Array.Copy(array, sizeOfShiftedArray, result, 0, count);
            return result;
        }

        public static T[] ShiftRight<T>(T[] array, int count) {
            ContractUtils.RequiresNotNull(array, "array");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            T[] result = new T[array.Length + count];
            System.Array.Copy(array, 0, result, count, array.Length);
            return result;
        }

        public static T[] ShiftLeft<T>(T[] array, int count) {
            ContractUtils.RequiresNotNull(array, "array");
            if (count < 0) throw new ArgumentOutOfRangeException("count");

            T[] result = new T[array.Length - count];
            System.Array.Copy(array, count, result, 0, result.Length);
            return result;
        }

        public static T[] Insert<T>(T item, IList<T> list) {
            T[] res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return res;
        }

        public static T[] Insert<T>(T item1, T item2, IList<T> list) {
            T[] res = new T[list.Count + 2];
            res[0] = item1;
            res[1] = item2;
            list.CopyTo(res, 2);
            return res;
        }

        public static T[] Insert<T>(T item, T[] array) {
            T[] result = ShiftRight(array, 1);
            result[0] = item;
            return result;
        }

        public static T[] Insert<T>(T item1, T item2, T[] array) {
            T[] result = ShiftRight(array, 2);
            result[0] = item1;
            result[1] = item2;
            return result;
        }

        public static T[] Append<T>(T[] array, T item) {
            System.Array.Resize<T>(ref array, (array == null) ? 1 : array.Length + 1);
            array[array.Length - 1] = item;
            return array;
        }

        public static T[] AppendRange<T>(T[] array, IList<T> items) {
            return AppendRange<T>(array, items, 0);
        }

        public static T[] AppendRange<T>(T[] array, IList<T> items, int additionalItemCount) {
            if (additionalItemCount < 0) {
                throw new ArgumentOutOfRangeException("additionalItemCount");
            }

            int j = (array == null) ? 0 : array.Length;
            System.Array.Resize<T>(ref array, j + items.Count + additionalItemCount);

            for (int i = 0; i < items.Count; i++, j++) {
                array[j] = items[i];
            }

            return array;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")] // TODO: fix
        public static T[,] Concatenate<T>(T[,] array1, T[,] array2) {
            int columnsCount = array1.GetLength(1);
            Debug.Assert(array2.GetLength(1) == columnsCount);

            int row1Count = array1.GetLength(0);
            int row2Count = array2.GetLength(0);
            int totalRowsCount = row1Count + row2Count;
            T[,] result = new T[totalRowsCount, columnsCount];

            for (int i = 0; i < row1Count; i++) {
                for (int j = 0; j < columnsCount; j++) {
                    result[i, j] = array1[i, j];
                }
            }

            for (int i = 0; i < row2Count; i++) {
                for (int j = 0; j < columnsCount; j++) {
                    result[(i + row1Count), j] = array2[i, j];
                }
            }

            return result;
        }

        public static void SwapLastTwo<T>(T[] array) {
            Debug.Assert(array != null && array.Length >= 2);

            T temp = array[array.Length - 1];
            array[array.Length - 1] = array[array.Length - 2];
            array[array.Length - 2] = temp;
        }

        public static T[] RemoveFirst<T>(IList<T> list) {
            return ShiftLeft(MakeArray(list), 1);
        }

        public static T[] RemoveFirst<T>(T[] array) {
            return ShiftLeft(array, 1);
        }

        public static T[] RemoveLast<T>(T[] array) {
            ContractUtils.RequiresNotNull(array, "array");

            System.Array.Resize(ref array, array.Length - 1);
            return array;
        }

        public static T[] RemoveAt<T>(IList<T> list, int indexToRemove) {
            return RemoveAt(MakeArray(list), indexToRemove);
        }

        public static T[] RemoveAt<T>(T[] array, int indexToRemove) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.Requires(indexToRemove >= 0 && indexToRemove < array.Length, "index");

            T[] result = new T[array.Length - 1];
            if (indexToRemove > 0) {
                Array.Copy(array, 0, result, 0, indexToRemove);
            }
            int remaining = array.Length - indexToRemove - 1;
            if (remaining > 0) {
                Array.Copy(array, array.Length - remaining, result, result.Length - remaining, remaining);
            }
            return result;
        }

        public static T[] InsertAt<T>(IList<T> list, int index, params T[] items) {
            return InsertAt(MakeArray(list), index, items);
        }

        public static T[] InsertAt<T>(T[] array, int index, params T[] items) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresNotNull(items, "items");
            ContractUtils.Requires(index >= 0 && index <= array.Length, "index");

            if (items.Length == 0) {
                return Copy(array);
            }

            T[] result = new T[array.Length + items.Length];
            if (index > 0) {
                Array.Copy(array, 0, result, 0, index);
            }
            Array.Copy(items, 0, result, index, items.Length);

            int remaining = array.Length - index;
            if (remaining > 0) {
                Array.Copy(array, array.Length - remaining, result, result.Length - remaining, remaining);
            }
            return result;
        }

        public static bool ValueEquals<T>(this T[] array, T[] other) {
            if (other.Length != array.Length) {
                return false;
            }

            for (int i = 0; i < array.Length; i++) {
                if (!Object.Equals(array[i], other[i])) {
                    return false;
                }
            }

            return true;
        }

        public static int GetValueHashCode<T>(this T[] array) {
            return GetValueHashCode<T>(array, 0, array.Length);
        }

        public static int GetValueHashCode<T>(this T[] array, int start, int count) {
            ContractUtils.RequiresNotNull(array, "array");
            ContractUtils.RequiresArrayRange(array.Length, start, count, "start", "count");
            
            if (count == 0) {
                return 0;
            }

            int result = array[start].GetHashCode();
            for (int i = 1; i < count; i++) {
                result = ((result << 5) | (result >> 27)) ^ array[start + i].GetHashCode();
            }

            return result;
        }

        public static T[] Reverse<T>(this T[] array) {
            T[] res = new T[array.Length];
            for (int i = 0; i < array.Length; i++) {
                res[array.Length - i - 1] = array[i];
            }
            return res;
        }
    }
}
