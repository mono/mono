using System;
using System.Data;
using System.Runtime.InteropServices;


namespace IBM.Data.DB2
{

	public class DB2Command : System.ComponentModel.Component,IDbCommand
	{
		#region Private data members
		
		
		private WeakReference refDataReader;
		private string commandText;
		private CommandType commandType = CommandType.Text;
		private DB2Connection db2Conn;
		private DB2Transaction db2Trans;
		private int commandTimeout;
		private bool prepared = false;
		private IntPtr hwndStmt = IntPtr.Zero;  //Our statement handle
		private DB2ParameterCollection parameters = new DB2ParameterCollection();
		private bool disposed = false;
		private bool statementOpen;

		#endregion

		#region Constructors

		public DB2Command()
		{
			commandTimeout = 30;
			hwndStmt = IntPtr.Zero;
		}
		public DB2Command(string commandStr):this()
		{
			commandText = commandStr;
			
		}
		public DB2Command(string commandStr, DB2Connection con) : this(commandStr)
		{
			db2Conn = con;
			AllocateStatement("Constructor 3");
		}
		public DB2Command (string commandStr, DB2Connection con, DB2Transaction trans):this(commandStr, con)
		{
			db2Trans = trans;
			if(null != con)
			{
				con.AddCommand(this);
			}
			AllocateStatement("Constructor 4");
		}
		#endregion

		#region Dispose
		public new void  Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			if(!disposed) 
			{
				if(disposing)
				{
					ConnectionClosed();
					if(db2Conn != null)
					{
						db2Conn.RemoveCommand(this);
						db2Conn = null;
					}
				}
			}
			base.Dispose(disposing);
			disposed = true;
		}


		~DB2Command()
		{
			Dispose(false);
		}

		internal void DataReaderClosed()
		{
			CloseStatementHandle(false);
			refDataReader = null;
		}

