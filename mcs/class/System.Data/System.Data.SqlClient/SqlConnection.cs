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
using System.Xml;

namespace System.Data.SqlClient {
	[DefaultEvent ("InfoMessage")]
	public sealed class SqlConnection : Component, IDbConnection, ICloneable	
	{
		#region Fields

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
			SetConnectionString (connectionString);
			this.connectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties

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
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public SqlTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			if (transaction != null)
				throw new InvalidOperationException ("SqlConnection does not support parallel transactions.");

			tds.ExecuteNonQuery (String.Format ("BEGIN TRANSACTION {0}", transactionName));
			CheckForErrors ();

			transaction = new SqlTransaction (this, iso);
			return transaction;
		}

		public void ChangeDatabase (string database) 
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid."));

			if (state != ConnectionState.Open)
				throw new InvalidOperationException ("The connection is not open");

			tds.ExecuteNonQuery (String.Format ("use {0}", database));
			CheckForErrors ();
		}

		internal void CheckForErrors ()
		{
			if (tds.Errors.Count > 0)
				throw SqlException.FromTdsError (tds.Errors);
                }

		public void Close () 
		{
			if (transaction != null && transaction.IsOpen)
				transaction.Rollback ();
			if (pooling)
				pool.ReleaseConnection (tds);
			else
				tds.Disconnect ();
			this.state = ConnectionState.Closed;
		}

		public SqlCommand CreateCommand () 
		{
			SqlCommand command = new SqlCommand ();
			command.Connection = this;
			return command;
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
				tds = new Tds70 (dataSource, port, packetSize);
			else {
				pool = (SqlConnectionPool) SqlConnectionPools [connectionString];
				if (pool == null) {
					pool = new SqlConnectionPool (dataSource, port, packetSize, minPoolSize, maxPoolSize);
					SqlConnectionPools [connectionString] = pool;
				}
				tds = pool.AllocateConnection ();
			}

			state = ConnectionState.Open;

			if (!tds.IsConnected) {
				tds.Connect (parms);
				CheckForErrors ();
				ChangeDatabase (parms.Database);
			} 
			else if (connectionReset) {
				tds.ExecuteNonQuery ("EXEC sp_connection_reset");
				CheckForErrors ();
			}
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

		#endregion // Methods

		#region Events
                
		public event SqlInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;
		
		#endregion // Events
	}
}
