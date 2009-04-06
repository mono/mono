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
//    Marek Safar <marek.safar@gmail.com>
//
// Copyright (C) Daniel Morgan, 2002, 2004-2005
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
#if NET_2_0
using System.Data.Common;
#endif
using System.Data.OracleClient.Oci;
using System.Drawing.Design;
using System.Text;

namespace System.Data.OracleClient
{
#if NET_2_0
	[DefaultEvent ("RecordsAffected")]
#endif
	[Designer ("Microsoft.VSDesigner.Data.VS.OracleCommandDesigner, " + Consts.AssemblyMicrosoft_VSDesigner)]
	[ToolboxItem (true)]
	public sealed class OracleCommand :
#if NET_2_0
		DbCommand, ICloneable
#else
		Component, ICloneable, IDbCommand
#endif
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
		OciStatementHandle preparedStatement;
		
		int moreResults;

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
			moreResults = -1;
			preparedStatement = null;
			CommandText = commandText;
			Connection = connection;
			Transaction = tx;
			CommandType = CommandType.Text;
			UpdatedRowSource = UpdateRowSource.Both;
			DesignTimeVisible = true;
			parameters = new OracleParameterCollection ();
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		[Editor ("Microsoft.VSDesigner.Data.Oracle.Design.OracleCommandTextEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		override
#endif
		string CommandText {
			get {
				if (commandText == null)
					return string.Empty;

				return commandText;
			}
			set { commandText = value; }
		}

		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue (CommandType.Text)]
		public
#if NET_2_0
		override
#endif
		CommandType CommandType {
			get { return commandType; }
			set {
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("OracleClient provider does not support TableDirect CommandType.");
				commandType = value;
			}
		}

		[DefaultValue (null)]
		[Editor ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		public
#if NET_2_0
		new
#endif
		OracleConnection Connection {
			get { return connection; }
			set { connection = value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override int CommandTimeout {
			get { return 0; }
			set { }
		}

		[MonoTODO]
		protected override DbConnection DbConnection {
			get { return Connection; }
			set { Connection = (OracleConnection) value; }
		}

		[MonoTODO]
		protected override DbParameterCollection DbParameterCollection {
			get { return Parameters; }
		}

		[MonoTODO]
		protected override DbTransaction DbTransaction {
			get { return Transaction; }
			set { Transaction = (OracleTransaction) value; }
		}
#endif

		[DefaultValue (true)]
		[Browsable (false)]
		[DesignOnly (true)]
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		override
#endif
		bool DesignTimeVisible {
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		internal OciEnvironmentHandle Environment {
			get { return Connection.Environment; }
		}

		internal OciErrorHandle ErrorHandle {
			get { return Connection.ErrorHandle; }
		}

#if !NET_2_0
		int IDbCommand.CommandTimeout {
			get { return 0; }
			set { }
		}

		[Editor ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, " + Consts.AssemblyMicrosoft_VSDesigner, typeof(UITypeEditor))]
		[DefaultValue (null)]
		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set {
				// InvalidCastException is expected when types do not match
				Connection = (OracleConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set {
				// InvalidCastException is expected when types do not match
				Transaction = (OracleTransaction) value;
			}
		}
#endif

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public
#if NET_2_0
		new
#endif
		OracleParameterCollection Parameters {
			get { return parameters; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
		new
#endif
		OracleTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		[DefaultValue (UpdateRowSource.Both)]
		public
#if NET_2_0
		override
#endif
		UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion

		#region Methods

		private void AssertCommandTextIsSet ()
		{
			if (CommandText.Length == 0)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
		}

		private void AssertConnectionIsOpen ()
		{
			if (Connection == null || Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("An open Connection object is required to continue.");
		}

		private void AssertTransactionMatch ()
		{
			if (Connection.Transaction != null && Transaction != Connection.Transaction)
				throw new InvalidOperationException ("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
		}

		private void BindParameters (OciStatementHandle statement)
		{
			for (int p = 0; p < Parameters.Count; p++)
				Parameters[p].Bind (statement, Connection, (uint) p);
		}

		[MonoTODO]
		public
#if NET_2_0
		override
#endif
		void Cancel ()
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
				//newParm.Precision = parm.Precision;
				//newParm.Scale = parm.Scale;
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

#if NET_2_0
		protected override DbParameter CreateDbParameter ()
		{
			return CreateParameter ();
		}

		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
#endif

		internal void UpdateParameterValues ()
		{
			moreResults = -1;
			if (Parameters.Count > 0) {
				bool foundCursor = false;
				for (int p = 0; p < Parameters.Count; p++) {
					OracleParameter parm = Parameters [p];
					if (parm.OracleType.Equals (OracleType.Cursor)) {
						if (!foundCursor && parm.Direction != ParameterDirection.Input) {
							// if there are multiple REF CURSORs,
							// you only can get the first cursor for now
							// because user of OracleDataReader
							// will do a NextResult to get the next 
							// REF CURSOR (if it exists)
							foundCursor = true;
							parm.Update (this);
							if (p + 1 == Parameters.Count)
								moreResults = -1;
							else
								moreResults = p;
						}
					} else
						parm.Update (this);
				}
			}
		}

		internal void CloseDataReader ()
		{
			Connection.DataReader = null;
			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public
#if NET_2_0
		new
#endif
		OracleParameter CreateParameter ()
		{
			return new OracleParameter ();
		}

		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("OracleCommandBuilder DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));

			//OracleParameterCollection localParameters = new OracleParameterCollection (this);

			throw new NotImplementedException ();
		}

		private int ExecuteNonQueryInternal (OciStatementHandle statement, bool useAutoCommit)
		{
			moreResults = -1;

			if (preparedStatement == null)
				PrepareStatement (statement);

			bool isNonQuery = IsNonQuery (statement);

			BindParameters (statement);
			if (isNonQuery == true)
				statement.ExecuteNonQuery (useAutoCommit);
			else
				statement.ExecuteQuery (false);

			UpdateParameterValues ();

			int rowsAffected = statement.GetAttributeInt32 (OciAttributeType.RowCount, ErrorHandle);

			return rowsAffected;
		}

		public
#if NET_2_0
		override
#endif
		int ExecuteNonQuery ()
		{
			moreResults = -1;

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
			} finally {
				SafeDisposeHandle (statement);
			}
		}

		public int ExecuteOracleNonQuery (out OracleString rowid)
		{
			moreResults = -1;

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
				OciRowIdDescriptor rowIdDescriptor = statement.GetAttributeRowIdDescriptor (ErrorHandle, Environment);
				string srowid = rowIdDescriptor.GetRowIdToString (ErrorHandle);
				rowid = new OracleString (srowid);
				rowIdDescriptor = null;
				return retval;
			} finally {
				SafeDisposeHandle (statement);
			}
		}

		public object ExecuteOracleScalar ()
		{
			moreResults = -1;

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

				bool isNonQuery = IsNonQuery (statement);

				BindParameters (statement);

				if (isNonQuery == true)
					ExecuteNonQueryInternal (statement, false);
				else {
					statement.ExecuteQuery (false);

					if (statement.Fetch ()) {
						OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [0];
						if (!defineHandle.IsNull)
							output = defineHandle.GetOracleValue (Connection.SessionFormatProvider, Connection);
						switch (defineHandle.DataType) {
						case OciDataType.Blob:
						case OciDataType.Clob:
							((OracleLob) output).connection = Connection;
							break;
						}
					}
					UpdateParameterValues ();
				}

				return output;
			} finally {
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

		public
#if NET_2_0
		new
#endif
		OracleDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public
#if NET_2_0
		new
#endif
		OracleDataReader ExecuteReader (CommandBehavior behavior)
		{
			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			moreResults = -1;

			bool hasRows = false;

			this.behavior = behavior;

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();
			OracleDataReader rd = null;

			try {
				if (preparedStatement == null)
					PrepareStatement (statement);
				else
					preparedStatement = null;	// OracleDataReader releases the statement handle

				bool isNonQuery = IsNonQuery (statement);

				BindParameters (statement);

				if (isNonQuery) 
					ExecuteNonQueryInternal (statement, false);
				else {	
					if ((behavior & CommandBehavior.SchemaOnly) != 0)
						statement.ExecuteQuery (true);
					else
						hasRows = statement.ExecuteQuery (false);

					UpdateParameterValues ();
				}

				if (Parameters.Count > 0) {
					for (int p = 0; p < Parameters.Count; p++) {
						OracleParameter parm = Parameters [p];
						if (parm.OracleType.Equals (OracleType.Cursor)) {
							if (parm.Direction != ParameterDirection.Input) {
								rd = (OracleDataReader) parm.Value;
								break;
							}
						}
					}					
				}

				if (rd == null)
					rd = new OracleDataReader (this, statement, hasRows, behavior);

			} finally {
				if (statement != null && rd == null)
					statement.Dispose();
			}

			return rd;
		}

		public
#if NET_2_0
		override
#endif
		object ExecuteScalar ()
		{
			moreResults = -1;
			object output = null;//if we find nothing we return this

			AssertConnectionIsOpen ();
			AssertTransactionMatch ();
			AssertCommandTextIsSet ();

			if (Transaction != null)
				Transaction.AttachToServiceContext ();

			OciStatementHandle statement = GetStatementHandle ();
			try {
				if (preparedStatement == null)
					PrepareStatement (statement);

				bool isNonQuery = IsNonQuery (statement);

				BindParameters (statement);

				if (isNonQuery == true)
					ExecuteNonQueryInternal (statement, false);
				else {
					statement.ExecuteQuery (false);

					if (statement.Fetch ()) {
						OciDefineHandle defineHandle = (OciDefineHandle) statement.Values [0];
						if (!defineHandle.IsNull)
						{
							switch (defineHandle.DataType) {
							case OciDataType.Blob:
							case OciDataType.Clob:
								OracleLob lob = (OracleLob) defineHandle.GetValue (
									Connection.SessionFormatProvider, Connection);
								lob.connection = Connection;
								output = lob.Value;
								lob.Close ();
								break;
							default:
								output = defineHandle.GetValue (
									Connection.SessionFormatProvider, Connection);
								break;
							}
						}
					}
					UpdateParameterValues ();
				}
			} finally {
				SafeDisposeHandle (statement);
			}

			return output;
		}

		internal OciStatementHandle GetNextResult () 
		{
			if (moreResults == -1)
				return null;

			if (Parameters.Count > 0) {
				int p = moreResults + 1;
				
				if (p >= Parameters.Count) {
					moreResults = -1;
					return null;
				}

				for (; p < Parameters.Count; p++) {
					OracleParameter parm = Parameters [p];
					if (parm.OracleType.Equals (OracleType.Cursor)) {
						if (parm.Direction != ParameterDirection.Input) {
							if (p + 1 == Parameters.Count)
								moreResults = -1;
							else 
								moreResults = p;
							return parm.GetOutRefCursor (this);
							
						}
					} 
				}
			}

			moreResults = -1;
			return null;
		}

		private OciStatementHandle GetStatementHandle ()
		{
			AssertConnectionIsOpen ();
			if (preparedStatement != null)
				return preparedStatement;

			OciStatementHandle h = (OciStatementHandle) Connection.Environment.Allocate (OciHandleType.Statement);
			h.ErrorHandle = Connection.ErrorHandle;
			h.Service = Connection.ServiceContext;
			h.Command = this;
			return h;
		}

		private void SafeDisposeHandle (OciStatementHandle h)
		{
			if (h != null && h != preparedStatement)
				h.Dispose();
		}

#if !NET_2_0
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
#endif

		void PrepareStatement (OciStatementHandle statement)
		{
			if (commandType == CommandType.StoredProcedure) {
				StringBuilder sb = new StringBuilder ();
				if (Parameters.Count > 0)
					foreach (OracleParameter parm in Parameters) {
						if (sb.Length > 0)
							sb.Append (",");
						sb.Append (parm.ParameterName + "=>:" + parm.ParameterName);
					}

				string sql = "begin " + commandText + "(" + sb.ToString() + "); end;";
				statement.Prepare (sql);
			} else	// Text
				statement.Prepare (commandText);
		}

		public
#if NET_2_0
		override
#endif
		void Prepare ()
		{
			AssertConnectionIsOpen ();
			OciStatementHandle statement = GetStatementHandle ();
			PrepareStatement (statement);
			preparedStatement = statement;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				if (Parameters.Count > 0)
					foreach (OracleParameter parm in Parameters)
						parm.FreeHandle ();
			base.Dispose (disposing);
		}

		#endregion // Methods
	}
}
