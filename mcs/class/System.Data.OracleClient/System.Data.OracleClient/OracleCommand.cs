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
//    Daniel Morgan <danielmorgan@verizon.net>
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Daniel Morgan, 2002, 2004-2005
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Drawing.Design;
using System.Text;

namespace System.Data.OracleClient {
	[Designer ("Microsoft.VSDesigner.Data.VS.OracleCommandDesigner, " + Consts.AssemblyMicrosoft_VSDesigner)]
	[ToolboxItem (true)]
	public sealed class OracleCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		CommandBehavior behavior;
		string commandText;
		CommandType commandType;
		OracleConnection connection;
		bool designTimeVisible;
		OracleParameterCollection parameters;
		OracleTransaction transaction;
		UpdateRowSource updatedRowSource;

		private OciStatementHandle preparedStatement;
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

		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		[Editor ("Microsoft.VSDesigner.Data.Oracle.Design.OracleCommandTextEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue (CommandType.Text)]
		public CommandType CommandType {
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("OracleClient provider does not support TableDirect CommandType.");
				commandType = value; 
			}
		}

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public OracleConnection Connection {
			get { return connection; }
			set { connection = value; }
		}

		[DefaultValue (true)]
		[Browsable (false)]
		[DesignOnly (true)]
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
			get { return 0; }
			set { }
		}

