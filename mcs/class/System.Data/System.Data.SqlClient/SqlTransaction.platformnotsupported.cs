// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data.Common;
using System.Diagnostics;

namespace System.Data.SqlClient
{
	public sealed class SqlTransaction : DbTransaction
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlTransaction is not supported on the current platform.";

		internal readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

		internal SqlTransaction(SqlInternalConnection internalConnection, SqlConnection con,
								IsolationLevel iso, SqlInternalTransaction internalTransaction)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlConnection Connection
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbConnection DbConnection
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlInternalTransaction InternalTransaction
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public IsolationLevel IsolationLevel
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool IsZombied
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlStatistics Statistics
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public void Commit()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void Dispose(bool disposing)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public void Rollback()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void Rollback(string transactionName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void Save(string savePointName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void Zombie()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
	}
}

