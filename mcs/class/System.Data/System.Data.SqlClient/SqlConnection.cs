//
// System.Data.SqlClient.SqlConnection.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Phillip Jerkins (Phillip.Jerkins@morgankeegan.com)
//   Diego Caravana (diego@toth.it)
//
// Copyright (C) Ximian, Inc 2002
// Copyright (C) Daniel Morgan 2002, 2003
// Copyright (C) Tim Coleman, 2002, 2003
// Copyright (C) Phillip Jerkins, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.EnterpriseServices;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	[DefaultEvent ("InfoMessage")]
	public sealed class SqlConnection : Component, IDbConnection, ICloneable	
	{
		#region Fields
		bool disposed = false;

		// The set of SQL connection pools
		static TdsConnectionPoolManager sqlConnectionPools = new TdsConnectionPoolManager (TdsVersion.tds70);

		// The current connection pool
		TdsConnectionPool pool;

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
		[EditorAttribute ("Microsoft.VSDesigner.Data.SQL.Design.SqlConnectionStringEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RecommendedAsConfigurable (true)]	
		[RefreshProperties (RefreshProperties.All)]
		[MonoTODO("persist security info, encrypt, enlist and , attachdbfilename keyword not implemented")]
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

			if (iso == IsolationLevel.Chaos)
				throw new ArgumentException ("Invalid IsolationLevel parameter: must be ReadCommitted, ReadUncommitted, RepeatableRead, or Serializable.");

			string isolevel = String.Empty;
			switch (iso) {
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

			if (dataReader != null || xmlReader != null) {
				if(tds != null) tds.SkipToEnd ();
				dataReader = null;
				xmlReader = null;
			}

			if (pooling)
				if(pool != null) pool.ReleaseConnection (tds);
			else
				if(tds != null) tds.Disconnect ();

			if(tds != null) {
				tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
				tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			}

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
			string serverName = "";
			if (connectionString == null)
				throw new InvalidOperationException ("Connection string has not been initialized.");

			try {
				if (!pooling) {
					if(!ParseDataSource (dataSource, out port, out serverName))
						throw new SqlException(20, 0, "SQL Server does not exist or access denied.",  17, "ConnectionOpen (Connect()).", dataSource, parms.ApplicationName, 0);
					tds = new Tds70 (serverName, port, PacketSize, ConnectionTimeout);
				}
				else {
					if(!ParseDataSource (dataSource, out port, out serverName))
						throw new SqlException(20, 0, "SQL Server does not exist or access denied.",  17, "ConnectionOpen (Connect()).", dataSource, parms.ApplicationName, 0);
					
 					TdsConnectionInfo info = new TdsConnectionInfo (serverName, port, packetSize, ConnectionTimeout, minPoolSize, maxPoolSize);
					pool = sqlConnectionPools.GetConnectionPool (connectionString, info);
					tds = pool.GetConnection ();
				}
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			tds.TdsErrorMessage += new TdsInternalErrorMessageEventHandler (ErrorHandler);
			tds.TdsInfoMessage += new TdsInternalInfoMessageEventHandler (MessageHandler);

			if (!tds.IsConnected) {
				try {
					tds.Connect (parms);
				}
				catch {
					if (pooling)
						pool.ReleaseConnection (tds);
					throw;
				}
			}

			/* Not sure ebout removing these 2 lines.
			 * The command that gets to the sql server is just
			 * 'sp_reset_connection' and it fails.
			 * Either remove them definitely or fix it
			else if (connectionReset)
				tds.ExecProc ("sp_reset_connection");
			*/
				
			ChangeState (ConnectionState.Open);
		}

		private bool ParseDataSource (string theDataSource, out int thePort, out string theServerName) 
		{
			theServerName = "";
			string theInstanceName = "";
			if ((theDataSource == null) || (theServerName == null))
				throw new ArgumentException("Format of initialization string doesnot conform to specifications");
				
			thePort = 1433; // default TCP port for SQL Server
			bool success = true;

			int idx = 0;
			if ((idx = theDataSource.IndexOf (",")) > -1) {
				theServerName = theDataSource.Substring (0, idx);
				string p = theDataSource.Substring (idx + 1);
				thePort = Int32.Parse (p);
			}
			else if ((idx = theDataSource.IndexOf ("\\")) > -1) {
				theServerName = theDataSource.Substring (0, idx);
				theInstanceName = theDataSource.Substring (idx + 1);
				// do port discovery via UDP port 1434
				port = DiscoverTcpPortViaSqlMonitor (theServerName, theInstanceName);
				if (port == -1)
					success = false;
			}
			else {
				theServerName = theDataSource;
			}

			if(theServerName.Equals("(local)"))
				theServerName = "localhost";

			return success;
		}

		private bool ConvertIntegratedSecurity (string value)
		{
			if (value.ToUpper() == "SSPI") 
			{
				return true;
			}

			return ConvertToBoolean("integrated security", value);
		}

		private bool ConvertToBoolean(string key, string value)
		{
			string upperValue = value.ToUpper();

			if (upperValue == "TRUE" ||upperValue == "YES")
			{
				return true;
			} 
			else if (upperValue == "FALSE" || upperValue == "NO")
			{
				return false;
			}

			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
				"Invalid value \"{0}\" for key '{1}'.", value, key));
		}

		private int ConvertToInt32(string key, string value)
		{
			try
			{
				return int.Parse(value);
			}
			catch (Exception ex)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
					"Invalid value \"{0}\" for key '{1}'.", value, key));
			}
		}

		private int DiscoverTcpPortViaSqlMonitor(string ServerName, string InstanceName) 
		{
			SqlMonitorSocket msock;
			msock = new SqlMonitorSocket (ServerName, InstanceName);
			int SqlServerPort = msock.DiscoverTcpPort ();
			msock = null;
			return SqlServerPort;
		}

		void SetConnectionString (string connectionString)
		{
			NameValueCollection parameters = new NameValueCollection ();

			if (( connectionString == null)||( connectionString.Length == 0)) 
				return;
			connectionString += ";";

			bool inQuote = false;
			bool inDQuote = false;
			bool inName = true;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < connectionString.Length; i += 1) {
				char c = connectionString [i];
				char peek;
				if (i == connectionString.Length - 1)
					peek = '\0';
				else
					peek = connectionString [i + 1];

				switch (c) {
				case '\'':
					if (inDQuote)
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					}
					else
						inQuote = !inQuote;
					break;
				case '"':
					if (inQuote)
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					}
					else
						inDQuote = !inDQuote;
					break;
				case ';':
					if (inDQuote || inQuote)
						sb.Append (c);
					else {
						if (name != String.Empty && name != null) {
							value = sb.ToString ();
							parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
						inName = true;
						name = String.Empty;
						value = String.Empty;
						sb = new StringBuilder ();
					}
					break;
				case '=':
					if (inDQuote || inQuote || !inName)
						sb.Append (c);
					else if (peek.Equals (c)) {
						sb.Append (c);
						i += 1;
					}
					else {
						name = sb.ToString ();
						sb = new StringBuilder ();
						inName = false;
					}
					break;
				case ' ':
					if (inQuote || inDQuote)
						sb.Append (c);
					else if (sb.Length > 0 && !peek.Equals (';'))
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
			if (null == parameters.Get ("APPLICATION NAME") && null == parameters.Get ("APP"))
				parameters["APPLICATION NAME"] = "Mono SqlClient Data Provider";
			if (null == parameters.Get ("TIMEOUT") && null == parameters.Get ("CONNECT TIMEOUT") && null == parameters.Get ("CONNECTION TIMEOUT"))
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
			if (null == parameters.Get ("NETWORK LIBRARY") && null == parameters.Get ("NET") && null == parameters.Get ("NETWORK"))
				parameters["NETWORK LIBRARY"] = "dbmssocn";
			if (null == parameters.Get ("PACKET SIZE"))
				parameters["PACKET SIZE"] = "512";
			if (null == parameters.Get ("PERSIST SECURITY INFO") && null == parameters.Get ("PERSISTSECURITYINFO"))
				parameters["PERSIST SECURITY INFO"] = "false";
			if (null == parameters.Get ("POOLING"))
				parameters["POOLING"] = "true";
			if (null == parameters.Get ("WORKSTATION ID") && null == parameters.Get ("WSID"))
				parameters["WORKSTATION ID"] = Dns.GetHostName();
		}

		private void SetProperties (NameValueCollection parameters)
		{
			foreach (string name in parameters) {
				string value = parameters[name];

				switch (name) {
					case "APP" :
					case "APPLICATION NAME" :
						parms.ApplicationName = value;
						break;
					case "ATTACHDBFILENAME" :
					case "EXTENDED PROPERTIES" :
					case "INITIAL FILE NAME" :
						throw new NotImplementedException("Attachable database support is not implemented.");
					case "TIMEOUT" :
					case "CONNECT TIMEOUT" :
					case "CONNECTION TIMEOUT" :
						connectionTimeout = ConvertToInt32 ("connection timeout", value);
						break;
					case "CONNECTION LIFETIME" :
						break;
					case "CONNECTION RESET" :
						connectionReset = ConvertToBoolean ("connection reset", value);
						break;
					case "LANGUAGE" :
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
					case "ENCRYPT":
						if (ConvertToBoolean("encrypt", value))
						{
							throw new NotImplementedException("SSL encryption for"
								+ " data sent between client and server is not"
								+ " implemented.");
						}
						break;
					case "ENLIST" :
						if (!ConvertToBoolean("enlist", value))
						{
							throw new NotImplementedException("Disabling the automatic"
								+ " enlistment of connections in the thread's current"
								+ " transaction context is not implemented.");
						}
						break;
					case "INITIAL CATALOG" :
					case "DATABASE" :
						parms.Database = value;
						break;
					case "INTEGRATED SECURITY" :
					case "TRUSTED_CONNECTION" :
						parms.DomainLogin = ConvertIntegratedSecurity(value);
						break;
					case "MAX POOL SIZE" :
						maxPoolSize = ConvertToInt32 ("max pool size", value);
						break;
					case "MIN POOL SIZE" :
						minPoolSize = ConvertToInt32 ("min pool size", value);
						break;
#if NET_2_0
				case "MULTIPLEACTIVERESULTSETS":
					break;
#endif
					case "NET" :
					case "NETWORK" :
					case "NETWORK LIBRARY" :
						if (!value.ToUpper ().Equals ("DBMSSOCN"))
							throw new ArgumentException ("Unsupported network library.");
						break;
					case "PACKET SIZE" :
						packetSize = ConvertToInt32 ("packet size", value);
						break;
					case "PASSWORD" :
					case "PWD" :
						parms.Password = value;
						break;
					case "PERSISTSECURITYINFO" :
					case "PERSIST SECURITY INFO" :
						// FIXME : not implemented
						break;
					case "POOLING" :
						pooling = ConvertToBoolean("pooling", value);
						break;
					case "UID" :
					case "USER" :
					case "USER ID" :
						parms.User = value;
						break;
					case "WSID" :
					case "WORKSTATION ID" :
						parms.Hostname = value;
						break;
					default :
						throw new ArgumentException("Keyword not supported :"+name);
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

		private sealed class SqlMonitorSocket : UdpClient 
		{
			// UDP port that the SQL Monitor listens
			private static readonly int SqlMonitorUdpPort = 1434;
			private static readonly string SqlServerNotExist = "SQL Server does not exist or access denied";

			private string server;
			private string instance;

			internal SqlMonitorSocket (string ServerName, string InstanceName) 
				: base (ServerName, SqlMonitorUdpPort) 
			{
				server = ServerName;
				instance = InstanceName;
			}

			internal int DiscoverTcpPort () 
			{
				int SqlServerTcpPort;
				Client.Blocking = false;
				// send command to UDP 1434 (SQL Monitor) to get
				// the TCP port to connect to the MS SQL server		
				ASCIIEncoding enc = new ASCIIEncoding ();
				Byte[] rawrq = new Byte [instance.Length + 1];
				rawrq[0] = 4;
				enc.GetBytes (instance, 0, instance.Length, rawrq, 1);
				int bytes = Send (rawrq, rawrq.Length);

				if (!Active)
					return -1; // Error
				
				bool result;
				result = Client.Poll (100, SelectMode.SelectRead);
				if (result == false)
					return -1; // Error

				if (Client.Available <= 0)
					return -1; // Error

				IPEndPoint endpoint = new IPEndPoint (Dns.GetHostByName ("localhost").AddressList [0], 0);
				Byte [] rawrs;

				rawrs = Receive (ref endpoint);

				string rs = Encoding.ASCII.GetString (rawrs);

				string[] rawtokens = rs.Split (';');
				Hashtable data = new Hashtable ();
				for (int i = 0; i < rawtokens.Length / 2 && i < 256; i++) {
					data [rawtokens [i * 2]] = rawtokens [ i * 2 + 1];
				}
				if (!data.ContainsKey ("tcp")) 
					throw new NotImplementedException ("Only TCP/IP is supported.");

				SqlServerTcpPort = int.Parse ((string) data ["tcp"]);
				Close ();

				return SqlServerTcpPort;
			}
		}

		#endregion // Methods
	}
}
