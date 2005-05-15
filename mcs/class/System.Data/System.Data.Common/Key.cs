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
using Mono.Data.SqlExpressions;
using System.ComponentModel;

namespace System.Data.Common
{
	internal class Key
	{
		#region Fields

		DataTable _table;
		DataColumn[] _columns;
		ListSortDirection[] _sortDirection;
		DataViewRowState _rowStateFilter;
		IExpression _filter;
		//Currently IExpression.Eval does not receive DataRowVersion
		//	and always uses the _current version
		//so need a temp row for Eval calls
		DataRow _tmpRow;

		#endregion //Fields

		#region Constructors

		internal Key(DataTable table,DataColumn[] columns,ListSortDirection[] sort, DataViewRowState rowState, IExpression filter)
		{
			_table = table;
			_filter = filter;
			if (_filter != null)
				_tmpRow = _table.NewNotInitializedRow();
			_columns = columns;
			if (sort != null && sort.Length == columns.Length) {
				_sortDirection = sort;
			}
			else {
				_sortDirection = new ListSortDirection[columns.Length];
				for(int i=0; i < _sortDirection.Length; i++) {
					_sortDirection[i] = ListSortDirection.Ascending;
				}
			}

			if (rowState != DataViewRowState.None) {
				_rowStateFilter = rowState;
			}
			else {
				// FIXME : what is the correct value ?
				_rowStateFilter = DataViewRowState.CurrentRows;
			}
		}

		#endregion // Constructors

		#region Properties

		internal DataColumn[] Columns
		{
			get {
				return _columns;
			}
		}

		internal DataTable Table
		{
			get {
				return _table;
			}
		}

		ListSortDirection[] Sort 
		{
			get {
				return _sortDirection;
			}
		}

		internal DataViewRowState RowStateFilter
		{
			get {
				return _rowStateFilter;
			}

			set {
				_rowStateFilter = value;
			}
		}

		#endregion // Properties

		#region Methods

		internal int CompareRecords(int first, int second)
		{
			if (first == second) {
				return 0;
			}

			for(int i = 0; i < Columns.Length; i++) {

				int res = Columns[i].CompareValues(first,second);

				if (res == 0) {
					continue;
				}

				return (Sort[i] == ListSortDirection.Ascending) ? res : -res;
			}
			return 0;
		}

		internal int GetRecord(DataRow row)
		{
			int index = Key.GetRecord(row,_rowStateFilter);
			if (_filter == null)
				return index;

			if (index < 0)
				return index;

			_tmpRow._current = index;
			return _filter.EvalBoolean(_tmpRow) ? index : -1;
		}

		internal static int GetRecord(DataRow row, DataViewRowState rowStateFilter)
		{

			if (row.Original == row.Current) {
				 if ((rowStateFilter & DataViewRowState.Unchanged) != DataViewRowState.None) {
					 return row.Current;
				 }
			}
			else if (row.Original == -1) {
				  if ((rowStateFilter & DataViewRowState.Added) != DataViewRowState.None) {
					return row.Current;
				  }
			}
			else if (row.Current == -1) {
				     if ((rowStateFilter & DataViewRowState.Deleted) != DataViewRowState.None) {
						return row.Original;
					 }
			}
			else if ((rowStateFilter & DataViewRowState.ModifiedCurrent) != DataViewRowState.None) {
				return row.Current;
			}
			else if ((rowStateFilter & DataViewRowState.ModifiedOriginal) != DataViewRowState.None) {
				return row.Original;
			}

            return -1;
		}

		/// <summary>
		/// Checks for key equality to parameters set given
		/// </summary>
		/// <param name="columns">Columns the key consits of. If this parameter is null, it does not affects equality check</param>
		/// <param name="sort">Sort order of columns. If this parameter is null, it does not affects equality check</param>
		/// <param name="rowState">DataViewRowState to check for.If this parameter is null, it does not affects equality check</param>
		/// <param name="unique">Indicates whenever the index managed by this key allows non-uniqie keys to appear.</param>
		/// <param name="strict">Indicates whenever unique parameter should affect the equality check.</param>
		/// <returns></returns>
		internal bool Equals(DataColumn[] columns, ListSortDirection[] sort, DataViewRowState rowState, IExpression filter) 
		{
			if (rowState != DataViewRowState.None && RowStateFilter != rowState) {
				return false;
			}

			if (_filter != filter)
				return false;

			if (Columns.Length != columns.Length) {
				return false;
			}

			if (sort != null && Sort.Length != sort.Length) {
				return false;
			}

			if (sort != null) {
				for(int i=0; i < columns.Length; i++) {
					if (Sort[i] != sort[i] || Columns[i] != columns[i]) {
						return false;
					}
				}
			}
			else {
				for(int i=0; i < columns.Length; i++) {
					if (Columns[i] != columns[i]) {
						return false;
					}
				}
			}
			return true;
		}

		#endregion // Methods
	}
}
