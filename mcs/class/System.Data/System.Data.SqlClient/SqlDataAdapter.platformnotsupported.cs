// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;

namespace System.Data.SqlClient
{
	public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlDataAdapter is not supported on the current platform.";

		public SqlDataAdapter() : base() {}
		public SqlDataAdapter(SqlCommand selectCommand) : this() {}
		public SqlDataAdapter(string selectCommandText, string selectConnectionString) : this() {}
		public SqlDataAdapter(string selectCommandText, SqlConnection selectConnection) : this() {}

		new public SqlCommand DeleteCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		new public SqlCommand InsertCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		new public SqlCommand SelectCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		new public SqlCommand UpdateCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override int UpdateBatchSize {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected override int AddToBatch(IDbCommand command)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void ClearBatch()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override int ExecuteBatch()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void InitializeBatching()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override void TerminateBatching()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		object ICloneable.Clone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public event SqlRowUpdatedEventHandler RowUpdated;
		public event SqlRowUpdatingEventHandler RowUpdating;

		override protected void OnRowUpdated(RowUpdatedEventArgs value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected void OnRowUpdating(RowUpdatingEventArgs value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
	}
}
