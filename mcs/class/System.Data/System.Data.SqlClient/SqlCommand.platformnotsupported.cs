// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.SqlServer.Server;

namespace System.Data.SqlClient
{
	public sealed partial class SqlCommand : DbCommand, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlCommand is not supported on the current platform.";

		internal SqlDependency _sqlDep;
		internal int _rowsAffected = -1;
		internal bool InPrepare
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand() : base() => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		public SqlCommand(string cmdText) : this() {}
		public SqlCommand(string cmdText, SqlConnection connection) : this() {}
		public SqlCommand(string cmdText, SqlConnection connection, SqlTransaction transaction) : this() {}

		new public SqlConnection Connection {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		override protected DbConnection DbConnection {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public SqlNotificationRequest Notification {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal SqlStatistics Statistics
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlTransaction Transaction {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		override protected DbTransaction DbTransaction {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		override public string CommandText {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		override public int CommandTimeout {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void ResetCommandTimeout()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public CommandType CommandType {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

#if DEBUG
		internal static int DebugForceAsyncWriteDelay {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
#endif

		public override bool DesignTimeVisible {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		new public SqlParameterCollection Parameters
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbParameterCollection DbParameterCollection
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public UpdateRowSource UpdatedRowSource {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public event StatementCompletedEventHandler StatementCompleted;

		internal void OnStatementCompleted(int recordCount)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public void Prepare()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void Unprepare()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public void Cancel()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlParameter CreateParameter()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbParameter CreateDbParameter()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected void Dispose(bool disposing) {}

		override public object ExecuteScalar()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override public int ExecuteNonQuery()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public XmlReader ExecuteXmlReader()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlDataReader ExecuteReader()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlDataReader ExecuteReader(CommandBehavior behavior)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlDataReader EndExecuteReader(IAsyncResult asyncResult)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal IAsyncResult BeginExecuteReader(CommandBehavior behavior, AsyncCallback callback, object stateObject)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public Task<SqlDataReader> ExecuteReaderAsync()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public Task<SqlDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task<XmlReader> ExecuteXmlReaderAsync()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Task<XmlReader> ExecuteXmlReaderAsync(CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal static readonly string[] PreKatmaiProcParamsNames = new string[] {};

		internal static readonly string[] KatmaiProcParamsNames = new string[] {};

		internal void DeriveParameters()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal _SqlMetaDataSet MetaData
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, [CallerMemberName] string method = "")
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, TaskCompletionSource<object> completion, int timeout, out Task task, bool asyncWrite = false, [CallerMemberName] string method = "")
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnDoneProc()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnReturnStatus(int status)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnReturnValue(SqlReturnValue rec, TdsParserStateObject stateObj)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal string BuildParamList(TdsParser parser, SqlParameterCollection parameters)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void CheckThrowSNIException()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnConnectionClosed()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal TdsParserStateObject StateObject
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool IsDirty {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal int InternalRecordsAffected {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void ClearBatchCommand()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool BatchRPCMode {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal void AddBatchCommand(string commandText, SqlParameterCollection parameters, CommandType cmdType)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int ExecuteBatchRPCCommand()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int? GetRecordsAffected(int commandIndex)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlException GetErrors(int commandIndex)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

#if DEBUG
		internal void CompletePendingReadWithSuccess(bool resetForcePendingReadsToWait)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void CompletePendingReadWithFailure(int errorCode, bool resetForcePendingReadsToWait)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
#endif

		internal void CancelIgnoreFailure()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		object ICloneable.Clone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlCommand Clone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public bool NotificationAutoEnlist {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginExecuteNonQuery()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IAsyncResult BeginExecuteXmlReader()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IAsyncResult BeginExecuteReader()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IAsyncResult BeginExecuteReader(CommandBehavior behavior)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
	}
}
