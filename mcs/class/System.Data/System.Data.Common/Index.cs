//
// System.Data.Common.Key.cs
//
// Author:
//   Boris Kirzner  <borisk@mainsoft.com>
//   Konstantin Triger (kostat@mainsoft.com)
//

/*
  * Copyright (c) 2002-2004 Mainsoft Corporation.
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  *
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  *
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */

using System;
using System.Collections;

using System.Text;

namespace System.Data.Common
{
	/// <summary>
	/// Summary description for Index.
	/// </summary>
	internal class Index {
		#region Fields

		static readonly int [] empty = new int [0];

		int [] _array;
		int _size;
		Key _key;
		int _refCount;

		// Implement a tri-state, with the property that 'know_no_duplicates' has meaning only when '!know_have_duplicates'
		bool know_have_duplicates, know_no_duplicates;

		#endregion // Fields

		#region Constructors

		internal Index (Key key)
		{
			_key = key;
			Reset ();
		}

		#endregion // Constructors

		#region Properties

		internal Key Key {
			get { return _key; }
		}

		internal int Size {
			get { return _size; }
		}

		internal int RefCount {
			get { return _refCount; }
		}

		internal int IndexToRecord (int index)
		{
			return index < 0 ? index : _array [index];
		}

		internal bool HasDuplicates {
			get {
				if (!know_have_duplicates && !know_no_duplicates) {
					for (int i = 0; i < _size - 1; i++) {
						if (Key.CompareRecords (_array [i], _array [i + 1]) == 0) {
							know_have_duplicates = true;
							break;
						}
					}
					know_no_duplicates = !know_have_duplicates;
				}
				return know_have_duplicates;
			}
		}

		#endregion // Properties

		#region Methods

		internal int [] Duplicates {
			get {
				if (!HasDuplicates)
					return null;

				ArrayList dups = new ArrayList ();

				bool inRange = false;
				for (int i = 0; i < _size - 1; i++) {
					if (Key.CompareRecords (_array [i], _array [i + 1]) == 0) {
						if (!inRange) {
							dups.Add (_array [i]);
							inRange = true;
						}

						dups.Add (_array [i + 1]);
					} else {
						inRange = false;
					}
				}

				return (int []) dups.ToArray (typeof (int));
			}
		}

		internal int [] GetAll ()
		{
			return _array;
		}

		internal DataRow [] GetAllRows ()
		{
			DataRow [] list = new DataRow [_size];
			for (int i = 0; i < _size; ++i)
				list [i] = Key.Table.RecordCache [_array [i]];
			return list;
		}

		internal DataRow [] GetDistinctRows ()
		{
			ArrayList list = new ArrayList ();
			list.Add (Key.Table.RecordCache [_array [0]]);
			int currRecord = _array [0];
			for (int i = 1; i <  _size; ++i) {
				if (Key.CompareRecords (currRecord, _array [i]) == 0)
					continue;
				list.Add (Key.Table.RecordCache [_array [i]]);
				currRecord = _array [i];
			}
			return (DataRow []) list.ToArray (typeof (DataRow));
		}

		internal void Reset ()
		{
			_array = empty;
			_size = 0;
			RebuildIndex ();
		}

		private void RebuildIndex ()
		{
			int rows_upperbound = Key.Table.RecordCache.CurrentCapacity;

			if (rows_upperbound == 0)
				return;

			// consider better capacity approximation
			_array = new int [rows_upperbound];
			_size = 0;
			foreach (DataRow row in Key.Table.Rows) {
				int record = Key.GetRecord (row);
				if (record != -1)
					_array [_size++] = record;
			}
			know_have_duplicates = know_no_duplicates = false;
			Sort ();
			know_no_duplicates = !know_have_duplicates;
		}

		private void Sort ()
		{
			//QuickSort(_array,0,_size-1);
			MergeSort (_array, _size);
		}

