// 
// OracleCommand.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;

namespace System.Data.OracleClient {
	public class OracleCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		bool disposed = false;
		CommandBehavior behavior;
		string commandText;
		CommandType commandType;
		OracleConnection connection;
		bool designTimeVisible;
		OracleParameterCollection parameters;
		OracleTransaction transaction;
		UpdateRowSource updatedRowSource;

		OciStatementHandle preparedStatement;
		OciStatementType statementType;

		#endregion // Fields

		#region Constructors

		public OracleCommand ()
			: this (String.Empty, null, null)
		{
		}

		public OracleCommand (string commandText)
			: this (commandText, null, null)
		{
		}

		public OracleCommand (string commandText, OracleConnection connection)
			: this (commandText, connection, null)
		{
		}

		public OracleCommand (string commandText, OracleConnection connection, OracleTransaction tx)
		{
			preparedStatement = null;
			CommandText = commandText;
			Connection = connection;
			Transaction = tx;
			CommandType = CommandType.Text;
			UpdatedRowSource = UpdateRowSource.Both;
			DesignTimeVisible = false;

                        parameters = new OracleParameterCollection (this);
		}

		#endregion // Constructors

		#region Properties

		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public CommandType CommandType {
			get { return commandType; }
			set { commandType = value; }
		}

		public OracleConnection Connection {
			get { return connection; }
			set { connection = value; }
		}

		public bool DesignTimeVisible {
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		internal OciEnvironmentHandle Environment {
			get { return Connection.Environment; }
		}

		internal OciErrorHandle ErrorHandle {
			get { return Connection.ErrorHandle; }
		}

		int IDbCommand.CommandTimeout {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is OracleConnection))
					throw new InvalidCastException ("The value was not a valid OracleConnection.");
				Connection = (OracleConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is OracleTransaction))
					throw new ArgumentException ();
				Transaction = (OracleTransaction) value; 
			}
		}

		public OracleParameterCollection Parameters {
			get { return parameters; }
		}

		public OracleTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		public UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion

		#region Methods

		private void AssertCommandTextIsSet ()
		{
			if (CommandText == String.Empty || CommandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
		}

		private void AssertConnectionIsOpen ()
		{
			if (Connection == null || Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("An open Connection object is required to continue.");
		}

		private void AssertNoDataReader ()
		{
			if (Connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
		}

		private void AssertTransactionMatch ()
		{
			if (Connection.Transaction != null && Transaction != Connection.Transaction)
				throw new InvalidOperationException ("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
		}

		private void BindParameters (OciStatementHandle statement)
		{
			foreach (OracleParameter p in Parameters) 
				p.Bind (statement, Connection);
		}

		[MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		internal void CloseDataReader ()
		{
			Connection.DataReader = null;
			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public OracleParameter CreateParameter ()
		{
			return new OracleParameter ();
		}

		private int ExecuteNonQueryInternal (OciStatementHandle statement)
		{
			if (preparedStatement == null)
				statement.Prepare (CommandText);

			BindParameters (statement);
			statement.ExecuteNonQuery ();

			int rowsAffected = statement.GetAttributeInt32 (OciAttributeType.RowCount, ErrorHandle);
			
			statement.Dispose ();
			return rowsAffected;
		}

		public int ExecuteNonQuery () 
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			return ExecuteNonQueryInternal (GetStatementHandle ());
		}

		public int ExecuteOracleNonQuery (out OracleString rowid)
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();

			int retval = ExecuteNonQueryInternal (statement);

			OciRowIdDescriptor descriptor = (OciRowIdDescriptor) Environment.Allocate (OciHandleType.RowId);
			descriptor.SetHandle (statement.GetAttributeIntPtr (OciAttributeType.RowId, ErrorHandle));

			rowid = new OracleString (descriptor.GetRowId (ErrorHandle));

			return retval;
		}

		[MonoTODO]
		public object ExecuteOracleScalar (out OracleString rowid)
		{
			throw new NotImplementedException ();
		}

		public OracleDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public OracleDataReader ExecuteReader (CommandBehavior behavior)
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();
			AssertNoDataReader ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();

			if (preparedStatement == null)
				statement.Prepare (CommandText);
			BindParameters (statement);
			statement.ExecuteQuery ();

			return new OracleDataReader (this, statement);
		}

		public object ExecuteScalar ()
		{
			object output;

			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();
			if (preparedStatement == null)
				statement.Prepare (CommandText);
			BindParameters (statement);

			statement.ExecuteQuery ();

			if (statement.Fetch ()) 
				output = ((OciDefineHandle) statement.Values [0]).GetValue ();
			else
				output = DBNull.Value;

			return output;
		}

		private OciStatementHandle GetStatementHandle ()
		{
			AssertConnectionIsOpen ();
			if (preparedStatement != null) 
				return preparedStatement;
			
			OciStatementHandle h = (OciStatementHandle) Connection.Environment.Allocate (OciHandleType.Statement);
			h.ErrorHandle = Connection.ErrorHandle;
			h.Service = Connection.ServiceContext;
			return h;
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		public void Prepare ()
		{
			AssertConnectionIsOpen ();
			OciStatementHandle statement = GetStatementHandle ();
			statement.Prepare (CommandText);
			preparedStatement = statement;
		}

		#endregion // Methods
	}
}
