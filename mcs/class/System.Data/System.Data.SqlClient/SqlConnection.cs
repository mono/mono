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
#if NET_2_0
	public sealed class SqlConnection : DbConnection, IDbConnection, ICloneable	
#else
	public sealed class SqlConnection : Component, IDbConnection, ICloneable	                
#endif // NET_2_0
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
		
		internal TdsConnectionParameters parms = new TdsConnectionParameters ();
		NameValueCollection connStringParameters = null;
		bool connectionReset;
		bool pooling;
		string dataSource;
		int connectionTimeout;
		int minPoolSize;
		int maxPoolSize;
		int packetSize;
		int port = 1433;
		bool fireInfoMessageEventOnUserErrors;
		bool statisticsEnabled;

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
                        Init (connectionString);
		}

                private void Init (string connectionString)
                {
                        connectionTimeout       = 15; // default timeout
                        dataSource              = ""; // default datasource
                        packetSize              = 8192; // default packetsize
                        ConnectionString        = connectionString;
                }
                

		#endregion // Constructors

		#region Properties

		[DataCategory ("Data")]
#if !NET_2_0
		[DataSysDescription ("Information used to connect to a DataSource, such as 'Data Source=x;Initial Catalog=x;Integrated Security=SSPI'.")]
#endif
		[DefaultValue ("")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.SQL.Design.SqlConnectionStringEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RecommendedAsConfigurable (true)]	
		[RefreshProperties (RefreshProperties.All)]
		[MonoTODO("persist security info, encrypt, enlist and , attachdbfilename keyword not implemented")]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                string ConnectionString	{
			get { return connectionString; }
			set {
				if (state == ConnectionState.Open)
					throw new InvalidOperationException ("Not Allowed to change ConnectionString property while Connection state is OPEN");
				SetConnectionString (value); 
			}
		}
	
#if !NET_2_0
		[DataSysDescription ("Current connection timeout value, 'Connect Timeout=X' in the ConnectionString.")]	
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                
		int ConnectionTimeout {
			get { return connectionTimeout; }
		}

