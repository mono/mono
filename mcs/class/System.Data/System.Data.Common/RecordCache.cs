
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
using System;
using System.Collections;

namespace System.Data.Common
{
	internal class RecordCache
	{
		#region Fields

		const int MIN_CACHE_SIZE = 128;

		Stack _records = new Stack(16);
		int _nextFreeIndex = 0;
		int _currentCapacity = 0;
		DataTable _table;

		#endregion // Fields

		#region Constructors

		internal RecordCache(DataTable table)
		{
			_table = table;
		}

		#endregion //Constructors

		#region Properties

		internal int CurrentCapacity 
		{
			get {
				return _currentCapacity;
			}
		}

		#endregion // Properties

		#region Methods

		internal int NewRecord()
		{
			if (_records.Count > 0) {
                                return (int)_records.Pop();
			}
			else {
				DataColumnCollection cols = _table.Columns;
				if (_nextFreeIndex >= _currentCapacity) {
					_currentCapacity *= 2;
					if ( _currentCapacity < MIN_CACHE_SIZE ) {
						_currentCapacity = MIN_CACHE_SIZE;
					}
					foreach(DataColumn col in cols) {
						col.DataContainer.Capacity = _currentCapacity;
					}
				}
				return _nextFreeIndex++;
			}
		}

		internal void DisposeRecord(int index)
		{
			if ( index < 0 ) {
				throw new ArgumentException();
			}
                        if (! _records.Contains (index)) {
                                _records.Push(index);
                        }
		}

		internal int CopyRecord(DataTable fromTable,int fromRecordIndex,int toRecordIndex)
		{
			int recordIndex = toRecordIndex;
			if (toRecordIndex == -1) {
				recordIndex = NewRecord();
			}

			foreach(DataColumn fromColumn in fromTable.Columns) {
				DataColumn column = _table.Columns[fromColumn.ColumnName];
				if (column != null) {
					column.DataContainer.CopyValue(fromColumn.DataContainer,fromRecordIndex,recordIndex);
				}
			}

			return recordIndex;
		}

                /// <summary>
                ///     Compares two records in the given data table. The numbers are the offset
                ///     into the container tables.
                /// </summary>
                internal static bool CompareRecords (DataTable table, int x, int y)
                {
                        foreach (DataColumn dc in table.Columns) {
                                if (dc.DataContainer.CompareValues (x, y) != 0)
                                        return false;
                        }
                        return true;
                }
                
		#endregion // Methods

	}
}