		/*
		 * Returns record number of the record equal to the key values supplied
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int Find (object [] keys)
		{
			int index = FindIndex (keys);
			return IndexToRecord (index);
		}

		/*
		 * Returns record index (location) of the record equal to the key values supplied
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int FindIndex (object [] keys)
		{
			if (keys == null || keys.Length != Key.Columns.Length)
				throw new ArgumentException("Expecting " + Key.Columns.Length + " value(s) for the key being indexed, " +
					"but received " + ((keys == null) ? 0 : keys.Length) + " value(s).");

			int tmp = Key.Table.RecordCache.NewRecord ();
			try {
				// init key values for temporal record
				for (int i = 0; i < Key.Columns.Length; i++)
					Key.Columns [i].DataContainer [tmp] = keys [i];
				return FindIndex (tmp);
			} finally {
				Key.Table.RecordCache.DisposeRecord (tmp);
			}
		}

		/*
		 * Returns record number of the record equal to the record supplied
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int Find (int record)
		{
			int index = FindIndex (record);
			return IndexToRecord (index);
		}

		/*
		 * Returns array of record numbers of the records equal equal to the key values supplied
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int [] FindAll (object [] keys)
		{
			int [] indexes = FindAllIndexes (keys);
			IndexesToRecords (indexes);
			return indexes;
		}

		/*
		 * Returns array of indexes of the records inside the index equal equal to the key values supplied
		 * in the meaning of index key, or -1 if no equal record found.
		 */
		internal int [] FindAllIndexes (object [] keys)
		{
			if (keys == null || keys.Length != Key.Columns.Length)
				throw new ArgumentException("Expecting " + Key.Columns.Length + " value(s) for the key being indexed," +
					"but received " + ((keys == null) ? 0 : keys.Length) + " value(s).");

			int tmp = Key.Table.RecordCache.NewRecord ();
			try {
				// init key values for temporal record
				for (int i = 0; i < Key.Columns.Length; i++)
					Key.Columns [i].DataContainer [tmp] = keys [i];
				return FindAllIndexes (tmp);
			} catch (FormatException) {
				return empty;
			} catch (InvalidCastException) {
				return empty;
			} finally {
				Key.Table.RecordCache.DisposeRecord (tmp);
			}
		}

		/*
		 * Returns array of record numbers of the records equal to the record supplied
		 * in the meaning of index key, or empty list if no equal records found.
		 */
		internal int [] FindAll (int record)
		{
			int [] indexes = FindAllIndexes (record);
			IndexesToRecords (indexes);
			return indexes;
		}

		/*
		 * Returns array of indexes of the records inside the index that equal to the record supplied
		 * in the meaning of index key, or empty list if no equal records found.
		 */
		internal int [] FindAllIndexes (int record)
		{
			int index = FindIndex (record);
			if (index == -1)
				return empty;

			int startIndex = index++;
			int endIndex = index;

			for (; startIndex >= 0 && Key.CompareRecords (_array [startIndex], record) == 0; startIndex--) {
			}
			for (; endIndex < _size && Key.CompareRecords (_array [endIndex], record) == 0; endIndex++) {
			}

			int length = endIndex - startIndex - 1;
			int [] indexes = new int [length];

			for (int i = 0; i < length; i++)
				indexes [i] = ++startIndex;

			return indexes;
		}

		/*
		 * Returns index inside the array where record number of the record equal to the record supplied
		 * in the meaning of index key is sored, or -1 if no equal record found.
		 */
		private int FindIndex (int record)
		{
			if (_size == 0)
				return -1;
			return BinarySearch (_array, 0, _size - 1, record);
		}

		/*
		 * Finds exact location of the record specified
		 */
		private int FindIndexExact (int record)
		{
			for (int i = 0, size = _size; i < size; i++)
				if (_array [i] == record)
					return i;
			return -1;
		}

		/*
		 * Returns array of records from the indexes (locations) inside the index
		 */
		private void IndexesToRecords (int [] indexes)
		{
			for (int i = 0; i < indexes.Length; i++)
				indexes [i] = _array [indexes [i]];
		}

		internal void Delete (DataRow row)
		{
			int oldRecord = Key.GetRecord (row);
			Delete (oldRecord);
		}

