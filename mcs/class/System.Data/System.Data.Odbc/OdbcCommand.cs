//
// System.Data.Odbc.OdbcCommand
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
	/// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	public sealed class OdbcCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		string commandText;
		int timeout;
		CommandType commandType;
		OdbcConnection connection;
		OdbcParameterCollection parameters;
		OdbcTransaction transaction;
		bool designTimeVisible;
		bool prepared=false;
		OdbcDataReader dataReader;
		CommandBehavior behavior;
		internal IntPtr hstmt;
		
		#endregion // Fields

		#region Constructors

		public OdbcCommand ()
	        {
			commandText = String.Empty;
			timeout = 30; // default timeout 
			commandType = CommandType.Text;
			connection = null;
			parameters = new OdbcParameterCollection ();
			transaction = null;
			designTimeVisible = false;
			dataReader = null;
			behavior = CommandBehavior.Default;
		}

		public OdbcCommand (string cmdText) : this ()
		{
			CommandText = cmdText;
		}

		public OdbcCommand (string cmdText, OdbcConnection connection)
			: this (cmdText)
		{
			Connection = connection;
		}

		public OdbcCommand (string cmdText,
				     OdbcConnection connection,
				     OdbcTransaction transaction) : this (cmdText, connection)
		{
			this.transaction = transaction;
		}

		#endregion // Constructors

		#region Properties

		internal IntPtr hStmt
		{
			get { return hstmt; }
		}

		public string CommandText 
		{
			get {
				return commandText;
			}
			set { 
				prepared=false;
				commandText = value;
			}
		}

		public int CommandTimeout {
			get {
				return timeout;
			}
			set {
				timeout = value;
			}
		}

		public CommandType CommandType { 
			get {
				return commandType;
			}
			set {
				commandType = value;
			}
		}

		public OdbcConnection Connection { 
			get {
				return connection;
			}
			set {
				connection = value;
			}
		}

		public bool DesignTimeVisible { 
			get {
				return designTimeVisible;
			}
			set {
				designTimeVisible = value;
			}
		}

		public OdbcParameterCollection Parameters {
			get {
				return parameters;
			}
			set {
				parameters = value;
			}
		}

		public OdbcTransaction Transaction {
			get {
				return transaction;
			}
			set {
				transaction = value;
			}
		}

		public UpdateRowSource UpdatedRowSource { 
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		IDbConnection IDbCommand.Connection {
			get {
				return Connection;
			}
			set {
				Connection = (OdbcConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters  {
			get {
				return Parameters;
			}
		}

		IDbTransaction IDbCommand.Transaction  {
			get {
				return (IDbTransaction) Transaction;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		#region Methods

		public void Cancel () 
		{
			if (hstmt!=IntPtr.Zero)
			{
				OdbcReturn ret=libodbc.SQLCancel(hstmt);
				libodbchelper.DisplayError("SQLCancel",ret);
			}
			else
				throw new InvalidOperationException();
		}

		public OdbcParameter CreateParameter ()
		{
			return new OdbcParameter ();
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
		}
		
		private void ExecSQL(string sql)
		{
			OdbcReturn ret;
	
			if (!prepared)
			{
				Prepare();
				if (Parameters.Count>0)
					Parameters.Bind(hstmt);
			}
			
			if (prepared)
			{
				ret=libodbc.SQLExecute(hstmt);
				libodbchelper.DisplayError("SQLExecute",ret);
			}
			else
			{
				ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, Connection.hDbc, ref hstmt);
				libodbchelper.DisplayError("SQLAllocHandle(hstmt)",ret);
				ret=libodbc.SQLExecDirect(hstmt, sql, sql.Length);
				libodbchelper.DisplayError("SQLExecDirect",ret);
			}
		}

		public int ExecuteNonQuery ()
		{
			if (connection == null)
				throw new InvalidOperationException ();
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			// FIXME: a third check is mentioned in .NET docs
			if (connection.DataReader != null)
				throw new InvalidOperationException ();

			ExecSQL(CommandText);

			if (!prepared)
				libodbc.SQLFreeHandle( (ushort) OdbcHandleType.Stmt, hstmt);
			return 0;
		}

		public void Prepare()
		{
			OdbcReturn ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, Connection.hDbc, ref hstmt);
			libodbchelper.DisplayError("SQLAlloc(Prepare)",ret);
			ret=libodbc.SQLPrepare(hstmt, CommandText, CommandText.Length);
			libodbchelper.DisplayError("SQLPrepare",ret);
			prepared=true;
		}

		public OdbcDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		public OdbcDataReader ExecuteReader (CommandBehavior behavior)
		{
			ExecuteNonQuery();
			dataReader=new OdbcDataReader(this);
			return dataReader;
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
		
		public object ExecuteScalar ()
		{
					throw new NotImplementedException ();
//			if (connection.DataReader != null)
//				throw new InvalidOperationException ();
//			
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();	
		}

		public void ResetCommandTimeout ()
		{
			timeout = 30;
		}

		#endregion
	}
}
