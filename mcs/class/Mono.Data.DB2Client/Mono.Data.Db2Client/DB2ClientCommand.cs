#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;
using System.Runtime.InteropServices;


namespace DB2ClientCS
{
	/// <summary>
	/// Summary description for DB2ClientCommand.
	/// </summary>
	public class DB2ClientCommand : IDbCommand
	{
		#region Private data members
		private string commandText;
		private DB2ClientConnection db2Conn;
		private DB2ClientTransaction db2Trans;
		private int commandTimeout;
		private bool prepared = false;
		private IntPtr hwndStmt;  //Our statement handle
		private DB2ClientParameterCollection parameters = new DB2ClientParameterCollection();

		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor.  Since I'm using CLI functions to do this stuff, we're stuck
		/// until we get the database environment handle.
		/// </summary>
		public DB2ClientCommand()
		{
			hwndStmt = IntPtr.Zero;
		}
		public DB2ClientCommand(string commandStr)
		{
			commandText = commandStr;
			hwndStmt = IntPtr.Zero;
		}
		public DB2ClientCommand(string commandStr, DB2ClientConnection con)
		{
			commandText = commandStr;
			db2Conn = con;
			AllocateStatement("Constructor 3");
		}
		public DB2ClientCommand (string commandStr, DB2ClientConnection con, DB2ClientTransaction trans)
		{
			commandText = commandStr;
			db2Conn = con;
			db2Trans = trans;
			AllocateStatement("Constructor 4");
		}
		///DB2 Specific constructors
		///
		public DB2ClientCommand (IntPtr hwndSt)
		{
			hwndStmt = hwndSt;
		}
		public void Dispose()
		{
		}
		#endregion
		#region CommandText property
		///
		///The query;  If it gets set, reset the prepared property
		///
		public string CommandText
		{
			get
			{
				return commandText;
			}
			set
			{
				prepared = false;
				commandText = value;
			}
		}
		#endregion
		#region CommandTimeout property
		///
		/// The Timeout property states how long we wait for results to return
		/// 
		public int CommandTimeout
		{
			get
			{
				return commandTimeout;
			}
			set 
			{
				commandTimeout = value;
			}
		}
		#endregion
		#region CommandType property
		///
		/// I believe this one is left as text all of the time for DB2, but I have to check that...
		/// 
		public CommandType CommandType
		{
			get
			{
				return CommandType.Text;
			}
			set
			{
				///Do nothing
			}
		}
		#endregion
		#region Connection property
		///
		///  The connection we'll be executing on.
		///  
		public IDbConnection Connection
		{
			get
			{
				return db2Conn;
			}
			set
			{
				db2Conn = (DB2ClientConnection)value;
			}
		}
		#endregion
		#region Parameters property
		///
		/// Parameter list, Not yet implemented
		/// 
		public DB2ClientParameterCollection Parameters
		{
			get
			{
				return parameters;
			}
		}
		IDataParameterCollection IDbCommand.Parameters
		{
			get
			{
				return parameters;
			}
		}
		#endregion
		#region Transaction property
		///
		/// The transaction this command is associated with
		/// 
		public IDbTransaction Transaction
		{
			get
			{
				return db2Trans;
			}
			set
			{
				db2Trans = (DB2ClientTransaction)value;
			}
		}
		#endregion
		#region UpdatedRowSource property
		///
		/// Need to see how this works with DB2...
		/// 
		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				throw new DB2ClientException ("TBD");
			}
			set
			{
				throw new DB2ClientException ("TBD");
			}
		}
		#endregion
		#region Statement Handle
		///
		/// returns the DB2Client statement handle
		/// 
		public IntPtr statementHandle
		{
			get
			{
				return hwndStmt;
			}
		}
		#endregion
		#region AllocateStatement function
		///
		/// Allocate a statement handle, internal.  Pass in the name of the caller for exception info.
		/// I think I'll make the handle a property and add a constructor with the handle argument so that 
		/// statements can be executed on the same handle if need be, though you could accomplish the same by 
		/// just keeping the command object open. 
		/// 
		internal void AllocateStatement(string location)
		{
			short sqlRet;
			sqlRet = DB2ClientPrototypes.SQLAllocHandle(DB2ClientConstants.SQL_HANDLE_STMT, db2Conn.DBHandle, ref hwndStmt);
			if (sqlRet == DB2ClientConstants.SQL_ERROR)
				throw new DB2ClientException(DB2ClientConstants.SQL_HANDLE_DBC, db2Conn.DBHandle, location +": Unable to allocate statement handle.");
		}
		#endregion
		#region Cancel
		/// <summary>
		/// Attempt to cancel an executing command
		/// </summary>
		public void Cancel()
		{
			DB2ClientPrototypes.SQLCancel(hwndStmt);
		}
		#endregion
		#region CreateParameter
		///
		///Returns a parameter
		///
		public IDbDataParameter CreateParameter()
		{
			throw new DB2ClientException("TBD");
		}
		#endregion
		#region ExecuteNonQuery
		///
		/// ExecuteNonQuery  Executes an SQL statement without returning a DataSet
		/// 
		public int ExecuteNonQuery()
		{
			short sqlRet;
			if (prepared)
				sqlRet = DB2ClientPrototypes.SQLExecute(hwndStmt);
			else
				sqlRet = DB2ClientPrototypes.SQLExecDirect(hwndStmt, commandText, commandText.Length);
			
			int numRows = 0;
			sqlRet = DB2ClientPrototypes.SQLRowCount(hwndStmt, ref numRows);   //How many rows affected.  numRows will be -1 if we aren't dealing with an Insert, Delete or Update, or if the statement did not execute successfully
			///At this point, I think we need to save any results, but not return them
			///For now, we will go execute and return the number of rows affected
			return numRows;
		}
		#endregion
		#region ExecuteReader calls
		///
		///ExecuteReader
		///
		public IDataReader ExecuteReader()
		{
			DB2ClientDataReader reader;

			if (!prepared) 
			{
				ExecuteNonQuery();
				reader = new DB2ClientDataReader(db2Conn, this);
			}
			else
				reader = new DB2ClientDataReader(db2Conn, this, prepared);

			return reader;

		}
		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			throw new DB2ClientException("TBD");
		}
		#endregion
		#region ExecuteScalar
		///
		/// ExecuteScalar
		/// 
		public object ExecuteScalar()
		{
			throw new DB2ClientException("TBD");
		}
		#endregion

		#region Prepare
		///
		/// Prepare.  
		/// 
		public void Prepare()
		{
			DB2ClientUtils util = new DB2ClientUtils();
			short sqlRet = 0;

			IntPtr numParams = IntPtr.Zero;
			sqlRet = DB2ClientPrototypes.SQLPrepare(hwndStmt, commandText, commandText.Length);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "SQLPrepare error.");
			short i=1;
			foreach ( DB2ClientParameter param in parameters) 
			{
				sqlRet = param.Bind(this.hwndStmt, i);
				util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in DB2ClientCommand: ");
				i++;
			}
			prepared=true;
		}
		#endregion
	}
}