//
// Mono.Data.TdsClient.TdsConnection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Text;

namespace Mono.Data.TdsClient {
        public class TdsConnection : Component, ICloneable, IDbConnection
	{
		#region Fields

		public static readonly string LibraryName = "Mono.Data.TdsClient";

		string dataSource;    // the database server name
		int port;             // which port to use
		int packetSize = 512; // what size are the packets we send/receive?
		bool connectionReset;
		bool pooling;
		int minPoolSize;
		int maxPoolSize;

		string connectionString = null;

		int connectionTimeout = 15;
		IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

		ConnectionState state = ConnectionState.Closed;
		TdsConnectionParameters parms = new TdsConnectionParameters ();
		TdsTransaction transaction = null;

		// This is the collection of connection pools available
		static Hashtable pools = new Hashtable ();
		TdsConnectionPool pool;

		// Our TDS object, the real workhorse
		ITds tds = null;

		TdsDataReader dataReader = null;

		static readonly object EventTdsInfoMessage = new object ();
		static readonly object EventStateChange = new object ();

		#endregion // Fields

		#region Constructors

		public TdsConnection ()
			: this (String.Empty, 1433)
		{
		}

		public TdsConnection (string connectionString)
			: this (connectionString, 1433)
		{
		}

		public TdsConnection (string connectionString, int port)
		{
			this.port = port;
			parms.User = null;
			parms.Password = null;
			parms.LibraryName = LibraryName;
			SetConnectionString (connectionString);
		}
			
		#endregion // Constructors

		#region Properties

		public string ConnectionString {
			get { return connectionString; }
			set { SetConnectionString (value); }
		}

		public int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		public string Database {
			get { return tds.Database; }
		}

		internal TdsDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
		}

		public string DataSource {
			get { return DataSource; }
		}

		public int PacketSize {
			get { return PacketSize; }
		}

		public string ServerVersion {
			get {
				if (state == ConnectionState.Closed)
					throw new InvalidOperationException ();
				return tds.ServerVersion; 
			}
		}

		public ConnectionState State {
			get { return state; }
		}
		
		internal ITds Tds {
			get { return tds; }
		}

		internal TdsTransaction Transaction {	
			get { return transaction; }
		}
	
		public string WorkstationId {
			get { return parms.Hostname; }
		}

		#endregion // Properties
		
		#region Events

		public event TdsInfoMessageEventHandler InfoMessage {
			add { Events.AddHandler (EventTdsInfoMessage, value); }
			remove { Events.RemoveHandler (EventTdsInfoMessage, value); }
		}

		public event StateChangeEventHandler StateChange {
			add { Events.AddHandler (EventStateChange, value); }
			remove { Events.RemoveHandler (EventStateChange, value); }
		}

		#endregion // Events

		#region Delegates

		private void ErrorHandler (object sender, TdsInternalErrorMessageEventArgs e)
		{
			throw new TdsException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono TdsClient Data Provider", e.State);
		} 

		private void MessageHandler (object sender, TdsInternalInfoMessageEventArgs e)
		{
			OnTdsInfoMessage (CreateTdsInfoMessageEvent (e.Errors));
		}

		#endregion // Delegates

		#region Methods

		public TdsTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public TdsTransaction BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso, String.Empty);
		}

		public TdsTransaction BeginTransaction (string transactionName)
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, transactionName);
		}

		public TdsTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			if (state == ConnectionState.Closed)
				throw new InvalidOperationException ("Invalid operation. The connection is closed.");
			if (transaction != null && transaction.IsOpen)
				throw new InvalidOperationException ("TdsConnection does not support parallel transactions.");
			tds.ExecuteNonQuery (String.Format ("BEGIN TRANSACTION {0}", transactionName));
			transaction = new TdsTransaction (this, iso);
			return transaction;
		}

		public void ChangeDatabase (string database)
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid.", database));
			if (Database == database)
				return;
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
			// rollback any open transactions
			if (transaction != null && transaction.IsOpen)
				transaction.Rollback ();

			// if we aren't pooling, just close the connection
			// otherwise, relinquish the lock that we established in
			// the connection pool.
			if (pooling)
				pool.ReleaseConnection (tds);
			else
				tds.Disconnect ();

			tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			ChangeState (ConnectionState.Closed);
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			Close ();
		}

		public TdsCommand CreateCommand ()
		{
			TdsCommand command = new TdsCommand ();
			command.Connection = this;
			return command;
		}

		private TdsInfoMessageEventArgs CreateTdsInfoMessageEvent (TdsInternalErrorCollection errors)
		{
			return new TdsInfoMessageEventArgs (errors);
		}

		private StateChangeEventArgs CreateStateChangeEvent (ConnectionState originalState, ConnectionState currentState)
		{
			return new StateChangeEventArgs (originalState, currentState);
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
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

		public void Open ()
		{
			if (connectionString == null)
				throw new InvalidOperationException ("The ConnectionString property has not been initialized.");
			if (parms.User == null)
				throw new ArgumentException ("User name is null.");
			if (parms.Password == null)
				throw new ArgumentException ("Password is null.  This may be a bug with blank passwords.");

			if (!pooling)
				tds = new Tds42 (DataSource, port, PacketSize, ConnectionTimeout);
			else {
				pool = (TdsConnectionPool) pools[connectionString];
				if (pool == null) {
					lock (pools) {
						pool = new TdsConnectionPool (dataSource, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
						pools[connectionString] = pool;
					}
				}
				tds = pool.AllocateConnection ();
			}

			tds.TdsErrorMessage += new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage += new TdsInternalInfoMessageEventHandler (MessageHandler);

			if (!tds.IsConnected) {
				tds.Connect (parms);
				ChangeDatabase (parms.Database);
			}

			ChangeState (ConnectionState.Open);
		}

		[MonoTODO]
		private void SetConnectionString (string connectionString)
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
						value = sb.ToString ();
						parameters [name.ToUpper ().Trim ()] = value.Trim ();
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


		private void SetDefaultConnectionParameters (NameValueCollection parameters)
		{
			if (null == parameters.Get ("APPLICATION NAME"))
				parameters["APPLICATION NAME"] = ".Net SqlClient Data Provider";
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
						throw new NotSupportedException ("Unsupported network library.");
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

		private void OnTdsInfoMessage (TdsInfoMessageEventArgs value)
		{
			TdsInfoMessageEventHandler handler = (TdsInfoMessageEventHandler) Events [EventTdsInfoMessage];
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