		private void CloseStatementHandle(bool dispose)
		{
			if(hwndStmt != IntPtr.Zero)
			{
				if(statementOpen)
				{
					short sqlRet = DB2CLIWrapper.SQLFreeStmt(hwndStmt, DB2Constants.SQL_CLOSE);
					Db2Environment.Log("Close stm {0,-4}  {1}", hwndStmt, sqlRet);
				}
				if((!prepared && statementOpen) ||
					dispose)
				{
					short sqlRet = DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_STMT, hwndStmt);

					hwndStmt = IntPtr.Zero;
					prepared = false;
				}
				statementOpen = false;
			}
		}

		internal void ConnectionClosed()
		{
			DB2DataReader reader = null;
			if((refDataReader != null) && refDataReader.IsAlive)
			{
				reader = (DB2DataReader)refDataReader.Target;
			}
			if((reader != null) && refDataReader.IsAlive)
			{
				reader.Dispose();
				refDataReader = null;
			}
			CloseStatementHandle(true);

			db2Trans = null;
		}

		#endregion

		#region SelfDescribe property
		///
		/// Property dictates whether or not any paramter markers will get their describe info
		/// from the database, or if the user will supply the information
		/// 
		bool selfDescribe = false;
		public bool SelfDescribe
		{
			get 
			{
				return selfDescribe;
			}
			set 
			{
				selfDescribe = value;
			}
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

		public CommandType CommandType
		{
			get
			{
				return commandType;
			}
			set
			{
				commandType = value;
			}
		}
		#endregion

		#region Connection property
		///
		///  The connection we'll be executing on.
		///  
		IDbConnection IDbCommand.Connection
		{
			get
			{
				return db2Conn;
			}
			set
			{
				db2Conn = (DB2Connection)value;
				//AllocateStatement("Connection property set");
			}
		}

		public DB2Connection Connection
		{
			get
			{
				return db2Conn;
			}
			set
			{
				if(db2Conn != null)
				{
					db2Conn.RemoveCommand(this);
				}
				db2Conn = value;
				if(db2Conn != null)
				{
					db2Conn.AddCommand(this);
				}
			}
		}
		#endregion

		#region Parameters property
		///
		/// Parameter list, Not yet implemented
		/// 
		public DB2ParameterCollection Parameters
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
				db2Trans = (DB2Transaction)value;
			}
		}
		#endregion

		#region UpdatedRowSource property

		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				throw new DB2Exception ("TBD");
			}
			set
			{
				throw new DB2Exception ("TBD");
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

		internal void AllocateStatement(string location)
		{
			if (db2Conn.DBHandle.ToInt32() == 0) return;
			short sqlRet;
			sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_STMT, db2Conn.DBHandle , ref hwndStmt);
			Console.WriteLine("Connection handle : {0}", db2Conn.DBHandle.ToInt32().ToString());
			Db2Environment.Log("Alloc stm {0,-4}  {1}", hwndStmt, sqlRet);
			if ((sqlRet != DB2Constants.SQL_SUCCESS) && (sqlRet != DB2Constants.SQL_SUCCESS_WITH_INFO))
				throw new DB2Exception(DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, location +": Unable to allocate statement handle.");
			parameters.HwndStmt = hwndStmt;
		}
		#endregion

		#region Cancel
		/// <summary>
		/// Attempt to cancel an executing command
		/// </summary>
		public void Cancel()
		{
			if(hwndStmt == IntPtr.Zero)
			{
				throw new InvalidOperationException("Nothing to Cancel.");
			}
			DB2CLIWrapper.SQLCancel(hwndStmt);
		}
		#endregion

		#region CreateParameter
		///
		///Returns a parameter
		///
		public IDbDataParameter CreateParameter()
		{
			throw new DB2Exception("TBD");
		}
		#endregion

		#region ExecuteNonQuery

		public int ExecuteNonQuery()
		{
			int result = ExecuteNonQueryInternal();
			CloseStatementHandle(false);
			
			return result;
		}

		public int ExecuteNonQueryInternal()
		{
			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");

			if (hwndStmt == IntPtr.Zero)
			{
				AllocateStatement("InternalExecuteNonQuery");
			}
			if ((commandText == null) ||(commandText.Length == 0))
				throw new InvalidOperationException("Command string is empty");
				
			if(CommandType.StoredProcedure == commandType && !commandText.StartsWith("CALL "))
				commandText = "CALL " + commandText;
				
			short sqlRet;
			if (prepared)
			{
				sqlRet = DB2CLIWrapper.SQLExecute(hwndStmt);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecute error.");
			}
			else
			{
				sqlRet = DB2CLIWrapper.SQLExecDirect(hwndStmt, commandText, commandText.Length);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecDirect error.");
			}
			statementOpen = true;
			
			int numRows = 0;
			sqlRet = DB2CLIWrapper.SQLRowCount(hwndStmt, ref numRows);   //How many rows affected.  numRows will be -1 if we aren't dealing with an Insert, Delete or Update, or if the statement did not execute successfully
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecDirect error.");
			

			parameters.GetOutValues();
			///At this point, I think we need to save any results, but not return them
			///For now, we will go execute and return the number of rows affected
			return numRows;
		}
		#endregion

		#region ExecuteReader calls
		///
		///ExecuteReader
		///
		IDataReader IDbCommand.ExecuteReader()
		{
			return ExecuteReader(0);
		}

		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}

		public DB2DataReader ExecuteReader()
		{
			return ExecuteReader(0);
		}

		public DB2DataReader ExecuteReader(CommandBehavior behavior)
		{
			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");

			DB2DataReader reader;
			
			int nrOfRows = ExecuteNonQueryInternal();
			
			reader = new DB2DataReader(db2Conn, this);
			reader.recordsAffected = nrOfRows;
			
			refDataReader = new WeakReference(reader);

			return reader;
		}
		#endregion

		#region ExecuteScalar
		///
		/// ExecuteScalar
		/// 
		public object ExecuteScalar()
		{
			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");

			using(DB2DataReader reader = ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow))
			{
				if(reader.Read() && (reader.FieldCount > 0))
				{
					return reader[0];
				}
			}
			return null;
		}
		#endregion

		#region Prepare ()

		public void Prepare ()
		{
			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");

			CloseStatementHandle(false);
			if (hwndStmt == IntPtr.Zero)
			{
				AllocateStatement("InternalExecuteNonQuery");
			}

			short sqlRet = 0;

			IntPtr numParams = IntPtr.Zero;
			sqlRet = DB2CLIWrapper.SQLPrepare(hwndStmt, commandText, commandText.Length);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLPrepare error.");

			statementOpen = true;

			short i=1;
			foreach ( DB2Parameter param in parameters) 
			{
				
				if (selfDescribe) 
				{
					sqlRet = param.Describe(this.hwndStmt, i);
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in Db2Command: ");
				}
				sqlRet = param.Bind(this.hwndStmt, i);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in Db2Command: ");
				i++;
			}
			prepared=true;
		}
		#endregion
	}
}
