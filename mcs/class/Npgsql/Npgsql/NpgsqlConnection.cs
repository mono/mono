// created on 10/5/2002 at 23:01
// Npgsql.NpgsqlConnection.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Collections;
using System.Resources;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls;

using NpgsqlTypes;

#if WITHDESIGN
using Npgsql.Design;
#endif

namespace Npgsql
{
    /// <summary>
    /// Represents the method that handles the <see cref="Npgsql.NpgsqlConnection.Notification">Notice</see> events.
    /// </summary>
    /// <param name="e">A <see cref="Npgsql.NpgsqlNoticeEventArgs">NpgsqlNoticeEventArgs</see> that contains the event data.</param>
    public delegate void NoticeEventHandler(Object sender, NpgsqlNoticeEventArgs e);

    /// <summary>
    /// Represents the method that handles the <see cref="Npgsql.NpgsqlConnection.Notification">Notification</see> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="Npgsql.NpgsqlNotificationEventArgs">NpgsqlNotificationEventArgs</see> that contains the event data.</param>
    public delegate void NotificationEventHandler(Object sender, NpgsqlNotificationEventArgs e);

    /// <summary>
    /// This class represents a connection to a
    /// PostgreSQL server.
    /// </summary>
    #if WITHDESIGN
    [System.Drawing.ToolboxBitmapAttribute(typeof(NpgsqlConnection))]
    #endif
    
    public sealed class NpgsqlConnection : Component, IDbConnection, ICloneable
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlConnection";
        private static ResourceManager resman = new System.Resources.ResourceManager(typeof(NpgsqlConnection));

        /// <summary>
        /// Occurs on NoticeResponses from the PostgreSQL backend.
        /// </summary>
        public event NoticeEventHandler			           Notice;
        internal NoticeEventHandler                    NoticeDelegate;

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        public event NotificationEventHandler          Notification;
        internal NotificationEventHandler              NotificationDelegate;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateSelectionCallback delegate.
        /// </summary>
        public event CertificateSelectionCallback      CertificateSelectionCallback;
        internal CertificateSelectionCallback          CertificateSelectionCallbackDelegate;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateValidationCallback delegate.
        /// </summary>
        public event CertificateValidationCallback     CertificateValidationCallback;
        internal CertificateValidationCallback         CertificateValidationCallbackDelegate;

        /// <summary>
        /// Mono.Security.Protocol.Tls.PrivateKeySelectionCallback delegate.
        /// </summary>
        public event PrivateKeySelectionCallback       PrivateKeySelectionCallback;
        internal PrivateKeySelectionCallback           PrivateKeySelectionCallbackDelegate;

        // Set this when disposed is called.
        private bool                                   disposed = false;

        // Connection string values.
        private NpgsqlConnectionString                 connection_string;

        // Connector being used for the active connection.
        private NpgsqlConnector                        connector = null;


        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> class.
        /// </summary>
        public NpgsqlConnection() : this(String.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> class
        /// and sets the <see cref="Npgsql.NpgsqlConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        /// <param name="ConnectionString">The connection used to open the PostgreSQL database.</param>
        public NpgsqlConnection(String ConnectionString)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, "NpgsqlConnection()");
            
            connection_string = NpgsqlConnectionString.ParseConnectionString(ConnectionString);
            LogConnectionString();

            NoticeDelegate = new NoticeEventHandler(OnNotice);
            NotificationDelegate = new NotificationEventHandler(OnNotification);

