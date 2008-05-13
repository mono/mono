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
using System.Collections.Generic;
using System.Linq;


namespace System.Data
{

	public static class DataTableExtensions
	{
		[MonoTODO]
		public static DataView AsDataView (this DataTable table)
		{
			return table.DefaultView;
		}

		[MonoTODO]
		public static DataView AsDataView<T> (this EnumerableRowCollection<T> source)
			where T : DataRow
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static EnumerableRowCollection<DataRow> AsEnumerable (this DataTable source)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static DataTable CopyToDataTable<T> (this IEnumerable<T> source)
			where T : DataRow
		{
			DataTable dt = new DataTable ();
			CopyToDataTable<T> (source, dt, LoadOption.PreserveChanges);
			return dt;
		}

		[MonoTODO]
		public static void CopyToDataTable<T> (this IEnumerable<T> source, DataTable table, LoadOption options)
			where T : DataRow
		{
			CopyToDataTable<T> (source, table, options, null);
		}

		[MonoTODO]
		public static void CopyToDataTable<T> (this IEnumerable<T> source, DataTable table, LoadOption options, FillErrorEventHandler errorHandler)
			where T : DataRow
		{
			throw new NotImplementedException ();
		}
	}
}