#if !NET_2_0
		[DataSysDescription ("Current SQL Server database, 'Initial Catalog=X' in the connection string.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                string Database	{
			get { 
                                if (State == ConnectionState.Open)
                                        return tds.Database; 
                                return parms.Database ;
                        }
		}
		
		internal SqlDataReader DataReader {
			get { return dataReader; }
			set { dataReader = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Current SqlServer that the connection is opened to, 'Data Source=X' in the connection string. ")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable(true)]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                string DataSource {
			get { return dataSource; }
		}

#if !NET_2_0
		[DataSysDescription ("Network packet size, 'Packet Size=x' in the connection string.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int PacketSize {
			get {	
				if (State == ConnectionState.Open) 
					return ((Tds)tds).PacketSize ;
				return packetSize; 
			}
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("Version of the SQL Server accessed by the SqlConnection.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                string ServerVersion {
			get { 
				if (state == ConnectionState.Closed)
					throw new InvalidOperationException ("Invalid Operation.The Connection is Closed");
				else
					return tds.ServerVersion; 
			}
		}

		[Browsable (false)]
#if !NET_2_0
		[DataSysDescription ("The ConnectionState indicating whether the connection is open or closed.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public 
#if NET_2_0
		override
#endif // NET_2_0
                ConnectionState State {
			get { return state; }
		}

		internal ITds Tds {
			get { return tds; }
		}

		internal SqlTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

#if !NET_2_0
		[DataSysDescription ("Workstation Id, 'Workstation ID=x' in the connection string.")]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string WorkstationId {
			get { return parms.Hostname; }
		}

		internal XmlReader XmlReader {
			get { return xmlReader; }
			set { xmlReader = value; }
		}

#if NET_2_0
		public bool FireInfoMessageEventOnUserErrors { 
			get { return fireInfoMessageEventOnUserErrors; } 
			set { fireInfoMessageEventOnUserErrors = value; }
		}
		
		public bool StatisticsEnabled { 
			get { return statisticsEnabled; } 
			set { statisticsEnabled = value; }
		}
#endif
		#endregion // Properties

		#region Events

		[DataCategory ("InfoMessage")]
#if !NET_2_0
		[DataSysDescription ("Event triggered when messages arrive from the DataSource.")]
#endif
		public event SqlInfoMessageEventHandler InfoMessage;

		[DataCategory ("StateChange")]
#if !NET_2_0
		[DataSysDescription ("Event triggered when the connection changes state.")]
#endif
		public 
#if NET_2_0
		override
#endif // NET_2_0
                event StateChangeEventHandler StateChange;
		
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
                
                internal string GetConnStringKeyValue (params string [] keys)
                {
                        if (connStringParameters == null || connStringParameters.Count == 0)
                                return "";
                        foreach (string key in keys) {
                                string value = connStringParameters [key];
                                if (value != null)
                                        return value;
                        }
                        
                        return "";
                }
                

		public new SqlTransaction BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted, String.Empty);
		}

		public new SqlTransaction BeginTransaction (IsolationLevel iso)
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

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void ChangeDatabase (string database) 
		{
			if (!IsValidDatabaseName (database))
				throw new ArgumentException (String.Format ("The database name {0} is not valid.", database));
			if (state != ConnectionState.Open)
				throw new InvalidOperationException ("The connection is not open.");
			tds.Execute (String.Format ("use [{0}]", database));
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

			if (dataReader != null || xmlReader != null) {
				if(tds != null) tds.SkipToEnd ();
				dataReader = null;
				xmlReader = null;
			}

			if (pooling) {
                                if(pool != null) pool.ReleaseConnection (tds);
			}else
                                if(tds != null) tds.Disconnect ();

			if(tds != null) {
				tds.TdsErrorMessage -= new TdsInternalErrorMessageEventHandler (ErrorHandler);
				tds.TdsInfoMessage -= new TdsInternalInfoMessageEventHandler (MessageHandler);
			}

			ChangeState (ConnectionState.Closed);
		}

		public new SqlCommand CreateCommand () 
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
			if (disposed)
				return;

			try {
				if (disposing) {
					if (State == ConnectionState.Open) 
						Close ();
					ConnectionString = "";
					SetDefaultConnectionParameters (this.connStringParameters); 
				}
			} finally {
				disposed = true;
				base.Dispose (disposing);
			}
		}

		[MonoTODO ("Not sure what this means at present.")]
		public 
                void EnlistDistributedTransaction (ITransaction transaction)
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
#if NET_2_0
		protected override DbTransaction BeginDbTransaction (IsolationLevel level)
		{
			return (DbTransaction)BeginTransaction (level);
		}

		protected override DbCommand CreateDbCommand ()
		{
			return CreateCommand ();
		}
#endif

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
                void Open () 
		{
			string serverName = "";
			if (state == ConnectionState.Open)
				throw new InvalidOperationException ("The Connection is already Open (State=Open)");

			if (connectionString == null || connectionString.Trim().Length == 0)
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
			} catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}catch (TdsInternalException e) {
				throw SqlException.FromTdsInternalException (e);
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
			} else if (connectionReset) {
				tds.Reset ();
			}

                        disposed = false; // reset this, so using () would call Close ().
			ChangeState (ConnectionState.Open);
		}

		private bool ParseDataSource (string theDataSource, out int thePort, out string theServerName) 
		{
			theServerName = "";
			string theInstanceName = "";
	
			if (theDataSource == null)
				throw new ArgumentException("Format of initialization string does not conform to specifications");

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
			else if (theDataSource == "" || theDataSource == "(local)")
				theServerName = "localhost";
			else
				theServerName = theDataSource;

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
                        SetDefaultConnectionParameters (parameters);

			if ((connectionString == null) || (connectionString.Trim().Length == 0)) {
				this.connectionString = connectionString;
				this.connStringParameters = parameters;
				return;
                        }

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
							SetProperties (name.ToUpper ().Trim() , value);
							parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
						else if (sb.Length != 0)
							throw new ArgumentException ("Format of initialization string does not conform to specifications");
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

			connectionString = connectionString.Substring (0 , connectionString.Length-1);
			this.connectionString = connectionString;
			this.connStringParameters = parameters;
		}

		void SetDefaultConnectionParameters (NameValueCollection parameters)
		{
			parms.Reset ();
			dataSource = "";
			connectionTimeout= 15;
			connectionReset = true;
			pooling = true;
			maxPoolSize = 100; 
			minPoolSize = 0;
			packetSize = 8192; 
			
			parameters["APPLICATION NAME"] = "Mono SqlClient Data Provider";
			parameters["CONNECT TIMEOUT"] = "15";
			parameters["CONNECTION LIFETIME"] = "0";
			parameters["CONNECTION RESET"] = "true";
			parameters["ENLIST"] = "true";
			parameters["INTEGRATED SECURITY"] = "false";
			parameters["INITIAL CATALOG"] = "";
			parameters["MAX POOL SIZE"] = "100";
			parameters["MIN POOL SIZE"] = "0";
			parameters["NETWORK LIBRARY"] = "dbmssocn";
			parameters["PACKET SIZE"] = "8192";
			parameters["PERSIST SECURITY INFO"] = "false";
			parameters["POOLING"] = "true";
			parameters["WORKSTATION ID"] = Dns.GetHostName();
 #if NET_2_0
			async = false;
                	parameters ["ASYNCHRONOUS PROCESSING"] = "false";
 #endif
		}
		
		private void SetProperties (string name , string value)
		{

			switch (name) 
			{
			case "APP" :
			case "APPLICATION NAME" :
				parms.ApplicationName = value;
				break;
			case "ATTACHDBFILENAME" :
			case "EXTENDED PROPERTIES" :
			case "INITIAL FILE NAME" :
				parms.AttachDBFileName = value;
				break;
			case "TIMEOUT" :
			case "CONNECT TIMEOUT" :
			case "CONNECTION TIMEOUT" :
				int tmpTimeout = ConvertToInt32 ("connection timeout", value);
				if (tmpTimeout < 0)
					throw new ArgumentException ("Invalid CONNECTION TIMEOUT .. Must be an integer >=0 ");
				else 
					connectionTimeout = tmpTimeout;
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
				int tmpMaxPoolSize = ConvertToInt32 ("max pool size" , value);
				if (tmpMaxPoolSize < 0)
					throw new ArgumentException ("Invalid MAX POOL SIZE. Must be a intger >= 0");
				else
					maxPoolSize = tmpMaxPoolSize; 
				break;
			case "MIN POOL SIZE" :
				int tmpMinPoolSize = ConvertToInt32 ("min pool size" , value);
				if (tmpMinPoolSize < 0)
					throw new ArgumentException ("Invalid MIN POOL SIZE. Must be a intger >= 0");
				else
					minPoolSize = tmpMinPoolSize;
				break;
#if NET_2_0	
			case "MULTIPLEACTIVERESULTSETS":
				break;
			case "ASYNCHRONOUS PROCESSING" :
			case "ASYNC" :
				async = ConvertToBoolean (name, value);
				break;
#endif	
			case "NET" :
			case "NETWORK" :
			case "NETWORK LIBRARY" :
				if (!value.ToUpper ().Equals ("DBMSSOCN"))
					throw new ArgumentException ("Unsupported network library.");
				break;
			case "PACKET SIZE" :
				int tmpPacketSize = ConvertToInt32 ("packet size", value);
				if (tmpPacketSize < 512 || tmpPacketSize > 32767)
					throw new ArgumentException ("Invalid PACKET SIZE. The integer must be between 512 and 32767");
				else
					packetSize = tmpPacketSize;
				break;
			case "PASSWORD" :
			case "PWD" :
				parms.Password = value;
				break;
			case "PERSISTSECURITYINFO" :
			case "PERSIST SECURITY INFO" :
				// FIXME : not implemented
				// throw new NotImplementedException ();
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

		static bool IsValidDatabaseName (string database)
		{
			if ( database == null || database.Trim() == String.Empty || database.Length > 128)
				return false ;
			
			if (database[0] == '"' && database[database.Length] == '"')
				database = database.Substring (1, database.Length - 2);
			else if (Char.IsDigit (database[0]))
				return false;

			if (database[0] == '_')
				return false;

			foreach (char c  in database.Substring (1, database.Length - 1))
				if (!Char.IsLetterOrDigit (c) && c != '_' && c != '-')
					return false;
			return true;
		}

		private void OnSqlInfoMessage (SqlInfoMessageEventArgs value)
		{
			if (InfoMessage != null)
				InfoMessage (this, value);
		}

		private new void OnStateChange (StateChangeEventArgs value)
		{
			if (StateChange != null)
				StateChange (this, value);
		}

		private sealed class SqlMonitorSocket : UdpClient 
		{
			// UDP port that the SQL Monitor listens
			private static readonly int SqlMonitorUdpPort = 1434;
			//private static readonly string SqlServerNotExist = "SQL Server does not exist or access denied";

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

#if NET_2_0
		struct ColumnInfo {
			public string name;
			public Type type;
			public ColumnInfo (string name, Type type)
			{
				this.name = name; this.type = type;
			}
		}

		static class ReservedWords
		{
			static readonly string [] reservedWords =
			{
				"ADD", "EXCEPT", "PERCENT", "ALL", "EXEC", "PLAN", "ALTER",
				  "EXECUTE", "PRECISION", "AND", "EXISTS", "PRIMARY", "ANY",
				  "EXIT", "PRINT", "AS", "FETCH", "PROC", "ASC", "FILE",
				  "PROCEDURE", "AUTHORIZATION", "FILLFACTOR", "PUBLIC",
				  "BACKUP", "FOR", "RAISERROR", "BEGIN", "FOREIGN", "READ",
				  "BETWEEN", "FREETEXT", "READTEXT", "BREAK", "FREETEXTTABLE",
				  "RECONFIGURE", "BROWSE", "FROM", "REFERENCES", "BULK",
				  "FULL", "REPLICATION", "BY", "FUNCTION", "RESTORE",
				  "CASCADE", "GOTO", "RESTRICT", "CASE", "GRANT", "RETURN",
				  "CHECK", "GROUP", "REVOKE", "CHECKPOINT", "HAVING", "RIGHT",
				  "CLOSE", "HOLDLOCK", "ROLLBACK", "CLUSTERED", "IDENTITY",
				  "ROWCOUNT", "COALESCE", "IDENTITY_INSERT", "ROWGUIDCOL",
				  "COLLATE", "IDENTITYCOL", "RULE", "COLUMN", "IF", "SAVE",
				  "COMMIT", "IN", "SCHEMA", "COMPUTE", "INDEX", "SELECT",
				  "CONSTRAINT", "INNER", "SESSION_USER", "CONTAINS", "INSERT",
				  "SET", "CONTAINSTABLE", "INTERSECT", "SETUSER", "CONTINUE",
				  "INTO", "SHUTDOWN", "CONVERT", "IS", "SOME", "CREATE",
				  "JOIN", "STATISTICS", "CROSS", "KEY", "SYSTEM_USER",
				  "CURRENT", "KILL", "TABLE", "CURRENT_DATE", "LEFT",
				  "TEXTSIZE", "CURRENT_TIME", "LIKE", "THEN",
				  "CURRENT_TIMESTAMP", "LINENO", "TO", "CURRENT_USER", "LOAD",
				  "TOP", "CURSOR", "NATIONAL", "TRAN", "DATABASE", "NOCHECK",
				  "TRANSACTION", "DBCC", "NONCLUSTERED", "TRIGGER",
				  "DEALLOCATE", "NOT", "TRUNCATE", "DECLARE", "NULL",
				  "TSEQUAL", "DEFAULT", "NULLIF", "UNION", "DELETE", "OF",
				  "UNIQUE", "DENY", "OFF", "UPDATE", "DESC", "OFFSETS",
				  "UPDATETEXT", "DISK", "ON", "USE", "DISTINCT", "OPEN",
				  "USER", "DISTRIBUTED", "OPENDATASOURCE", "VALUES", "DOUBLE",
				  "OPENQUERY", "VARYING", "DROP", "OPENROWSET", "VIEW",
				  "DUMMY", "OPENXML", "WAITFOR", "DUMP", "OPTION", "WHEN",
				  "ELSE", "OR", "WHERE", "END", "ORDER", "WHILE", "ERRLVL",
				  "OUTER", "WITH", "ESCAPE", "OVER", "WRITETEXT", "ABSOLUTE",
				  "FOUND", "PRESERVE", "ACTION", "FREE", "PRIOR", "ADMIN",
				  "GENERAL", "PRIVILEGES", "AFTER", "GET", "READS",
				  "AGGREGATE", "GLOBAL", "REAL", "ALIAS", "GO", "RECURSIVE",
				  "ALLOCATE", "GROUPING", "REF", "ARE", "HOST", "REFERENCING",
				  "ARRAY", "HOUR", "RELATIVE", "ASSERTION", "IGNORE", "RESULT",
				  "AT", "IMMEDIATE", "RETURNS", "BEFORE", "INDICATOR", "ROLE",
				  "BINARY", "INITIALIZE", "ROLLUP", "BIT", "INITIALLY",
				  "ROUTINE", "BLOB", "INOUT", "ROW", "BOOLEAN", "INPUT",
				  "ROWS", "BOTH", "INT", "SAVEPOINT", "BREADTH", "INTEGER",
				  "SCROLL", "CALL", "INTERVAL", "SCOPE", "CASCADED",
				  "ISOLATION", "SEARCH", "CAST", "ITERATE", "SECOND",
				  "CATALOG", "LANGUAGE", "SECTION", "CHAR", "LARGE",
				  "SEQUENCE", "CHARACTER", "LAST", "SESSION", "CLASS",
				  "LATERAL", "SETS", "CLOB", "LEADING", "SIZE", "COLLATION",
				  "LESS", "SMALLINT", "COMPLETION", "LEVEL", "SPACE",
				  "CONNECT", "LIMIT", "SPECIFIC", "CONNECTION", "LOCAL",
				  "SPECIFICTYPE", "CONSTRAINTS", "LOCALTIME", "SQL",
				  "CONSTRUCTOR", "LOCALTIMESTAMP", "SQLEXCEPTION",
				  "CORRESPONDING", "LOCATOR", "SQLSTATE", "CUBE", "MAP",
				  "SQLWARNING", "CURRENT_PATH", "MATCH", "START",
				  "CURRENT_ROLE", "MINUTE", "STATE", "CYCLE", "MODIFIES",
				  "STATEMENT", "DATA", "MODIFY", "STATIC", "DATE", "MODULE",
				  "STRUCTURE", "DAY", "MONTH", "TEMPORARY", "DEC", "NAMES",
				  "TERMINATE", "DECIMAL", "NATURAL", "THAN", "DEFERRABLE",
				  "NCHAR", "TIME", "DEFERRED", "NCLOB", "TIMESTAMP", "DEPTH",
				  "NEW", "TIMEZONE_HOUR", "DEREF", "NEXT", "TIMEZONE_MINUTE",
				  "DESCRIBE", "NO", "TRAILING", "DESCRIPTOR", "NONE",
				  "TRANSLATION", "DESTROY", "NUMERIC", "TREAT", "DESTRUCTOR",
				  "OBJECT", "TRUE", "DETERMINISTIC", "OLD", "UNDER",
				  "DICTIONARY", "ONLY", "UNKNOWN", "DIAGNOSTICS", "OPERATION",
				  "UNNEST", "DISCONNECT", "ORDINALITY", "USAGE", "DOMAIN",
				  "OUT", "USING", "DYNAMIC", "OUTPUT", "VALUE", "EACH",
				  "PAD", "VARCHAR", "END-EXEC", "PARAMETER", "VARIABLE",
				  "EQUALS", "PARAMETERS", "WHENEVER", "EVERY", "PARTIAL",
				  "WITHOUT", "EXCEPTION", "PATH", "WORK", "EXTERNAL",
				  "POSTFIX", "WRITE", "FALSE", "PREFIX", "YEAR", "FIRST",
				  "PREORDER", "ZONE", "FLOAT", "PREPARE", "ADA", "AVG",
				  "BIT_LENGTH", "CHAR_LENGTH", "CHARACTER_LENGTH", "COUNT",
				  "EXTRACT", "FORTRAN", "INCLUDE", "INSENSITIVE", "LOWER",
				  "MAX", "MIN", "OCTET_LENGTH", "OVERLAPS", "PASCAL",
				  "POSITION", "SQLCA", "SQLCODE", "SQLERROR", "SUBSTRING",
				  "SUM", "TRANSLATE", "TRIM", "UPPER"
			};
			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						DataRow row = null;
						instance = new DataTable ("ReservedWords");
						instance.Columns.Add ("ReservedWord", typeof(string));
						foreach (string reservedWord in reservedWords)
						{
							row = instance.NewRow();

							row["ReservedWord"] = reservedWord;
							instance.Rows.Add(row);
						}
					}
					return instance;
				}
			}
		}

		static class MetaDataCollections
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("CollectionName", typeof (string)),
				new ColumnInfo ("NumberOfRestrictions", typeof (int)),
				new ColumnInfo ("NumberOfIdentifierParts", typeof (int))
			};

			static readonly object [][] rows = {
				new object [] {"MetaDataCollections", 0, 0},
				new object [] {"DataSourceInformation", 0, 0},
				new object [] {"DataTypes", 0, 0},
				new object [] {"Restrictions", 0, 0},
				new object [] {"ReservedWords", 0, 0},
				new object [] {"Users", 1, 1},
				new object [] {"Databases", 1, 1},
				new object [] {"Tables", 4, 3},
				new object [] {"Columns", 4, 4},
				new object [] {"Views", 3, 3},
				new object [] {"ViewColumns", 4, 4},
				new object [] {"ProcedureParameters", 4, 1},
				new object [] {"Procedures", 4, 3},
				new object [] {"ForeignKeys", 4, 3},
				new object [] {"IndexColumns", 5, 4},
				new object [] {"Indexes", 4, 3},
				new object [] {"UserDefinedTypes", 2, 1}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("GetSchema");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		static class DataTypes
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("TypeName", typeof(string)),
				new ColumnInfo ("ProviderDbType", typeof(int)),
				new ColumnInfo ("ColumnSize", typeof(long)),
				new ColumnInfo ("CreateFormat", typeof(string)),
				new ColumnInfo ("CreateParameters", typeof(string)),
				new ColumnInfo ("DataType", typeof(string)),
				new ColumnInfo ("IsAutoIncrementable", typeof(bool)),
				new ColumnInfo ("IsBestMatch", typeof(bool)),
				new ColumnInfo ("IsCaseSensitive", typeof(bool)),
				new ColumnInfo ("IsFixedLength", typeof(bool)),
				new ColumnInfo ("IsFixedPrecisionScale", typeof(bool)),
				new ColumnInfo ("IsLong", typeof(bool)),
				new ColumnInfo ("IsNullable", typeof(bool)),
				new ColumnInfo ("IsSearchable", typeof(bool)),
				new ColumnInfo ("IsSearchableWithLike", typeof(bool)),
				new ColumnInfo ("IsUnsigned", typeof(bool)),
				new ColumnInfo ("MaximumScale", typeof(short)),
				new ColumnInfo ("MinimumScale", typeof(short)),
				new ColumnInfo ("IsConcurrencyType", typeof(bool)),
				new ColumnInfo ("IsLiteralSupported", typeof(bool)),
				new ColumnInfo ("LiteralPrefix", typeof(string)),
				new ColumnInfo ("LiteralSuffix", typeof(string))
			};

			static readonly object [][] rows = {
				new object [] {"smallint", 16, 5, "smallint", null, "System.Int16", true, true,
					       false, true, true, false, true, true, false, false, null,
					       null, false, null, null, null},
				new object [] {"int", 8, 10, "int", null, "System.Int32",
					       true, true, false, true, true, false, true, true, false,
					       false, null, null, false, null, null, null},
				new object [] {"real", 13, 7, "real", null,
					       "System.Single", false, true, false, true, false, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"float", 6, 53, "float({0})",
					       "number of bits used to store the mantissa", "System.Double",
					       false, true, false, true, false, false, true, true,
					       false, false, null, null, false, null, null, null},
				new object [] {"money", 9, 19, "money", null,
					       "System.Decimal", false, false, false, true, true,
					       false, true, true, false, false, null, null, false,
					       null, null, null},
				new object [] {"smallmoney", 17, 10, "smallmoney", null,
					       "System.Decimal", false, false, false, true, true, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"bit", 2, 1, "bit", null, "System.Boolean",
					       false, false, false, true, false, false, true, true,
					       false, null, null, null, false, null, null, null},
				new object [] {"tinyint", 20, 3, "tinyint", null,
					       "System.SByte", true, true, false, true, true, false,
					       true, true, false, true, null, null, false, null, null, null},
				new object [] {"bigint", 0, 19, "bigint", null,
					       "System.Int64", true, true, false, true, true, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"timestamp", 19, 8, "timestamp", null,
					       "System.Byte[]", false, false, false, true, false, false,
					       false, true, false, null, null, null, true, null, "0x", null},
				new object [] {"binary", 1, 8000, "binary({0})", "length",
					       "System.Byte[]", false, true, false, true, false, false,
					       true, true, false, null, null, null, false, null, "0x", null},
				new object [] {"image", 7, 2147483647, "image", null,
					       "System.Byte[]", false, true, false, false, false, true,
					       true, false, false, null, null, null, false, null, "0x", null},
				new object [] {"text", 18, 2147483647, "text", null,
					       "System.String", false, true, false, false, false, true,
					       true, false, true, null, null, null, false, null, "'", "'"},
				new object [] {"ntext", 11, 1073741823, "ntext", null,
					       "System.String", false, true, false, false, false, true,
					       true, false, true, null, null, null, false, null, "N'", "'"},
				new object [] {"decimal", 5, 38, "decimal({0}, {1})",
					       "precision,scale", "System.Decimal", true, true, false,
					       true, false, false, true, true, false, false, 38, 0,
					       false, null, null, null},
				new object [] {"numeric", 5, 38, "numeric({0}, {1})",
					       "precision,scale", "System.Decimal", true, true, false,
					       true, false, false, true, true, false, false, 38, 0,
					       false, null, null, null},
				new object [] {"datetime", 4, 23, "datetime", null,
					       "System.DateTime", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "{ts '", "'}"},
				new object [] {"smalldatetime", 15, 16, "smalldatetime", null,
					       "System.DateTime", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "{ts '", "'}"},
				new object [] {"sql_variant", 23, null, "sql_variant",
					       null, "System.Object", false, true, false, false, false,
					       false, true, true, false, null, null, null, false, false,
					       null, null},
				new object [] {"xml", 25, 2147483647, "xml", null,
					       "System.String", false, false, false, false, false, true,
					       true, false, false, null, null, null, false, false, null, null},
				new object [] {"varchar", 22, 2147483647, "varchar({0})",
					       "max length", "System.String", false, true, false, false,
					       false, false, true, true, true, null, null, null, false,
					       null, "'", "'"},
				new object [] {"char", 3, 2147483647, "char({0})", "length",
					       "System.String", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "'", "'"},
				new object [] {"nchar", 10, 1073741823, "nchar({0})", "length",
					       "System.String", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "N'", "'"},
				new object [] {"nvarchar", 12, 1073741823, "nvarchar({0})", "max length",
					       "System.String", false, true, false, false, false, false, true, true,
					       true, null, null, null, false, null, "N'", "'"},
				new object [] {"varbinary", 21, 1073741823, "varbinary({0})",
					       "max length", "System.Byte[]", false, true, false, false,
					       false, false, true, true, false, null, null, null, false,
					       null, "0x", null},
				new object [] {"uniqueidentifier", 14, 16, "uniqueidentifier", null,
					       "System.Guid", false, true, false, true, false, false, true,
					       true, false, null, null, null, false, null, "'", "'"}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("DataTypes");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		static class Restrictions
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("CollectionName", typeof (string)),
				new ColumnInfo ("RestrictionName", typeof(string)),
				new ColumnInfo ("ParameterName", typeof(string)),
				new ColumnInfo ("RestrictionDefault", typeof(string)),
				new ColumnInfo ("RestrictionNumber", typeof(int))
			};

			static readonly object [][] rows = {
				new object [] {"Users", "User_Name", "@Name", "name", 1},
				new object [] {"Databases", "Name", "@Name", "Name", 1},

				new object [] {"Tables", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Tables", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Tables", "Table", "@Name", "TABLE_NAME", 3},
				new object [] {"Tables", "TableType", "@TableType", "TABLE_TYPE", 4},

				new object [] {"Columns", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Columns", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Columns", "Table", "@Table", "TABLE_NAME", 3},
				new object [] {"Columns", "Column", "@Column", "COLUMN_NAME", 4},

				new object [] {"Views", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Views", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Views", "Table", "@Table", "TABLE_NAME", 3},

				new object [] {"ViewColumns", "Catalog", "@Catalog", "VIEW_CATALOG", 1},
				new object [] {"ViewColumns", "Owner", "@Owner", "VIEW_SCHEMA", 2},
				new object [] {"ViewColumns", "Table", "@Table", "VIEW_NAME", 3},
				new object [] {"ViewColumns", "Column", "@Column", "COLUMN_NAME", 4},

				new object [] {"ProcedureParameters", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1},
				new object [] {"ProcedureParameters", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2},
				new object [] {"ProcedureParameters", "Name", "@Name", "SPECIFIC_NAME", 3},
				new object [] {"ProcedureParameters", "Parameter", "@Parameter", "PARAMETER_NAME", 4},

				new object [] {"Procedures", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1},
				new object [] {"Procedures", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2},
				new object [] {"Procedures", "Name", "@Name", "SPECIFIC_NAME", 3},
				new object [] {"Procedures", "Type", "@Type", "ROUTINE_TYPE", 4},

				new object [] {"IndexColumns", "Catalog", "@Catalog", "db_name(}", 1},
				new object [] {"IndexColumns", "Owner", "@Owner", "user_name(}", 2},
				new object [] {"IndexColumns", "Table", "@Table", "o.name", 3},
				new object [] {"IndexColumns", "ConstraintName", "@ConstraintName", "x.name", 4},
				new object [] {"IndexColumns", "Column", "@Column", "c.name", 5},

				new object [] {"Indexes", "Catalog", "@Catalog", "db_name(}", 1},
				new object [] {"Indexes", "Owner", "@Owner", "user_name(}", 2},
				new object [] {"Indexes", "Table", "@Table", "o.name", 3},
				new object [] {"Indexes", "Name", "@Name", "x.name", 4},

				new object [] {"UserDefinedTypes", "assembly_name", "@AssemblyName", "assemblies.name", 1},
				new object [] {"UserDefinedTypes", "udt_name", "@UDTName", "types.assembly_class", 2},

				new object [] {"ForeignKeys", "Catalog", "@Catalog", "CONSTRAINT_CATALOG", 1},
				new object [] {"ForeignKeys", "Owner", "@Owner", "CONSTRAINT_SCHEMA", 2},
				new object [] {"ForeignKeys", "Table", "@Table", "TABLE_NAME", 3},
				new object [] {"ForeignKeys", "Name", "@Name", "CONSTRAINT_NAME", 4}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("Restrictions");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		public override DataTable GetSchema ()
		{
			return MetaDataCollections.Instance;
		}

		public override DataTable GetSchema (String collectionName)
		{
			return GetSchema (collectionName, null);
		}

		public override DataTable GetSchema (String collectionName, string [] restrictionValues)
		{
			if (collectionName == null)
				//LAMESPEC: In MS.NET, if collectionName is null, it throws ArgumentException.
				throw new ArgumentException ();

			String cName          = null;
			DataTable schemaTable = MetaDataCollections.Instance;
			int length = restrictionValues == null ? 0 : restrictionValues.Length;

			foreach (DataRow row in schemaTable.Rows) {
				if (String.Compare ((string) row["CollectionName"], collectionName, true) == 0) {
					if (length > (int) row["NumberOfRestrictions"]) {
						throw new ArgumentException ("More restrictions were provided " +
									     "than the requested schema ('" +
									     row["CollectionName"].ToString () + "') supports");
					}
					cName = row["CollectionName"].ToString();
				}
			}
			if (cName == null)
				throw new ArgumentException ("The requested collection ('" + collectionName + "') is not defined.");

			SqlCommand command     = null;
			DataTable dataTable    = new DataTable ();
			SqlDataAdapter dataAdapter = new SqlDataAdapter ();

			switch (cName)
			{
			case "Databases":
				command = new SqlCommand ("select name as database_name, dbid, crdate as create_date " +
							  "from master.sys.sysdatabases where (name = @Name or (@Name " +
							  "is null))", this);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				break;
			case "ForeignKeys":
				command = new SqlCommand ("select CONSTRAINT_CATALOG, CONSTRAINT_SCHEMA, CONSTRAINT_NAME, " +
							  "TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_TYPE, " +
							  "IS_DEFERRABLE, INITIALLY_DEFERRED from " +
							  "INFORMATION_SCHEMA.TABLE_CONSTRAINTS where (CONSTRAINT_CATALOG" +
							  " = @Catalog or (@Catalog is null)) and (CONSTRAINT_SCHEMA = " +
							  "@Owner or (@Owner is null)) and (TABLE_NAME = @Table or (" +
							  "@Table is null)) and (CONSTRAINT_NAME = @Name or (@Name is null))" +
							  " and CONSTRAINT_TYPE = 'FOREIGN KEY' order by CONSTRAINT_CATALOG," +
							  " CONSTRAINT_SCHEMA, CONSTRAINT_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				break;
			case "Indexes":
				command = new SqlCommand ("select distinct db_name() as constraint_catalog, " +
							  "constraint_schema = user_name (o.uid), " +
							  "constraint_name = x.name, table_catalog = db_name (), " +
							  "table_schema = user_name (o.uid), table_name = o.name, " +
							  "index_name  = x.name from sysobjects o, sysindexes x, " +
							  "sysindexkeys xk where o.type in ('U') and x.id = o.id and " +
							  "o.id = xk.id and x.indid = xk.indid and xk.keyno = x.keycnt " +
							  "and (db_name() = @Catalog or (@Catalog is null)) and " +
							  "(user_name() = @Owner or (@Owner is null)) and (o.name = " +
							  "@Table or (@Table is null)) and (x.name = @Name or (@Name is null))" +
							  "order by table_name, index_name", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				break;
			case "IndexColumns":
				command = new SqlCommand ("select distinct db_name() as constraint_catalog, " +
							  "constraint_schema = user_name (o.uid), constraint_name = x.name, " +
							  "table_catalog = db_name (), table_schema = user_name (o.uid), " +
							  "table_name = o.name, column_name = c.name, " +
							  "ordinal_position = convert (int, xk.keyno), keyType = c.xtype, " +
							  "index_name = x.name from sysobjects o, sysindexes x, syscolumns c, " +
							  "sysindexkeys xk where o.type in ('U') and x.id = o.id and o.id = c.id " +
							  "and o.id = xk.id and x.indid = xk.indid and c.colid = xk.colid " +
							  "and xk.keyno <= x.keycnt and permissions (o.id, c.name) <> 0 " +
							  "and (db_name() = @Catalog or (@Catalog is null)) and (user_name() " +
							  "= @Owner or (@Owner is null)) and (o.name = @Table or (@Table is" +
							  " null)) and (x.name = @ConstraintName or (@ConstraintName is null)) " +
							  "and (c.name = @Column or (@Column is null)) order by table_name, " +
							  "index_name", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 8);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 13);
				command.Parameters.Add ("@ConstraintName", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Column", SqlDbType.NVarChar, 4000);
				break;
			case "Procedures":
				command = new SqlCommand ("select SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, " +
							  "ROUTINE_CATALOG, ROUTINE_SCHEMA, ROUTINE_NAME, ROUTINE_TYPE, " +
							  "CREATED, LAST_ALTERED from INFORMATION_SCHEMA.ROUTINES where " +
							  "(SPECIFIC_CATALOG = @Catalog or (@Catalog is null)) and " +
							  "(SPECIFIC_SCHEMA = @Owner or (@Owner is null)) and (SPECIFIC_NAME" +
							  " = @Name or (@Name is null)) and (ROUTINE_TYPE = @Type or (@Type " +
							  "is null)) order by SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Type", SqlDbType.NVarChar, 4000);
				break;
			case "ProcedureParameters":
				command = new SqlCommand ("select SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, " +
							  "ORDINAL_POSITION, PARAMETER_MODE, IS_RESULT, AS_LOCATOR, " +
							  "PARAMETER_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, " +
							  "CHARACTER_OCTET_LENGTH, COLLATION_CATALOG, COLLATION_SCHEMA, " +
							  "COLLATION_NAME, CHARACTER_SET_CATALOG, CHARACTER_SET_SCHEMA, " +
							  "CHARACTER_SET_NAME, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, " +
							  "NUMERIC_SCALE, DATETIME_PRECISION, INTERVAL_TYPE, " +
							  "INTERVAL_PRECISION from INFORMATION_SCHEMA.PARAMETERS where " +
							  "(SPECIFIC_CATALOG = @Catalog or (@Catalog is null)) and " +
							  "(SPECIFIC_SCHEMA = @Owner or (@Owner is null)) and (SPECIFIC_NAME = " +
							  "@Name or (@Name is null)) and (PARAMETER_NAME = @Parameter or (" +
							  "@Parameter is null)) order by SPECIFIC_CATALOG, SPECIFIC_SCHEMA," +
							  " SPECIFIC_NAME, PARAMETER_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Parameter", SqlDbType.NVarChar, 4000);
				break;
			case "Tables":
				command = new SqlCommand ("select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE " +
							  "from INFORMATION_SCHEMA.TABLES where" +
							  " (TABLE_CATALOG = @catalog or (@catalog is null)) and " +
							  "(TABLE_SCHEMA = @owner or (@owner is null))and " +
							  "(TABLE_NAME = @name or (@name is null)) and " +
							  "(TABLE_TYPE = @table_type or (@table_type is null))", this);
				command.Parameters.Add ("@catalog", SqlDbType.NVarChar, 8);
				command.Parameters.Add ("@owner", SqlDbType.NVarChar, 3);
				command.Parameters.Add ("@name", SqlDbType.NVarChar, 11);
				command.Parameters.Add ("@table_type", SqlDbType.NVarChar, 10);
				break;
			case "Columns":
				command = new SqlCommand ("select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, " +
							  "ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, " +
							  "CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH, " +
							  "NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, NUMERIC_SCALE, " +
							  "DATETIME_PRECISION, CHARACTER_SET_CATALOG, CHARACTER_SET_SCHEMA, " +
							  "CHARACTER_SET_NAME, COLLATION_CATALOG from INFORMATION_SCHEMA.COLUMNS" +
							  " where (TABLE_CATALOG = @Catalog or (@Catalog is null)) and (" +
							  "TABLE_SCHEMA = @Owner or (@Owner is null)) and (TABLE_NAME = @table" +
							  " or (@Table is null)) and (COLUMN_NAME = @column or (@Column is null" +
							  ")) order by TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Column", SqlDbType.NVarChar, 4000);
				break;
			case "Users":
				command = new SqlCommand ("select uid, name as user_name, createdate, updatedate from sysusers" +
							  " where (name = @Name or (@Name is null))", this);
				command.Parameters.Add ("@Name", SqlDbType.NVarChar, 4000);
				break;
			case "Views":
				command = new SqlCommand ("select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, CHECK_OPTION, " +
							  "IS_UPDATABLE from INFORMATION_SCHEMA.VIEWS where (TABLE_CATALOG" +
							  " = @Catalog or (@Catalog is null)) TABLE_SCHEMA = @Owner or " +
							  "(@Owner is null)) and (TABLE_NAME = @table or (@Table is null))" +
							  " order by TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 4000);
				break;
			case "ViewColumns":
				command = new SqlCommand ("select VIEW_CATALOG, VIEW_SCHEMA, VIEW_NAME, TABLE_CATALOG, " +
							  "TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME from " +
							  "INFORMATION_SCHEMA.VIEW_COLUMN_USAGE where (VIEW_CATALOG = " +
							  "@Catalog (@Catalog is null)) and (VIEW_SCHEMA = @Owner (@Owner" +
							  " is null)) and (VIEW_NAME = @Table or (@Table is null)) and " +
							  "(COLUMN_NAME = @Column or (@Column is null)) order by " +
							  "VIEW_CATALOG, VIEW_SCHEMA, VIEW_NAME", this);
				command.Parameters.Add ("@Catalog", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Owner", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Table", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@Column", SqlDbType.NVarChar, 4000);
				break;
			case "UserDefinedTypes":
				command = new SqlCommand ("select assemblies.name as assembly_name, types.assembly_class " +
							  "as udt_name, ASSEMBLYPROPERTY(assemblies.name, 'VersionMajor') " +
							  "as version_major, ASSEMBLYPROPERTY(assemblies.name, 'VersionMinor') " +
							  "as version_minor, ASSEMBLYPROPERTY(assemblies.name, 'VersionBuild') " +
							  "as version_build, ASSEMBLYPROPERTY(assemblies.name, 'VersionRevision') " +
							  "as version_revision, ASSEMBLYPROPERTY(assemblies.name, 'CultureInfo') " +
							  "as culture_info, ASSEMBLYPROPERTY(assemblies.name, 'PublicKey') " +
							  "as public_key, is_fixed_length, max_length, Create_Date, " +
							  "Permission_set_desc from sys.assemblies as assemblies join " +
							  "sys.assembly_types as types on assemblies.assembly_id = types.assembly_id" +
							  " where (assportemblies.name = @AssemblyName or (@AssemblyName is null)) and " +
							  "(types.assembly_class = @UDTName or (@UDTName is null))",
							  this);
				command.Parameters.Add ("@AssemblyName", SqlDbType.NVarChar, 4000);
				command.Parameters.Add ("@UDTName", SqlDbType.NVarChar, 4000);
				break;
			case "MetaDataCollections":
				return MetaDataCollections.Instance;
			case "DataSourceInformation":
				throw new NotImplementedException ();
			case "DataTypes":
				return DataTypes.Instance;
			case "ReservedWords":
				return ReservedWords.Instance;
			case "Restrictions":
				return Restrictions.Instance;
			}
			for (int i = 0; i < length; i++) {
				command.Parameters[i].Value = restrictionValues[i];
			}
			dataAdapter.SelectCommand = command;
			dataAdapter.Fill (dataTable);
			return dataTable;
		}
		
		public static void ChangePassword (string connectionString, string newPassword)
		{
			if (connectionString == null || newPassword == null || newPassword == String.Empty)
				throw new ArgumentNullException ();
			if (newPassword.Length > 128)
				throw new ArgumentException ("The value of newPassword exceeds its permittable length which is 128");
			SqlConnection conn = new SqlConnection (connectionString);
			using (conn) {
				conn.Open ();
				conn.tds.Execute (String.Format ("sp_password '{0}', '{1}', '{2}'",
								 conn.parms.Password, newPassword, conn.parms.User));
				conn.Close ();
			}
		}
#endif // NET_2_0

		#endregion // Methods

#if NET_2_0
		#region Fields Net 2

		bool async = false;

		#endregion // Fields  Net 2

                #region Properties Net 2

#if !NET_2_0
                [DataSysDescription ("Enable Asynchronous processing, 'Asynchrouse Processing=true/false' in the ConnectionString.")]	
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		internal bool AsyncProcessing  {
			get { return async; }
		}

                #endregion // Properties Net 2

#endif // NET_2_0

	}
}