            CertificateValidationCallbackDelegate = new CertificateValidationCallback(DefaultCertificateValidationCallback);
            CertificateSelectionCallbackDelegate = new CertificateSelectionCallback(DefaultCertificateSelectionCallback);
            PrivateKeySelectionCallbackDelegate = new PrivateKeySelectionCallback(DefaultPrivateKeySelectionCallback);
        }

        /// <summary>
        /// Gets or sets the string used to connect to a PostgreSQL database.
        /// Valid values are:
        /// <ul>
        /// <li>
        /// Server:             Address/Name of Postgresql Server;
        /// </li>
        /// <li>
        /// Port:               Port to connect to;
        /// </li>
        /// <li>
        /// Protocol:           Protocol version to use, instead of automatic; Integer 2 or 3;
        /// </li>
        /// <li>
        /// Database:           Database name. Defaults to user name if not specified;
        /// </li>
        /// <li>
        
        /// User Id:            User name;
        /// </li>
        /// <li>
        
        /// Password:           Password for clear text authentication;
        /// </li>
        /// <li>
        
        /// SSL:                True or False. Controls whether to attempt a secure connection. Default = False;
        /// </li>
        /// <li>
        
        /// Pooling:            True or False. Controls whether connection pooling is used. Default = True;
        /// </li>
        /// <li>
        
        /// MinPoolSize:        Min size of connection pool;
        /// </li>
        /// <li>
        
        /// MaxPoolSize:        Max size of connection pool;
        /// </li>
        /// <li>
        
        /// Encoding:           Encoding to be used; Can be ASCII or UNICODE. Default is ASCII. Use UNICODE if you are having problems with accents.
        /// </li>
        /// <li>
        
        /// Timeout:            Time to wait for connection open in seconds. Default is 15.
        /// </li>
        /// <li>
        
        /// CommandTimeout:     Time to wait for command to finish execution before throw an exception. In seconds. Default is 20.
        /// </li>
        /// <li>
        
        /// Sslmode:            Mode for ssl connection control. Can be Prefer, Require, Allow or Disable. Default is Disable. Check user manual for explanation of values.
        /// </li>
        
        /// <li>
        
        /// ConnectionLifeTime: Time to wait before closing unused connections in the pool in seconds. Default is 15.
        /// </li>
        /// <li>
        
        /// SyncNotification:   Specifies if Npgsql should use synchronous notifications.
        /// </li>
        /// </ul>
        
        
        /// </summary>
        /// <value>The connection string that includes the server name,
        /// the database name, and other parameters needed to establish
        /// the initial connection. The default value is an empty string.
        /// </value>
        
        #if WITHDESIGN
        [RefreshProperties(RefreshProperties.All), DefaultValue(""), RecommendedAsConfigurable(true)]
        [NpgsqlSysDescription("Description_ConnectionString", typeof(NpgsqlConnection)), Category("Data")]
        [Editor(typeof(ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        #endif
        
        public String ConnectionString {
            get
            {
                return connection_string.ToString();
            }
            set
            {
                // Connection string is used as the key to the connector.  Because of this,
                // we cannot change it while we own a connector.
                CheckConnectionClosed();
                NpgsqlEventLog.LogPropertySet(LogLevel.Debug, CLASSNAME, "ConnectionString", value);
                connection_string = NpgsqlConnectionString.ParseConnectionString(value);
                LogConnectionString();
            }
        }

        /// <summary>
        /// Backend server host name.
        /// </summary>
        [Browsable(true)]
        public String Host {
            get
            {
                return connection_string.ToString(ConnectionStringKeys.Host);
            }
        }

        /// <summary>
        /// Backend server port.
        /// </summary>
        [Browsable(true)]
        public Int32 Port {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.Port, ConnectionStringDefaults.Port);
            }
        }

        /// <summary>
        /// If true, the connection will attempt to use SSL.
        /// </summary>
        [Browsable(true)]
        public Boolean SSL {
            get
            {
                return connection_string.ToBool(ConnectionStringKeys.SSL);
            }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection
        /// before terminating the attempt and generating an error.
        /// </summary>
        /// <value>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</value>
        
        #if WITHDESIGN
        [NpgsqlSysDescription("Description_ConnectionTimeout", typeof(NpgsqlConnection))]
        #endif
        
        public Int32 ConnectionTimeout {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.Timeout, ConnectionStringDefaults.Timeout);
            }
        }
        
        /// <summary>
        /// Gets the time to wait before closing unused connections in the pool if the count
        /// of all connections exeeds MinPoolSize.
        /// </summary>
        /// <remarks>
        /// If connection pool contains unused connections for ConnectionLifeTime seconds,
        /// the half of them will be closed. If there will be unused connections in a second
        /// later then again the half of them will be closed and so on.
        /// This strategy provide smooth change of connection count in the pool.
        /// </remarks>
        /// <value>The time (in seconds) to wait. The default value is 15 seconds.</value>
        public Int32 ConnectionLifeTime {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.ConnectionLifeTime, ConnectionStringDefaults.ConnectionLifeTime);
            }
        }

        ///<summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <value>The name of the current database or the name of the database to be
        /// used after a connection is opened. The default value is the empty string.</value>
        #if WITHDESIGN
        [NpgsqlSysDescription("Description_Database", typeof(NpgsqlConnection))]
        #endif
        
        public String Database {
            get
            {
                return connection_string.ToString(ConnectionStringKeys.Database);
            }
        }
        
        /// <summary>
        /// Gets flag indicating if we are using Synchronous notification or not.
        /// The default value is false.
        /// </summary>
        public Boolean SyncNotification
        {
            get
            {
                return connection_string.ToBool(ConnectionStringKeys.SyncNotification, ConnectionStringDefaults.SyncNotification);
            }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <value>A bitwise combination of the <see cref="System.Data.ConnectionState">ConnectionState</see> values. The default is <b>Closed</b>.</value>
        [Browsable(false)]
        public ConnectionState State {
            get
            {
                CheckNotDisposed();

                if (connector != null)
                {
                    return connector.State;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
        }

        /// <summary>
        /// Version of the PostgreSQL backend.
        /// This can only be called when there is an active connection.
        /// </summary>
        [Browsable(false)]
        public ServerVersion ServerVersion {
            get
            {
                CheckConnectionOpen();
                return connector.ServerVersion;
            }
        }

        /// <summary>
        /// Protocol version in use.
        /// This can only be called when there is an active connection.
        /// </summary>
        [Browsable(false)]
        public ProtocolVersion BackendProtocolVersion {
            get
            {
                CheckConnectionOpen();
                return connector.BackendProtocolVersion;
            }
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>An <see cref="System.Data.IDbTransaction">IDbTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        IDbTransaction IDbConnection.BeginTransaction()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.BeginTransaction");

            return BeginTransaction();
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="level">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>An <see cref="System.Data.IDbTransaction">IDbTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.BeginTransaction", level);

            return BeginTransaction(level);
        }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently there's no support for nested transactions.
        /// </remarks>
        public NpgsqlTransaction BeginTransaction()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "BeginTransaction");
            return this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Begins a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="level">The <see cref="System.Data.IsolationLevel">isolation level</see> under which the transaction should run.</param>
        /// <returns>A <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see>
        /// object representing the new transaction.</returns>
        /// <remarks>
        /// Currently the IsolationLevel ReadCommitted and Serializable are supported by the PostgreSQL backend.
        /// There's no support for nested transactions.
        /// </remarks>
        public NpgsqlTransaction BeginTransaction(IsolationLevel level)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "BeginTransaction", level);

            CheckConnectionOpen();

            if (connector.Transaction != null)
            {
                throw new InvalidOperationException(resman.GetString("Exception_NoNestedTransactions"));
            }

            return new NpgsqlTransaction(this, level);
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the
        /// <see cref="Npgsql.NpgsqlConnection.ConnectionString">ConnectionString</see>.
        /// </summary>
        public void Open()
        {
            CheckConnectionClosed();

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Open");

            // Check if there is any missing argument.
            if (! connection_string.Contains(ConnectionStringKeys.Host))
                throw new ArgumentException(resman.GetString("Exception_MissingConnStrArg"), ConnectionStringKeys.Host);
            if (! connection_string.Contains(ConnectionStringKeys.UserName))
                throw new ArgumentException(resman.GetString("Exception_MissingConnStrArg"), ConnectionStringKeys.UserName);

            // Get a Connector.  The connector returned is guaranteed to be connected and ready to go.
            connector = NpgsqlConnectorPool.ConnectorPoolMgr.RequestConnector (this);
            
            connector.Notice += NoticeDelegate;
            connector.Notification += NotificationDelegate;
            
            if (SyncNotification)
                connector.AddNotificationThread();
            
        }

        /// <summary>
        /// This method changes the current database by disconnecting from the actual
        /// database and connecting to the specified.
        /// </summary>
        /// <param name="dbName">The name of the database to use in place of the current database.</param>
        public void ChangeDatabase(String dbName)
        {
            CheckNotDisposed();

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ChangeDatabase", dbName);

            if (dbName == null)
                throw new ArgumentNullException("dbName");

            if (dbName == String.Empty)
                throw new ArgumentOutOfRangeException(String.Format(resman.GetString("Exception_InvalidDbName"), dbName), "dbName");

            String oldDatabaseName = Database;

            Close();

            connection_string[ConnectionStringKeys.Database] = dbName;

            Open();
        }

        /// <summary>
        /// Releases the connection to the database.  If the connection is pooled, it will be
        ///	made available for re-use.  If it is non-pooled, the actual connection will be shutdown.
        /// </summary>
        public void Close()
        {
            if (!disposed)
            {
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Close");

                if (connector != null)
                {

                    connector.Notification -= NotificationDelegate;
                    connector.Notice -= NoticeDelegate;
                    
                    if (SyncNotification)
                        connector.RemoveNotificationThread();

                    NpgsqlConnectorPool.ConnectorPoolMgr.ReleaseConnector(this, connector);
                    
                    
                    
                    connector = null;
                }
            }
        }

        /// <summary>
        /// Creates and returns a <see cref="System.Data.IDbCommand">IDbCommand</see>
        /// object associated with the <see cref="System.Data.IDbConnection">IDbConnection</see>.
        /// </summary>
        /// <returns>A <see cref="System.Data.IDbCommand">IDbCommand</see> object.</returns>
        IDbCommand IDbConnection.CreateCommand()
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "IDbConnection.CreateCommand");
            return (NpgsqlCommand) CreateCommand();
        }

        /// <summary>
        /// Creates and returns a <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see>
        /// object associated with the <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <returns>A <see cref="Npgsql.NpgsqlCommand">NpgsqlCommand</see> object.</returns>
        public NpgsqlCommand CreateCommand()
        {
            CheckNotDisposed();

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CreateCommand");
            return new NpgsqlCommand("", this);
        }

        /// <summary>
        /// Releases all resources used by the
        /// <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see>.
        /// </summary>
        /// <param name="disposing"><b>true</b> when called from Dispose();
        /// <b>false</b> when being called from the finalizer.</param>
        protected override void Dispose(bool disposing)
        {

            if (!disposed)
            {
                if (disposing)
                {
                    NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Dispose");
                    Close();
                }
                else
                {
                    if (State != ConnectionState.Closed)
                    {
                        NpgsqlEventLog.LogMsg(resman, "Log_ConnectionLeaking", LogLevel.Debug);
                        NpgsqlConnectorPool.ConnectorPoolMgr.FixPoolCountBecauseOfConnectionDisposeFalse(this);
                    }
                }

                base.Dispose (disposing);
                disposed = true;

            }
        }

        /// <summary>
        /// Create a new connection based on this one.
        /// </summary>
        /// <returns>A new NpgsqlConnection object.</returns>
        Object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Create a new connection based on this one.
        /// </summary>
        /// <returns>A new NpgsqlConnection object.</returns>
        public NpgsqlConnection Clone()
        {
            CheckNotDisposed();

            NpgsqlConnection C = new NpgsqlConnection(ConnectionString);

            C.Notice += this.Notice;

            if (connector != null)
            {
                C.Open();
            }

            return C;
        }

        //
        // Internal methods and properties
        //
        internal void OnNotice(object O, NpgsqlNoticeEventArgs E)
        {
            if (Notice != null)
            {
                Notice(this, E);
            }
        }

        internal void OnNotification(object O, NpgsqlNotificationEventArgs E)
        {
            if (Notification != null)
            {
                Notification(this, E);
            }
        }

        /// <summary>
        /// The connector object connected to the backend.
        /// </summary>
        internal NpgsqlConnector Connector
        {
            get
            {
                return connector;
            }
        }

        /// <summary>
        /// Gets the NpgsqlConnectionString containing the parsed connection string values.
        /// </summary>
        internal NpgsqlConnectionString ConnectionStringValues {
            get
            {
                return connection_string;
            }
        }

        /// <summary>
        /// User name.
        /// </summary>
        internal String UserName {
            get
            {
                return connection_string.ToString(ConnectionStringKeys.UserName);
            }
        }

        /// <summary>
        /// Password.
        /// </summary>
        internal String Password {
            get
            {
                return connection_string.ToString(ConnectionStringKeys.Password);
            }
        }

        /// <summary>
        /// Determine if connection pooling will be used for this connection.
        /// </summary>
        internal Boolean Pooling {
            get
            {
                return (
                           connection_string.ToBool(ConnectionStringKeys.Pooling, ConnectionStringDefaults.Pooling) &&
                           connection_string.ToInt32(ConnectionStringKeys.MaxPoolSize, ConnectionStringDefaults.MaxPoolSize) > 0
                       );
            }
        }

        internal Int32 MinPoolSize {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.MinPoolSize, 0, MaxPoolSize, ConnectionStringDefaults.MinPoolSize);
            }
        }

        internal Int32 MaxPoolSize {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.MaxPoolSize, 0, 1024, ConnectionStringDefaults.MaxPoolSize);
            }
        }

        internal Int32 Timeout {
            get
            {
                return connection_string.ToInt32(ConnectionStringKeys.Timeout, 0, 1024, ConnectionStringDefaults.Timeout);
            }
        }



        //
        // Event handlers
        //

        /// <summary>
        /// Default SSL CertificateSelectionCallback implementation.
        /// </summary>
        internal X509Certificate DefaultCertificateSelectionCallback(
            X509CertificateCollection      clientCertificates,
            X509Certificate                serverCertificate,
            string                         targetHost,
            X509CertificateCollection      serverRequestedCertificates)
        {
            if (CertificateSelectionCallback != null)
            {
                return CertificateSelectionCallback(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Default SSL CertificateValidationCallback implementation.
        /// </summary>
        internal bool DefaultCertificateValidationCallback(
            X509Certificate       certificate,
            int[]                 certificateErrors)
        {
            if (CertificateValidationCallback != null)
            {
                return CertificateValidationCallback(certificate, certificateErrors);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Default SSL PrivateKeySelectionCallback implementation.
        /// </summary>
        internal AsymmetricAlgorithm DefaultPrivateKeySelectionCallback(
            X509Certificate                certificate,
            string                         targetHost)
        {
            if (PrivateKeySelectionCallback != null)
            {
                return PrivateKeySelectionCallback(certificate, targetHost);
            }
            else
            {
                return null;
            }
        }



        //
        // Private methods and properties
        //


        /// <summary>
        /// Write each key/value pair in the connection string to the log.
        /// </summary>
        private void LogConnectionString()
        {
            foreach (DictionaryEntry DE in connection_string)
            {
                NpgsqlEventLog.LogMsg(resman, "Log_ConnectionStringValues", LogLevel.Debug, DE.Key, DE.Value);
            }
        }

        private void CheckConnectionOpen()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(CLASSNAME);
            }

            if (connector == null)
            {
                throw new InvalidOperationException(resman.GetString("Exception_ConnNotOpen"));
            }
        }

        private void CheckConnectionClosed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(CLASSNAME);
            }

            if (connector != null)
            {
                throw new InvalidOperationException(resman.GetString("Exception_ConnOpen"));
            }
        }

        private void CheckNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(CLASSNAME);
            }
        }
        
        
        /// <summary>
        /// Returns the supported collections
        /// <summary>
        public DataTable GetSchema()
        {
            return NpgsqlSchema.GetMetaDataCollections();
        }

        /// <summary>
        /// Returns the schema collection specified by the collection name.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <returns>The collection specified.</returns>
        public DataTable GetSchema(string collectionName)
        {
            return GetSchema(collectionName, null);
        }
        
        /// <summary>
        /// Returns the schema collection specified by the collection name filtered by the restrictions.
        /// </summary>
        /// <param name="collectionName">The collection name.</param>
        /// <param name="restrictions">
        /// The restriction values to filter the results.  A description of the restrictions is contained
        /// in the Restrictions collection.
        /// </param>
        /// <returns>The collection specified.</returns>
        public DataTable GetSchema(string collectionName, string[] restrictions)
        {
            switch(collectionName)
            {
                case "MetaDataCollections":
                    return NpgsqlSchema.GetMetaDataCollections();
                case "Restrictions":
                    return NpgsqlSchema.GetRestrictions();
                case "DataSourceInformation":
                case "DataTypes":
                case "ReservedWords":
                    throw new NotSupportedException();
                    // custom collections for npgsql
                case "Databases":
                    return new NpgsqlSchema(new NpgsqlConnection(ConnectionString)).GetDatabases(restrictions);
                case "Tables":
                    return new NpgsqlSchema(new NpgsqlConnection(ConnectionString)).GetTables(restrictions);
                case "Columns":
                    return new NpgsqlSchema(new NpgsqlConnection(ConnectionString)).GetColumns(restrictions);
                case "Views":
                    return new NpgsqlSchema(new NpgsqlConnection(ConnectionString)).GetViews(restrictions);
                case "Users":
                    return new NpgsqlSchema(new NpgsqlConnection(ConnectionString)).GetUsers(restrictions);
                default:
                    throw new NotSupportedException();
            }
        }

	public void ClearPool()
	{
	    NpgsqlConnectorPool.ConnectorPoolMgr.ClearPool(this);
        }

        public void ClearAllPools()
        {
            NpgsqlConnectorPool.ConnectorPoolMgr.ClearAllPools();
        }

    }



}
