//
// SqlCommandBuilder.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace System.Data.SqlClient
{
	public class SqlCommandBuilder : DbCommandBuilder 
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlCommandBuilder is not supported on the current platform.";

		public SqlCommandBuilder ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommandBuilder (SqlDataAdapter adapter)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static void DeriveParameters (SqlCommand command)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetDeleteCommand ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetInsertCommand ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetUpdateCommand ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetUpdateCommand (bool useColumnsForParameterNames)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetDeleteCommand (bool useColumnsForParameterNames)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlCommand GetInsertCommand (bool useColumnsForParameterNames)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string QuoteIdentifier (string unquotedIdentifier)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string UnquoteIdentifier (string quotedIdentifier)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override string GetParameterName (int parameterOrdinal)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override string GetParameterName (string parameterName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override string GetParameterPlaceholder (int parameterOrdinal)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DataTable GetSchemaTable (DbCommand srcCommand)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override DbCommand InitializeCommand (DbCommand command)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlDataAdapter DataAdapter {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string QuotePrefix {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string QuoteSuffix {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string CatalogSeparator {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string SchemaSeparator {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override CatalogLocation CatalogLocation {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
	}
}
