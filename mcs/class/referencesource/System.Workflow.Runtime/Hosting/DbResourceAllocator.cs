//------------------------------------------------------------------------------
// <copyright file="DbResourceAllocator.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region Using directives

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Threading;

#endregion

namespace System.Workflow.Runtime.Hosting
{
    /// <summary>
    /// Local database providers we support
    /// </summary>
    internal enum Provider
    {
        SqlClient = 0,
        OleDB = 1
    }

    /// <summary>
    /// Internal Database access abstraction to 
    /// - abstract the derived Out-of-box SharedConnectionInfo from all DB hosting services
    /// - provide uniform connection string management
    /// - and support different database providers 
    /// </summary>
    internal sealed class DbResourceAllocator
    {
        const string EnlistFalseToken = ";Enlist=false";
        internal const string ConnectionStringToken = "ConnectionString";

        string connString;
        Provider localProvider;

        /// <summary>
        /// Initialize the object by getting the connection string from the parameter or 
        /// out of the configuration settings
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        internal DbResourceAllocator(
            WorkflowRuntime runtime,
            NameValueCollection parameters,
            string connectionString)
        {
            // If connection string not specified in input, search the config sections
            if (String.IsNullOrEmpty(connectionString))
            {
                if (parameters != null)
                {
                    // First search in this service's parameters
                    foreach (string key in parameters.AllKeys)
                    {
                        if (string.Compare(ConnectionStringToken, key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            connectionString = parameters[ConnectionStringToken];
                            break;
                        }
                    }
                }
                if (String.IsNullOrEmpty(connectionString) && (runtime != null))
                {
                    NameValueConfigurationCollection commonConfigurationParameters = runtime.CommonParameters;
                    if (commonConfigurationParameters != null)
                    {
                        // Then scan for connection string in the common configuration parameters section
                        foreach (string key in commonConfigurationParameters.AllKeys)
                        {
                            if (string.Compare(ConnectionStringToken, key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                connectionString = commonConfigurationParameters[ConnectionStringToken].Value;
                                break;
                            }
                        }
                    }
                }

                // If no connectionString parsed out of the params, inner layer throws 
                //   System.ArgumentNullException: Connection string cannot be null or empty
                //   Parameter name: connectionString
                // But this API caller does not have connectionString param.
                // So throw ArgumentException with the original message.
                if (String.IsNullOrEmpty(connectionString))
                    throw new ArgumentNullException(ConnectionStringToken, ExecutionStringManager.MissingConnectionString);
            }

            Init(connectionString);
        }

        #region Accessors

        internal string ConnectionString
        {
            get { return this.connString; }
        }

        #endregion Accessors


        #region Internal Methods
        /// <summary>
        /// Disallow the hosting service to have different connection string if using SharedConnectionWorkflowTransactionService
        /// Should be called after all hosting services are added to the WorkflowRuntime
        /// </summary>
        /// <param name="transactionService"></param>
        internal void DetectSharedConnectionConflict(WorkflowCommitWorkBatchService transactionService)
        {
            SharedConnectionWorkflowCommitWorkBatchService sharedConnectionTransactionService = transactionService as SharedConnectionWorkflowCommitWorkBatchService;
            if (sharedConnectionTransactionService != null)
            {
                if (String.Compare(sharedConnectionTransactionService.ConnectionString, this.connString, StringComparison.Ordinal) != 0)
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                        ExecutionStringManager.SharedConnectionStringSpecificationConflict, this.connString, sharedConnectionTransactionService.ConnectionString));
            }

        }

        #region Get a connection

        internal DbConnection OpenNewConnection()
        {
            // Always disallow AutoEnlist since we enlist explicitly when necessary
            return OpenNewConnection(true);
        }

        internal DbConnection OpenNewConnectionNoEnlist()
        {
            return OpenNewConnection(true);
        }

        internal DbConnection OpenNewConnection(bool disallowEnlist)
        {
            DbConnection connection = null;
            string connectionStr = this.connString;

            if (disallowEnlist)
                connectionStr += DbResourceAllocator.EnlistFalseToken;

            if (this.localProvider == Provider.SqlClient)
                connection = new SqlConnection(connectionStr);
            else
                connection = new OleDbConnection(connectionStr);

            connection.Open();

            return connection;
        }

        /// <summary>
        /// Gets a connection enlisted to the transaction.  
        /// If the transaction already has a connection attached to it, we return that,
        /// otherwise we create a new connection and enlist to the transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="isNewConnection">output if we created a connection</param>
        /// <returns></returns>
        internal DbConnection GetEnlistedConnection(WorkflowCommitWorkBatchService txSvc, Transaction transaction, out bool isNewConnection)
        {
            DbConnection connection;
            SharedConnectionInfo connectionInfo = GetConnectionInfo(txSvc, transaction);

            if (connectionInfo != null)
            {
                connection = connectionInfo.DBConnection;
                Debug.Assert((connection != null), "null connection");
                Debug.Assert((connection.State == System.Data.ConnectionState.Open),
                    "Invalid connection state " + connection.State + " for connection " + connection);

                isNewConnection = false;
            }
            else
            {
                connection = this.OpenNewConnection();
                connection.EnlistTransaction(transaction);

                isNewConnection = true;
            }

            return connection;
        }