		[Editor ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		[DefaultValue (null)]
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

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public OracleParameterCollection Parameters {
			get { return parameters; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public OracleTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		[DefaultValue (UpdateRowSource.Both)]
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
			// create a new OracleCommand object with the same properties
			
			OracleCommand cmd = new OracleCommand ();
			
			cmd.CommandText = this.CommandText;
			cmd.CommandType = this.CommandType;

			// FIXME: not sure if I should set the same object here
			// or get a clone of these too
			cmd.Connection = this.Connection;
			cmd.Transaction = this.Transaction;
			
			foreach (OracleParameter parm in this.Parameters) {

				OracleParameter newParm = cmd.CreateParameter ();

				newParm.DbType = parm.DbType;
				newParm.Direction = parm.Direction;
				newParm.IsNullable = parm.IsNullable;
				newParm.Offset = parm.Offset;
				newParm.OracleType = parm.OracleType;
				newParm.ParameterName = parm.ParameterName;
				newParm.Precision = parm.Precision;
				newParm.Scale = parm.Scale;
				newParm.SourceColumn = parm.SourceColumn;
				newParm.SourceVersion = parm.SourceVersion;
				newParm.Value = parm.Value;

				cmd.Parameters.Add (newParm);
			}

			//cmd.Container = this.Container;
			cmd.DesignTimeVisible = this.DesignTimeVisible;
			//cmd.DesignMode = this.DesignMode;
			cmd.Site = this.Site;
			//cmd.UpdateRowSource = this.UpdateRowSource;

			return cmd;
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

		private int ExecuteNonQueryInternal (OciStatementHandle statement, bool useAutoCommit)
		{
			if (preparedStatement == null)
				PrepareStatement (statement);

			BindParameters (statement);
			statement.ExecuteNonQuery (useAutoCommit);

			int rowsAffected = statement.GetAttributeInt32 (OciAttributeType.RowCount, ErrorHandle);
		
			return rowsAffected;
		}

		public int ExecuteNonQuery () 
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();
			bool useAutoCommit = false; 

			if (Transaction != null)
				Transaction.AttachToServiceContext ();
			else
				useAutoCommit = true;

			OciStatementHandle statement = GetStatementHandle ();
			try {
				return ExecuteNonQueryInternal (statement, useAutoCommit);
			}
			finally	{
				SafeDisposeHandle (statement);
			}
		}

		public int ExecuteOracleNonQuery (out OracleString rowid)
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();
			bool useAutoCommit = false; 

			if (Transaction != null)
				Transaction.AttachToServiceContext ();
			else
				useAutoCommit = true;

			OciStatementHandle statement = GetStatementHandle ();

			try {
				int retval = ExecuteNonQueryInternal (statement, useAutoCommit);

				OciRowIdDescriptor descriptor = (OciRowIdDescriptor) Environment.Allocate (OciHandleType.RowId);
				descriptor.SetHandle (statement.GetAttributeIntPtr (OciAttributeType.RowId, ErrorHandle));

				rowid = new OracleString (descriptor.GetRowId (ErrorHandle));

				return retval;
			}
			finally	{
				SafeDisposeHandle (statement);
			}
		}

		[MonoTODO]
		public object ExecuteOracleScalar ()
		{
			object output = DBNull.Value;

			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();
			try {
				if (preparedStatement == null)
					PrepareStatement (statement);
				BindParameters (statement);

				statement.ExecuteQuery ();

				if (statement.Fetch ()) {
					OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [0];
					if (!defineHandle.IsNull)
						output = defineHandle.GetOracleValue ();
						switch (defineHandle.DataType) {
						case OciDataType.Blob:
						case OciDataType.Clob:
							((OracleLob) output).connection = Connection;
							break;
						}
				}

				return output;
			}
			finally {
				SafeDisposeHandle (statement);
			}
		}

		private bool IsNonQuery (OciStatementHandle statementHandle) 
		{
			// assumes Prepare() has been called prior to calling this function

			OciStatementType statementType = statementHandle.GetStatementType ();
			if (statementType.Equals (OciStatementType.Select))
				return false;

			return true;
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
			bool hasRows = false;
				
			if (Transaction != null) 
				Transaction.AttachToServiceContext ();
			
			OciStatementHandle statement = GetStatementHandle ();
			OracleDataReader rd = null;

			try	{
				if (preparedStatement == null)
					PrepareStatement (statement);
				else
					preparedStatement = null;	// OracleDataReader releases the statement handle

				bool isNonQuery = IsNonQuery (statement);

				BindParameters (statement);

				if (isNonQuery)
					ExecuteNonQueryInternal (statement, false);
				else
					hasRows = statement.ExecuteQuery ();

				rd = new OracleDataReader (this, statement, hasRows);
			}
			finally	{
				if (statement != null && rd == null)
					statement.Dispose();
			}

			return rd;
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
			try {
				if (preparedStatement == null)
					PrepareStatement (statement);
				BindParameters (statement);

				statement.ExecuteQuery ();

				if (statement.Fetch ()) {
					OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [0];
					if (defineHandle.IsNull)
						output = DBNull.Value;
					else {
						switch (defineHandle.DataType) {
						case OciDataType.Blob:
						case OciDataType.Clob:
							OracleLob lob = (OracleLob) defineHandle.GetValue ();
							lob.connection = Connection;
							output = lob.Value;
							lob.Close ();
							break;
						default:
							output = defineHandle.GetValue ();
							break;
						}
					}
				}
				else
					output = DBNull.Value;
			}
			finally {
				SafeDisposeHandle (statement);
			}

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

		private void SafeDisposeHandle (OciStatementHandle h)
		{
			if (h != null && h != preparedStatement) 
				h.Dispose();
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

		void PrepareStatement (OciStatementHandle statement) 
		{
			if (commandType == CommandType.StoredProcedure) {
				StringBuilder sb = new StringBuilder ();
				if (Parameters.Count > 0)
					foreach (OracleParameter parm in Parameters) {
						if (sb.Length > 0)
							sb.Append (",");
						sb.Append (":" + parm.ParameterName);
					}

				string sql = "call " + commandText + "(" + sb.ToString() + ")";
				statement.Prepare (sql);
			}
			else	// Text
				statement.Prepare (commandText);
		}

		public void Prepare ()
		{
			AssertConnectionIsOpen ();
			OciStatementHandle statement = GetStatementHandle ();
			PrepareStatement (statement);
			preparedStatement = statement;
		}

		#endregion // Methods
	}
}
