//
// System.Data.Common.SchemaInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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

namespace System.Data.Common {
	internal class SchemaInfo
	{
		#region Fields

		string columnName;
		string tableName;
		string dataTypeName;
		bool allowDBNull;
		bool isReadOnly;
		int ordinal;
		int size;
		byte precision;
		byte scale;
		Type fieldType;

		#endregion // Fields

		#region Constructors

		public SchemaInfo ()
		{
		}

		#endregion // Constructors

		#region Properties

		public bool AllowDBNull {
			get { return allowDBNull; }
			set { allowDBNull = value; }
		}

		public string ColumnName {
			get { return columnName; }
			set { columnName = value; }
		}

		public int ColumnOrdinal {
			get { return ordinal; }
			set { ordinal = value; }
		}

		public int ColumnSize {
			get { return size; }
			set { size = value; }
		}

		public String DataTypeName {
			get { return dataTypeName; }
			set { dataTypeName = value; }
		}

		public Type FieldType {
			get { return fieldType; }
			set { fieldType = value; }
		}

		public byte NumericPrecision {
			get { return precision; }
			set { precision = value; }
		}

		public byte NumericScale {
			get { return scale; }
			set { scale = value; }
		}

		public string TableName {
			get { return tableName; }
			set { tableName = value; }
		}

		public bool IsReadOnly {
			get { return isReadOnly; }
			set { isReadOnly = value; }
		}
		
		#endregion // Properties

	}
}
