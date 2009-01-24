//
// Mono.Data.SybaseClient.SybaseConnection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//
// Copyright (C) Tim Coleman, 2002-2003
// Copyright (C) Daniel Morgan, 2003, 2008
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

#if NET_2_0
using System.Collections.Generic;
#endif

namespace Mono.Data.SybaseClient {
	[DefaultEvent ("InfoMessage")]
#if NET_2_0
	public sealed class SybaseConnection : DbConnection, IDbConnection, ICloneable
#else
	public sealed class SybaseConnection : Component, IDbConnection, ICloneable
#endif // NET_2_0
	{
		#region Fields
		bool disposed = false;

		// The set of SQL connection pools
		static TdsConnectionPoolManager sybaseConnectionPools = new TdsConnectionPoolManager (TdsVersion.tds50);

		// The current connection pool
		TdsConnectionPool pool;

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
		int port = 2048;

		// The current state
		ConnectionState state = ConnectionState.Closed;

		SybaseDataReader dataReader = null;

		// The TDS object
		MDTP.Tds tds;

		#endregion // Fields

		#region Constructors

		public SybaseConnection () 
			: this (String.Empty)
		{
		}
	
		public SybaseConnection (string connectionString) 
		{
			ConnectionString = connectionString;
		}

		#endregion // Constructors

		#region Properties
		
		public
#if NET_2_0
		override
#endif // NET_2_0 
		string ConnectionString	{
			get { return connectionString; }
			set { SetConnectionString (value); }
		}
		
		public
#if NET_2_0
		override
#endif // NET_2_0
		int ConnectionTimeout {
			get { return connectionTimeout; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0 
		string Database	{
			get { return tds.Database; }
		}

		internal SybaseDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0 
		string DataSource {
			get { return dataSource; }
		}

		public int PacketSize {
			get { return packetSize; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0 
		string ServerVersion {
			get { return tds.ServerVersion; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		ConnectionState State {
			get { return state; }
		}

		internal MDTP.Tds Tds {
			get { return tds; }
		}

		internal SybaseTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		public string WorkstationId {
			get { return parms.Hostname; }
		}

		#endregion // Properties

		#region Events and Delegates
                
		public event SybaseInfoMessageEventHandler InfoMessage;

#if !NET_2_0
		public new event StateChangeEventHandler StateChange;
#endif
		
		private void ErrorHandler (object sender, TdsInternalErrorMessageEventArgs e)
		{
			throw new SybaseException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		private void MessageHandler (object sender, TdsInternalInfoMessageEventArgs e)
		{
			OnSybaseInfoMessage (CreateSybaseInfoMessageEvent (e.Errors));
		}

		#endregion // Events and Delegates

		#region Methods

		public new SybaseTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public new SybaseTransaction BeginTransaction (IsolationLevel iso)
		{
			return BeginTransaction (iso, String.Empty);
		}

		public SybaseTransaction BeginTransaction (string transactionName)
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, transactionName);
		}

		public SybaseTransaction BeginTransaction (IsolationLevel iso, string transactionName)
		{
			if (State == ConnectionState.Closed)
				throw new InvalidOperationException ("The connection is not open.");
			if (Transaction != null)
				throw new InvalidOperationException ("SybaseConnection does not support parallel transactions.");

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
			transaction = new SybaseTransaction (this, iso);
			return transaction;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void ChangeDatabase (string database) 
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

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Close () 
		{
			if (transaction != null && transaction.IsOpen)
				transaction.Rollback ();

			if (dataReader != null) {
				if(tds != null) tds.SkipToEnd ();
				dataReader = null;
			}

			if (tds != null && tds.IsConnected) {
				if (pooling && tds.Pooling) {
					if(pool != null) pool.ReleaseConnection (tds);
				}else
					if(tds != null) tds.Disconnect ();
			}

			if (tds != null) {
				tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
				tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			}

			ChangeState (ConnectionState.Closed);
		}

		public new SybaseCommand CreateCommand () 
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
			return new SybaseConnection (ConnectionString);
		}

#if NET_2_0
		protected override DbTransaction BeginDbTransaction (IsolationLevel isolationLevel)
		{
			return BeginTransaction (isolationLevel);
		}

		protected override DbCommand CreateDbCommand ()
		{
			return CreateCommand ();
		}
#else
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
#endif

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO ("Figure out the Sybase way to reset the connection.")]
		public
#if NET_2_0
		override
#endif // NET_2_0
		void Open () 
		{
			string serverName = "";
			if (connectionString == null || connectionString.Equals (""))
				throw new InvalidOperationException ("Connection string has not been initialized.");

			try {
				if (!pooling) {
					ParseDataSource (dataSource, out port, out serverName);
					tds = new Tds50 (serverName, port, PacketSize, ConnectionTimeout);
				}
				else {
					ParseDataSource (dataSource, out port, out serverName);
 					TdsConnectionInfo info = new TdsConnectionInfo (serverName, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
					pool = sybaseConnectionPools.GetConnectionPool (connectionString, info);
					tds = pool.GetConnection ();
				}
			}
			catch (TdsTimeoutException e) {
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
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
			thePort = 2048; 
                        			
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

		private string ParseValue (string name, string value)
		{
			if (name.Length == 0 && value.Length > 0)
				throw new ArgumentException ("Expected '=' delimiter while parsing connection value pair.");
			if (name.Length > 0)
				return value.Trim ();
			return String.Empty;
		}

		private void SetConnectionString (string connectionString)
		{
			if (connectionString == String.Empty) {
				this.connectionString = connectionString;
				return;
			}

			NameValueCollection parameters = new NameValueCollection ();

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			char delimiter = '\0';

			foreach (char c in connectionString) {
				switch (c) {
				case '\'' :
				case '"' :
					if (delimiter.Equals (c))
						delimiter = '\0';
					else if (delimiter.Equals ('\0'))
						delimiter = c;
					else
						sb.Append (c);
					break;
				case ';' :
					if (delimiter.Equals ('\0')) {
						value = ParseValue (name, sb.ToString ());
						if (!value.Equals ("")) 
							parameters [name.ToUpper ().Trim ()] = value;
						name = String.Empty;
						sb = new StringBuilder ();
					} 
					else
						sb.Append (c);
					break;
				case '=' :
					if (delimiter.Equals ('\0')) {
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

			if (!delimiter.Equals ('\0'))
				throw new ArgumentException (String.Format ("Matching end delimiter {0} not found in connection option value.", delimiter));

			value = ParseValue (name, sb.ToString ());
			if (!value.Equals (""))
				parameters [name.ToUpper ().Trim ()] = value;

			SetDefaultConnectionParameters (parameters);
			SetProperties (parameters);

			this.connectionString = connectionString;
		}

		private void SetDefaultConnectionParameters (NameValueCollection parameters)
		{
			if (null == parameters.Get ("APPLICATION NAME"))
				parameters["APPLICATION NAME"] = "Mono SybaseClient Data Provider";
			if (null == parameters.Get ("CONNECT TIMEOUT") && null == parameters.Get ("CONNECTION TIMEOUT")) {
				parameters["CONNECT TIMEOUT"] = "15";
				connectionTimeout = 15;
			}
			if (null == parameters.Get ("CONNECTION LIFETIME"))
				parameters["CONNECTION LIFETIME"] = "0";
			if (null == parameters.Get ("CONNECTION RESET"))
				parameters["CONNECTION RESET"] = "true";
			if (null == parameters.Get ("ENLIST"))
				parameters["ENLIST"] = "true";
			if (null == parameters.Get ("INTEGRATED SECURITY") && null == parameters.Get ("TRUSTED_CONNECTION"))
				parameters["INTEGRATED SECURITY"] = "false";
			if (null == parameters.Get ("MAX POOL SIZE")) {
				parameters["MAX POOL SIZE"] = "100";
				maxPoolSize = 100;
			}
			if (null == parameters.Get ("MIN POOL SIZE")) {
				parameters["MIN POOL SIZE"] = "0";
				maxPoolSize = 0;
			}
			if (null == parameters.Get ("NETWORK LIBRARY") && null == parameters.Get ("NET"))
				parameters["NETWORK LIBRARY"] = "dbmssocn";
			if (null == parameters.Get ("PACKET SIZE")) {
				parameters["PACKET SIZE"] = "512";
				packetSize = 512;
			}
			if (null == parameters.Get ("PERSIST SECURITY INFO"))
				parameters["PERSIST SECURITY INFO"] = "false";
			if (null == parameters.Get ("POOLING"))
				parameters["POOLING"] = "true";
			if (null == parameters.Get ("WORKSTATION ID")) {
#if NET_2_0
				parameters["WORKSTATION ID"] = Dns.GetHostEntry ("localhost").HostName;
#else
				parameters["WORKSTATION ID"] = Dns.GetHostByName ("localhost").HostName;
#endif
			}
		}

		private void SetProperties (NameValueCollection parameters)
		{
			string value;
			foreach (string name in parameters) {
				value = parameters [name];

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

		private static bool IsValidDatabaseName (string database)
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
			if (InfoMessage != null)
				InfoMessage (this, value);
		}

#if !NET_2_0
		private void OnStateChange (StateChangeEventArgs value)
		{
			if (StateChange != null)
				StateChange (this, value);
		}
#endif

#if NET_2_0
		public override DataTable GetSchema ()
		{
			throw new NotImplementedException ();
		}

		public override DataTable GetSchema (String collectionName)
		{
			throw new NotImplementedException ();
		}

		public override DataTable GetSchema (String collectionName, string [] restrictionValues)
		{
			throw new NotImplementedException ();
		}
		
		public static void ChangePassword (string connectionString, string newPassword)
		{
			throw new NotImplementedException ();
		}

		public static void ClearAllPools ()
		{
			throw new NotImplementedException ();
		}

		public static void ClearPool (SybaseConnection connection)
		{
			throw new NotImplementedException ();
		}

#endif // NET_2_0


		#endregion // Methods
	}
}
