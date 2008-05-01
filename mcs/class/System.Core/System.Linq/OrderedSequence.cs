//
// InternalOrderedSequence.cs
//
// Authors:
//	Alejandro Serrano "Serras" (trupill@yahoo.es)
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
	sealed class InternalOrderedSequence<TElement, TKey> : AOrderedEnumerable<TElement>
        {
		readonly IEnumerable<TElement> source;
		readonly Func<TElement, TKey> key_selector;
		readonly IComparer<TKey> comparer;
		readonly bool descending;
                
                internal InternalOrderedSequence (IEnumerable<TElement> source, Func<TElement, TKey> keySelector,
                                                  IComparer<TKey> comparer, bool descending)
                {
                        this.source = source;
                        this.key_selector = keySelector;
                        this.comparer = comparer ?? Comparer<TKey>.Default;
                        this.descending = descending;
                }
                
                public override IEnumerable<TElement> Sort (IEnumerable<TElement> parentSource)
                {
                        if (parent != null)
                                return parent.Sort (source);
                        return PerformSort (parentSource);
                }
                
                public override IEnumerator<TElement> GetEnumerator ()
                {
                        return PerformSort (source).GetEnumerator ();
                }
                
                List<TElement> source_list;
                TKey[] keys;
                int[] indexes;
                
                IEnumerable<TElement> PerformSort (IEnumerable<TElement> items)
                {
                        // It first enumerates source, collecting all elements
                        source_list = new List<TElement> (items);
                        
                        // If the source contains just zero or one element, there's no need to sort
                        if (source_list.Count <= 1)
                                return source_list;
                        
                        // Then evaluate the keySelector function for each element,
                        // collecting the key values
                        keys = new TKey [source_list.Count];
                        indexes = new int [source_list.Count];
                        for (int i = 0; i < source_list.Count; i++) {
                                keys [i] = key_selector (source_list [i]);
                                indexes [i] = i;
                        }
                        
                        // Then sorts the elements according to the collected
                        // key values and the selected ordering
                        QuickSort(0, indexes.Length - 1);
                        
                        // Return the values as IEnumerable<TElement>
                        TElement[] orderedList = new TElement [indexes.Length];
                        for (int i = 0; i < indexes.Length; i++)
                                orderedList [i] = source_list [indexes [i]];
                        return orderedList;
                }
                
                int CompareItems (int firstIndex, int secondIndex)
                {
                        int comparison = comparer.Compare (keys [firstIndex], keys [secondIndex]);
                       
                        // If descending, return the opposite comparison
                        return (descending ? -comparison : comparison);
                }
                
                // We look at the first, middle, and last items in the subarray.
                // Then we put the largest on the right side, the smallest on
                // the left side, and the median becomes our pivot.
                int MedianOfThree (int left, int right)
                {
                        int center = (left + right) / 2;
                        if (CompareItems (indexes [center], indexes [left]) < 0)
                                Swap (left, center);
                        if (CompareItems (indexes [right], indexes [left]) < 0)
                                Swap (left, right);
                        if (CompareItems (indexes [right], indexes [center]) < 0)
                                Swap (center, right);
                        Swap (center, right - 1);
                        return indexes [right - 1];
                }
                
                void QuickSort (int left, int right)
                {
                        if (left + 3 <= right) {
                                int l = left, r = right - 1, pivot = MedianOfThree (left, right);
                                while (true) {
                                        while (CompareItems (indexes [++l], pivot) < 0) {}
                                        while (CompareItems (indexes [--r], pivot) > 0) {}
                                        if (l < r)
                                                Swap (l, r);
                                        else
                                                break;
                                }
                                // Restore pivot
                                Swap (l, right - 1);
                                // Partition and sort
                                QuickSort (left, l - 1);
                                QuickSort (l + 1, right);
                        } else
                                // If there are three items in the subarray, insertion sort is better
                                InsertionSort (left, right);
                }
                
                void InsertionSort (int left, int right)
                {
                        for (int i = left + 1; i <= right; i++) {
                                int j, tmp = indexes [i];
                                for (j = i; j > left && CompareItems (tmp, indexes [j - 1]) < 0; j--)
                                     indexes [j] = indexes [j - 1];
                                indexes [j] = tmp;
                        }
                }
                
                void Swap (int left, int right)
                {
                        int temp = indexes [right];
                        indexes [right] = indexes [left];
                        indexes [left] = temp;
                }
                
        }
}
