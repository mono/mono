//
// DataRowExtensions.cs
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
using System.Runtime.CompilerServices;

namespace System.Data
{
	public static class DataRowExtensions
	{
		public static T Field<T> (this DataRow row, int columnIndex)
		{
			return Field<T> (row, columnIndex, DataRowVersion.Current);
		}

		public static T Field<T> (this DataRow row, int columnIndex, DataRowVersion version)
		{
			object ret = row [columnIndex, version];
			if (ret == DBNull.Value) {
				Type type = typeof (T);
				Type genericTypeDef = type.IsGenericType ? type.GetGenericTypeDefinition () : null;
				if (!type.IsValueType || genericTypeDef != null && genericTypeDef == typeof (Nullable <>))
					return default (T);
				
				throw new StrongTypingException ("Cannot get strong typed value since it is DB null. Please use a nullable type.", null);
			}
			
			return (T) ret;
		}

		public static T Field<T> (this DataRow row, string columnName)
		{
			return Field<T> (row, columnName, DataRowVersion.Current);
		}

		public static T Field<T> (this DataRow row, string columnName, DataRowVersion version)
		{
			return Field<T> (row, row.Table.Columns [columnName], version);
		}

		public static T Field<T> (this DataRow row, DataColumn column)
		{
			return Field<T> (row, column, DataRowVersion.Current);
		}

		public static T Field<T> (this DataRow row, DataColumn column, DataRowVersion version)
		{
			return Field<T> (row, row.Table.Columns.IndexOf (column), version);
		}

		public static void SetField<T> (this DataRow row, int columnIndex, T value)
		{
			row [columnIndex] = value;
		}

		public static void SetField<T> (this DataRow row, string columnName, T value)
		{
			row [columnName] = value;
		}

		public static void SetField<T> (this DataRow row, DataColumn column, T value)
		{
			row [column] = value;
		}
	}
}
