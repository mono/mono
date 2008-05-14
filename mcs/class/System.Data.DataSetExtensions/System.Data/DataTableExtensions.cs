//
// DataTableExtensions.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
//

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
using System.Linq;


namespace System.Data
{

	public static class DataTableExtensions
	{
		public static DataView AsDataView (this DataTable table)
		{
			return AsDataView<DataRow> (table.AsEnumerable ());
		}

		[MonoTODO ("We should implement an effective DataView derivation; looks like .NET does.")]
		public static DataView AsDataView<T> (this EnumerableRowCollection<T> source)
			where T : DataRow
		{
			return CopyToDataTable<T> (source).DefaultView;
		}

		public static EnumerableRowCollection<DataRow> AsEnumerable (this DataTable source)
		{
			return new EnumerableRowCollection<DataRow> (new DataRowEnumerable<DataRow> (source));
		}

		public static DataTable CopyToDataTable<T> (this IEnumerable<T> source)
			where T : DataRow
		{
			DataTable dt = new DataTable ();
			IEnumerator<T> e = source.GetEnumerator ();
			if (!e.MoveNext ())
				throw new InvalidOperationException ("The source contains no DataRows");
			foreach (DataColumn col in e.Current.Table.Columns)
				dt.Columns.Add (new DataColumn (col.ColumnName, col.DataType, col.Expression, col.ColumnMapping));
			CopyToDataTable<T> (source, dt, LoadOption.PreserveChanges);
			return dt;
		}

		public static void CopyToDataTable<T> (this IEnumerable<T> source, DataTable table, LoadOption options)
			where T : DataRow
		{
			CopyToDataTable<T> (source, table, options, null);
		}

		public static void CopyToDataTable<T> (this IEnumerable<T> source, DataTable table, LoadOption options, FillErrorEventHandler errorHandler)
			where T : DataRow
		{
			var reader = new RowEnumerableDataReader (source, 0);
			table.Load (reader, options, errorHandler);
		}
	}

	class DataRowEnumerable<TRow> : IEnumerable<TRow>
	{
		DataTable source;

		public DataRowEnumerable (DataTable source)
		{
			this.source = source;
		}

		public IEnumerator<TRow> GetEnumerator ()
		{
			foreach (TRow row in source.Rows)
				yield return row;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
