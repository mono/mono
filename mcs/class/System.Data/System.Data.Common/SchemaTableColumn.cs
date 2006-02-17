//
// System.Data.Common.SchemaTableColumn.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

namespace System.Data.Common {
	public static class SchemaTableColumn 
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
			public static readonly string NonVersionedProviderType = "NonVersionedProviderType";
			public static readonly string NumericPrecision = "NumericPrecision";
			public static readonly string NumericScale = "NumericScale";
			public static readonly string ProviderType = "ProviderType";

		#endregion // Fields
	}
}

#endif // NET_2_0
