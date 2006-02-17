//
// System.Data.Common.SchemaTableOptionalColumn.cs
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
	public static class SchemaTableOptionalColumn 
	{
		#region Fields
			public static readonly string AutoIncrementSeed = "AutoIncrementSeed";
			public static readonly string AutoIncrementStep = "AutoIncrementStep";
			public static readonly string BaseCatalogName = "BaseCatalogName";
			public static readonly string BaseColumnNamespace = "BaseColumnNamespace";
			public static readonly string BaseServerName = "BaseServerName";
			public static readonly string BaseTableNamespace = "BaseTableNamespace";
			public static readonly string ColumnMapping = "ColumnMapping";
			public static readonly string DefaultValue = "DefaultValue";
			public static readonly string Expression = "Expression";
			public static readonly string IsAutoIncrement = "IsAutoIncrement";
			public static readonly string IsHidden = "IsHidden";
			public static readonly string IsReadOnly = "IsReadOnly";
			public static readonly string IsRowVersion = "IsRowVersion";
			public static readonly string ProviderSpecificDataType = "ProviderSpecificDataType";


		#endregion // Fields
	}
}

#endif // NET_2_0
