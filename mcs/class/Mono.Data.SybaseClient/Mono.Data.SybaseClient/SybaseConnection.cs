//
// Mono.Data.SybaseClient.SybaseConnection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
using System.Net;
using System.Text;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseConnection : Component, IDbConnection, ICloneable	
	{
		#region Fields

		// The set of SQL connection pools
		static Hashtable SybaseConnectionPools = new Hashtable ();

		// The current connection pool
		SybaseConnectionPool pool;

		// The connection string that identifies this connection
		string connectionString = null;

		// The transaction object for the current transaction
		SybaseTransaction transaction = null;

		// Connection parameters
		TdsConnectionParameters parms = new TdsConnectionParameters ();
		bool connectionReset;
		bool pooling;
		string dataSource;
		int connectionTimeout;
		int minPoolSize;
		int maxPoolSize;
		int packetSize;
		int port = 1533;

		// The current state
		ConnectionState state = ConnectionState.Closed;
		bool dataReaderOpen = false;

		// The TDS object
		ITds tds;

		static readonly object EventStateChange = new object ();
		static readonly object EventSybaseInfoMessage = new object ();

		#endregion // Fields

		#region Constructors

		public SybaseConnection () 
			: this (String.Empty)
		{
		}
	
		public SybaseConnection (string connectionString) 
		{
			SetConnectionString (connectionString);
			this.connectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties
		
		public string ConnectionString	{
			get { return connectionString; }
			set { SetConnectionString (value); }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		public string Database	{
			get { return tds.Database; }
		}

		internal bool DataReaderOpen {
			get { return dataReaderOpen; }
			set { dataReaderOpen = value; }
		}

		public string DataSource {
			get { return dataSource; }
		}

		public int PacketSize {
			get { return packetSize; }
		}

		public string ServerVersion {
			get { return tds.ServerVersion; }
		}

		public ConnectionState State {
			get { return state; }
		}

		internal ITds Tds {
			get { return tds; }
		}

		internal SybaseTransaction Transaction {
			get { return transaction; }
		}

		public string WorkstationId {
			get { return parms.Hostname; }
		}

		#endregion // Properties

		#region Events
                
		public event SybaseInfoMessageEventHandler InfoMessage {
			add { Events.AddHandler (EventSybaseInfoMessage, value); }
			remove { Events.RemoveHandler (EventSybaseInfoMessage, value); }
		}

		public event StateChangeEventHandler StateChange {
			add { Events.AddHandler (EventStateChange, value); }
			remove { Events.RemoveHandler (EventStateChange, value); }
		}
		
		#endregion // Events

		#region Delegates

		#endregion // Delegates

		private void ErrorHandler (object sender, TdsInternalErrorMessageEventArgs e)
		{
			throw new SybaseException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		private void MessageHandler (object sender, TdsInternalInfoMessageEventArgs e)
		{
			OnSybaseInfoMessage (CreateSybaseInfoMessageEvent (e.Errors));
		}

		#region Methods

		public SybaseTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public SybaseTransaction BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso, String.Empty);
		}

		public SybaseTransaction BeginTransaction (string transactionName)
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public SybaseTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			if (transaction != null)
				throw new InvalidOperationException ("SybaseConnection does not support parallel transactions.");

			tds.ExecuteNonQuery (String.Format ("BEGIN TRANSACTION {0}", transactionName));

			transaction = new SybaseTransaction (this, iso);
			return transaction;
		}

		public void ChangeDatabase (string database) 
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid."));

			if (state != ConnectionState.Open)
				throw new InvalidOperationException ("The connection is not open");

			tds.ExecuteNonQuery (String.Format ("use {0}", database));
		}

		private void ChangeState (ConnectionState currentState)
		{
			ConnectionState originalState = state;
			state = currentState;
			OnStateChange (CreateStateChangeEvent (originalState, currentState));
		}

		public void Close () 
		{
			if (transaction != null && transaction.IsOpen)
				transaction.Rollback ();
			if (pooling)
				pool.ReleaseConnection (tds);
			else
				tds.Disconnect ();
			tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			ChangeState (ConnectionState.Closed);
		}

		public SybaseCommand CreateCommand () 
		{
			SybaseCommand command = new SybaseCommand ();
			command.Connection = this;
			return command;
		}

		private StateChangeEventArgs CreateStateChangeEvent (ConnectionState originalState, ConnectionState currentState)
		{
			return new StateChangeEventArgs (originalState, currentState);
		}

		private SybaseInfoMessageEventArgs CreateSybaseInfoMessageEvent (TdsInternalErrorCollection errors)
		{
			return new SybaseInfoMessageEventArgs (errors);
		}

		protected override void Dispose (bool disposing) 
		{
			Close ();
		}

		[MonoTODO]
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		void IDisposable.Dispose ()
		{
			Dispose ();
		}

		public void Open () 
		{
			if (connectionString == null)
				throw new InvalidOperationException ("Connection string has not been initialized.");
			if (!pooling)
				tds = new Tds50 (DataSource, port, PacketSize, ConnectionTimeout);
			else {
				pool = (SybaseConnectionPool) SybaseConnectionPools [connectionString];
				if (pool == null) {
					pool = new SybaseConnectionPool (dataSource, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
					SybaseConnectionPools [connectionString] = pool;
				}
				tds = pool.AllocateConnection ();
			}

			tds.TdsErrorMessage += new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage += new TdsInternalInfoMessageEventHandler (MessageHandler);

			if (!tds.IsConnected) {
				tds.Connect (parms);
				ChangeDatabase (parms.Database);
			} 
			else if (connectionReset) 
				tds.ExecuteNonQuery ("EXEC sp_reset_connection");

			ChangeState (ConnectionState.Open);
		}

                void SetConnectionString (string connectionString)
                {
                        connectionString += ";";
                        NameValueCollection parameters = new NameValueCollection ();

                        if (connectionString == String.Empty)
                                return;

                        bool inQuote = false;
                        bool inDQuote = false;

                        string name = String.Empty;
                        string value = String.Empty;
                        StringBuilder sb = new StringBuilder ();

                        foreach (char c in connectionString)
                        {
                                switch (c) {
                                case '\'':
                                        inQuote = !inQuote;
                                        break;
                                case '"' :
                                        inDQuote = !inDQuote;
                                        break;
                                case ';' :
                                        if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
                                                	value = sb.ToString ();
                                                	parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
                                                name = String.Empty;
                                                value = String.Empty;
                                                sb = new StringBuilder ();
                                        }
                                        else
                                                sb.Append (c);
                                        break;
                                case '=' :
                                        if (!inDQuote && !inQuote) {
                                                name = sb.ToString ();
                                                sb = new StringBuilder ();
                                        }
                                        else
                                                sb.Append (c);
                                        break;
                                default:
                                        sb.Append (c);
                                        break;
                                }
                        }

                        if (this.ConnectionString == null)
                        {
                                SetDefaultConnectionParameters (parameters);
                        }

                        SetProperties (parameters);

                        this.connectionString = connectionString;
                }

                void SetDefaultConnectionParameters (NameValueCollection parameters)
                {
                        if (null == parameters.Get ("APPLICATION NAME"))
                                parameters["APPLICATION NAME"] = ".Net SybaseClient Data Provider";
                        if (null == parameters.Get ("CONNECT TIMEOUT") && null == parameters.Get ("CONNECTION TIMEOUT"))
                                parameters["CONNECT TIMEOUT"] = "15";
                        if (null == parameters.Get ("CONNECTION LIFETIME"))
                                parameters["CONNECTION LIFETIME"] = "0";
                        if (null == parameters.Get ("CONNECTION RESET"))
                                parameters["CONNECTION RESET"] = "true";
                        if (null == parameters.Get ("ENLIST"))
                                parameters["ENLIST"] = "true";
                        if (null == parameters.Get ("INTEGRATED SECURITY") && null == parameters.Get ("TRUSTED_CONNECTION"))
                                parameters["INTEGRATED SECURITY"] = "false";
                        if (null == parameters.Get ("MAX POOL SIZE"))
                                parameters["MAX POOL SIZE"] = "100";
                        if (null == parameters.Get ("MIN POOL SIZE"))
                                parameters["MIN POOL SIZE"] = "0";
                        if (null == parameters.Get ("NETWORK LIBRARY") && null == parameters.Get ("NET"))
                                parameters["NETWORK LIBRARY"] = "dbmssocn";
                        if (null == parameters.Get ("PACKET SIZE"))
                                parameters["PACKET SIZE"] = "512";
                        if (null == parameters.Get ("PERSIST SECURITY INFO"))
                                parameters["PERSIST SECURITY INFO"] = "false";
                        if (null == parameters.Get ("POOLING"))
                                parameters["POOLING"] = "true";
                        if (null == parameters.Get ("WORKSTATION ID"))
                                parameters["WORKSTATION ID"] = Dns.GetHostByName ("localhost").HostName;
                }

                private void SetProperties (NameValueCollection parameters)
                {
                        string value;
                        foreach (string name in parameters) {
                                value = parameters[name];

                                switch (name) {
                                case "APPLICATION NAME" :
                                        parms.ApplicationName = value;
                                        break;
                                case "ATTACHDBFILENAME" :
                                case "EXTENDED PROPERTIES" :
                                case "INITIAL FILE NAME" :
                                        break;
                                case "CONNECT TIMEOUT" :
                                case "CONNECTION TIMEOUT" :
                                        connectionTimeout = Int32.Parse (value);
                                        break;
                                case "CONNECTION LIFETIME" :
                                        break;
                                case "CONNECTION RESET" :
                                        connectionReset = !(value.ToUpper ().Equals ("FALSE") || value.ToUpper ().Equals ("NO"));
                                        break;
                                case "CURRENT LANGUAGE" :
                                        parms.Language = value;
                                        break;
                                case "DATA SOURCE" :
                                case "SERVER" :
                                case "ADDRESS" :
                                case "ADDR" :
                                case "NETWORK ADDRESS" :
                                        dataSource = value;
                                        break;
                                case "ENLIST" :
                                        break;
                                case "INITIAL CATALOG" :
                                case "DATABASE" :
                                        parms.Database = value;
                                        break;
                                case "INTEGRATED SECURITY" :
                                case "TRUSTED_CONNECTION" :
                                        break;
                                case "MAX POOL SIZE" :
                                        maxPoolSize = Int32.Parse (value);
                                        break;
                                case "MIN POOL SIZE" :
                                        minPoolSize = Int32.Parse (value);
                                        break;
                                case "NET" :
                                case "NETWORK LIBRARY" :
                                        if (!value.ToUpper ().Equals ("DBMSSOCN"))
                                                throw new ArgumentException ("Unsupported network library.");
                                        break;
                                case "PACKET SIZE" :
                                        packetSize = Int32.Parse (value);
                                        break;
                                case "PASSWORD" :
                                case "PWD" :
                                        parms.Password = value;
                                        break;
                                case "PERSIST SECURITY INFO" :
                                        break;
                                case "POOLING" :
                                        pooling = !(value.ToUpper ().Equals ("FALSE") || value.ToUpper ().Equals ("NO"));
                                        break;
                                case "USER ID" :
                                        parms.User = value;
                                        break;
                                case "WORKSTATION ID" :
                                        parms.Hostname = value;
                                        break;
                                }
			}
		}


		static bool IsValidDatabaseName (string database)
		{
			if (database.Length > 32 || database.Length < 1)
				return false;

			if (database[0] == '"' && database[database.Length] == '"')
				database = database.Substring (1, database.Length - 2);
			else if (Char.IsDigit (database[0]))
				return false;

			if (database[0] == '_')
				return false;

			foreach (char c in database.Substring (1, database.Length - 1))
				if (!Char.IsLetterOrDigit (c) && c != '_')
					return false;
			return true;
		}

		private void OnSybaseInfoMessage (SybaseInfoMessageEventArgs value)
		{
			SybaseInfoMessageEventHandler handler = (SybaseInfoMessageEventHandler) Events [EventSybaseInfoMessage];
			if (handler != null)
				handler (this, value);
		}

		private void OnStateChange (StateChangeEventArgs value)
		{
			StateChangeEventHandler handler = (StateChangeEventHandler) Events [EventStateChange];
			if (handler != null)
				handler (this, value);
		}

		#endregion // Methods
	}
}
