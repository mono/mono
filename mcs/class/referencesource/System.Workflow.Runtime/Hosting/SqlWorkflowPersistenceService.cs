using System;
using System.IO;
using System.Transactions;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Threading;

using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Globalization;

namespace System.Workflow.Runtime.Hosting
{
    #region PersistenceDBAccessor


    internal sealed class PendingWorkItem
    {
        public enum ItemType { Instance, CompletedScope, ActivationComplete };

        public ItemType Type;
        public Guid InstanceId;
        public Guid StateId;
        public Byte[] SerializedActivity;
        public int Status;
        public int Blocked;
        public string Info;
        public bool Unlocked;
        public SqlDateTime NextTimer;
    }

    /// <summary>
    /// This class does DB accessing work in the context of one connection
    /// </summary>
    internal sealed class PersistenceDBAccessor : IDisposable
    {

        DbResourceAllocator dbResourceAllocator;
        DbTransaction localTransaction;
        DbConnection connection;
        bool needToCloseConnection;
        DbRetry dbRetry = null;

        private class RetryReadException : Exception
        {
        }

#if DEBUG
        private static Dictionary<System.Guid, string> _persistenceToDatabaseMap = new Dictionary<Guid, string>();

        private static void InsertToDbMap(Guid serviceId, string dbName)
        {
            lock (_persistenceToDatabaseMap)
            {
                if (!_persistenceToDatabaseMap.ContainsKey(serviceId))
                {
                    _persistenceToDatabaseMap[serviceId] = dbName;
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): writing to database: {1}", serviceId.ToString(), dbName);
                }
            }
        }
#endif
        #region Constructor/Disposer


        /// <summary>
        /// DB access done under a transaction and uses a connection enlisted to this transaction.
        /// </summary>
        /// <param name="dbResourceAllocator">Helper to get database connection/command/store procedure parameters/etc</param>
        /// <param name="transaction">The transaction to do this work under</param>        
        internal PersistenceDBAccessor(
            DbResourceAllocator dbResourceAllocator,
            System.Transactions.Transaction transaction,
            WorkflowCommitWorkBatchService transactionService)
        {
            this.dbResourceAllocator = dbResourceAllocator;
            this.localTransaction = DbResourceAllocator.GetLocalTransaction(
                transactionService, transaction);
            // Get a connection enlisted to this transaction, may or may not need to be freed depending on 
            // if the transaction does connection sharing
            this.connection = this.dbResourceAllocator.GetEnlistedConnection(
                transactionService, transaction, out needToCloseConnection);
            //
            // No retries for external transactions
            this.dbRetry = new DbRetry(false);
        }

        /// <summary>
        /// DB access done without a transaction in a newly opened connection 
        /// </summary>
        /// <param name="dbResourceAllocator">Helper to get database connection/command/store procedure parameters/etc</param>
        internal PersistenceDBAccessor(DbResourceAllocator dbResourceAllocator, bool enableRetries)
        {
            this.dbResourceAllocator = dbResourceAllocator;
            this.dbRetry = new DbRetry(enableRetries);
            DbConnection conn = null;
            short count = 0;
            while (true)
            {
                try
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService OpenConnection start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    conn = this.dbResourceAllocator.OpenNewConnection();
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService. OpenConnection end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));

                    if ((null == conn) || (ConnectionState.Open != conn.State))
                        throw new InvalidOperationException(ExecutionStringManager.InvalidConnection);
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService caught exception from OpenConnection: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService retrying.");
                        continue;
                    }
                    throw;
                }
            }
            connection = conn;
            needToCloseConnection = true;
        }


        public void Dispose()
        {
            if (needToCloseConnection)
            {
                Debug.Assert(this.connection != null, "No connection to dispose");
                this.connection.Dispose();
            }
        }

        #endregion Constructor/Disposer

        #region Public Methods Exposed for the Batch to call

        private object DbOwnerId(Guid ownerId)
        {
            // Empty guid signals no lock, but the database uses null for that, so convert empty to null
            if (ownerId == Guid.Empty)
                return null;
            return ownerId;
        }

        public void InsertInstanceState(PendingWorkItem item, Guid ownerId, DateTime ownedUntil)
        {
            /* sproc params
            @uidInstanceID uniqueidentifier,
            @state image,
            @status int,
            @artifacts image,
            @queueingState image,
            @unlocked int,
            @blocked int,
            @info ntext,
            @ownerId uniqueidentifier,
            @ownedUntil datetime
            @nextTimer datetime
            */
            DbCommand command = NewStoredProcCommand("InsertInstanceState");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", item.InstanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@state", item.SerializedActivity));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@status", item.Status));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@unlocked", item.Unlocked));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@blocked", item.Blocked));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@info", item.Info));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", ownedUntil == DateTime.MaxValue ? SqlDateTime.MaxValue : ownedUntil));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@nextTimer", item.NextTimer));
            DbParameter p1 = this.dbResourceAllocator.NewDbParameter();
            p1.ParameterName = "@result";
            p1.DbType = DbType.Int32;
            p1.Value = 0;
            p1.Direction = ParameterDirection.InputOutput;
            command.Parameters.Add(p1);
            //command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@result", DbType.Int32, ParameterDirection.Output));
            DbParameter p2 = this.dbResourceAllocator.NewDbParameter();
            p2.ParameterName = "@currentOwnerID";
            p2.DbType = DbType.Guid;
            p2.Value = Guid.Empty;
            p2.Direction = ParameterDirection.InputOutput;
            command.Parameters.Add(p2);
            //command.Parameters.Add(new DbParameter(this.dbResourceAllocator.NewDbParameter("@currentOwnerID", DbType.Guid, ParameterDirection.InputOutput));
