//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    abstract class SqlWorkflowInstanceStoreAsyncResult : AsyncResult
    {
        static Action<AsyncResult, Exception> finallyCallback = new Action<AsyncResult, Exception>(Finally);
        static AsyncCompletion onBindReclaimed = new AsyncCompletion(OnBindReclaimed);
        static AsyncCompletion onSqlCommandAsyncResultCallback = new AsyncCompletion(SqlCommandAsyncResultCallback);

        SqlCommand sqlCommand;
        int maximumRetries;

        protected SqlWorkflowInstanceStoreAsyncResult
            (
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout,
            AsyncCallback callback,
            object state
            ) 
            : this(context, command, store, storeLock, currentTransaction, timeout, store.MaxConnectionRetries, callback, state)
        {
        }

        // ExtendLockAsyncResult and RecoverInstanceLocksAsyncResult directly call this ctor
        protected SqlWorkflowInstanceStoreAsyncResult
            (
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout,
            int maximumRetries,
            AsyncCallback callback,
            object state
            ) :
            base(callback, state)
        {
            this.DependentTransaction = (currentTransaction != null) ? currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete) : null;
            this.InstancePersistenceContext = context;
            this.InstancePersistenceCommand = command;
            this.Store = store;
            this.StoreLock = storeLock;
            this.TimeoutHelper = new TimeoutHelper(timeout);
            this.OnCompleting += SqlWorkflowInstanceStoreAsyncResult.finallyCallback;
            this.maximumRetries = maximumRetries;
        }

        protected virtual string ConnectionString
        {
            get
            {
                return this.Store.CachedConnectionString;
            }
        }

        protected DependentTransaction DependentTransaction
        {
            get;
            set;
        }

        protected InstancePersistenceCommand InstancePersistenceCommand
        {
            get;
            private set;
        }

        protected InstancePersistenceContext InstancePersistenceContext
        {
            get;
            private set;
        }

        protected SqlWorkflowInstanceStore Store
        {
            get;
            private set;
        }

        protected SqlWorkflowInstanceStoreLock StoreLock
        {
            get;
            private set;
        }

        protected TimeoutHelper TimeoutHelper
        {
            get;
            set;
        }

        public static bool End(IAsyncResult result)
        {
            if (result == null)
            {
                throw FxTrace.Exception.ArgumentNull("result");
            }

            AsyncResult.End<SqlWorkflowInstanceStoreAsyncResult>(result);

            return true;
        }

        public virtual void ScheduleCallback()
        {
            ActionItem.Schedule(SqlWorkflowInstanceStoreAsyncResult.StartOperationCallback, this);
        }

        protected abstract void GenerateSqlCommand(SqlCommand sqlCommand);
        protected abstract string GetSqlCommandText();
        protected abstract CommandType GetSqlCommandType();

        protected virtual void OnCommandCompletion()
        {
        }
        protected abstract Exception ProcessSqlResult(SqlDataReader reader);

        protected virtual bool OnSqlProcessingComplete()
        {
            return true;
        }

        protected virtual void OnSqlException(Exception exception, out bool handled)
        {
            handled = false;
        }

        static void Finally(AsyncResult result, Exception currentException)
        {
            SqlWorkflowInstanceStoreAsyncResult thisPtr = result as SqlWorkflowInstanceStoreAsyncResult;

            try
            {
                if (thisPtr.DependentTransaction != null)
                {
                    using (thisPtr.DependentTransaction)
                    {
                        thisPtr.DependentTransaction.Complete();
                    }
                }
            }
            catch (TransactionException)
            {
                if (currentException == null)
                {
                    throw;
                }
            }
            finally
            {
                thisPtr.OnCommandCompletion();
                thisPtr.ClearMembers();
                StoreUtilities.TraceSqlCommand(thisPtr.sqlCommand, false);
            }
        }

        static bool OnBindReclaimed(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult thisPtr = (SqlWorkflowInstanceStoreAsyncResult)result.AsyncState;
            thisPtr.InstancePersistenceContext.EndBindReclaimedLock(result);
            Guid instanceId = thisPtr.InstancePersistenceContext.InstanceView.InstanceId;
            long lockVersion = thisPtr.InstancePersistenceContext.InstanceVersion;

            InstanceLockTracking instanceLockTracking = (InstanceLockTracking)(thisPtr.InstancePersistenceContext.UserContext);
            instanceLockTracking.TrackStoreLock(instanceId, lockVersion, null);
            thisPtr.InstancePersistenceContext.InstanceHandle.Free();

            throw FxTrace.Exception.AsError(new InstanceLockLostException(thisPtr.InstancePersistenceCommand.Name, instanceId));
        }

        static bool SqlCommandAsyncResultCallback(IAsyncResult result)
        {
            SqlWorkflowInstanceStoreAsyncResult thisPtr = (SqlWorkflowInstanceStoreAsyncResult)result.AsyncState;
            Exception delayedException = null;
            bool completeFlag = true;

            try
            {
                using (thisPtr.sqlCommand)
                {
                    using (SqlDataReader reader = SqlCommandAsyncResult.End(result))
                    {
                        delayedException = thisPtr.ProcessSqlResult(reader);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                Guid instanceId = (thisPtr.InstancePersistenceContext != null) ? thisPtr.InstancePersistenceContext.InstanceView.InstanceId : Guid.Empty;
                delayedException = new InstancePersistenceCommandException(thisPtr.InstancePersistenceCommand.Name, instanceId, exception);
            }

            if (delayedException is InstanceAlreadyLockedToOwnerException)
            {
                InstanceAlreadyLockedToOwnerException alreadyLockedException = (InstanceAlreadyLockedToOwnerException) delayedException;
                long reclaimLockAtVersion = alreadyLockedException.InstanceVersion;

                if (!thisPtr.InstancePersistenceContext.InstanceView.IsBoundToInstance)
                {
                    thisPtr.InstancePersistenceContext.BindInstance(alreadyLockedException.InstanceId);
                }

                IAsyncResult bindReclaimedAsyncResult = thisPtr.InstancePersistenceContext.BeginBindReclaimedLock(reclaimLockAtVersion, thisPtr.TimeoutHelper.RemainingTime(), thisPtr.PrepareAsyncCompletion(SqlWorkflowInstanceStoreAsyncResult.onBindReclaimed), thisPtr);

                if (!thisPtr.SyncContinue(bindReclaimedAsyncResult))
                {
                    completeFlag = false;
                }
            }
            else if (delayedException != null)
            {
                if (thisPtr.sqlCommand.Connection != null)
                {
                    thisPtr.sqlCommand.Connection.Close();
                }

                bool handled = false;
                thisPtr.OnSqlException(delayedException, out handled);

                if (!handled)
                {
                    thisPtr.TraceException(delayedException);

                    throw FxTrace.Exception.AsError(delayedException);
                }
            }

            if (completeFlag)
            {
                completeFlag = thisPtr.OnSqlProcessingComplete();
            }

            return completeFlag;
        }

        static void StartOperationCallback(object state)
        {
            SqlWorkflowInstanceStoreAsyncResult sqlWorkflowInstanceStoreAsyncResult = (SqlWorkflowInstanceStoreAsyncResult) state;
            sqlWorkflowInstanceStoreAsyncResult.StartOperation();
        }

        void ClearMembers()
        {
            // Clear all AsyncResult Members so that ADO.NET's cached DbAsyncResult objects don't inadvertently hold
            // strong references to InstanceOwner, InstanceHandle or other Persistence Runtime objects

            this.InstancePersistenceCommand = null;
            this.InstancePersistenceContext = null;
            this.Store = null;
            this.StoreLock = null;
        }

        void StartOperation()
        {
            Guid instanceId = (this.InstancePersistenceContext != null) ? this.InstancePersistenceContext.InstanceView.InstanceId : Guid.Empty;
            Exception delayedException = null;

            try
            {
                this.sqlCommand = new SqlCommand();
                this.GenerateSqlCommand(this.sqlCommand);
                this.sqlCommand.CommandText = this.GetSqlCommandText();
                this.sqlCommand.CommandType = this.GetSqlCommandType();

                StoreUtilities.TraceSqlCommand(this.sqlCommand, true);
                SqlCommandAsyncResult sqlCommandResult = new SqlCommandAsyncResult(this.sqlCommand, this.ConnectionString,
                    (this.InstancePersistenceContext != null) ? this.InstancePersistenceContext.EventTraceActivity : null,
                    this.DependentTransaction, this.TimeoutHelper.RemainingTime(), 0, this.maximumRetries, PrepareAsyncCompletion(onSqlCommandAsyncResultCallback), this);
                sqlCommandResult.StartCommand();

                if (!SyncContinue(sqlCommandResult))
                {
                    return;
                }
            }
            catch (InstancePersistenceException instancePersistenceException)
            {
                delayedException = instancePersistenceException;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                delayedException = new InstancePersistenceCommandException(this.InstancePersistenceCommand.Name, instanceId, exception);
            }

            if (delayedException != null)
            {
                if (this.sqlCommand.Connection != null)
                {
                    this.sqlCommand.Connection.Close();
                }

                this.sqlCommand.Dispose();
                this.TraceException(delayedException);
            }

            this.Complete(false, delayedException);
        }

        void TraceException(Exception exception)
        {
            bool traceException = true;

            if (this.Store.IsLockRetryEnabled() && (exception is InstanceLockedException))
            {
                traceException = false;
            }

            if (traceException && TD.FoundProcessingErrorIsEnabled())
            {
                TD.FoundProcessingError((this.InstancePersistenceContext != null) ? this.InstancePersistenceContext.EventTraceActivity : null,
                    exception.Message, exception);
            }
        }
    }
}
