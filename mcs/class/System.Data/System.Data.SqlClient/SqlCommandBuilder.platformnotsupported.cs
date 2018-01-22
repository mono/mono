// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommandBuilder (SqlDataAdapter adapter)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public static void DeriveParameters (SqlCommand command)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetDeleteCommand ()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetInsertCommand ()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetUpdateCommand ()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetUpdateCommand (bool useColumnsForParameterNames)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetDeleteCommand (bool useColumnsForParameterNames)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand GetInsertCommand (bool useColumnsForParameterNames)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string QuoteIdentifier (string unquotedIdentifier)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string UnquoteIdentifier (string quotedIdentifier)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, bool whereClause)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override string GetParameterName (int parameterOrdinal)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override string GetParameterName (string parameterName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override string GetParameterPlaceholder (int parameterOrdinal)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override DataTable GetSchemaTable (DbCommand srcCommand)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override DbCommand InitializeCommand (DbCommand command)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlDataAdapter DataAdapter {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string QuotePrefix {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string QuoteSuffix {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string CatalogSeparator {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override string SchemaSeparator {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override CatalogLocation CatalogLocation {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
