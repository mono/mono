//
// Mono.Data.TdsClient.TdsConnection.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// Copyright (C) Tim Coleman, 2002, 2003
// Copyright (C) Daniel Morgan, 2003
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.Data.Tds.Protocol;
using MDTP = Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Mono.Data.TdsClient {
	public sealed class TdsConnection : Component, IDbConnection, ICloneable	
	{
		#region Fields
		bool disposed = false;

		// The set of SQL connection pools
		static TdsConnectionPoolManager tdsConnectionPools = new TdsConnectionPoolManager (TdsVersion.tds42);

		// The current connection pool
		TdsConnectionPool pool;

		// The connection string that identifies this connection
		string connectionString = null;

		// The transaction object for the current transaction
		TdsTransaction transaction = null;

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

		TdsDataReader dataReader = null;

		// The TDS object
		MDTP.Tds tds;

		#endregion // Fields

		#region Constructors

		public TdsConnection () 
			: this (String.Empty)
		{
		}
	
		public TdsConnection (string connectionString) 
		{
			ConnectionString = connectionString;
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

		internal TdsDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
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

		internal MDTP.Tds Tds {
			get { return tds; }
		}

		internal TdsTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		public string WorkstationId {
			get { return parms.Hostname; }
		}

		#endregion // Properties

		#region Events and Delegates
                
		public event TdsInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;
		
		private void ErrorHandler (object sender, TdsInternalErrorMessageEventArgs e)
		{
			throw new TdsException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono TdsClient Data Provider", e.State);
		}

		private void MessageHandler (object sender, TdsInternalInfoMessageEventArgs e)
		{
			OnTdsInfoMessage (CreateTdsInfoMessageEvent (e.Errors));
		}

		#endregion // Events and Delegates

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
			if (State == ConnectionState.Closed)
				throw new InvalidOperationException ("The connection is not open.");
			if (Transaction != null)
				throw new InvalidOperationException ("TdsConnection does not support parallel transactions.");

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

			tds.Execute (String.Format ("SET TRANSACTION ISOLATION LEVEL {0}\nBEGIN TRANSACTION {1}", isolevel, transactionName));
			transaction = new TdsTransaction (this, iso);
			return transaction;
		}

		public void ChangeDatabase (string database) 
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid."));
			if (State != ConnectionState.Open)
				throw new InvalidOperationException ("The connection is not open");
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
			if (Transaction != null && Transaction.IsOpen)
				Transaction.Rollback ();
			if (pooling)
				pool.ReleaseConnection (tds);
			else
				tds.Disconnect ();
			tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			ChangeState (ConnectionState.Closed);
		}

		public TdsCommand CreateCommand () 
		{
			TdsCommand command = new TdsCommand ();
			command.Connection = this;
			return command;
		}

		private StateChangeEventArgs CreateStateChangeEvent (ConnectionState originalState, ConnectionState currentState)
		{
			return new StateChangeEventArgs (originalState, currentState);
		}

		private TdsInfoMessageEventArgs CreateTdsInfoMessageEvent (TdsInternalErrorCollection errors)
		{
			return new TdsInfoMessageEventArgs (errors);
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

		[MonoTODO]
		public void EnlistDistributedTransaction (ITransaction transaction)
		{
			throw new NotImplementedException ();
		}

		object ICloneable.Clone ()
		{
			return new TdsConnection (ConnectionString);
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

		[MonoTODO ("Figure out the Tds way to reset the connection.")]
		public void Open () 
		{
			string serverName = "";
			if (connectionString == null)
				throw new InvalidOperationException ("Connection string has not been initialized.");

			try {
				if (!pooling) {
					ParseDataSource (dataSource, out port, out serverName);
					tds = new Tds42 (serverName, port, PacketSize, ConnectionTimeout);
				}
				else {
					ParseDataSource (dataSource, out port, out serverName);
 					TdsConnectionInfo info = new TdsConnectionInfo (serverName, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
					pool = tdsConnectionPools.GetConnectionPool (connectionString, info);
					tds = pool.GetConnection ();
				}
			}
			catch (TdsTimeoutException e) {
				throw TdsException.FromTdsInternalException ((TdsInternalException) e);
			}

			tds.TdsErrorMessage += new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage += new TdsInternalInfoMessageEventHandler (MessageHandler);

			if (!tds.IsConnected) {
				try {
					tds.Connect (parms);
					ChangeState (ConnectionState.Open);
					ChangeDatabase (parms.Database);
				}
				catch {
					if (pooling)
						pool.ReleaseConnection (tds);
					throw;
				}
			}
			else if (connectionReset) {
				// tds.ExecuteNonQuery ("EXEC sp_reset_connection"); FIXME
				ChangeState (ConnectionState.Open);
			}
		}

		private void ParseDataSource (string theDataSource, out int thePort, out string theServerName) 
		{
			theServerName = "";
			thePort = 1433; // default TCP port for SQL Server

			int idx = 0;
			if ((idx = theDataSource.IndexOf (",")) > -1) {
				theServerName = theDataSource.Substring (0, idx);
				string p = theDataSource.Substring (idx + 1);
				thePort = Int32.Parse (p);
			}
			else {
				theServerName = theDataSource;
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
                                parameters["APPLICATION NAME"] = ".Net TdsClient Data Provider";
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

		private void OnTdsInfoMessage (TdsInfoMessageEventArgs value)
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
