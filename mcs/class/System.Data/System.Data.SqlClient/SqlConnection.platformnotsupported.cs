// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using Microsoft.SqlServer.Server;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Security;

namespace System.Data.SqlClient
{
	public sealed partial class SqlConnection : DbConnection, ICloneable
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlConnection is not supported on the current platform.";

		internal SqlStatistics _statistics;
		internal Task _currentReconnectionTask;
		internal SessionData _recoverySessionData;
		internal bool _suppressStateChangeForReconnection;
		internal bool _applyTransientFaultHandling = false;

		public SqlConnection() : base() => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		public SqlConnection(string connectionString) : this() {}

		public bool StatisticsEnabled {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal bool AsyncCommandInProgress {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal SqlConnectionString.TransactionBindingEnum TransactionBinding
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlConnectionString.TypeSystem TypeSystem
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal Version TypeSystemAssemblyVersion
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int ConnectRetryInterval
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string ConnectionString {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override int ConnectionTimeout
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string Database
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string DataSource
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public int PacketSize
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Guid ClientConnectionId
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string ServerVersion
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override ConnectionState State
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlStatistics Statistics
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public string WorkstationId
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		protected override DbProviderFactory DbProviderFactory
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public event SqlInfoMessageEventHandler InfoMessage;

		public bool FireInfoMessageEventOnUserErrors {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		internal int ReconnectCount
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool ForceNewConnection {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		protected override void OnStateChange(StateChangeEventArgs stateChange)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlTransaction BeginTransaction()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlTransaction BeginTransaction(IsolationLevel iso)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlTransaction BeginTransaction(string transactionName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlTransaction BeginTransaction(IsolationLevel iso, string transactionName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void ChangeDatabase(string database)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public static void ChangePassword(string connectionString, string newPassword)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public static void ChangePassword(string connectionString, SqlCredential credential, SecureString newSecurePassword)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public static void ClearAllPools()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public static void ClearPool(SqlConnection connection)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void Close()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		new public SqlCommand CreateCommand()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override void Open()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void RegisterWaitingForReconnect(Task waitingTask)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal Task ValidateAndReconnect(Action beforeDisconnect, int timeout)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override Task OpenAsync(CancellationToken cancellationToken)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override DataTable GetSchema()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override DataTable GetSchema(string collectionName)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override DataTable GetSchema(string collectionName, string[] restrictionValues)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool HasLocalTransaction
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool HasLocalTransactionFromAPI
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool IsKatmaiOrNewer
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal TdsParser Parser
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void ValidateConnectionForExecute(string method, SqlCommand command)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal static string FixupDatabaseTransactionName(string name)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnError(SqlException exception, bool breakConnection, Action<Action> wrapCloseInAction)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlInternalConnectionTds GetOpenTdsConnection()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal SqlInternalConnectionTds GetOpenTdsConnection(string method)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnInfoMessage(SqlInfoMessageEventArgs imevent)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void OnInfoMessage(SqlInfoMessageEventArgs imevent, out bool notified)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void RegisterForConnectionCloseNotification<T>(ref Task<T> outerTask, object value, int tag)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public void ResetStatistics()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public IDictionary RetrieveStatistics()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		object ICloneable.Clone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void CheckGetExtendedUDTInfo(SqlMetaDataPriv metaData, bool fThrow)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal object GetUdtValue(object value, SqlMetaDataPriv metaData, bool returnDBNull)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal byte[] GetBytes(object o)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal byte[] GetBytes(object o, out Format format, out int maxSize)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal int CloseCount
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal DbConnectionFactory ConnectionFactory
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal DbConnectionOptions ConnectionOptions
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);


		internal DbConnectionInternal InnerConnection
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal System.Data.ProviderBase.DbConnectionPoolGroup PoolGroup {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		internal DbConnectionOptions UserConnectionOptions
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void Abort(Exception e)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void AddWeakReference(object value, int tag)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected DbCommand CreateDbCommand()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		override protected void Dispose(bool disposing) {}

		public override void EnlistTransaction(Transaction transaction)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void NotifyWeakReference(int message)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void PermissionDemand()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void RemoveWeakReference(object value)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void SetInnerConnectionEvent(DbConnectionInternal to)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal bool SetInnerConnectionFrom(DbConnectionInternal to, DbConnectionInternal from)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		internal void SetInnerConnectionTo(DbConnectionInternal to)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
	}
}
