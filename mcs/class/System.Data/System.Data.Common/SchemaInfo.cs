//
// System.Data.Common.SchemaInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Data.Common {
	internal class SchemaInfo
	{
		#region Fields

		string columnName;
		string tableName;
		string dataTypeName;
		object value;
		bool nullable;
		bool writable;
		int ordinal;
		int size;
		byte precision;
		byte scale;

		#endregion // Fields

		#region Constructors

		public SchemaInfo ()
		{
		}

		#endregion // Constructors

		#region Properties

		public string TableName {
			get { return tableName; }
			set { tableName = value; }
		}

		public int ColumnOrdinal {
			get { return ordinal; }
			set { ordinal = value; }
		}

		public byte NumericPrecision {
			get { return precision; }
			set { precision = value; }
		}

		public byte NumericScale {
			get { return scale; }
			set { scale = value; }
		}

		public int ColumnSize {
			get { return size; }
			set { size = value; }
		}

		public string ColumnName {
			get { return columnName; }
			set { columnName = value; }
		}

		public String DataTypeName {
			get { return dataTypeName; }
			set { dataTypeName = value; }
		}

		public bool Nullable {
			get { return nullable; }
			set { nullable = value; }
		}

		public bool Writable {
			get { return writable; }
			set { writable = value; }
		}
		
		#endregion // Properties

	}
}
