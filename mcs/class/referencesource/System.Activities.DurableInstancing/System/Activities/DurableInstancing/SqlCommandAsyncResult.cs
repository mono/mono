//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Transactions;
    using System.Runtime.Diagnostics;

    sealed class SqlCommandAsyncResult : TransactedAsyncResult
    {

        static readonly TimeSpan MaximumOpenTimeout = TimeSpan.FromMinutes(2);

        static readonly RetryErrorCode[] retryErrorCodes = 
            { 
                new RetryErrorCode(-2, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SqlError: Timeout
                new RetryErrorCode(20, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - a connection failed early in the login process. to SQL Server.
                new RetryErrorCode(53, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), //A network-related or instance-specific error occurred while establishing a connection to SQL Server.
                new RetryErrorCode(64, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), //A transport-level error has occurred when receiving results from the server (TCP Provider, error: 0 - The specified network name is no longer available).
                new RetryErrorCode(121, RetryErrorOptions.RetryBeginOrEnd), //  A transport-level error has occurred
                new RetryErrorCode(233, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // Severed shared memory/named pipe connection drawn from the pool
                new RetryErrorCode(1205, RetryErrorOptions.RetryBeginOrEnd), // Deadlock
                new RetryErrorCode(1222, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // Lock Request Timeout
                new RetryErrorCode(3910, RetryErrorOptions.RetryOnBegin | RetryErrorOptions.RetryWhenTransaction), // Transaction context in use by another session.
                new RetryErrorCode(4060, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // Database not online
                new RetryErrorCode(8645, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // A timeout occurred while waiting for memory resources
                new RetryErrorCode(8641, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // Could not perform the operation because the requested memory grant was not available
                new RetryErrorCode(10053, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // A transport-level error has occurred when receiving results from the server
                new RetryErrorCode(10054, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // Severed tcp connection drawn from the pool
                new RetryErrorCode(10060, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // The server was not found or was not accessible. 
                new RetryErrorCode(10061, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Server not started
                new RetryErrorCode(10928, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - The limit for the database resource has been reached.
                new RetryErrorCode(10929, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - The server is currently too busy to support requests up to the maximum limit.
                new RetryErrorCode(40143, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - server encountered error processing the request.
                new RetryErrorCode(40197, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - server encountered error processing the request.
                new RetryErrorCode(40501, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - server is currently busy.
                new RetryErrorCode(40549, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - transaction blocking system calls
                new RetryErrorCode(40553, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction), // SQL Azure error - excessive memory usage
                new RetryErrorCode(40613, RetryErrorOptions.RetryBeginOrEnd | RetryErrorOptions.RetryWhenTransaction) // SQL Azure error - database on server is not available.
            };
        
        static AsyncCompletion onExecuteReaderCallback = new AsyncCompletion(OnExecuteReader);
        static AsyncCompletion onRetryCommandCallback = new AsyncCompletion(OnRetryCommand);
        string connectionString;
        DependentTransaction dependentTransaction;
        int maximumRetries;
        int retryCount;
        EventTraceActivity eventTraceActivity;

        SqlCommand sqlCommand;
        SqlDataReader sqlDataReader;
        TimeoutHelper timeoutHelper;

        public SqlCommandAsyncResult(SqlCommand sqlCommand, string connectionString, EventTraceActivity eventTraceActivity, DependentTransaction dependentTransaction,
            TimeSpan timeout, int retryCount, int maximumRetries, AsyncCallback callback, object state)
            : base(callback, state)
        {
            long openTimeout = Math.Min(timeout.Ticks, SqlCommandAsyncResult.MaximumOpenTimeout.Ticks);
            this.sqlCommand = sqlCommand;
            this.connectionString = connectionString;
            this.eventTraceActivity = eventTraceActivity;
            this.dependentTransaction = dependentTransaction;
            this.timeoutHelper = new TimeoutHelper(TimeSpan.FromTicks(openTimeout));
            this.retryCount = retryCount;
            this.maximumRetries = maximumRetries;
        }

        [Flags]
        enum RetryErrorOptions
        {
            RetryOnBegin = 1,
            RetryOnEnd = 2,
            RetryWhenTransaction = 4,
            RetryBeginOrEnd = RetryOnBegin | RetryOnEnd
        }

        public static SqlDataReader End(IAsyncResult result)
        {
            SqlCommandAsyncResult SqlCommandAsyncResult = AsyncResult.End<SqlCommandAsyncResult>(result);
            return SqlCommandAsyncResult.sqlDataReader;
        }

        public void StartCommand()
        {
            StartCommandInternal(true);
        }

        static bool OnExecuteReader(IAsyncResult result)
        {
            SqlCommandAsyncResult thisPtr = (SqlCommandAsyncResult)(result.AsyncState);
            return thisPtr.CompleteExecuteReader(result);
        }

        static bool OnRetryCommand(IAsyncResult childPtr)
        {
            SqlCommandAsyncResult parentPtr = (SqlCommandAsyncResult)(childPtr.AsyncState);
            parentPtr.sqlDataReader = SqlCommandAsyncResult.End(childPtr);
            return true;
        }

        static bool ShouldRetryForSqlError(int error, RetryErrorOptions retryErrorOptions)
        {
            if (Transaction.Current != null)
            {
                retryErrorOptions |= RetryErrorOptions.RetryWhenTransaction;
            }
            return SqlCommandAsyncResult.retryErrorCodes.Any(x => x.ErrorCode == error && (x.RetryErrorOptions & retryErrorOptions) == retryErrorOptions);
        }

        static void StartCommandCallback(object state)
        {
            SqlCommandAsyncResult thisPtr = (SqlCommandAsyncResult) state;
            try
            {
                // this can throw on the sync path - we need to signal the callback
                thisPtr.StartCommandInternal(false);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                if (thisPtr.sqlCommand.Connection != null)
                {
                    thisPtr.sqlCommand.Connection.Close();
                }

                thisPtr.Complete(false, e);
            }
        }
        bool CheckRetryCount()
        {
            return (++this.retryCount < maximumRetries);
        }

        bool CheckRetryCountAndTimer()
        {
            return (this.CheckRetryCount() && !this.HasOperationTimedOut());
        }

        bool CompleteExecuteReader(IAsyncResult result)
        {
            bool completeSelf = true;

            try
            {
                this.sqlDataReader = this.sqlCommand.EndExecuteReader(result);
            }
            catch (SqlException exception)
            {
                if (TD.SqlExceptionCaughtIsEnabled())
                {
                    TD.SqlExceptionCaught(this.eventTraceActivity, exception.Number.ToString(CultureInfo.InvariantCulture), exception.Message);
                }

                if (this.sqlDataReader != null)
                {
                    this.sqlDataReader.Close();
                }

                if (this.sqlCommand.Connection != null)
                {
                    this.sqlCommand.Connection.Close();
                }

                // If we completed sync then any retry is done by the original caller.
                if (!result.CompletedSynchronously)
                {
                    if (this.CheckRetryCountAndTimer() && ShouldRetryForSqlError(exception.Number, RetryErrorOptions.RetryOnEnd))
                    {
                        if (this.EnqueueRetry())
                        {
                            if (TD.RetryingSqlCommandDueToSqlErrorIsEnabled())
                            {
                                TD.RetryingSqlCommandDueToSqlError(this.eventTraceActivity, exception.Number.ToString(CultureInfo.InvariantCulture));
                            }
                            completeSelf = false;
                        }
                    }
                }

                if (completeSelf)
                {
                    if (this.retryCount == maximumRetries && TD.MaximumRetriesExceededForSqlCommandIsEnabled())
                    {
                        TD.MaximumRetriesExceededForSqlCommand(this.eventTraceActivity);
                    }

                    throw;
                }
            }

            return completeSelf;
        }

        bool EnqueueRetry()
        {
            bool result = false;

            int delay = this.GetRetryDelay();

            if (this.timeoutHelper.RemainingTime().TotalMilliseconds > delay)
            {
                this.sqlCommand.Dispose();
                IOThreadTimer iott = new IOThreadTimer(StartCommandCallback, new SqlCommandAsyncResult(CloneSqlCommand(this.sqlCommand), this.connectionString, this.eventTraceActivity, this.dependentTransaction,
                    this.timeoutHelper.RemainingTime(), this.retryCount, this.maximumRetries, this.PrepareAsyncCompletion(onRetryCommandCallback), this), false);
                iott.Set(delay);

                if (TD.QueuingSqlRetryIsEnabled())
                {
                    TD.QueuingSqlRetry(this.eventTraceActivity, delay.ToString(CultureInfo.InvariantCulture));
                }

                result = true;
            }

            return result;
        }

        static SqlCommand CloneSqlCommand(SqlCommand command)
        {
            //We do not want to use SqlCommand.Clone here because we do not want to replicate the parameters
            SqlCommand newCommand = new SqlCommand()
            {
                CommandType = command.CommandType,
                CommandText = command.CommandText,
            };

            SqlParameter[] tempParameterList = new SqlParameter[command.Parameters.Count];
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                tempParameterList[i] = command.Parameters[i];
            }
            command.Parameters.Clear();
            newCommand.Parameters.AddRange(tempParameterList);

            return newCommand;
        }

        int GetRetryDelay()
        {
            return 1000;
        }

        bool HasOperationTimedOut()
        {
            return (this.timeoutHelper.RemainingTime() <= TimeSpan.Zero);
        }

        void StartCommandInternal(bool synchronous)
        {
            if (!this.HasOperationTimedOut())
            {
                try
                {
                    IAsyncResult result;

                    using (this.PrepareTransactionalCall(this.dependentTransaction))
                    {
                        AsyncCallback wrappedCallback = this.PrepareAsyncCompletion(onExecuteReaderCallback);
                        this.sqlCommand.Connection = StoreUtilities.CreateConnection(this.connectionString);
                        if (!this.HasOperationTimedOut())
                        {
                            result = this.sqlCommand.BeginExecuteReader(wrappedCallback, this, CommandBehavior.CloseConnection);
                        }
                        else 
                        {
                            this.sqlCommand.Connection.Close();
                            this.Complete(synchronous, new TimeoutException(SR.TimeoutOnSqlOperation(this.timeoutHelper.OriginalTimeout.ToString())));
                            return;
                        }
                    }

                    if (this.CheckSyncContinue(result))
                    {
                        if (this.CompleteExecuteReader(result))
                        {
                            this.Complete(synchronous);
                        }
                    }
                    return;
                }
                catch (SqlException exception)
                {
                    if (TD.SqlExceptionCaughtIsEnabled())
                    {
                        TD.SqlExceptionCaught(this.eventTraceActivity, exception.Number.ToString(null, CultureInfo.InvariantCulture), exception.Message);
                    }

                    if (this.sqlCommand.Connection != null)
                    {
                        this.sqlCommand.Connection.Close();
                    }

                    if (!this.CheckRetryCount() || !ShouldRetryForSqlError(exception.Number, RetryErrorOptions.RetryOnBegin))
                    {
                        throw;
                    }

                    if (TD.RetryingSqlCommandDueToSqlErrorIsEnabled())
                    {
                        TD.RetryingSqlCommandDueToSqlError(this.eventTraceActivity, exception.Number.ToString(CultureInfo.InvariantCulture));
                    }
                }
                catch (InvalidOperationException)
                {
                    if (!this.CheckRetryCount())
                    {
                        throw;
                    }
                }

                if (this.EnqueueRetry())
                {
                    return;
                }
            }

            if (this.HasOperationTimedOut())
            {
                if (TD.TimeoutOpeningSqlConnectionIsEnabled())
                {
                    TD.TimeoutOpeningSqlConnection(this.eventTraceActivity, this.timeoutHelper.OriginalTimeout.ToString());
                }
            }
            else
            {
                if (TD.MaximumRetriesExceededForSqlCommandIsEnabled())
                {
                    TD.MaximumRetriesExceededForSqlCommand(this.eventTraceActivity);
                }
            }

            this.Complete(synchronous, new TimeoutException(SR.TimeoutOnSqlOperation(this.timeoutHelper.OriginalTimeout.ToString())));
        }

        class RetryErrorCode
        {
            public RetryErrorCode(int code, RetryErrorOptions retryErrorOptions)
            {
                this.ErrorCode = code;
                this.RetryErrorOptions = retryErrorOptions;
            }

            public int ErrorCode
            {
                get;
                private set;
            }

            public RetryErrorOptions RetryErrorOptions
            {
                get;
                private set;
            }
        }
    }
}
