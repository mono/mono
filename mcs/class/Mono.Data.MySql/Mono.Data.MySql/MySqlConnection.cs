//
// Mono.Data.MySql.MyConnection.cs
//
// Author:
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Data.MySql {

	public sealed class MySqlConnection : Component, ICloneable, IDbConnection {

		#region Fields

		private IntPtr mysqlInitStruct = IntPtr.Zero;
		private IntPtr mysqlConn = IntPtr.Zero;    
		
		private string connectionString = "";    
		private string mysqlConnectionString = ""; 
		
		private MySqlTransaction trans = null;
		private int connectionTimeout = 15;     
		// default for 15 seconds

		// MySQL connection string parameters
		string host = "";
		string user = "";
		string passwd = "";
                string dbname = "";
		uint port = MySql.Port;
		string socketName = "";
		uint flags = 0;

		// connection state
		private ConnectionState conState = ConnectionState.Closed;
		
		// DataReader state
		//private MySqlDataReader rdr = null;
		private bool dataReaderOpen = false;
		// FIXME: if true, throw an exception if SqlConnection 
		//        is used for anything other than reading
		//        data using SqlDataReader
		
		private string versionString = "Unknown";
		private bool disposed = false;

		#endregion // Fields

		#region Constructors

		// A lot of the defaults were initialized in the Fields
		[MonoTODO]
		public MySqlConnection () {

		}
	
		[MonoTODO]
		public MySqlConnection (String connectionString) {
			SetConnectionString (connectionString);
		}

		#endregion // Constructors

		#region Destructors

		protected override void Dispose(bool disposing) {
			if(!this.disposed)
				try {
					if(disposing) {
						// release any managed resources
						trans = null;
					}
					// release any unmanaged resources
					mysqlInitStruct = IntPtr.Zero;
					IntPtr mysqlConn = IntPtr.Zero;    		

					// close any handles
										
					this.disposed = true;
				}
				finally {
					base.Dispose(disposing);
				}			
		}
	
		// aka Finalize
		~MySqlConnection() {
			Dispose (false);
		}
		
		#endregion // Destructors

		#region Public Methods

		IDbTransaction IDbConnection.BeginTransaction () {
			return BeginTransaction ();
		}

		public MySqlTransaction BeginTransaction () {
			return TransactionBegin (); // call private method
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel 
			il) {
			return BeginTransaction (il);
		}

		public MySqlTransaction BeginTransaction (IsolationLevel il) {
			return TransactionBegin (il); // call private method
		}

		[MonoTODO]
		public MySqlTransaction BeginTransaction(string transactionName) {

			// FIXME: Can MySQL handle named transactions?
			return TransactionBegin (); // call private method
		}

		[MonoTODO]
		public MySqlTransaction BeginTransaction(IsolationLevel iso,
			string transactionName) {

			// FIXME: Can MySQL handle named transactions?
			return TransactionBegin (iso); // call private method
		}

		public void ChangeDatabase (string databaseName) {
			dbname = databaseName;
			int sdb = MySql.SelectDb(mysqlInitStruct, dbname);
			if (sdb != 0) {
				string msg = 
					"MySql Error: " + 
					"Can not select the " +
					dbname + 
					" database because: " + 
					MySql.Error(mysqlInitStruct);
				throw new MySqlException (msg);
			}
		}
		
		object ICloneable.Clone() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Close () {
			if(dataReaderOpen == true) {
				// TODO: what do I do if
				// the user Closes the connection
				// without closing the Reader first?

			}			
			CloseDataSource ();
		}

		IDbCommand IDbConnection.CreateCommand () {
			return CreateCommand ();
		}

		public MySqlCommand CreateCommand () {
			MySqlCommand sqlcmd = new MySqlCommand ("", this);

			return sqlcmd;
		}

		[MonoTODO]
		public void Open () {
			if(dbname.Equals(""))
				throw new InvalidOperationException(
					"dbname missing");
			else if(conState == ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnnectionState is already Open");
			else if(connectionString.Equals(String.Empty))
				throw new InvalidOperationException(
					"ConnectionString is not set");

			// FIXME: check to make sure we have 
			//        everything to connect,
			//        otherwise, throw an exception

			mysqlInitStruct = MySql.Init(IntPtr.Zero);
			if (mysqlInitStruct == IntPtr.Zero) {
				throw new MySqlException("MySQL Init failed.");
			}

			
			// *** this is what it should be ***
			//mysqlConn = MySql.Connect(mysqlInitStruct, 
			//	host.Equals("") ? null : host, 
			//	user.Equals("") ? null : user, 
			//	passwd.Equals("") ? null : passwd, 
			//	dbname.Equals("") ? null : dbname, 
			//	port,
			//	socketName.Equals("") ? null : socketName,
			//	flags);
			//	
			mysqlConn = MySql.Connect(mysqlInitStruct, 
				host, 
				user, 
				passwd, 
				dbname, 
				port,
				socketName,
				flags);
			if (mysqlConn == IntPtr.Zero) {
				string msg = "MySQL Connect failed, " +
					MySql.Error(mysqlInitStruct);
				throw new MySqlException(msg);
			}

			this.ChangeDatabase (dbname);
		
			// Successfully Connected
			SetupConnection();
		}

		#endregion // Public Methods

		#region Protected Methods

		#endregion

		#region Internal Methods

		// Used to prevent MySqlConnection
		// from doing anything while
		// MySqlDataReader is open.
		// Open the Reader. (called from MySqlCommand)
		/*
		internal void OpenReader(MySqlDataReader reader) {	
			if(dataReaderOpen == true) {
				// TODO: throw exception here?
				//       because a reader
				//       is already open
			}
			else {
				rdr = reader;
				dataReaderOpen = true;
			}
		}
		*/

		// Used to prevent MySqlConnection
		// from doing anything while
		// MySqlDataReader is open
		// Close the Reader (called from MySqlCommand)
		// if closeConnection true, Close() the connection
		// this is based on CommandBehavior.CloseConnection
		internal void CloseReader(bool closeConnection) {
			if(closeConnection == true)
				CloseDataSource();
			else
				dataReaderOpen = false;
		}

		#endregion // Internal Methods

		#region Private Methods

		private void SetupConnection() {
			conState = ConnectionState.Open;

			versionString = GetDatabaseServerVersion();
		}

		private string GetDatabaseServerVersion() {
			MySqlCommand cmd = new MySqlCommand("select version()",this);
			return (string) cmd.ExecuteScalar();
		}

		private void CloseDataSource () {
			// FIXME: just a quick hack
			if(conState == ConnectionState.Open) {
				/*
				if(trans != null)
					if(trans.DoingTransaction == true) {
						trans.Rollback();
						// trans.Dispose();
						trans = null;
					}
				*/
				conState = ConnectionState.Closed;
				MySql.Close(mysqlInitStruct);
				MySql.ThreadEnd();
				mysqlConn = IntPtr.Zero;
			}
		}

		void SetConnectionString (string connectionString) {
			this.connectionString = connectionString;
			
			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			if (connectionString == String.Empty)
				return;

			bool inQuote = false;
			bool inDQuote = false;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			foreach (char c in connectionString) {
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

			SetProperties (parameters);
		}

		private void SetProperties (NameValueCollection parameters) {

			StringBuilder connectionStr = new StringBuilder();

			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				bool found = true;
				switch (name) {
				case "PORT" :
					port = UInt32.Parse(value);
					break;
				case "DATA SOURCE" :
				case "SERVER" :
				case "HOST" :
					// set DataSource property
					host = value;
					break;
				case "INITIAL CATALOG" :
				case "DATABASE" :
				case "DBNAME" :
					// set Database property
					dbname = value;
					break;
				case "PASSWORD" :
				case "PWD" :
				case "PASSWD" :
					passwd = value;
					break;
				case "USER ID" :
				case "UID" :
				case "USER" :
					user = value;
					break;
				case "SOCKETNAME":
					socketName = value;
					break;
				case "FLAGS" :
					// FIXME: how to get these flags and
					//        and pass to MySQL?
					//        flags is a bitfield
					flags = UInt32.Parse(value);
					break;
				default:
					found = false;
					// FIXME: throw exception?
					break;
				}
				if (found == true) {
					string valuePair = name + "=" + value;					
					connectionStr.Append (valuePair + " ");
				}
			}
			this.mysqlConnectionString = connectionStr.ToString ();
		}

		private MySqlTransaction TransactionBegin () {
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new MySqlTransaction ();
			// using internal methods of SqlTransaction
			trans.SetConnection (this);
			trans.Begin();

			return trans;
		}

		private MySqlTransaction TransactionBegin (IsolationLevel il) {
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new MySqlTransaction ();
			// using internal methods of MySqlTransaction
			trans.SetConnection (this);
			trans.SetIsolationLevel (il);
			trans.Begin();

			return trans;
		}
		
		#endregion

		#region Public Properties

		[MonoTODO]
		public ConnectionState State 		{
			get { 
				return conState;
			}
		}

		public string ConnectionString	{
			get { 
				return connectionString;
			}
			set { 
				SetConnectionString (value);
			}
		}
		
		public int ConnectionTimeout {
			get { 
				return connectionTimeout; 
			}
		}

		public string Database	{
			get { 
				return dbname; 
			}
		}

		public string DataSource {
			get {
				return host;
			}
		}

		public int PacketSize {
			get { 
				throw new NotImplementedException ();
			}
		}

		public string ServerVersion {
			get { 
				return versionString;
			}
		}

		#endregion // Public Properties

		#region Internal Properties

		// For Mono.Data.MySql classes
		// to get the current transaction
		// in progress - if any
		internal MySqlTransaction Transaction {
			get {
				return trans;
			}
		}

		// For Mono.Data.MySql classes 
		// to get the unmanaged MySql connection
		internal IntPtr NativeMySqlConnection {
			get {
				return mysqlConn;
			}
		}

		// For Mono.Data.MySql classes 
		// to get the unmanaged MySql connection
		internal IntPtr NativeMySqlInitStruct {
			get {
				return mysqlInitStruct;
			}
		}

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open
		internal bool IsReaderOpen {
			get {
				return dataReaderOpen;
			}
		}

		#endregion // Internal Properties

		#region Events
/*                
		public event 
			MyInfoMessageEventHandler InfoMessage;

		public event 
			StateChangeEventHandler StateChange;
*/		
		#endregion

	}
}
