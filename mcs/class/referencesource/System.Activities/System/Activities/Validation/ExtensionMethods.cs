//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Validation
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this ICollection c)
        {
            return (c == null || c.Count == 0);
        }

        public static string AsCommaSeparatedValues(this IEnumerable<string> c)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in c)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    if (sb.Length == 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(", ");
                        sb.Append(s);
                    }
                }
            }
            return sb.ToString();
        }

        public static int BinarySearch<T>(this IList<T> items, T value, IComparer<T> comparer)
        {
            return BinarySearch(items, 0, items.Count, value, comparer);
        }

        public static void QuickSort<T>(this IList<T> items, IComparer<T> comparer)
        {
            QuickSort(items, 0, items.Count - 1, comparer);
        }

        static int BinarySearch<T>(IList<T> items, int startIndex, int length, T value, IComparer<T> comparer)
        {
            int start = startIndex;
            int end = (startIndex + length) - 1;
            while (start <= end)
            {
                int mid = start + ((end - start) >> 1);
                int result = comparer.Compare(items[mid], value);
                if (result == 0)
                {
                    return mid;
                }
                if (result < 0)
                {
                    start = mid + 1;
                }
                else
                {
                    end = mid - 1;
                }
            }
            return ~start;
        }

        static void QuickSort<T>(IList<T> items, int startIndex, int endIndex, IComparer<T> comparer)
        {
            Stack<int> bounds = new Stack<int>();
            do
            {
                if (bounds.Count != 0)
                {
                    endIndex = bounds.Pop();
                    startIndex = bounds.Pop();
                }

                T pivot = items[startIndex];
                int pivotIndex = startIndex;

                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (comparer.Compare(pivot, items[i]) > 0)
                    {
                        pivotIndex++;
                        if (pivotIndex != i)
                        {
                            items.Swap(pivotIndex, i);
                        }
                    }
                }

                if (startIndex != pivotIndex)
                {
                    items.Swap(startIndex, pivotIndex);
                }

                if (pivotIndex + 1 < endIndex)
                {
                    bounds.Push(pivotIndex + 1); 
                    bounds.Push(endIndex);
                }

                if (startIndex < pivotIndex - 1)
                {
                    bounds.Push(startIndex); 
                    bounds.Push(pivotIndex - 1);
                }

            } while (bounds.Count != 0);
        }

        static void Swap<T>(this IList<T> items, int i, int j)
        {
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
    }
}
