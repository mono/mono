//
// System.Data.Common.SchemaTableColumn.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Common {
	public sealed class SchemaTableColumn 
	{
		#region Fields
			public static readonly string AllowDBNull = "AllowDBNull";
			public static readonly string BaseColumnName = "BaseColumnName";
			public static readonly string BaseSchemaName = "BaseSchemaName";
			public static readonly string BaseTableName = "BaseTableName";
			public static readonly string ColumnName = "ColumnName";
			public static readonly string ColumnOrdinal = "ColumnOrdinal";
			public static readonly string ColumnSize = "ColumnSize";
			public static readonly string DataType = "DataType";
			public static readonly string IsAliased = "IsAliased";
			public static readonly string IsExpression = "IsExpression";
			public static readonly string IsKey = "IsKey";
			public static readonly string IsLong = "IsLong";
			public static readonly string IsUnique = "IsUnique";
			public static readonly string NumericPrecision = "NumericPrecision";
			public static readonly string NumericScale = "NumericScale";
			public static readonly string ProviderType = "ProviderType";

		#endregion // Fields
	}
}

#endif // NET_1_2
