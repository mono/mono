//
// System.Data.SqlClient.SqlConnection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
using System.Net;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	[DefaultEvent ("InfoMessage")]
	public sealed class SqlConnection : Component, IDbConnection, ICloneable	
	{
		#region Fields
		bool disposed = false;

		// The set of SQL connection pools
		static Hashtable SqlConnectionPools = new Hashtable ();

		// The current connection pool
		SqlConnectionPool pool;

		// The connection string that identifies this connection
		string connectionString = null;

		// The transaction object for the current transaction
		SqlTransaction transaction = null;

		// Connection parameters
		TdsConnectionParameters parms = new TdsConnectionParameters ();
		bool connectionReset;
		bool pooling;
		string dataSource;
		int connectionTimeout;
		int minPoolSize;
		int maxPoolSize;
		int packetSize;
		int port = 1433;

		// The current state
		ConnectionState state = ConnectionState.Closed;

		SqlDataReader dataReader = null;
		XmlReader xmlReader = null;

		// The TDS object
		ITds tds;

		#endregion // Fields

		#region Constructors

		public SqlConnection () 
			: this (String.Empty)
		{
		}
	
		public SqlConnection (string connectionString) 
		{
			ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Information used to connect to a DataSource, such as 'Data Source=x;Initial Catalog=x;Integrated Security=SSPI'.")]
		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]	
		[RefreshProperties (RefreshProperties.All)]
		public string ConnectionString	{
			get { return connectionString; }
			set { SetConnectionString (value); }
		}
	
		[DataSysDescription ("Current connection timeout value, 'Connect Timeout=X' in the ConnectionString.")]	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		[DataSysDescription ("Current SQL Server database, 'Initial Catalog=X' in the ConnectionString.")]	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Database	{
			get { return tds.Database; }
		}
		
		internal SqlDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
		}

		[DataSysDescription ("Current SqlServer that the connection is opened to, 'Data Source=X' in the ConnectionString.")]	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string DataSource {
			get { return dataSource; }
		}

		[DataSysDescription ("Network packet size, 'Packet Size=x' in the ConnectionString.")]	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int PacketSize {
			get { return packetSize; }
		}

		[Browsable (false)]
		[DataSysDescription ("Version of the SQL Server accessed by the SqlConnection.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string ServerVersion {
			get { return tds.ServerVersion; }
		}

		[Browsable (false)]
		[DataSysDescription ("The ConnectionState indicating whether the connection is open or closed.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ConnectionState State {
			get { return state; }
		}

		internal ITds Tds {
			get { return tds; }
		}

		internal SqlTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		[DataSysDescription ("Workstation Id, 'Workstation Id=x' in the ConnectionString.")]	
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string WorkstationId {
			get { return parms.Hostname; }
		}

		internal XmlReader XmlReader {
			get { return xmlReader; }
			set { xmlReader = value; }
		}

		#endregion // Properties

		#region Events
               
		[DataCategory ("InfoMessage")]
		[DataSysDescription ("Event triggered when messages arrive from the DataSource.")]
		public event SqlInfoMessageEventHandler InfoMessage;

		[DataCategory ("StateChange")]
		[DataSysDescription ("Event triggered when the connection changes state.")]
		public event StateChangeEventHandler StateChange;
		
		#endregion // Events

		#region Delegates

		private void ErrorHandler (object sender, TdsInternalErrorMessageEventArgs e)
		{
			throw new SqlException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SqlClient Data Provider", e.State);
		}

		private void MessageHandler (object sender, TdsInternalInfoMessageEventArgs e)
		{
			OnSqlInfoMessage (CreateSqlInfoMessageEvent (e.Errors));
		}

		#endregion // Delegates

		#region Methods

		public SqlTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public SqlTransaction BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso, String.Empty);
		}

		public SqlTransaction BeginTransaction (string transactionName)
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, transactionName);
		}

		public SqlTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			if (state == ConnectionState.Closed)
				throw new InvalidOperationException ("The connection is not open.");
			if (transaction != null)
				throw new InvalidOperationException ("SqlConnection does not support parallel transactions.");

			string isolevel = String.Empty;
			switch (iso) {
			case IsolationLevel.Chaos:
				isolevel = "CHAOS";
				break;
			case IsolationLevel.ReadCommitted:
				isolevel = "READ COMMITTED";
				break;
			case IsolationLevel.ReadUncommitted:
				isolevel = "READ UNCOMMITTED";
				break;
			case IsolationLevel.RepeatableRead:
				isolevel = "REPEATABLE READ";
				break;
			case IsolationLevel.Serializable:
				isolevel = "SERIALIZABLE";
				break;
			}

			tds.Execute (String.Format ("SET TRANSACTION ISOLATION LEVEL {0};BEGIN TRANSACTION {1}", isolevel, transactionName));

			transaction = new SqlTransaction (this, iso);
			return transaction;
		}

		public void ChangeDatabase (string database) 
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid."));
			if (state != ConnectionState.Open)
				throw new InvalidOperationException ("The connection is not open.");
			tds.Execute (String.Format ("use {0}", database));
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

		public SqlCommand CreateCommand () 
		{
			SqlCommand command = new SqlCommand ();
			command.Connection = this;
			return command;
		}
		
		private SqlInfoMessageEventArgs CreateSqlInfoMessageEvent (TdsInternalErrorCollection errors)
		{
			return new SqlInfoMessageEventArgs (errors);
		}

		private StateChangeEventArgs CreateStateChangeEvent (ConnectionState originalState, ConnectionState currentState)
		{
			return new StateChangeEventArgs (originalState, currentState);
		}

		protected override void Dispose (bool disposing) 
		{
			if (!disposed) { 
				if (disposing) {
					if (State == ConnectionState.Open) 
						Close ();
					parms = null;
					dataSource = null;
				}
				base.Dispose (disposing);
				disposed = true;
			}
		}

		[MonoTODO ("Not sure what this means at present.")]
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		object ICloneable.Clone ()
		{
			return new SqlConnection (ConnectionString);
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
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public void Open () 
		{
			if (connectionString == null)
				throw new InvalidOperationException ("Connection string has not been initialized.");

			try {
				if (!pooling)
					tds = new Tds70 (DataSource, port, PacketSize, ConnectionTimeout);
				else {
					pool = (SqlConnectionPool) SqlConnectionPools [connectionString];
					if (pool == null) {
						pool = new SqlConnectionPool (dataSource, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
						SqlConnectionPools [connectionString] = pool;
					}
					tds = pool.AllocateConnection ();
				}
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			tds.TdsErrorMessage += new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage += new TdsInternalInfoMessageEventHandler (MessageHandler);

			if (!tds.IsConnected) 
				tds.Connect (parms);
			else if (connectionReset)
				tds.ExecProc ("sp_reset_connection");
				
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
                                parameters["APPLICATION NAME"] = "Mono SqlClient Data Provider";
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

		private void OnSqlInfoMessage (SqlInfoMessageEventArgs value)
		{
			if (InfoMessage != null)
				InfoMessage (this, value);
		}

		private void OnStateChange (StateChangeEventArgs value)
		{
			if (StateChange != null)
				StateChange (this, value);
		}

		#endregion // Methods

	}
}
