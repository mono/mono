//
// System.Data.Common.SchemaTableOptionalColumn.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

namespace System.Data.Common {
	public sealed class SchemaTableOptionalColumn 
	{
		#region Fields
			public static readonly string BaseCatalogName = "BaseCatalogName";
			public static readonly string BaseServerName = "BaseServerName";
			public static readonly string IsAutoIncrement = "IsAutoIncrement";
			public static readonly string IsHidden = "IsHidden";
			public static readonly string IsReadOnly = "IsReadOnly";
			public static readonly string IsRowVersion = "IsRowVersion";
			public static readonly string ProviderSpecificDataType = "ProviderSpecificDataType";

		#endregion // Fields
	}
}

#endif // NET_1_2
