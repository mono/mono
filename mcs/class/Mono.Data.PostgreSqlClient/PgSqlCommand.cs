//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rdorigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Xml;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a SQL statement that is executed 
	/// while connected to a SQL database.
	/// </summary>
	// public sealed class SqlCommand : Component, IDbCommand, ICloneable
	public sealed class SqlCommand : IDbCommand
	{
		// FIXME: Console.WriteLine() is used for debugging throughout

		#region Fields

		string sql = "";
		int timeout = 30; 
		// default is 30 seconds 
		// for command execution

		SqlConnection conn = null;
		SqlTransaction trans = null;
		CommandType cmdType = CommandType.Text;
		bool designTime = false;
		SqlParameterCollection parmCollection = new 
			SqlParameterCollection();

		#endregion // Fields

		#region Constructors

		public SqlCommand()
		{
			sql = "";
		}

		public SqlCommand (string cmdText)
		{
			sql = cmdText;
		}

		public SqlCommand (string cmdText, SqlConnection connection)
		{
			sql = cmdText;
			conn = connection;
		}

		public SqlCommand (string cmdText, SqlConnection connection, 
						SqlTransaction transaction)
		{
			sql = cmdText;
			conn = connection;
			trans = transaction;
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		// FIXME: is this the correct way to return a stronger type?
		[MonoTODO]
		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		[MonoTODO]
		public SqlParameter CreateParameter ()
		{
			return new SqlParameter ();
		}

		[MonoTODO]
		public int ExecuteNonQuery ()
		{	
			IntPtr pgResult; // PGresult
			int rowsAffected = -1;
			ExecStatusType execStatus;
			String rowsAffectedString;

			// FIXME: throw an 
			// InvalidOperationException
			// exception if the the connection
			// does not exist or is not open

			// FIXME: PQexec blocks 
			// while PQsendQuery is non-blocking
			// which is better to use?
			// int PQsendQuery(PGconn *conn,
			//        const char *query);

			// execute SQL command
			// uses internal property to get the PGConn IntPtr
			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, sql);

			/* FIXME: throw an SqlException exception
			 * if there is a SQL Error
			 */

			/*
			 * FIXME: get status
			 */
                        execStatus = PostgresLibrary.
					PQresultStatus (pgResult);
			
			if(execStatus == ExecStatusType.PGRES_COMMAND_OK)
			{
				Console.WriteLine("*** SqlCommand Execute " +
					"got PGRES_COMMAND_OK");
				rowsAffectedString = PostgresLibrary.
					PQcmdTuples (pgResult);
				Console.WriteLine("*** Rows Affected: " + 
					rowsAffectedString);
				// FIXME: convert string to number
			}
			else
			{
				Console.WriteLine("*** Error: SqlCommand " +
					"did not get PGRES_COMMAND_OK");
				String statusString;
				
				statusString = PostgresLibrary.
					PQresStatus(execStatus);
				Console.WriteLine("*** Command Status: " +
					statusString);

				String errorMessage;
				errorMessage = PostgresLibrary.
					PQresultErrorMessage(pgResult);

				Console.WriteLine("*** Error message: " +
					statusString);				
			}
			
			String cmdStatus;
			cmdStatus = PostgresLibrary.
				PQcmdStatus(pgResult);

			Console.WriteLine("*** Command Status: " +
				cmdStatus);

			PostgresLibrary.PQclear (pgResult);
			
			// FIXME: get number of rows
			// affected for INSERT, UPDATE, or DELETE
			// any other, return -1 (such as, CREATE TABLE)
			return rowsAffected;
		}
		
		// FIXME: temporarily commmented out, so I could get a simple working
		//        SqlConnection and SqlCommand.  I had to temporarily
		//        comment it out the ExecuteReader in IDbCommand as well.
		/*
		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		SqlDataReader ExecuteReader ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader (
					CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}
		*/

		[MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader ExecuteXmlReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand Clone ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Properties

		public string CommandText {
			get { 
				return sql; 
			}

			set { 
				sql = value; 
			}
		}

		public int CommandTimeout {
			get { 
				return timeout;  
			}
			
			set {
				// FIXME: if value < 0, throw
				// ArgumentException
				// if (value < 0)
				//	throw ArgumentException;
				timeout = value;
			}
		}

		public CommandType CommandType	{
			get {
				return cmdType;
			}

			set { 
				cmdType = value;
			}
		}

		// FIXME: for property Connection, is this the correct
		//        way to handle a return of a stronger type?
		IDbConnection IDbCommand.Connection {
			get { 
				return Connection;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during a 
				// transaction in progress
				Connection = (SqlConnection) value; 
				// FIXME: set Transaction property to null
			}
		}
		
		public SqlConnection Connection {
			get { 
				// conn defaults to null
				return conn;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during 
				// a transaction in progress
				conn = value; 
				// FIXME: set Transaction property to null
			}
		}

		public bool DesignTimeVisible {
			get {
				return designTime;
			} 
			
			set{
				designTime = value;
			}
		}

		// FIXME; for property Parameters, is this the correct
		//        way to handle a stronger return type?
		IDataParameterCollection IDbCommand.Parameters	{
			get { 
				return Parameters;
			}
		}

		SqlParameterCollection Parameters {
			get { 
				return parmCollection;
			}
		}

		// FIXME: for property Transaction, is this the correct
		//        way to handle a return of a stronger type?
		IDbTransaction IDbCommand.Transaction 	{
			get { 
				return Transaction;
			}

			set { 
				// FIXME: error handling
				Transaction = (SqlTransaction) value; 
			}
		}

		public SqlTransaction Transaction {
			get { 
				return trans; 
			}

			set { 
				// FIXME: error handling
				trans = value; 
			}
		}	

		[MonoTODO]
		public UpdateRowSource UpdatedRowSource	{
			// FIXME: do this once DbDataAdaptor 
			// and DataRow are done
			get { 		
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		#endregion // Properties

		#region Desctructors
/*		
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlCommand()
		{
			FIXME: need proper way to release resources
		}
*/
		#endregion //Destructors
	}
}