#if DEBUG
            InsertToDbMap(ownerId, connection.Database);
#endif
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): inserting instance: {1}, unlocking: {2} database: {3}", ownerId.ToString(), item.InstanceId.ToString(), item.Unlocked.ToString(), connection.Database);
            //
            // Cannot retry locally here as we don't own the tx
            // Rely on external retries at the batch commit or workflow level (for tx scopes)
            command.ExecuteNonQuery();
            CheckOwnershipResult(command);
        }


        public void InsertCompletedScope(Guid instanceId, Guid scopeId, Byte[] state)
        {
            /* sproc params
            @completedScopeID uniqueidentifier,
            @state image
            */
            DbCommand command = NewStoredProcCommand("InsertCompletedScope");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("instanceID", instanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("completedScopeID", scopeId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("state", state));
            //
            // Cannot retry locally here as we don't own the tx
            // Rely on external retries at the batch commit or workflow level (for tx scopes)
            command.ExecuteNonQuery();
        }

        public void ActivationComplete(Guid instanceId, Guid ownerId)
        {
            /* sproc params
            @instanceID uniqueidentifier,
            @ownerID uniqueidentifier,
            */
            DbCommand command = NewStoredProcCommand("UnlockInstanceState");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", instanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
#if DEBUG
            InsertToDbMap(ownerId, connection.Database);
#endif
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): unlocking instance: {1}, database: {2}", ownerId.ToString(), instanceId.ToString(), connection.Database);
            command.ExecuteNonQuery();
        }

        public IList<Guid> RetrieveNonblockingInstanceStateIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> gs = null;
            DbDataReader dr = null;
            short count = 0;
            while (true)
            {
                try
                {
                    //
                    // Check and reset the connection as needed before building the command
                    if ((null == connection) || (ConnectionState.Open != connection.State))
                        ResetConnection();

                    DbCommand command = NewStoredProcCommand("RetrieveNonblockingInstanceStateIds");
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", ownedUntil == DateTime.MaxValue ? SqlDateTime.MaxValue : ownedUntil));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@now", DateTime.UtcNow));

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds ExecuteReader start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds ExecuteReader end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));

                    gs = new List<Guid>();
                    while (dr.Read())
                    {
                        gs.Add(dr.GetGuid(0));
                    }
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds caught exception from ExecuteReader: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds retrying.");
                        continue;
                    }
                    throw;
                }
                finally
                {
                    if (dr != null)
                        dr.Close();
                }
            }

            return gs;
        }

        public bool TryRetrieveANonblockingInstanceStateId(Guid ownerId, DateTime ownedUntil, out Guid instanceId)
        {
            short count = 0;
            while (true)
            {
                try
                {
                    //
                    // Check and reset the connection as needed before building the command
                    if ((null == connection) || (ConnectionState.Open != connection.State))
                        ResetConnection();

                    DbCommand command = NewStoredProcCommand("RetrieveANonblockingInstanceStateId");
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", ownedUntil == DateTime.MaxValue ? SqlDateTime.MaxValue : ownedUntil));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
                    DbParameter p2 = this.dbResourceAllocator.NewDbParameter();
                    p2.ParameterName = "@uidInstanceID";
                    p2.DbType = DbType.Guid;
                    p2.Value = null;
                    p2.Direction = ParameterDirection.InputOutput;

                    command.Parameters.Add(p2);

                    DbParameter found = this.dbResourceAllocator.NewDbParameter();
                    found.ParameterName = "@found";
                    found.DbType = DbType.Boolean;
                    found.Value = null;
                    found.Direction = ParameterDirection.Output;

                    command.Parameters.Add(found);

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    command.ExecuteNonQuery();
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));

                    if ((null != found.Value) && ((bool)found.Value))
                    {
                        instanceId = (Guid)p2.Value;
                        return true;
                    }
                    else
                    {
                        instanceId = Guid.Empty;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId caught exception from ExecuteNonQuery: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId retrying.");
                        continue;
                    }
                    throw;
                }
            }

        }


        public IList<Guid> RetrieveExpiredTimerIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> gs = null;
            DbDataReader dr = null;

            short count = 0;
            while (true)
            {
                try
                {
                    //
                    // Check and reset the connection as needed before building the command
                    if ((null == connection) || (ConnectionState.Open != connection.State))
                        ResetConnection();

                    DbCommand command = NewStoredProcCommand("RetrieveExpiredTimerIds");
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", ownedUntil == DateTime.MaxValue ? SqlDateTime.MaxValue : ownedUntil));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@now", DateTime.UtcNow));

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds ExecuteReader start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds ExecuteReader end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));

                    gs = new List<Guid>();
                    while (dr.Read())
                    {
                        gs.Add(dr.GetGuid(0));
                    }
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds caught exception from ExecuteReader: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds retrying.");
                        continue;
                    }
                    throw;
                }
                finally
                {
                    if (dr != null)
                        dr.Close();
                }
            }

            return gs;
        }

        public Byte[] RetrieveInstanceState(Guid instanceStateId, Guid ownerId, DateTime timeout)
        {
            short count = 0;
            byte[] state = null;
            while (true)
            {
                try
                {
                    //
                    // Check and reset the connection as needed before building the command
                    if ((null == connection) || (ConnectionState.Open != connection.State))
                        ResetConnection();

                    DbCommand command = NewStoredProcCommand("RetrieveInstanceState");
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", instanceStateId));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", DbOwnerId(ownerId)));
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", timeout == DateTime.MaxValue ? SqlDateTime.MaxValue : timeout));
                    DbParameter p1 = this.dbResourceAllocator.NewDbParameter();
                    p1.ParameterName = "@result";
                    p1.DbType = DbType.Int32;
                    p1.Value = 0;
                    p1.Direction = ParameterDirection.InputOutput;
                    command.Parameters.Add(p1);
                    //command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@result", DbType.Int32, ParameterDirection.Output));
                    DbParameter p2 = this.dbResourceAllocator.NewDbParameter();
                    p2.ParameterName = "@currentOwnerID";
                    p2.DbType = DbType.Guid;
                    p2.Value = Guid.Empty;
                    p2.Direction = ParameterDirection.InputOutput;
                    command.Parameters.Add(p2);
                    //command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@currentOwnerID", DbType.Guid, ParameterDirection.InputOutput));
#if DEBUG
                    InsertToDbMap(ownerId, connection.Database);
#endif
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): retreiving instance: {1}, database: {2}", ownerId.ToString(), instanceStateId.ToString(), connection.Database);
                    state = RetrieveStateFromDB(command, true, instanceStateId);

                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveInstanceState caught exception: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveInstanceState retrying.");
                        continue;
                    }
                    else if (e is RetryReadException)    // ### hardcoded retry to work around sql ADM64 read bug ###
                    {
                        count++;
                        if (count < 10)
                            continue;
                        else
                            break;  // give up
                    }
                    throw;
                }
            }

            if (state == null || state.Length == 0)
            {
                Exception e = new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.InstanceNotFound, instanceStateId));
                e.Data["WorkflowNotFound"] = true;
                throw e;
            }

            return state;
        }

        public Byte[] RetrieveCompletedScope(Guid scopeId)
        {
            short count = 0;
            byte[] state = null;
            while (true)
            {
                try
                {
                    //
                    // Check and reset the connection as needed before building the command
                    if ((null == connection) || (ConnectionState.Open != connection.State))
                        ResetConnection();

                    DbCommand command = NewStoredProcCommand("RetrieveCompletedScope");
                    command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@completedScopeID", scopeId));
                    DbParameter p1 = this.dbResourceAllocator.NewDbParameter();
                    p1.ParameterName = "@result";
                    p1.DbType = DbType.Int32;
                    p1.Value = 0;
                    p1.Direction = ParameterDirection.InputOutput;
                    command.Parameters.Add(p1);

                    state = RetrieveStateFromDB(command, false, WorkflowEnvironment.WorkflowInstanceId);

                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveCompletedScope caught exception: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveCompletedScope retrying.");
                        continue;
                    }
                    else if (e is RetryReadException)    // ### hardcoded retry to work around sql ADM64 read bug ###
                    {
                        count++;
                        if (count < 10)
                            continue;
                        else
                            break;  // give up
                    }
                    throw;
                }
            }

            if (state == null || state.Length == 0)
                throw new InvalidOperationException(
                    string.Format(Thread.CurrentThread.CurrentCulture,
                    ExecutionStringManager.CompletedScopeNotFound, scopeId));
            return state;
        }

        #endregion

        #region private DB accessing methods

        /// <summary>
        /// This should only be called for non batch commits
        /// </summary>
        /// <returns></returns>
        private DbConnection ResetConnection()
        {
            if (null != localTransaction)
                throw new InvalidOperationException(ExecutionStringManager.InvalidOpConnectionReset);

            if (!needToCloseConnection)
                throw new InvalidOperationException(ExecutionStringManager.InvalidOpConnectionNotLocal);

            if ((null != connection) && (ConnectionState.Closed != connection.State))
                connection.Close();
            connection.Dispose();

            connection = this.dbResourceAllocator.OpenNewConnection();

            return connection;
        }

        /// <summary>
        /// Returns a stored procedure type DBCommand object with current connection and transaction
        /// </summary>
        /// <param name="commandText"></param>
        /// <returns>the command object</returns>
        private DbCommand NewStoredProcCommand(string commandText)
        {
            DbCommand command = DbResourceAllocator.NewCommand(commandText, this.connection, this.localTransaction);
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        private static void CheckOwnershipResult(DbCommand command)
        {
            DbParameter result = command.Parameters["@result"];
            if (result != null && result.Value != null && (int)result.Value == -2)   // -2 is an ownership conflict
            {
                if (command.Parameters.Contains("@currentOwnerID"))
                {
                    Guid currentOwnerId = Guid.Empty;
                    if (command.Parameters["@currentOwnerID"].Value is System.Guid)
                        currentOwnerId = (Guid)command.Parameters["@currentOwnerID"].Value;
                    Guid myId = (Guid)command.Parameters["@ownerID"].Value;
                    Guid instId = (Guid)command.Parameters["@uidInstanceID"].Value;
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): owership violation with {1} on instance {2}", myId.ToString(), currentOwnerId.ToString(), instId);
                }
                DbParameter instanceId = command.Parameters["@uidInstanceID"];
                throw new WorkflowOwnershipException((Guid)instanceId.Value);
            }
        }

        /// <summary>
        /// Helper to Public methods RetrieveInstanceState and RetrieveCompletedScope.
        /// Retrieves an object from the DB by calling the specified stored procedure with specified stored proc params.
        /// </summary>
        /// <param name="command">Contains the stored procedure setting to be used to query against the Database</param>
        /// <returns>an object to be casted to an activity
        ///     In case of RetrieveInstanceState, only running or suspended instances are returned and
        ///     exception is thrown for completed/terminated/not-found instances 
        /// </returns>
        private static Byte[] RetrieveStateFromDB(DbCommand command, bool checkOwnership, Guid instanceId)
        {
            DbDataReader dr = null;
            byte[] result = null;

            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB {0} ExecuteReader start: {1}", instanceId, DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                dr = command.ExecuteReader();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB {0} ExecuteReader end: {1}", instanceId, DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));

                if (dr.Read())
                {
                    result = (byte[])dr.GetValue(0);
                }
                else
                {
                    DbParameter resultParam = command.Parameters["@result"];
                    if (resultParam == null || resultParam.Value == null)
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB Failed to read results {0}", instanceId);
                    }
                    else if ((int)resultParam.Value > 0)    // found results but failed to read - sql bug - retry the query
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB Failed to read results {1}, @result == {0}", (int)resultParam.Value, instanceId);
                        throw new RetryReadException();
                    }
                }
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }

            if (checkOwnership)
                CheckOwnershipResult(command);

            return result;
        }

        #endregion private DB accessing methods


        internal IEnumerable<SqlPersistenceWorkflowInstanceDescription> RetrieveAllInstanceDescriptions()
        {
            List<SqlPersistenceWorkflowInstanceDescription> retval = new List<SqlPersistenceWorkflowInstanceDescription>();
            DbDataReader dr = null;
            try
            {
                DbCommand command = NewStoredProcCommand("RetrieveAllInstanceDescriptions");
                dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    retval.Add(new SqlPersistenceWorkflowInstanceDescription(
                        dr.GetGuid(0),
                        (WorkflowStatus)dr.GetInt32(1),
                        dr.GetInt32(2) == 1 ? true : false,
                        dr.GetString(3),
                        (SqlDateTime)dr.GetDateTime(4)
                    ));
                }
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }
            return retval;
        }
    }
    #endregion

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SqlWorkflowPersistenceService : WorkflowPersistenceService, IPendingWork
    {
        #region constants
        // Configure parameters for this services
        private const string InstanceOwnershipTimeoutSecondsToken = "OwnershipTimeoutSeconds";
        private const string UnloadOnIdleToken = "UnloadOnIdle";
        private const string EnableRetriesToken = "EnableRetries";

        #endregion constants

        private bool _enableRetries = false;
        private bool _ignoreCommonEnableRetries = false;
        private DbResourceAllocator _dbResourceAllocator;
        private WorkflowCommitWorkBatchService _transactionService;
        private Guid _serviceInstanceId = Guid.Empty;
        private TimeSpan _ownershipDelta;
        private Boolean _unloadOnIdle;
        const string LoadingIntervalToken = "LoadIntervalSeconds";
        TimeSpan loadingInterval = new TimeSpan(0, 2, 0);
        private TimeSpan maxLoadingInterval = new TimeSpan(365, 0, 0, 0, 0);
        SmartTimer loadingTimer;
        object timerLock = new object();
        TimeSpan infinite = new TimeSpan(Timeout.Infinite);
        private static int _deadlock = 1205;

        // Saved from constructor input to be used in service start initialization
        NameValueCollection configParameters;
        string unvalidatedConnectionString;

        public Guid ServiceInstanceId
        {
            get
            {
                return _serviceInstanceId;
            }
        }

        public TimeSpan LoadingInterval
        {
            get { return loadingInterval; }
        }

        public bool EnableRetries
        {
            get { return _enableRetries; }
            set
            {
                _enableRetries = value;
                _ignoreCommonEnableRetries = true;
            }
        }

        private DateTime OwnershipTimeout
        {
            get
            {
                DateTime timeout;
                if (_ownershipDelta == TimeSpan.MaxValue)
                    timeout = DateTime.MaxValue;
                else
                    timeout = DateTime.UtcNow + _ownershipDelta;
                return timeout;
            }
        }

        public SqlWorkflowPersistenceService(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);

            this.unvalidatedConnectionString = connectionString;
        }

        public SqlWorkflowPersistenceService(NameValueCollection parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);

            _ownershipDelta = TimeSpan.MaxValue;   // default is to never timeout
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (key.Equals(DbResourceAllocator.ConnectionStringToken, StringComparison.OrdinalIgnoreCase))
                    {
                        // the resource allocator (below) will process the connection string
                    }
                    else if (key.Equals(SqlWorkflowPersistenceService.InstanceOwnershipTimeoutSecondsToken, StringComparison.OrdinalIgnoreCase))
                    {
                        int seconds = Convert.ToInt32(parameters[SqlWorkflowPersistenceService.InstanceOwnershipTimeoutSecondsToken], System.Globalization.CultureInfo.CurrentCulture);
                        if (seconds < 0)
                            throw new ArgumentOutOfRangeException(InstanceOwnershipTimeoutSecondsToken, seconds, ExecutionStringManager.InvalidOwnershipTimeoutValue);
                        _ownershipDelta = new TimeSpan(0, 0, seconds);
                        _serviceInstanceId = Guid.NewGuid();
                        continue;
                    }
                    else if (key.Equals(SqlWorkflowPersistenceService.UnloadOnIdleToken, StringComparison.OrdinalIgnoreCase))
                    {
                        _unloadOnIdle = bool.Parse(parameters[key]);
                    }
                    else if (key.Equals(LoadingIntervalToken, StringComparison.OrdinalIgnoreCase))
                    {
                        int interval = int.Parse(parameters[key], CultureInfo.CurrentCulture);
                        if (interval > 0)
                            this.loadingInterval = new TimeSpan(0, 0, interval);
                        else
                            this.loadingInterval = TimeSpan.Zero;
                        if (this.loadingInterval > maxLoadingInterval)
                            throw new ArgumentOutOfRangeException(LoadingIntervalToken, this.LoadingInterval, ExecutionStringManager.LoadingIntervalTooLarge);
                    }
                    else if (key.Equals(SqlWorkflowPersistenceService.EnableRetriesToken, StringComparison.OrdinalIgnoreCase))
                    {
                        //
                        // We have a local value for enable retries
                        _enableRetries = bool.Parse(parameters[key]);
                        _ignoreCommonEnableRetries = true;
                    }
                    else
                    {
                        throw new ArgumentException(
                            String.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, key), "parameters");
                    }
                }
            }

            this.configParameters = parameters;
        }

        public SqlWorkflowPersistenceService(string connectionString, bool unloadOnIdle, TimeSpan instanceOwnershipDuration, TimeSpan loadingInterval)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            if (loadingInterval > maxLoadingInterval)
                throw new ArgumentOutOfRangeException("loadingInterval", loadingInterval, ExecutionStringManager.LoadingIntervalTooLarge);
            if (instanceOwnershipDuration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("instanceOwnershipDuration", instanceOwnershipDuration, ExecutionStringManager.InvalidOwnershipTimeoutValue);

            this._ownershipDelta = instanceOwnershipDuration;
            this._unloadOnIdle = unloadOnIdle;
            this.loadingInterval = loadingInterval;
            this.unvalidatedConnectionString = connectionString;
            _serviceInstanceId = Guid.NewGuid();
        }

        #region WorkflowRuntimeService

        override protected internal void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({1}): Starting, LoadInternalSeconds={0}", loadingInterval.TotalSeconds, _serviceInstanceId.ToString());

            _dbResourceAllocator = new DbResourceAllocator(this.Runtime, this.configParameters, this.unvalidatedConnectionString);

            // Check connection string mismatch if using SharedConnectionWorkflowTransactionService
            _transactionService = Runtime.GetService<WorkflowCommitWorkBatchService>();
            _dbResourceAllocator.DetectSharedConnectionConflict(_transactionService);

            //
            // If we didn't find a local value for enable retries
            // check in the common section
            if ((!_ignoreCommonEnableRetries) && (null != base.Runtime))
            {
                NameValueConfigurationCollection commonConfigurationParameters = base.Runtime.CommonParameters;
                if (commonConfigurationParameters != null)
                {
                    // Then scan for connection string in the common configuration parameters section
                    foreach (string key in commonConfigurationParameters.AllKeys)
                    {
                        if (string.Compare(EnableRetriesToken, key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _enableRetries = bool.Parse(commonConfigurationParameters[key].Value);
                            break;
                        }
                    }
                }
            }

            base.Start();
        }

        protected override void OnStarted()
        {
            if (loadingInterval > TimeSpan.Zero)
            {
                lock (timerLock)
                {
                    base.OnStarted();
                    loadingTimer = new SmartTimer(new TimerCallback(LoadWorkflowsWithExpiredTimers), null, loadingInterval, loadingInterval);
                }
            }
            RecoverRunningWorkflowInstances();
        }

        protected internal override void Stop()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): Stopping", _serviceInstanceId.ToString());
            lock (timerLock)
            {
                base.Stop();
                if (loadingTimer != null)
                {
                    loadingTimer.Dispose();
                    loadingTimer = null;
                }
            }
        }

        #endregion WorkflowRuntimeService

        private void RecoverRunningWorkflowInstances()
        {
            if (Guid.Empty == _serviceInstanceId)
            {
                //
                // Only one host, get all the ids in one go
                IList<Guid> instanceIds = null;
                using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
                {
                    instanceIds = persistenceDBAccessor.RetrieveNonblockingInstanceStateIds(_serviceInstanceId, OwnershipTimeout);
                }
                foreach (Guid instanceId in instanceIds)
                {
                    try
                    {
                        WorkflowInstance instance = Runtime.GetWorkflow(instanceId);
                        instance.Load();
                    }
                    catch (Exception e)
                    {
                        RaiseServicesExceptionNotHandledEvent(e, instanceId);
                    }
                }
            }
            else
            {
                using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
                {
                    //
                    // Load one at a time to avoid thrashing with other hosts
                    Guid instanceId;
                    while (persistenceDBAccessor.TryRetrieveANonblockingInstanceStateId(_serviceInstanceId, OwnershipTimeout, out instanceId))
                    {
                        try
                        {
                            WorkflowInstance instance = Runtime.GetWorkflow(instanceId);
                            instance.Load();
                        }
                        catch (Exception e)
                        {
                            RaiseServicesExceptionNotHandledEvent(e, instanceId);
                        }
                    }
                }
            }
        }


        private void LoadWorkflowsWithExpiredTimers(object ignored)
        {
            lock (timerLock)
            {
                if (this.State == WorkflowRuntimeServiceState.Started)
                {
                    IList<Guid> ids = null;
                    try
                    {

                        ids = LoadExpiredTimerIds();
                    }
                    catch (Exception e)
                    {
                        RaiseServicesExceptionNotHandledEvent(e, Guid.Empty);
                    }
                    if (ids != null)
                    {
                        foreach (Guid id in ids)
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({1}): Loading instance with expired timers {0}", id, _serviceInstanceId.ToString());
                            try
                            {
                                Runtime.GetWorkflow(id).Load();
                            }
                            // Ignore cases where the workflow has been stolen out from under us
                            catch (WorkflowOwnershipException)
                            { }
                            catch (ObjectDisposedException)
                            {
                                throw;
                            }
                            catch (InvalidOperationException ioe)
                            {
                                if (!ioe.Data.Contains("WorkflowNotFound"))
                                    RaiseServicesExceptionNotHandledEvent(ioe, id);
                            }
                            catch (Exception e)
                            {
                                RaiseServicesExceptionNotHandledEvent(e, id);
                            }
                        }
                    }
                }
            }
        }


        // uidInstanceID, status, blocked, info, nextTimer

        internal protected override void SaveWorkflowInstanceState(Activity rootActivity, bool unlock)
        {
            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");

            WorkflowStatus workflowStatus = WorkflowPersistenceService.GetWorkflowStatus(rootActivity);
            bool isInstanceBlocked = WorkflowPersistenceService.GetIsBlocked(rootActivity);
            string instanceInfo = WorkflowPersistenceService.GetSuspendOrTerminateInfo(rootActivity);
            Guid contextGuid = (Guid)rootActivity.GetValue(Activity.ActivityContextGuidProperty);

            PendingWorkItem item = new PendingWorkItem();
            item.Type = PendingWorkItem.ItemType.Instance;
            item.InstanceId = WorkflowEnvironment.WorkflowInstanceId;
            if (workflowStatus != WorkflowStatus.Completed && workflowStatus != WorkflowStatus.Terminated)
                item.SerializedActivity = WorkflowPersistenceService.GetDefaultSerializedForm(rootActivity);
            else
                item.SerializedActivity = new Byte[0];
            item.Status = (int)workflowStatus;
            item.Blocked = isInstanceBlocked ? 1 : 0;
            item.Info = instanceInfo;
            item.StateId = contextGuid;
            item.Unlocked = unlock;
            TimerEventSubscriptionCollection timers = (TimerEventSubscriptionCollection)rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
            Debug.Assert(timers != null, "TimerEventSubscriptionCollection should never be null, but it is");
            TimerEventSubscription sub = timers.Peek();
            item.NextTimer = sub == null ? SqlDateTime.MaxValue : sub.ExpiresAt;
            if (item.Info == null)
                item.Info = "";

            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({4}):Committing instance {0}, Blocked={1}, Unlocked={2}, NextTimer={3}", contextGuid.ToString(), item.Blocked, item.Unlocked, item.NextTimer.Value.ToLocalTime(), _serviceInstanceId.ToString());

            WorkflowEnvironment.WorkBatch.Add(this, item);
        }

        internal protected override void UnlockWorkflowInstanceState(Activity rootActivity)
        {
            PendingWorkItem item = new PendingWorkItem();
            item.Type = PendingWorkItem.ItemType.ActivationComplete;
            item.InstanceId = WorkflowEnvironment.WorkflowInstanceId;
            WorkflowEnvironment.WorkBatch.Add(this, item);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}):Unlocking instance {1}", _serviceInstanceId.ToString(), item.InstanceId.ToString());
        }

        internal protected override Activity LoadWorkflowInstanceState(Guid id)
        {
            using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}):Loading instance {1}", _serviceInstanceId.ToString(), id.ToString());
                byte[] state = persistenceDBAccessor.RetrieveInstanceState(id, _serviceInstanceId, OwnershipTimeout);
                return WorkflowPersistenceService.RestoreFromDefaultSerializedForm(state, null);
            }

        }

        public IList<Guid> LoadExpiredTimerWorkflowIds()
        {
            if (State == WorkflowRuntimeServiceState.Started)
            {
                return LoadExpiredTimerIds();
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowRuntimeNotStarted));
            }
        }

        private IList<Guid> LoadExpiredTimerIds()
        {
            using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
            {
                return persistenceDBAccessor.RetrieveExpiredTimerIds(_serviceInstanceId, OwnershipTimeout);
            }
        }

        internal protected override void SaveCompletedContextActivity(Activity completedScopeActivity)
        {
            PendingWorkItem item = new PendingWorkItem();
            item.Type = PendingWorkItem.ItemType.CompletedScope;
            item.SerializedActivity = WorkflowPersistenceService.GetDefaultSerializedForm(completedScopeActivity);
            item.InstanceId = WorkflowEnvironment.WorkflowInstanceId;
            item.StateId = ((ActivityExecutionContextInfo)completedScopeActivity.GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;

            WorkflowEnvironment.WorkBatch.Add(this, item);
        }

        internal protected override Activity LoadCompletedContextActivity(Guid id, Activity outerActivity)
        {
            using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
            {
                byte[] state = persistenceDBAccessor.RetrieveCompletedScope(id);
                return WorkflowPersistenceService.RestoreFromDefaultSerializedForm(state, outerActivity);
            }
        }

        internal protected override bool UnloadOnIdle(Activity activity)
        {
            return _unloadOnIdle;
        }

        public IEnumerable<SqlPersistenceWorkflowInstanceDescription> GetAllWorkflows()
        {
            if (State == WorkflowRuntimeServiceState.Started)
            {
                using (PersistenceDBAccessor persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, _enableRetries))
                {
                    return persistenceDBAccessor.RetrieveAllInstanceDescriptions();
                }
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.WorkflowRuntimeNotStarted));
            }
        }

        #region IPendingWork methods

        bool IPendingWork.MustCommit(ICollection items)
        {
            return true;
        }

        /// <summary>
        /// Commmit the work items using the transaction
        /// </summary>
        /// <param name="transaction"></param>        
        /// <param name="items"></param>
        void IPendingWork.Commit(System.Transactions.Transaction transaction, ICollection items)
        {
            PersistenceDBAccessor persistenceDBAccessor = null;
            try
            {
                persistenceDBAccessor = new PersistenceDBAccessor(_dbResourceAllocator, transaction, _transactionService);
                foreach (PendingWorkItem item in items)
                {
                    switch (item.Type)
                    {
                        case PendingWorkItem.ItemType.Instance:
                            persistenceDBAccessor.InsertInstanceState(item, _serviceInstanceId, OwnershipTimeout);
                            break;

                        case PendingWorkItem.ItemType.CompletedScope:
                            persistenceDBAccessor.InsertCompletedScope(item.InstanceId, item.StateId, item.SerializedActivity);
                            break;

                        case PendingWorkItem.ItemType.ActivationComplete:
                            persistenceDBAccessor.ActivationComplete(item.InstanceId, _serviceInstanceId);
                            break;

                        default:
                            Debug.Assert(false, "Committing unknown pending work item type in SqlPersistenceService.Commit()");
                            break;
                    }

                }
            }
            catch (SqlException se)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService({1})Exception thrown while persisting instance: {0}", se.Message, _serviceInstanceId.ToString());
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "stacktrace : {0}", se.StackTrace);

                if (se.Number == _deadlock)
                {
                    PersistenceException pe = new PersistenceException(se.Message, se);
                    throw pe;
                }
                else
                {
                    throw;
                }

            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService({1}): Exception thrown while persisting instance: {0}", e.Message, _serviceInstanceId.ToString());
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "stacktrace : {0}", e.StackTrace);
                throw e;
            }
            finally
            {
                if (persistenceDBAccessor != null)
                    persistenceDBAccessor.Dispose();
            }
        }


        /// <summary>
        /// Perform necesssary cleanup. Called when the scope
        /// has finished processing this batch of work items
        /// </summary>
        /// <param name="succeeded"></param>
        /// <param name="items"></param>
        void IPendingWork.Complete(bool succeeded, ICollection items)
        {
            if (loadingTimer != null && succeeded)
            {
                foreach (PendingWorkItem item in items)
                {
                    if (item.Type.Equals(PendingWorkItem.ItemType.Instance))
                    {
                        loadingTimer.Update((DateTime)item.NextTimer);
                    }
                }
            }
        }

        #endregion IPendingWork Methods

    }

    internal class SmartTimer : IDisposable
    {
        private object locker = new object();
        private Timer timer;
        private DateTime next;
        private bool nextChanged;
        private TimeSpan period;
        private TimerCallback callback;
        private TimeSpan minUpdate = new TimeSpan(0, 0, 5);
        private TimeSpan infinite = new TimeSpan(Timeout.Infinite);

        public SmartTimer(TimerCallback callback, object state, TimeSpan due, TimeSpan period)
        {
            this.period = period;
            this.callback = callback;
            this.next = DateTime.UtcNow + due;
            this.timer = new Timer(HandleCallback, state, due, infinite);
        }

        public void Update(DateTime newNext)
        {
            if (newNext < next && (next - DateTime.UtcNow) > minUpdate)
            {
                lock (locker)
                {
                    if (newNext < next && (next - DateTime.UtcNow) > minUpdate && timer != null)
                    {
                        next = newNext;
                        nextChanged = true;
                        TimeSpan when = next - DateTime.UtcNow;
                        if (when < TimeSpan.Zero)
                            when = TimeSpan.Zero;
                        timer.Change(when, infinite);
                    }
                }
            }
        }

        private void HandleCallback(object state)
        {
            try
            {
                callback(state);
            }
            finally
            {
                lock (locker)
                {
                    if (timer != null)
                    {
                        if (!nextChanged)
                            next = DateTime.UtcNow + period;
                        else
                            nextChanged = false;
                        TimeSpan when = next - DateTime.UtcNow;
                        if (when < TimeSpan.Zero)
                            when = TimeSpan.Zero;
                        timer.Change(when, infinite);
                    }
                }
            }
        }


        public void Dispose()
        {
            lock (locker)
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }
        }
    }
}

