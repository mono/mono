//
// Mono.Data.TdsClient.Internal.TdsColumnSchema.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.TdsClient.Internal {
	internal class TdsColumnSchema
	{
		#region Fields

		string columnName;
		string tableName;
		TdsColumnType columnType;
		object value;
		bool nullable;
		bool writable;
		int ordinal;
		int size;
		byte precision;
		byte scale;

		#endregion // Fields

		#region Constructors

		public TdsColumnSchema ()
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

		public TdsColumnType ColumnType {
			get { return columnType; }
			set { columnType = value; }
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
