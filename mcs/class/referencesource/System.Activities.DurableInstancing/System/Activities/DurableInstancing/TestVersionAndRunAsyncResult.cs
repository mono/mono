// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;

    class TestDatabaseVersionAndRunAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static readonly AsyncCallback instanceCommandCompleteCallback = Fx.ThunkCallback(InstanceCommandCompleteCallback);
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[GetWorkflowInstanceStoreVersion]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        Transaction currentTransaction;
        Version targetVersion;

        public TestDatabaseVersionAndRunAsyncResult(
            InstancePersistenceContext context,
            InstancePersistenceCommand command,
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout,
            Version targetVersion,
            AsyncCallback callback,
            object state) :
            base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
            this.currentTransaction = currentTransaction;
            this.targetVersion = targetVersion;
        }

        public override void ScheduleCallback()
        {
            if (this.Store.DatabaseVersion != null)
            {
                // Database version has been fetched from the db, 
                // we can directly check the version and run the "real" command
                this.TestAndRun();
            }
            else
            {
                // Load it
                base.ScheduleCallback();
            }
        }
        
        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            // Nothing special to do here, the command has no parameters
        }

        protected override string GetSqlCommandText()
        {
            return commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            // reader returns rowset with Major, Minor, Build, Revision
            if (!reader.Read())
            {
                return new InvalidOperationException(SR.UnknownDatabaseVersion);
            }

            // In 4.0, the user could modify their version table to be invalid, and SWIS would still work.
            // So if the version is invalid, we just fall back to 4.0.
            this.Store.DatabaseVersion = GetVersion(reader) ?? StoreUtilities.Version40;
            return null;
        }

        protected override bool OnSqlProcessingComplete()
        {
            this.TestAndRun();
            return false;
        }

        protected override void OnSqlException(Exception exception, out bool handled)
        {
            handled = false;
            Exception currentException = exception;

            while (!(currentException is SqlException) && currentException.InnerException != null)
            {
                currentException = currentException.InnerException;
            }

            SqlException se = currentException as SqlException;

            if (se != null && se.Number == 2812)
            {
                // 2812 == object not found in sql
                // This is expected when running the db version lookup 
                // against a 4.0 database as the proc doesn't exist on 4.0
                // As a version check will only be run for commands that are
                // introduced post 4.0 hitting this path in production is unlikely
                // (it will be hit in development and the choice will be made to
                // either not use new features or upgrade the db, which will add this proc).
                // The alternative was to create a view over the db version table
                // and allow & issue ad-hoc queries against that view
                this.Store.DatabaseVersion = StoreUtilities.Version40;
                handled = true;
            }

            return;
        }

        static void InstanceCommandCompleteCallback(IAsyncResult result)
        {
            TestDatabaseVersionAndRunAsyncResult thisPtr = (TestDatabaseVersionAndRunAsyncResult)result.AsyncState;
            try
            {
                thisPtr.Store.EndTryCommand(result);
                thisPtr.Complete(false, null);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                thisPtr.Complete(false, ex);
            }
        }

        static Version GetVersion(SqlDataReader reader)
        {
            int major, minor, build, revision;
            if (TryGetInt(reader, 0, out major) && TryGetInt(reader, 1, out minor) && TryGetInt(reader, 2, out build) && TryGetInt(reader, 3, out revision))
            {
                return new Version(major, minor, build, revision);
            }
            else
            {
                return null;
            }
        }

        static bool TryGetInt(SqlDataReader reader, int column, out int result)
        {
            result = 0;
            if (reader.IsDBNull(column))
            {
                return false;
            }

            long value = reader.GetInt64(column);
            if (value < 0 || value > (long)int.MaxValue)
            {
                return false;
            }

            result = (int)value;
            return true;
        }

        void TestAndRun()
        {
            if (this.Store.DatabaseVersion >= this.targetVersion)
            {
                this.Store.BeginTryCommandInternal(this.InstancePersistenceContext, this.InstancePersistenceCommand, this.currentTransaction, this.TimeoutHelper.RemainingTime(), instanceCommandCompleteCallback, this);
            }
            else
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SR.DatabaseUpgradeRequiredForCommand(this.Store.DatabaseVersion, this.InstancePersistenceCommand, this.targetVersion)));
            }
        }
    }    
}
