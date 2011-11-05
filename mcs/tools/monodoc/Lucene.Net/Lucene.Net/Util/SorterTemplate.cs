/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Borrowed from Cglib. Allows custom swap so that two arrays can be sorted
	/// at the same time.
	/// </summary>
	public abstract class SorterTemplate
	{
		private const int MERGESORT_THRESHOLD = 12;
		private const int QUICKSORT_THRESHOLD = 7;
		
		abstract protected internal void  Swap(int i, int j);
		abstract protected internal int Compare(int i, int j);
		
		public virtual void  QuickSort(int lo, int hi)
		{
			QuickSortHelper(lo, hi);
			InsertionSort(lo, hi);
		}
		
		private void  QuickSortHelper(int lo, int hi)
		{
			for (; ; )
			{
				int diff = hi - lo;
				if (diff <= QUICKSORT_THRESHOLD)
				{
					break;
				}
				int i = (hi + lo) / 2;
				if (Compare(lo, i) > 0)
				{
					Swap(lo, i);
				}
				if (Compare(lo, hi) > 0)
				{
					Swap(lo, hi);
				}
				if (Compare(i, hi) > 0)
				{
					Swap(i, hi);
				}
				int j = hi - 1;
				Swap(i, j);
				i = lo;
				int v = j;
				for (; ; )
				{
					while (Compare(++i, v) < 0)
					{
						/* nothing */ ;
					}
					while (Compare(--j, v) > 0)
					{
						/* nothing */ ;
					}
					if (j < i)
					{
						break;
					}
					Swap(i, j);
				}
				Swap(i, hi - 1);
				if (j - lo <= hi - i + 1)
				{
					QuickSortHelper(lo, j);
					lo = i + 1;
				}
				else
				{
					QuickSortHelper(i + 1, hi);
					hi = j;
				}
			}
		}
		
		private void  InsertionSort(int lo, int hi)
		{
			for (int i = lo + 1; i <= hi; i++)
			{
				for (int j = i; j > lo; j--)
				{
					if (Compare(j - 1, j) > 0)
					{
						Swap(j - 1, j);
					}
					else
					{
						break;
					}
				}
			}
		}
		
		protected internal virtual void  MergeSort(int lo, int hi)
		{
			int diff = hi - lo;
			if (diff <= MERGESORT_THRESHOLD)
			{
				InsertionSort(lo, hi);
				return ;
			}
			int mid = lo + diff / 2;
			MergeSort(lo, mid);
			MergeSort(mid, hi);
			Merge(lo, mid, hi, mid - lo, hi - mid);
		}
		
		private void  Merge(int lo, int pivot, int hi, int len1, int len2)
		{
			if (len1 == 0 || len2 == 0)
			{
				return ;
			}
			if (len1 + len2 == 2)
			{
				if (Compare(pivot, lo) < 0)
				{
					Swap(pivot, lo);
				}
				return ;
			}
			int first_cut, second_cut;
			int len11, len22;
			if (len1 > len2)
			{
				len11 = len1 / 2;
				first_cut = lo + len11;
				second_cut = Lower(pivot, hi, first_cut);
				len22 = second_cut - pivot;
			}
			else
			{
				len22 = len2 / 2;
				second_cut = pivot + len22;
				first_cut = Upper(lo, pivot, second_cut);
				len11 = first_cut - lo;
			}
			Rotate(first_cut, pivot, second_cut);
			int new_mid = first_cut + len22;
			Merge(lo, first_cut, new_mid, len11, len22);
			Merge(new_mid, second_cut, hi, len1 - len11, len2 - len22);
		}
		
		private void  Rotate(int lo, int mid, int hi)
		{
			int lot = lo;
			int hit = mid - 1;
			while (lot < hit)
			{
				Swap(lot++, hit--);
			}
			lot = mid; hit = hi - 1;
			while (lot < hit)
			{
				Swap(lot++, hit--);
			}
			lot = lo; hit = hi - 1;
			while (lot < hit)
			{
				Swap(lot++, hit--);
			}
		}
		
		private int Lower(int lo, int hi, int val)
		{
			int len = hi - lo;
			while (len > 0)
			{
				int half = len / 2;
				int mid = lo + half;
				if (Compare(mid, val) < 0)
				{
					lo = mid + 1;
					len = len - half - 1;
				}
				else
				{
					len = half;
				}
			}
			return lo;
		}
		
		private int Upper(int lo, int hi, int val)
		{
			int len = hi - lo;
			while (len > 0)
			{
				int half = len / 2;
				int mid = lo + half;
				if (Compare(val, mid) < 0)
				{
					len = half;
				}
				else
				{
					lo = mid + 1;
					len = len - half - 1;
				}
			}
			return lo;
		}
	}
}
