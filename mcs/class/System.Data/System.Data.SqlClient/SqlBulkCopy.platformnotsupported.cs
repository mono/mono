// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace System.Data.SqlClient
{
	public sealed class SqlBulkCopy : IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlBulkCopy is not supported on the current platform.";

		public SqlBulkCopy(SqlConnection connection)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlBulkCopy(SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction)
			: this(connection)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlBulkCopy(string connectionString) : this(new SqlConnection(connectionString))
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlBulkCopy(string connectionString, SqlBulkCopyOptions copyOptions)
			: this(connectionString)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public int BatchSize {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int BulkCopyTimeout {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool EnableStreaming {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlBulkCopyColumnMappingCollection ColumnMappings
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public string DestinationTableName {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int NotifyAfter {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public event SqlRowsCopiedEventHandler SqlRowsCopied;

		internal SqlStatistics Statistics
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		void IDisposable.Dispose() {}

		public void Close()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void WriteToServer(DbDataReader reader)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void WriteToServer(IDataReader reader)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void WriteToServer(DataTable table)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void WriteToServer(DataTable table, DataRowState rowState)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void WriteToServer(DataRow[] rows)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataRow[] rows)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataRow[] rows, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DbDataReader reader)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DbDataReader reader, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(IDataReader reader)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataTable table)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataTable table, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataTable table, DataRowState rowState)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task WriteToServerAsync(DataTable table, DataRowState rowState, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnConnectionClosed()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

#if DEBUG
		internal static bool _setAlwaysTaskOnWrite = false;
		internal static bool SetAlwaysTaskOnWrite {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
#endif
	}

	internal sealed class _ColumnMapping
	{
		internal int _sourceColumnOrdinal;
		internal _SqlMetaData _metadata;
		internal _ColumnMapping(int columnId, _SqlMetaData metadata) {}
	}

	internal sealed class Row
	{
		internal Row(int rowCount) {}
		internal object[] DataFields => null;
		internal object this[int index] => null;
	}

	internal sealed class Result
	{
		internal Result(_SqlMetaDataSet metadata) {}
		internal int Count => 0;
		internal _SqlMetaDataSet MetaData => null;
		internal Row this[int index] => null;
		internal void AddRow(Row row) {}
	}

	internal sealed class BulkCopySimpleResultSet
	{
		internal BulkCopySimpleResultSet() {}
		internal Result this[int idx] => null;
		internal void SetMetaData(_SqlMetaDataSet metadata) {}
		internal int[] CreateIndexMap() => null;
		internal object[] CreateRowBuffer() => null;
	}
}