		internal void Delete (int oldRecord)
		{
			if (oldRecord == -1)
				return;

			int index = FindIndexExact (oldRecord);
			if (index != -1) {
				if (know_have_duplicates) {
					int c1 = 1;
					int c2 = 1;

					if (index > 0)
						c1 = Key.CompareRecords (_array [index - 1], oldRecord);
					if (index < _size - 1)
						c2 = Key.CompareRecords (_array [index + 1], oldRecord);

					if (c1 == 0 ^ c2 == 0)
						know_have_duplicates = know_no_duplicates = false;
				}
				Remove (index);
			}
		}

		private void Remove (int index)
		{
			if (_size > 1)
				System.Array.Copy (_array, index + 1, _array, index, _size - index - 1);
			_size--;
		}

		internal void Update (DataRow row, int oldRecord, DataRowVersion oldVersion, DataRowState oldState)
		{
			bool contains = Key.ContainsVersion (oldState, oldVersion);
			int newRecord = Key.GetRecord (row);
			// the record did not appeared in the index before update
			if (oldRecord == -1 || _size == 0 || !contains) {
				if (newRecord >= 0)
					if (FindIndexExact (newRecord) < 0)
						Add (row,newRecord);
				return;
			}

			// the record will not appeare in the index after update
			if (newRecord < 0 || !Key.CanContain (newRecord)) {
				Delete (oldRecord);
				return;
			}

			int oldIdx = FindIndexExact (oldRecord);
			if (oldIdx == -1) {
				Add (row, newRecord);
				return;
			}

			int newIdx = -1;
			int compare = Key.CompareRecords (_array [oldIdx], newRecord);
			int start, end;

			int c1 = 1;
			int c2 = 1;

			if (compare == 0) {
				if (_array [oldIdx] == newRecord) {
					// we deal with the same record that didn't change
					// in the context of current index.
					// so , do nothing.
					return;
				}
			} else {
				if (know_have_duplicates) {
					if (oldIdx > 0)
						c1 = Key.CompareRecords (_array [oldIdx - 1], newRecord);
					if (oldIdx < _size - 1)
						c2 = Key.CompareRecords (_array [oldIdx + 1], newRecord);

					if ((c1 == 0 ^ c2 == 0) && compare != 0)
						know_have_duplicates = know_no_duplicates = false;
				}
			}

			if ((oldIdx == 0 && compare > 0) || (oldIdx == (_size - 1) && compare < 0) || (compare == 0)) {
				// no need to switch cells
				newIdx = oldIdx;
			} else {
				if (compare < 0) {
					// search after the old place
					start = oldIdx + 1;
					end = _size - 1;
				} else {
					// search before the old palce
					start = 0;
					end = oldIdx - 1;
				}

				newIdx = LazyBinarySearch (_array, start, end, newRecord);

				if (oldIdx < newIdx) {
					System.Array.Copy (_array, oldIdx + 1, _array, oldIdx, newIdx - oldIdx);
					if (Key.CompareRecords (_array [newIdx], newRecord) > 0)
						--newIdx;
				} else if (oldIdx > newIdx){
					System.Array.Copy (_array, newIdx, _array, newIdx + 1, oldIdx - newIdx);
					if (Key.CompareRecords (_array [newIdx], newRecord) < 0)
						++newIdx;
				}
			}
			_array[newIdx] = newRecord;

			if (compare != 0) {
				if (!know_have_duplicates) {
					if (newIdx > 0)
						c1 = Key.CompareRecords (_array [newIdx - 1], newRecord);
					if (newIdx < _size - 1)
						c2 = Key.CompareRecords (_array [newIdx + 1], newRecord);

					if (c1 == 0 || c2 == 0)
						know_have_duplicates = true;
				}
			}
		}

		internal void Add (DataRow row)
		{
			Add(row, Key.GetRecord (row));
		}

