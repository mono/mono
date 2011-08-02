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
	internal class RecordCache {
		#region Fields

		const int MIN_CACHE_SIZE = 128;

		Stack _records = new Stack (16);
		int _nextFreeIndex = 0;
		int _currentCapacity = 0;
		DataTable _table;
		DataRow [] _rowsToRecords;

		#endregion // Fields

		#region Constructors

		internal RecordCache (DataTable table)
		{
			_table = table;
			_rowsToRecords = table.NewRowArray (16);
		}

		#endregion //Constructors

		#region Properties

		internal int CurrentCapacity {
			get { return _currentCapacity; }
		}

		internal DataRow this [int index] {
			get { return _rowsToRecords [index]; }
			set {
				if (index >= 0)
					_rowsToRecords [index] = value;
			}
		}

		#endregion // Properties

		#region Methods

		internal int NewRecord ()
		{
			if (_records.Count > 0)
				return (int) _records.Pop ();

			DataColumnCollection cols = _table.Columns;
			if (_nextFreeIndex >= _currentCapacity) {
				_currentCapacity *= 2;
				if (_currentCapacity < MIN_CACHE_SIZE)
					_currentCapacity = MIN_CACHE_SIZE;

				for (int i = 0; i < cols.Count; ++i)
					cols [i].DataContainer.Capacity = _currentCapacity;

				DataRow [] old = _rowsToRecords;
				_rowsToRecords = _table.NewRowArray (_currentCapacity);
				Array.Copy (old, 0, _rowsToRecords, 0, old.Length);
			}
			return _nextFreeIndex++;
		}

		internal void DisposeRecord (int index)
		{
			if (index < 0)
				throw new ArgumentException ();

			if (!_records.Contains (index))
				_records.Push (index);

			this [index] = null;
		}

		// FIXME: This doesn't seem to be the right class to have this method
		internal int CopyRecord (DataTable fromTable, int fromRecordIndex, int toRecordIndex)
		{
			int recordIndex = toRecordIndex;
			if (toRecordIndex == -1)
				recordIndex = NewRecord ();

			try {
				foreach (DataColumn toColumn in _table.Columns) {
					DataColumn fromColumn = fromTable.Columns [toColumn.ColumnName];
					if (fromColumn != null)
						toColumn.DataContainer.CopyValue (fromColumn.DataContainer, fromRecordIndex, recordIndex);
					else
						toColumn.DataContainer.CopyValue (_table.DefaultValuesRowIndex, recordIndex);					
				}
				return recordIndex;
			} catch {
				if (toRecordIndex == -1)
					DisposeRecord (recordIndex);

				throw;
			}
		}

		// FIXME: This doesn't seem to be the right class to have this method
		internal void ReadIDataRecord (int recordIndex, IDataRecord record, int [] mapping, int length)
		{
			if (mapping.Length > _table.Columns.Count)
				throw new ArgumentException ();

			int i = 0;
			for(; i < length; i++) {
				DataColumn column = _table.Columns [mapping [i]];
				column.DataContainer.SetItemFromDataRecord (recordIndex, record, i);
			}

			for (; i < mapping.Length; i++) {
				DataColumn column = _table.Columns [mapping [i]];
				if (column.AutoIncrement)
					column.DataContainer [recordIndex] = column.AutoIncrementValue ();
				else
					column.DataContainer [recordIndex] = column.DefaultValue;
			}
		}

		#endregion // Methods
	}
}