        #endregion Get a connection

        #region Get Local Transaction

        internal static DbTransaction GetLocalTransaction(WorkflowCommitWorkBatchService txSvc, Transaction transaction)
        {
            DbTransaction localTransaction = null;
            SharedConnectionInfo connectionInfo = GetConnectionInfo(txSvc, transaction);

            if (connectionInfo != null)
                localTransaction = connectionInfo.DBTransaction;

            return localTransaction;
        }

        #endregion Get Local Transaction

        #region Get a command object for querying

        internal DbCommand NewCommand()
        {
            DbConnection dbConnection = OpenNewConnection();
            return DbResourceAllocator.NewCommand(dbConnection);
        }

        internal static DbCommand NewCommand(DbConnection dbConnection)
        {
            return NewCommand(null, dbConnection, null);
        }
        internal static DbCommand NewCommand(string commandText, DbConnection dbConnection, DbTransaction transaction)
        {
            DbCommand command = dbConnection.CreateCommand();
            command.CommandText = commandText;
            command.Transaction = transaction;

            return command;
        }

        #endregion Get a command object for querying

        #region build a command parameter object for a stored procedure

        internal DbParameter NewDbParameter()
        {
            return NewDbParameter(null, null);
        }

        internal DbParameter NewDbParameter(string parameterName, DbType type)
        {
            if (this.localProvider == Provider.SqlClient)
            {
                if (type == DbType.Int64)
                    return new SqlParameter(parameterName, SqlDbType.BigInt);
                else
                    return new SqlParameter(parameterName, type);
            }
            else
            {

                if (type == DbType.Int64)
                    return new OleDbParameter(parameterName, OleDbType.BigInt);
                else
                    return new OleDbParameter(parameterName, type);
            }
        }

        internal DbParameter NewDbParameter(string parameterName, DbType type, ParameterDirection direction)
        {
            DbParameter parameter = NewDbParameter(parameterName, type);
            parameter.Direction = direction;

            return parameter;
        }

        internal DbParameter NewDbParameter(string parameterName, object value)
        {
            if (this.localProvider == Provider.SqlClient)
                return new SqlParameter(parameterName, value);
            else
                return new OleDbParameter(parameterName, value);
        }

        #endregion build a command parameter object for a stored procedure

        #endregion Public Methods


        #region Private Helpers

        private void Init(string connectionStr)
        {
            SetConnectionString(connectionStr);

            try
            {
                // Open a connection to see if it's a valid connection string
                using (DbConnection connection = this.OpenNewConnection(false))
                {
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidDbConnection, "connectionString", e);
            }

            // OLEDB connection pooling causes this exception in ExecuteInsertWorkflowInstance
            // "Cannot start more transactions on this session."
            // Disable pooling to avoid dirty connections.
            if (this.localProvider == Provider.OleDB)
                this.connString = String.Concat(this.connString, ";OLE DB Services=-4");
        }

        private void SetConnectionString(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(connectionString.Trim()))
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);

            DbConnectionStringBuilder dcsb = new DbConnectionStringBuilder();
            dcsb.ConnectionString = connectionString;

            // Don't allow the client to specify an auto-enlist value since we decide whether to participate in a transaction
            // (enlist for writing and not for reading).
            if (dcsb.ContainsKey("enlist"))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidEnlist);
            }

            this.connString = connectionString;
            //
            // We only support sqlclient, sql is the only data store our OOB services talk to.
            localProvider = Provider.SqlClient;
        }
        /*
        private void SetLocalProvider(string connectionString)
        {
            // Assume caller already validated the connection string
            MatchCollection providers = Regex.Matches(connectionString, @"(^|;)\s*provider\s*=[^;$]*(;|$)", RegexOptions.IgnoreCase);

            // Cannot use DbConnectionStringBuilder because it selects the last provider, not the first one, by itself.
            // A legal Sql connection string allows for multiple provider specification and 
            // selects the first provider
            if (providers.Count > 0)
            {
                // Check if the first one matches "sqloledb" or "sqloledb.<digit>"
                if (Regex.IsMatch(providers[0].Value, @"provider\s*=\s*sqloledb(\.\d+)?\s*(;|$)", RegexOptions.IgnoreCase))
                {
                    this.localProvider = Provider.OleDB;
                }
                else
                {
                    // We don't support other providers
                    throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,ExecutionStringManager.UnsupportedSqlProvider, providers[0].Value));
                }
            }
            else
            {
                // SqlClient provider requires no provider keyword specified in connection string
                this.localProvider = Provider.SqlClient;
            }
        }
        */

        private static SharedConnectionInfo GetConnectionInfo(WorkflowCommitWorkBatchService txSvc, Transaction transaction)
        {
            SharedConnectionInfo connectionInfo = null;

            SharedConnectionWorkflowCommitWorkBatchService scTxSvc = txSvc as SharedConnectionWorkflowCommitWorkBatchService;
            if (scTxSvc != null)
            {
                connectionInfo = scTxSvc.GetConnectionInfo(transaction);

                // The transaction service can't find entry if the transaction has been completed.
                // be sure to propate the error so durable services can cast to appropriate exception
                if (connectionInfo == null)
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidTransaction));
            }
            return connectionInfo;
        }

        #endregion Private Helpers
    }

}