		private void Add (DataRow row, int newRecord)
		{
			int newIdx;

			if (newRecord < 0 || !Key.CanContain (newRecord))
				return;

			if (_size == 0) {
				newIdx = 0;
			} else {
				newIdx = LazyBinarySearch (_array, 0, _size - 1, newRecord);
				// if newl value is greater - insert afer old value
				// else - insert before old value
				if (Key.CompareRecords (_array [newIdx], newRecord) < 0)
					newIdx++;
			}

			Insert (newIdx, newRecord);

			int c1 = 1;
			int c2 = 1;
			if (!know_have_duplicates) {
				if (newIdx > 0)
					c1 = Key.CompareRecords (_array [newIdx - 1], newRecord);
				if (newIdx < _size - 1)
					c2 = Key.CompareRecords (_array [newIdx + 1], newRecord);

				if (c1 == 0 || c2 == 0)
					know_have_duplicates = true;
			}
		}

		private void Insert (int index, int r)
		{
			if (_array.Length == _size) {
				int [] tmp = (_size == 0) ? new int [16] : new int [_size << 1];
				System.Array.Copy (_array, 0, tmp, 0, index);
				tmp [index] = r;
				System.Array.Copy (_array, index, tmp, index + 1, _size - index);
				_array = tmp;
			} else {
				System.Array.Copy (_array, index, _array, index + 1, _size - index);
				_array [index] = r;
			}
			_size++;
		}

		private void MergeSort (int [] to, int length)
		{
			int [] from = new int [length];
			System.Array.Copy (to, 0, from, 0, from.Length);
			MergeSort (from, to, 0, from.Length);
		}

		private void MergeSort (int [] from, int [] to, int p, int r)
		{
			int q = (p + r) >> 1;
			if (q == p)
				return;

			MergeSort (to, from, p, q);
			MergeSort (to, from, q, r);

			// merge
			for (int middle = q, current = p;;) {
				int res = Key.CompareRecords (from[p], from[q]);
				if (res > 0) {
					to [current++] = from [q++];

					if (q == r) {
						while (p < middle)
							to [current++] = from [p++];
						break;
					}
				} else {
					if (res == 0)
						know_have_duplicates = true;

					to [current++] = from [p++];

					if (p == middle) {
						while (q < r)
							to [current++] = from [q++];
						break;
					}
				}
			}
		}

		private void QuickSort (int [] a, int p, int r)
		{
			if (p < r) {
				int q = Partition (a, p, r);
				QuickSort (a, p, q);
				QuickSort (a, q + 1, r);
			}
		}

		private int Partition (int [] a, int p, int r)
		{
			int x = a [p];
			int i = p - 1;
			int j = r + 1;

			while (true) {
				// decrement upper limit while values are greater then border value
				do {
					j--;
				} while (Key.CompareRecords (a [j], x) > 0);

				do {
					i++;
				} while (Key.CompareRecords (a [i], x) < 0);

				if (i < j) {
					int tmp = a [j];
					a [j] = a [i];
					a [i] = tmp;
				} else {
					return j;
				}
			}
		}

		private int BinarySearch (int [] a, int p, int r, int b)
		{
			int i = LazyBinarySearch (a, p, r, b);
			return (Key.CompareRecords (a [i], b) == 0) ? i : -1;
		}

		// Lazy binary search only returns the cell number the search finished in,
		// but does not checks that the correct value was actually found
		private int LazyBinarySearch (int [] a, int p, int r, int b)
		{
			if (p == r)
				return p;

			int q = (p + r) >> 1;

			int compare = Key.CompareRecords (a [q], b);
			if (compare < 0)
				return LazyBinarySearch (a, q + 1, r, b);
			else if (compare > 0)
				return LazyBinarySearch (a, p, q, b);
			else
				return q;
		}

		internal void AddRef ()
		{
			_refCount++;
		}

		internal void RemoveRef ()
		{
			_refCount--;
		}

		/*
		// Prints indexes. For debugging.
		internal void Print ()
		{
			for (int i=0; i < _size; i++) {
				Console.Write ("Index {0} record {1}: ", i, _array [i]);
				for (int j=0; j < Key.Table.Columns.Count; j++) {
					DataColumn col = Key.Table.Columns [j];
					if (_array [i] >= 0)
						Console.Write ("{0,15} ", col [_array [i]]);
				}
				Console.WriteLine ();
			}
		}
		*/

		#endregion // Methods
	}
}
