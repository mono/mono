//
// Mono.Data.PostgreSqlClient.PgSqlConnection.cs
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
// Credits:
//    SQL and concepts were used from libgda 0.8.190 (GNOME Data Access)
//    http://www.gnome-db.org/
//    with permission from the authors of the
//    PostgreSQL provider in libgda:
//        Michael Lausch <michael@lausch.at>
//        Rodrigo Moya <rodrigo@gnome-db.org>
//        Vivien Malerba <malerba@gnome-db.org>
//        Gonzalo Paniagua Javier <gonzalo@gnome-db.org>
//

// use #define DEBUG_SqlConnection if you want to spew debug messages
// #define DEBUG_SqlConnection

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Data.PostgreSqlClient {

	/// <summary>
	/// Represents an open connection to a SQL data source
	/// </summary>
	public sealed class PgSqlConnection : Component, IDbConnection,
		ICloneable	
	{
		// FIXME: Need to implement class Component, 
		// and interfaces: ICloneable and IDisposable	

		#region Fields

		private PostgresTypes types = null;
		private IntPtr pgConn = IntPtr.Zero;    

		// PGConn (Postgres Connection)
		private string connectionString = "";    
		// OLE DB Connection String
		private string pgConnectionString = ""; 
		// PostgreSQL Connection String
		private PgSqlTransaction trans = null;
		private int connectionTimeout = 15;     
		// default for 15 seconds
		
		// connection parameters in connection string
		private string host = "";     
		// Name of host to connect to
		private string hostaddr = ""; 
		// IP address of host to connect to
		// should be in "n.n.n.n" format
		private string port = "";     
		// Port number to connect to at the server host
		private string dbname = "";   // The database name. 
		private string user = "";     // User name to connect as. 
		private string password = "";
		// Password to be used if the server 
		// demands password authentication.  		
		private string options = ""; 
		// Trace/debug options to be sent to the server. 
		private string tty = ""; 
		// A file or tty for optional 
		// debug output from the backend. 
		private string requiressl = "";
		// Set to 1 to require 
		// SSL connection to the backend. 
		// Libpq will then refuse to connect 
		// if the server does not 
		// support SSL. Set to 0 (default) to 
		// negotiate with server. 

		// connection state
		private ConnectionState conState = ConnectionState.Closed;
		
		// DataReader state
		private PgSqlDataReader rdr = null;
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
		public PgSqlConnection () {

		}
	
		[MonoTODO]
		public PgSqlConnection (String connectionString) {
			SetConnectionString (connectionString);
		}

		#endregion // Constructors

		#region Destructors

		protected override void Dispose(bool disposing) {
			if(!this.disposed)
				try {
					if(disposing) {
						// release any managed resources
					}
					// release any unmanaged resources
					// close any handles
										
					this.disposed = true;
				}
				finally {
					base.Dispose(disposing);
				}
		}
	
		// aka Finalize()
		// [ClassInterface(ClassInterfaceType.AutoDual)]
		[MonoTODO]
		~PgSqlConnection() {
			Dispose (false);
		}
		
		#endregion // Destructors

		#region Public Methods

		IDbTransaction IDbConnection.BeginTransaction () {
			return BeginTransaction ();
		}

		public PgSqlTransaction BeginTransaction () {
			return TransactionBegin (); // call private method
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel 
			il) {
			return BeginTransaction (il);
		}

		public PgSqlTransaction BeginTransaction (IsolationLevel il) {
			return TransactionBegin (il); // call private method
		}

		// PostgreSQL does not support named transactions/savepoint
		//            nor nested transactions
		[Obsolete]
		public PgSqlTransaction BeginTransaction(string transactionName) {
			return TransactionBegin (); // call private method
		}

		[Obsolete]
		public PgSqlTransaction BeginTransaction(IsolationLevel iso,
			string transactionName) {
			return TransactionBegin (iso); // call private method
		}

		[MonoTODO]
		public void ChangeDatabase (string databaseName) {
			throw new NotImplementedException ();
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

		public PgSqlCommand CreateCommand () {
			PgSqlCommand sqlcmd = new PgSqlCommand ("", this);

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

			ConnStatusType connStatus;

			// FIXME: check to make sure we have 
			//        everything to connect,
			//        otherwise, throw an exception

			pgConn = PostgresLibrary.PQconnectdb 
				(pgConnectionString);

			// FIXME: should we use PQconnectStart/PQconnectPoll
			//        instead of PQconnectdb?  
			// PQconnectdb blocks 
			// PQconnectStart/PQconnectPoll is non-blocking
			
			connStatus = PostgresLibrary.PQstatus (pgConn);
			if(connStatus == ConnStatusType.CONNECTION_OK) {
				// Successfully Connected
				disposed = false;

				SetupConnection();
			}
			else {
				String errorMessage = PostgresLibrary.
					PQerrorMessage (pgConn);
				errorMessage += ": Could not connect to database.";

				throw new PgSqlException(0, 0,
					errorMessage, 0, "",
					host, "SqlConnection", 0);
			}
		}

		#endregion // Public Methods

		#region Internal Methods

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open.
		// Open the Reader. (called from SqlCommand)
		internal void OpenReader(PgSqlDataReader reader) 
		{	
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

		// Used to prevent SqlConnection
		// from doing anything while
		// SqlDataReader is open
		// Close the Reader (called from SqlCommand)
		// if closeConnection true, Close() the connection
		// this is based on CommandBehavior.CloseConnection
		internal void CloseReader(bool closeConnection)
		{	if(closeConnection == true)
				CloseDataSource();
			else
				dataReaderOpen = false;
		}

		#endregion // Internal Methods

		#region Private Methods

		void SetupConnection() {
			
			conState = ConnectionState.Open;

			// FIXME: load types into hashtable
			types = new PostgresTypes(this);
			types.Load();

			versionString = GetDatabaseServerVersion();

			// set DATE style to YYYY/MM/DD
			IntPtr pgResult = IntPtr.Zero;
			pgResult = PostgresLibrary.PQexec (pgConn, "SET DATESTYLE TO 'ISO'");
			PostgresLibrary.PQclear (pgResult);
			pgResult = IntPtr.Zero;
		}

		string GetDatabaseServerVersion() 
		{
			PgSqlCommand cmd = new PgSqlCommand("select version()",this);
			return (string) cmd.ExecuteScalar();
		}

		void CloseDataSource () {
			// FIXME: just a quick hack
			if(conState == ConnectionState.Open) {
				if(trans != null)
					if(trans.DoingTransaction == true) {
						trans.Rollback();
						// trans.Dispose();
						trans = null;
					}

				conState = ConnectionState.Closed;
				PostgresLibrary.PQfinish (pgConn);
				pgConn = IntPtr.Zero;
			}
		}

		void SetConnectionString (string connectionString) {
			this.connectionString = connectionString;
			StringBuilder postgresConnectionString = new StringBuilder ();

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
			StringBuilder postgresConnectionString = new StringBuilder ();

			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				bool found = true;
				switch (name) {
				case "PORT" :
					port = value;
					break;
				case "DATA SOURCE" :
				case "SERVER" :
				case "HOST" :
					// set DataSource property
					host = value;
					break;
				case "OPTIONS" :
					options = value;
					break;
				case "TTY" :
					tty = value;
					break;		
				case "REQUIRESSL" :
					requiressl = value;
					break;
				case "ADDRESS" :
				case "ADDR" :
				case "NETWORK ADDRESS" :
				case "HOSTADDR" :
					hostaddr = value;
					break;
				case "INITIAL CATALOG" :
				case "DATABASE" :
				case "DBNAME":
					// set Database property
					dbname = value;
					break;
				case "PASSWORD" :
				case "PWD" :
					password = value;
					break;
				case "UID" :
				case "USER ID" :
				case "USER" :
					user = value;
					break;
				default:
					found = false;
					break;
				}
				if (found == true) {
					string valuePair = name.ToLower() + "=" + value;
					postgresConnectionString.Append (valuePair + " ");
				}
			}
			this.pgConnectionString = postgresConnectionString.ToString ();
		}

		private PgSqlTransaction TransactionBegin () {
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new PgSqlTransaction ();
			// using internal methods of SqlTransaction
			trans.SetConnection (this);
			trans.Begin();

			return trans;
		}

		private PgSqlTransaction TransactionBegin (IsolationLevel il) {
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new PgSqlTransaction ();
			// using internal methods of SqlTransaction
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

		// For System.Data.SqlClient classes
		// to get the current transaction
		// in progress - if any
		internal PgSqlTransaction Transaction {
			get {
				return trans;
			}
		}

		// For System.Data.SqlClient classes 
		// to get the unmanaged PostgreSQL connection
		internal IntPtr PostgresConnection {
			get {
				return pgConn;
			}
		}

		// For System.Data.SqlClient classes
		// to get the list PostgreSQL types
		// so can look up based on OID to
		// get the .NET System type.
		internal ArrayList Types {
			get {
				return types.List;
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
                
		public event 
		PgSqlInfoMessageEventHandler InfoMessage;

		public event 
		StateChangeEventHandler StateChange;
		
		#endregion
	
		#region Inner Classes

		private class PostgresTypes {
			// TODO: create hashtable for 
			// PostgreSQL types to .NET types
			// containing: oid, typname, SqlDbType

			private Hashtable hashTypes;
			private ArrayList pgTypes;
			private PgSqlConnection con;

			// Got this SQL with the permission from 
			// the authors of libgda
			private const string SEL_SQL_GetTypes = 
				"SELECT oid, typname FROM pg_type " +
				"WHERE typrelid = 0 AND typname !~ '^_' " +
				" AND  typname not in ('SET', 'cid', " +
				"'int2vector', 'oidvector', 'regproc', " +
				"'smgr', 'tid', 'unknown', 'xid') " +
				"ORDER BY typname";

			internal PostgresTypes(PgSqlConnection sqlcon) {
				
				con = sqlcon;
				hashTypes = new Hashtable();
			}

			private void AddPgType(Hashtable types, 
				string typname, DbType dbType) {

				PostgresType pgType = new PostgresType();
			
				pgType.typname = typname;
				pgType.dbType = dbType;	

				types.Add(pgType.typname, pgType);
			}

			private void BuildTypes(IntPtr pgResult, 
				int nRows, int nFields) {

				String value;

				int r;
				for(r = 0; r < nRows; r++) {
					PostgresType pgType = 
						new PostgresType();

					// get data value (oid)
					value = PostgresLibrary.
						PQgetvalue(
							pgResult,
							r, 0);
						
					pgType.oid = Int32.Parse(value);

					// get data value (typname)
					value = PostgresLibrary.
						PQgetvalue(
						pgResult,
						r, 1);	
					pgType.typname = String.Copy(value);
					pgType.dbType = PostgresHelper.
							TypnameToSqlDbType(
								pgType.typname);

					pgTypes.Add(pgType);
				}
				pgTypes = ArrayList.ReadOnly(pgTypes);
			}

			internal void Load() {
				pgTypes = new ArrayList();
				IntPtr pgResult = IntPtr.Zero; // PGresult
				
				if(con.State != ConnectionState.Open)
					throw new InvalidOperationException(
						"ConnnectionState is not Open");

				// FIXME: PQexec blocks 
				// while PQsendQuery is non-blocking
				// which is better to use?
				// int PQsendQuery(PGconn *conn,
				//        const char *query);

				// execute SQL command
				// uses internal property to get the PGConn IntPtr
				pgResult = PostgresLibrary.
					PQexec (con.PostgresConnection, SEL_SQL_GetTypes);

				if(pgResult.Equals(IntPtr.Zero)) {
					throw new PgSqlException(0, 0,
						"No Resultset from PostgreSQL", 0, "",
						con.DataSource, "SqlConnection", 0);
				}
				else {
					ExecStatusType execStatus;

					execStatus = PostgresLibrary.
						PQresultStatus (pgResult);
			
					if(execStatus == ExecStatusType.PGRES_TUPLES_OK) {
						int nRows;
						int nFields;

						nRows = PostgresLibrary.
							PQntuples(pgResult);

						nFields = PostgresLibrary.
							PQnfields(pgResult);

						BuildTypes (pgResult, nRows, nFields);

						// close result set
						PostgresLibrary.PQclear (pgResult);
						pgResult = IntPtr.Zero;
					}
					else {
						String errorMessage;
				
						errorMessage = PostgresLibrary.
							PQresStatus(execStatus);

						errorMessage += " " + PostgresLibrary.
							PQresultErrorMessage(pgResult);

						// close result set
						PostgresLibrary.PQclear (pgResult);
						pgResult = IntPtr.Zero;

						throw new PgSqlException(0, 0,
							errorMessage, 0, "",
							con.DataSource, "SqlConnection", 0);
					}
				}
			}

			public ArrayList List {
				get {
					return pgTypes;
				}
			}
		}

		#endregion
	}
}
