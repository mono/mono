//
// System.Data.SqlClient.SqlClientMetaDataCollectionNames.cs
//
// Author:
//   Umadevi S <sumadevi@novell.com>
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

namespace System.Data.SqlClient
{
	/// <summary>
	/// Collection names
	/// </summary>
	public static class SqlClientMetaDataCollectionNames {

		public static readonly string Columns = "Columns";
		public static readonly string Databases = "Databases";
		public static readonly string ForeignKeys = "ForeignKeys";
		public static readonly string IndexColumns = "IndexColumns";
		public static readonly string Indexes = "Indexes";
		public static readonly string Parameters = "Parameters";
		public static readonly string ProcedureColumns = "ProcedureColumns";
		public static readonly string Procedures = "Procedures";
		public static readonly string Tables = "Tables";
		public static readonly string UserDefinedTypes = "UserDefinedTypes";
		public static readonly string Users = "Users";
		public static readonly string ViewColumns = "ViewColumns";
		public static readonly string Views = "Views";

	}
}

#endif
