//
// System.Data.Common.TdsSchemaInfo.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsSchemaInfo
	{
		#region Fields

		string columnName;
		string tableName;
		TdsColumnType columnType;
		object value;
		bool nullable;
		bool isReadOnly;
		bool isIdentity = false;
		bool isKey;
		int ordinal;
		int size;
		byte precision;
		byte scale;

		#endregion // Fields

		#region Constructors

		public TdsSchemaInfo ()
		{
		}

		#endregion // Constructors

		#region Properties

		public bool AllowDBNull {
			get { return nullable; }
			set { nullable = value; }
		}

		public int ColumnOrdinal {
			get { return ordinal; }
			set { ordinal = value; }
		}

		public string ColumnName {
			get { return columnName; }
			set { columnName = value; }
		}

		public int ColumnSize {
			get { return size; }
			set { size = value; }
		}

		public TdsColumnType ColumnType {
			get { return columnType; }
			set { columnType = value; }
		}

		public bool IsIdentity {
			get { return isIdentity; }
			set { isIdentity = value; }
		}

		public bool IsKey {	
			get { return isKey; }
			set { isKey = value; }
		}

		public bool IsReadOnly {
			get { return isReadOnly; }
			set { isReadOnly = value; }
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

		#endregion // Properties

	}
}
