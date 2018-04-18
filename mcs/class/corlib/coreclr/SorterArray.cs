using System.Collections;
using System.Collections.Generic;

namespace System
{
	partial class Array
	{
        // Private value type used by the Sort methods.
        private struct SorterObjectArray
        {
            private Object[] keys;
            private Object[] items;
            private IComparer comparer;

            internal SorterObjectArray(Object[] keys, Object[] items, IComparer comparer)
            {
                if (comparer == null) comparer = Comparer.Default;
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare(keys[a], keys[b]) > 0)
                    {
                        Object temp = keys[a];
                        keys[a] = keys[b];
                        keys[b] = temp;
                        if (items != null)
                        {
                            Object item = items[a];
                            items[a] = items[b];
                            items[b] = item;
                        }
                    }
                }
            }

            private void Swap(int i, int j)
            {
                Object t = keys[i];
                keys[i] = keys[j];
                keys[j] = t;

                if (items != null)
                {
                    Object item = items[i];
                    items[i] = items[j];
                    items[j] = item;
                }
            }

            internal void Sort(int left, int length)
            {
                IntrospectiveSort(left, length);
            }

            private void IntrospectiveSort(int left, int length)
            {
                if (length < 2)
                    return;

                try
                {
                    IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length));
                }
                catch (IndexOutOfRangeException)
                {
                    IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                }
            }

            private void IntroSort(int lo, int hi, int depthLimit)
            {
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreaterWithItems(lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            SwapIfGreaterWithItems(lo, hi - 1);
                            SwapIfGreaterWithItems(lo, hi);
                            SwapIfGreaterWithItems(hi - 1, hi);
                            return;
                        }

                        InsertionSort(lo, hi);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(lo, hi);
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(lo, hi);
                    IntroSort(p + 1, hi, depthLimit);
                    hi = p - 1;
                }
            }

            private int PickPivotAndPartition(int lo, int hi)
            {
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int mid = lo + (hi - lo) / 2;
                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                SwapIfGreaterWithItems(lo, mid);
                SwapIfGreaterWithItems(lo, hi);
                SwapIfGreaterWithItems(mid, hi);

                Object pivot = keys[mid];
                Swap(mid, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    while (comparer.Compare(keys[++left], pivot) < 0) ;
                    while (comparer.Compare(pivot, keys[--right]) < 0) ;

                    if (left >= right)
                        break;

                    Swap(left, right);
                }

                // Put pivot in the right location.
                Swap(left, (hi - 1));
                return left;
            }

            private void Heapsort(int lo, int hi)
            {
                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; i = i - 1)
                {
                    DownHeap(i, n, lo);
                }
                for (int i = n; i > 1; i = i - 1)
                {
                    Swap(lo, lo + i - 1);

                    DownHeap(1, i - 1, lo);
                }
            }

            private void DownHeap(int i, int n, int lo)
            {
                Object d = keys[lo + i - 1];
                Object dt = (items != null) ? items[lo + i - 1] : null;
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    if (child < n && comparer.Compare(keys[lo + child - 1], keys[lo + child]) < 0)
                    {
                        child++;
                    }
                    if (!(comparer.Compare(d, keys[lo + child - 1]) < 0))
                        break;
                    keys[lo + i - 1] = keys[lo + child - 1];
                    if (items != null)
                        items[lo + i - 1] = items[lo + child - 1];
                    i = child;
                }
                keys[lo + i - 1] = d;
                if (items != null)
                    items[lo + i - 1] = dt;
            }

            private void InsertionSort(int lo, int hi)
            {
                int i, j;
                Object t, ti;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = keys[i + 1];
                    ti = (items != null) ? items[i + 1] : null;
                    while (j >= lo && comparer.Compare(t, keys[j]) < 0)
                    {
                        keys[j + 1] = keys[j];
                        if (items != null)
                            items[j + 1] = items[j];
                        j--;
                    }
                    keys[j + 1] = t;
                    if (items != null)
                        items[j + 1] = ti;
                }
            }
        }

        // Private value used by the Sort methods for instances of Array.
        // This is slower than the one for Object[], since we can't use the JIT helpers
        // to access the elements.  We must use GetValue & SetValue.
        private struct SorterGenericArray
        {
            private Array keys;
            private Array items;
            private IComparer comparer;

            internal SorterGenericArray(Array keys, Array items, IComparer comparer)
            {
                if (comparer == null) comparer = Comparer.Default;
                this.keys = keys;
                this.items = items;
                this.comparer = comparer;
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if (a != b)
                {
                    if (comparer.Compare(keys.GetValue(a), keys.GetValue(b)) > 0)
                    {
                        Object key = keys.GetValue(a);
                        keys.SetValue(keys.GetValue(b), a);
                        keys.SetValue(key, b);
                        if (items != null)
                        {
                            Object item = items.GetValue(a);
                            items.SetValue(items.GetValue(b), a);
                            items.SetValue(item, b);
                        }
                    }
                }
            }

            private void Swap(int i, int j)
            {
                Object t1 = keys.GetValue(i);
                keys.SetValue(keys.GetValue(j), i);
                keys.SetValue(t1, j);

                if (items != null)
                {
                    Object t2 = items.GetValue(i);
                    items.SetValue(items.GetValue(j), i);
                    items.SetValue(t2, j);
                }
            }

            internal void Sort(int left, int length)
            {
                IntrospectiveSort(left, length);
            }

            private void IntrospectiveSort(int left, int length)
            {
                if (length < 2)
                    return;

                try
                {
                    IntroSort(left, length + left - 1, 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(keys.Length));
                }
                catch (IndexOutOfRangeException)
                {
                    IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                }
            }

            private void IntroSort(int lo, int hi, int depthLimit)
            {
                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            SwapIfGreaterWithItems(lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            SwapIfGreaterWithItems(lo, hi - 1);
                            SwapIfGreaterWithItems(lo, hi);
                            SwapIfGreaterWithItems(hi - 1, hi);
                            return;
                        }

                        InsertionSort(lo, hi);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(lo, hi);
                        return;
                    }
                    depthLimit--;

                    int p = PickPivotAndPartition(lo, hi);
                    IntroSort(p + 1, hi, depthLimit);
                    hi = p - 1;
                }
            }

            private int PickPivotAndPartition(int lo, int hi)
            {
                // Compute median-of-three.  But also partition them, since we've done the comparison.
                int mid = lo + (hi - lo) / 2;

                SwapIfGreaterWithItems(lo, mid);
                SwapIfGreaterWithItems(lo, hi);
                SwapIfGreaterWithItems(mid, hi);

                Object pivot = keys.GetValue(mid);
                Swap(mid, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    while (comparer.Compare(keys.GetValue(++left), pivot) < 0) ;
                    while (comparer.Compare(pivot, keys.GetValue(--right)) < 0) ;

                    if (left >= right)
                        break;

                    Swap(left, right);
                }

                // Put pivot in the right location.
                Swap(left, (hi - 1));
                return left;
            }

            private void Heapsort(int lo, int hi)
            {
                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; i = i - 1)
                {
                    DownHeap(i, n, lo);
                }
                for (int i = n; i > 1; i = i - 1)
                {
                    Swap(lo, lo + i - 1);

                    DownHeap(1, i - 1, lo);
                }
            }

            private void DownHeap(int i, int n, int lo)
            {
                Object d = keys.GetValue(lo + i - 1);
                Object dt = (items != null) ? items.GetValue(lo + i - 1) : null;
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    if (child < n && comparer.Compare(keys.GetValue(lo + child - 1), keys.GetValue(lo + child)) < 0)
                    {
                        child++;
                    }

                    if (!(comparer.Compare(d, keys.GetValue(lo + child - 1)) < 0))
                        break;

                    keys.SetValue(keys.GetValue(lo + child - 1), lo + i - 1);
                    if (items != null)
                        items.SetValue(items.GetValue(lo + child - 1), lo + i - 1);
                    i = child;
                }
                keys.SetValue(d, lo + i - 1);
                if (items != null)
                    items.SetValue(dt, lo + i - 1);
            }

            private void InsertionSort(int lo, int hi)
            {
                int i, j;
                Object t, dt;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = keys.GetValue(i + 1);
                    dt = (items != null) ? items.GetValue(i + 1) : null;

                    while (j >= lo && comparer.Compare(t, keys.GetValue(j)) < 0)
                    {
                        keys.SetValue(keys.GetValue(j), j + 1);
                        if (items != null)
                            items.SetValue(items.GetValue(j), j + 1);
                        j--;
                    }

                    keys.SetValue(t, j + 1);
                    if (items != null)
                        items.SetValue(dt, j + 1);
                }
            }
        }
    }
}