using System;
using System.Data;
using System.Runtime.InteropServices;


namespace System.Data.Db2Client
{

	public class Db2Command : IDbCommand
	{
		#region Private data members
		
		
		private string commandText;
		private CommandType commandType = CommandType.Text;
		private Db2Connection db2Conn;
		private Db2Transaction db2Trans;
		private int commandTimeout;
		private bool prepared = false;
		private IntPtr hwndStmt = IntPtr.Zero;  //Our statement handle
		private Db2ParameterCollection parameters = new Db2ParameterCollection();

		#endregion

		#region Constructors

		public Db2Command()
		{
			hwndStmt = IntPtr.Zero;
		}
		public Db2Command(string commandStr)
		{
			commandText = commandStr;
			hwndStmt = IntPtr.Zero;
			
		}
		public Db2Command(string commandStr, Db2Connection con) : this()
		{
			commandText = commandStr;
			db2Conn = con;
			AllocateStatement("Constructor 3");
		}
		public Db2Command (string commandStr, Db2Connection con, Db2Transaction trans)
		{
			commandText = commandStr;
			db2Conn = con;
			db2Trans = trans;
			AllocateStatement("Constructor 4");
		}
		///Db2 Specific constructors
		///
		public Db2Command (IntPtr hwndSt)
		{
			hwndStmt = hwndSt;
		}
		public void Dispose()
		{
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
				db2Conn = (Db2Connection)value;
				AllocateStatement("Connection property set");
			}
		}

		public Db2Connection Connection
		{
			get
			{
				return db2Conn;
			}
			set
			{
				db2Conn = value;
				AllocateStatement("Connection property set");
			}
		}
		#endregion
		#region Parameters property
		///
		/// Parameter list, Not yet implemented
		/// 
		public Db2ParameterCollection Parameters
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
				db2Trans = (Db2Transaction)value;
			}
		}
		#endregion

		#region UpdatedRowSource property
		///
		/// Need to see how this works with Db2...
		/// 
		public UpdateRowSource UpdatedRowSource
		{
			get
			{
				throw new Db2Exception ("TBD");
			}
			set
			{
				throw new Db2Exception ("TBD");
			}
		}
		#endregion

		#region Statement Handle
		///
		/// returns the Db2Client statement handle
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
			sqlRet = Db2CLIWrapper.SQLAllocHandle(Db2Constants.SQL_HANDLE_STMT, db2Conn.DBHandle , ref hwndStmt);
			if ((sqlRet != Db2Constants.SQL_SUCCESS) && (sqlRet != Db2Constants.SQL_SUCCESS_WITH_INFO))
				throw new Db2Exception(Db2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, location +": Unable to allocate statement handle.");
			parameters.HwndStmt = hwndStmt;
		}
		#endregion

		#region Cancel
		/// <summary>
		/// Attempt to cancel an executing command
		/// </summary>
		public void Cancel()
		{
			Db2CLIWrapper.SQLCancel(hwndStmt);
		}
		#endregion

		#region CreateParameter
		///
		///Returns a parameter
		///
		public IDbDataParameter CreateParameter()
		{
			throw new Db2Exception("TBD");
		}
		#endregion

		#region ExecuteNonQuery

		public int ExecuteNonQuery()
		{
			if ((commandText == null) ||(commandText.Length == 0))
				throw new Db2Exception("Command string is empty");
				
			if(CommandType.StoredProcedure == commandType && !commandText.StartsWith("CALL "))
				commandText = "CALL " + commandText;
				
			Prepare();

			short sqlRet;
			if (prepared)
				sqlRet = Db2CLIWrapper.SQLExecute(hwndStmt);
			else
				sqlRet = Db2CLIWrapper.SQLExecDirect(hwndStmt, commandText, commandText.Length);
			
			int numRows = 0;
			sqlRet = Db2CLIWrapper.SQLRowCount(hwndStmt, ref numRows);   //How many rows affected.  numRows will be -1 if we aren't dealing with an Insert, Delete or Update, or if the statement did not execute successfully
			
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
			return ExecuteReader();
		}

		public Db2DataReader ExecuteReader()
		{
			Db2DataReader reader;

			
			if (!prepared) 
			{
				ExecuteNonQuery();
				reader = new Db2DataReader(db2Conn, this);
			}
			else
				reader = new Db2DataReader(db2Conn, this, true);
			
			return reader;
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			//throw new Db2Exception("TBD");
			return ExecuteReader();
		}
		#endregion

		#region ExecuteScalar
		///
		/// ExecuteScalar
		/// 
		public object ExecuteScalar()
		{
			throw new Db2Exception("TBD");
		}
		#endregion

		#region Prepare ()
		///
		/// Prepare a statement against the database
		/// 
		public void Prepare ()
		{
		
			short sqlRet = 0;

			IntPtr numParams = IntPtr.Zero;
			sqlRet = Db2CLIWrapper.SQLPrepare(hwndStmt, commandText, commandText.Length);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLPrepare error.");
			short i=1;
			foreach ( Db2Parameter param in parameters) 
			{
				
				if (selfDescribe) 
				{
					sqlRet = param.Describe(this.hwndStmt, i);
					Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in Db2Command: ");
				}
				sqlRet = param.Bind(this.hwndStmt, i);
				Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in Db2Command: ");
				i++;
			}
			prepared=true;
		}
		#endregion
	}
}
