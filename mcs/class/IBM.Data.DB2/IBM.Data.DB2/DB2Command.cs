
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
using System;
using System.Data;
using System.Runtime.InteropServices;


namespace IBM.Data.DB2
{

	public class DB2Command : System.ComponentModel.Component, IDbCommand, ICloneable
	{
		#region Private data members
		
		private WeakReference refDataReader;
		private string commandText;
		private CommandType commandType = CommandType.Text;
		private DB2Connection db2Conn;
		private DB2Transaction db2Trans;
		private int commandTimeout = 30;
		private bool prepared = false;
		private bool binded = false;
		private IntPtr hwndStmt = IntPtr.Zero;  //Our statement handle
		private DB2ParameterCollection parameters = new DB2ParameterCollection();
		private bool disposed = false;
		private bool statementOpen;
		private CommandBehavior previousBehavior;
		private UpdateRowSource updatedRowSource = UpdateRowSource.Both;
		private IntPtr statementParametersMemory;
		private int statementParametersMemorySize;

		#endregion

		#region Constructors

		public DB2Command()
		{
			hwndStmt = IntPtr.Zero;
		}
		public DB2Command(string commandStr):this()
		{
			commandText = commandStr;
			
		}
		public DB2Command(string commandStr, DB2Connection con) : this()
		{
			db2Conn = con;
			commandText = commandStr;
			if(con != null)
			{
				con.AddCommand(this);
			}
		}
		public DB2Command (string commandStr, DB2Connection con, DB2Transaction trans)
		{
			commandText = commandStr;
			db2Conn = con;
			db2Trans = trans;
			if(con != null)
			{
				con.AddCommand(this);
			}
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
				if(statementParametersMemory != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(statementParametersMemory);
					statementParametersMemory = IntPtr.Zero;
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
			if((previousBehavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close();
			refDataReader = null;
		}

		private void CloseStatementHandle(bool dispose)
		{
			if(hwndStmt != IntPtr.Zero)
			{
				if(statementOpen)
				{
					short sqlRet = DB2CLIWrapper.SQLFreeStmt(hwndStmt, DB2Constants.SQL_CLOSE);
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
				if(hwndStmt != IntPtr.Zero)
					SetStatementTimeout();
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
		IDbTransaction IDbCommand.Transaction
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

		public DB2Transaction Transaction
		{
			get
			{
				return db2Trans;
			}
			set
			{
				db2Trans = value;
			}
		}
		#endregion

		#region UpdatedRowSource property

		public UpdateRowSource UpdatedRowSource	
		{
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
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
			sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_STMT, db2Conn.DBHandle, out hwndStmt);
			if ((sqlRet != DB2Constants.SQL_SUCCESS) && (sqlRet != DB2Constants.SQL_SUCCESS_WITH_INFO))
				throw new DB2Exception(DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, location +": Unable to allocate statement handle.");

			parameters.HwndStmt = hwndStmt;

			SetStatementTimeout();
		}

		private void SetStatementTimeout()
		{
			short sqlRet = DB2CLIWrapper.SQLSetStmtAttr(hwndStmt, DB2Constants.SQL_ATTR_QUERY_TIMEOUT, new IntPtr(commandTimeout), 0);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Set statement timeout.", db2Conn);
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
			return new DB2Parameter();
		}
		#endregion

		#region ExecuteNonQuery

		public int ExecuteNonQuery()
		{
			ExecuteNonQueryInternal(CommandBehavior.Default);

			int numRows;

			//How many rows affected.  numRows will be -1 if we aren't dealing with an Insert, Delete or Update, or if the statement did not execute successfully
			short sqlRet = DB2CLIWrapper.SQLRowCount(hwndStmt, out numRows);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecDirect error.", db2Conn);

			do
			{
				sqlRet = DB2CLIWrapper.SQLMoreResults(this.hwndStmt);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLMoreResults", db2Conn);
			} while(sqlRet != DB2Constants.SQL_NO_DATA_FOUND);

			CloseStatementHandle(false);
			
			return numRows;
		}

		public void ExecuteNonQueryInternal(CommandBehavior behavior)
		{
			short sqlRet;

			if(prepared && binded)
			{
				sqlRet = DB2CLIWrapper.SQLExecute(hwndStmt);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecute error.", db2Conn);
				return;
			}

			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");
			if((refDataReader != null) &&
				(refDataReader.IsAlive))
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
			DB2Transaction connectionTransaction = null;
			if(db2Conn.WeakRefTransaction != null)
				connectionTransaction = (DB2Transaction)db2Conn.WeakRefTransaction.Target;
			if(!Object.ReferenceEquals(connectionTransaction, Transaction))
			{
				if(Transaction == null)
					throw new InvalidOperationException("A transaction was started in the connection, but the command doesn't specify a transaction");
				throw new InvalidOperationException("The transaction specified at the connection doesn't belong to the connection");
			}

			if (hwndStmt == IntPtr.Zero)
			{
				AllocateStatement("InternalExecuteNonQuery");
				previousBehavior = 0;
			}
			if(previousBehavior != behavior)
			{
				if(((previousBehavior ^ behavior) & CommandBehavior.SchemaOnly) != 0)
				{
					sqlRet = DB2CLIWrapper.SQLSetStmtAttr(hwndStmt, DB2Constants.SQL_ATTR_DEFERRED_PREPARE, 
						new IntPtr((behavior & CommandBehavior.SchemaOnly) != 0 ? 0 : 1), 0);
					// TODO: don't check. what if it is not supported???
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Defered prepare.", db2Conn);

					previousBehavior = (previousBehavior & ~CommandBehavior.SchemaOnly) | (behavior & CommandBehavior.SchemaOnly);
				}
				if(((previousBehavior ^ behavior) & CommandBehavior.SingleRow) != 0)
				{
					sqlRet = DB2CLIWrapper.SQLSetStmtAttr(hwndStmt, DB2Constants.SQL_ATTR_MAX_ROWS, 
						new IntPtr((behavior & CommandBehavior.SingleRow) == 0 ? 0 : 1), 0);
					// TODO: don't check. what if it is not supported???
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Set max rows", db2Conn);

					previousBehavior = (previousBehavior & ~CommandBehavior.SingleRow) | (behavior & CommandBehavior.SingleRow);
				}
				previousBehavior = behavior;
			}
			if((Transaction == null) &&
				!db2Conn.openConnection.autoCommit)
			{
				sqlRet = DB2CLIWrapper.SQLSetConnectAttr(db2Conn.DBHandle, DB2Constants.SQL_ATTR_AUTOCOMMIT, new IntPtr(DB2Constants.SQL_AUTOCOMMIT_ON), 0);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting AUTOCOMMIT ON in transaction CTOR.", db2Conn);
				db2Conn.openConnection.autoCommit = true;

				sqlRet = DB2CLIWrapper.SQLSetConnectAttr(db2Conn.DBHandle, DB2Constants.SQL_ATTR_TXN_ISOLATION, new IntPtr(DB2Constants.SQL_TXN_READ_COMMITTED), 0);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, db2Conn.DBHandle, "Error setting isolation level.", db2Conn);
			}


			if ((commandText == null) ||(commandText.Length == 0))
				throw new InvalidOperationException("Command string is empty");
				
			if(CommandType.StoredProcedure == commandType && !commandText.StartsWith("CALL "))
				commandText = "CALL " + commandText + " ()";
			
			if((behavior & CommandBehavior.SchemaOnly) != 0)
			{
				if(!prepared)
				{
					Prepare();
				}
			}
			else
			{
				if(statementParametersMemory != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(statementParametersMemory);
					statementParametersMemory = IntPtr.Zero;
				}

				BindParams();
				
				if (prepared)
				{
					sqlRet = DB2CLIWrapper.SQLExecute(hwndStmt);
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecute error.", db2Conn);
				}
				else
				{
					sqlRet = DB2CLIWrapper.SQLExecDirect(hwndStmt, commandText, commandText.Length);
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecDirect error.", db2Conn);
				}
				statementOpen = true;

				parameters.GetOutValues();

				if(statementParametersMemory != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(statementParametersMemory);
					statementParametersMemory = IntPtr.Zero;
				}
			}
		}
		#endregion

		#region ExecuteReader calls
		///
		///ExecuteReader
		///
		IDataReader IDbCommand.ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
		{
			return ExecuteReader(behavior);
		}

		public DB2DataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		public DB2DataReader ExecuteReader(CommandBehavior behavior)
		{
			if((db2Conn == null) || (db2Conn.State != ConnectionState.Open))
				throw new InvalidOperationException("Prepare needs an open connection");

			DB2DataReader reader;
			
			ExecuteNonQueryInternal(behavior);
			reader = new DB2DataReader(db2Conn, this, behavior);
			
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
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLPrepare error.", db2Conn);
			
			
			statementOpen = true;
			prepared=true;
		}
		#endregion

		private string AddCallParam( string _cmString)
		{
			if(_cmString.IndexOf("()") != -1)
			{
				return _cmString.Replace("()","(?)");
			}
			return _cmString.Replace(")", ",?)");
		}

		private void BindParams()
		{
			if(parameters.Count > 0)
			{
				statementParametersMemorySize = 0;
				int offset = 0;
				short sqlRet;
				for(int i = 0; i < parameters.Count; i++) 
				{
					if(commandType == CommandType.StoredProcedure)
					{
						commandText = AddCallParam(commandText);
					}
					DB2Parameter param = parameters[i];
					param.CalculateRequiredmemory();
					statementParametersMemorySize += param.requiredMemory + 8;
					param.internalBuffer = Marshal.AllocHGlobal(param.requiredMemory);
					offset += param.requiredMemory;
					param.internalLengthBuffer = Marshal.AllocHGlobal(4);
					Marshal.WriteInt32(param.internalLengthBuffer, param.requiredMemory);
					sqlRet = param.Bind(this.hwndStmt, (short)(i + 1));
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Error binding parameter in DB2Command: ", db2Conn);
				}
				binded = true;
			}
		}

		#region ICloneable Members

		object ICloneable.Clone()
		{
			DB2Command clone = new DB2Command();

			clone.Connection = Connection;
			clone.commandText = commandText;
			clone.commandType = commandType;
			clone.Transaction = db2Trans;
			clone.commandTimeout = commandTimeout;
			clone.updatedRowSource = updatedRowSource;
			clone.parameters = new DB2ParameterCollection();
			for(int i = 0; i < parameters.Count; i++)
				clone.Parameters.Add(((ICloneable)parameters[i]).Clone());

			return clone;
		}

		#endregion
	}
}
