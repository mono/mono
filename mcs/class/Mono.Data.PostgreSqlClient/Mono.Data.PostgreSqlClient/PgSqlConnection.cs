//
// System.Data.SqlClient.SqlConnection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.SqlClient
{
	using PGconn = IntPtr; 
	// PGconn is native C library type in libpq for Postgres Connection

	// using PGressult = IntPtr;
	// PGresult is native C library type in libpq for Postgres Resultset

	/// <summary>
	/// Represents an open connection to a SQL data source
	/// </summary>
	//public sealed class SqlConnection : Component, IDbConnection,
	//	ICloneable
	public sealed class SqlConnection : IDbConnection
	{
		#region Fields

		private PGconn pgConn = IntPtr.Zero;    
		// PGConn (Postgres Connection)
		private string connectionString = "";    
		// OLE DB Connection String
		private string pgConnectionString = ""; 
		// PostgreSQL Connection String
		private SqlTransaction trans = null;
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

		ConnectionState conState = ConnectionState.Closed;

		#endregion // Fields

		#region Constructors

		/*
		[MonoTODO]
		public SqlConnection () 
		{
			this.ConnectionString = null;
			this.ConnectionTimeout = 0;
			this.Database = null;
			this.State = 0;
		}
		
		[MonoTODO]
		public SqlConnection (string cs) : SqlConnection () 
		{
			this.ConnectionString = cs;
		}
		
		*/
		// A lot of the defaults were initialized in the Fields
		[MonoTODO]
		public SqlConnection ()
		{

		}
	
		[MonoTODO]
		public SqlConnection (String connectionString)
		{
			SetConnectionString (connectionString);
			OpenDataSource ();
		}

		#endregion

		#region Deconstructors

		// aka Finalize

		[MonoTODO]
		/*
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlConnection()
		{

		}
		*/
		#endregion // Deconstructors

		#region Public Methods

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		public SqlTransaction BeginTransaction ()
		{
			return TransactionBegin (); // call private method
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel 
						il)
		{
			return BeginTransaction (il);
		}

		public SqlTransaction BeginTransaction (IsolationLevel il)
		{
			return TransactionBegin (il); // call private method
		}

		[MonoTODO]
		public void ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}
				
		[MonoTODO]
		public void Close ()
		{
			CloseDataSource ();
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		public SqlCommand CreateCommand ()
		{
			SqlCommand sqlcmd = new SqlCommand ("", this);

			return sqlcmd;
		}

		[MonoTODO]
		public void Open ()
		{
			OpenDataSource ();
		}

		[MonoTODO]
		public void Dispose ()
		{	
			// FIXME: quick hack
			Close ();
			// Dispose (true);
		}
	
		#endregion

		#region Internal Methods

		// this is for System.Data.SqlClient classes
		// to get the Postgres connection
		internal IntPtr PostgresConnection {
			get {
				return pgConn;
			}
		}

		#endregion // Internal Methods

		#region Protected Methods

		// FIXME: protected override void Dispose overrides Component
		//        however, including Component causes other problems
		/*
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		*/

		#endregion

		#region Private Methods

		private void OpenDataSource ()
		{
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
			if(connStatus == ConnStatusType.CONNECTION_OK)
			{
				// Successfully Connected
				conState = ConnectionState.Open;
				Console.WriteLine ("*** Connected " +
						"Successfully");
			}
			else
			{
				// Unsuccessfull at Connecting
				Console.WriteLine ("*** Connected " +
						"Unsuccessfully");

				// PGconn *
				// PQconnectStart (const char *conninfo)
				//
				// PostgresPollingStatusType 
				// PQconnectPoll (PGconn *conn)

				String errorMessage = PostgresLibrary.
					PQerrorMessage (pgConn);
				Console.WriteLine ("Error: " + errorMessage);

				// FIXME: do error checking, 
				// if could not connect, 
				// throw an exception:
				//  InvalidOperationException - attempt 
				//    to connect to without specifying 
				//    database or server
				//  SqlException - error while 
				//    connecting to database
				
			}
			
		}

		private void CloseDataSource ()
		{
			// FIXME: just a quick hack
			conState = ConnectionState.Closed;
			PostgresLibrary.PQfinish (pgConn);
		}

		private void SetConnectionString (string connectionString)
		{
			// FIXME: perform error checking on string
			// while translating string from 
			// OLE DB format to PostgreSQL 
			// connection string format
			//
			//     OLE DB: "host=localhost;dbname=test;user=joe;password=smoe"
			// PostgreSQL: "host=localhost dbname=test user=joe password=smoe"
			//
			// For OLE DB, you would have the additional 
			// "provider=postgresql"
			// OleDbConnection you would be using libgda, maybe
			// it would be 
			// "provider=OAFIID:GNOME_Database_Postgres_Provider"
			// instead.
			//
			// Also, parse the connection string into properties

			// FIXME: if connection is open, you can 
			//        not set the connection
			//        string, throw an exception

			this.connectionString = connectionString;
			pgConnectionString = ConvertStringToPostgres (
				connectionString);
		}

		private String ConvertStringToPostgres (String 
			oleDbConnectionString)
		{
			StringBuilder postgresConnection = new StringBuilder();
			string result;
			string[] connectionParameters;

			char[] semicolon = new Char[1];
			semicolon[0] = ';';
			
			// FIXME: what is the max number of value pairs 
			//        can there be for the OLE DB 
			//	  connnection string? what about libgda max?  
			//        what about postgres max?

			// FIXME: currently assuming value pairs are like:
			//        "keyword1=value1;keyword2=value2;keyword3=value3"
			//        Need to deal with values that have
			//        single or double quotes.  And error 
			//        handling of that too.
			//        "keyword1=value1;keyword2='value2';keyword3=\"value3\""

			connectionParameters = oleDbConnectionString.
				Split (semicolon);
			foreach (string sParameter in connectionParameters) {
				if(sParameter.Length > 0) {
					BreakConnectionString (sParameter);
					postgresConnection.Append (sParameter + 
						" ");
				}
			}
			result = postgresConnection.ToString ();
			return result;
		}

		private bool BreakConnectionString (String sParameter)
		{	
			bool addParm = true;
			int index;

			index = sParameter.IndexOf ("=");
			if (index > 0) {	
				string parmKey, parmValue;

				// separate string "key=value" to 
				// string "key" and "value"
				parmKey = sParameter.Substring (0, index);
				parmValue = sParameter.Substring (index + 1, 
					sParameter.Length - index - 1);

				switch(parmKey.ToLower()) {
				case "host":
					// set DataSource property
					host = parmValue;
					break;

				case "dbname":
				// set Database property
					dbname = parmValue;
					break;

				case "user":
					user = parmValue;
					break;

				case "password":
					password = parmValue;
					addParm = false;
					break;

				case "options":
					options = parmValue;
					break;

				case "tty":
					tty = parmValue;
					break;
							
				case "requiressl":
					requiressl = parmValue;
					break;
				}
			}
			return addParm;
		}

		private SqlTransaction TransactionBegin ()
		{
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new SqlTransaction ();
			// using internal methods of SqlTransaction
			trans.SetConnection (this);
			return trans;
		}

		private SqlTransaction TransactionBegin (IsolationLevel il)
		{
			// FIXME: need to keep track of 
			// transaction in-progress
			trans = new SqlTransaction ();
			// using internal methods of SqlTransaction
			trans.SetConnection (this);
			trans.SetIsolationLevel (il);
			
			return trans;
		}

		#endregion

		#region Properties

		[MonoTODO]
		public ConnectionState State 		{
			get { 
				throw new NotImplementedException (); 
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

		/*
		 * FIXME: this is here because of Component?
		[MonoTODO]
		protected bool DesignMode {
			get { 
				throw new NotImplementedException (); 
			}
		}
		*/
		public int PacketSize {
			get { 
				throw new NotImplementedException ();
			}
		}

		public string ServerVersion {
			get { 
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region Events and Delegates
                
		// FIXME: the two events belong here
		// however, i do not know about the delegates
		// also, they are stubs for now
		/*
		public delegate void 
		SqlInfoMessageEventHandler (object sender,	
				SqlInfoMessageEventArgs e);

		public event 
		SqlInfoMessageEventHandler InfoMessage;

		public event 
		StateChangeEventHandler StateChange;
		*/

		#endregion
	}
}
