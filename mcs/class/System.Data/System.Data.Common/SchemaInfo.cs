//
// System.Data.Common.SchemaInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.Data.Common {
	internal class SchemaInfo
	{
		#region Fields

		string columnName;
		string tableName;
		string dataTypeName;
		object value;
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
