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
			_records.Push(index);
		}

		#endregion // Methods

	}
}
