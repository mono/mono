//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient {
	public sealed class SqlCommand : Component, IDbCommand, ICloneable
	{
		#region Fields

		bool disposed = false;

		int commandTimeout;
		bool designTimeVisible;
		string commandText;

		CommandType commandType;
		SqlConnection connection;
		SqlTransaction transaction;
		UpdateRowSource updatedRowSource;

		CommandBehavior behavior = CommandBehavior.Default;
		NameValueCollection preparedStatements = new NameValueCollection ();
		SqlParameterCollection parameters;

		#endregion // Fields

		#region Constructors

		public SqlCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SqlCommand (string commandText) 
			: this (commandText, null, null)
		{
			commandText = commandText;
		}

		public SqlCommand (string commandText, SqlConnection connection) 
			: this (commandText, connection, null)
		{
			Connection = connection;
		}

		public SqlCommand (string commandText, SqlConnection connection, SqlTransaction transaction) 
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;

			this.designTimeVisible = false;
			this.commandTimeout = 30;
			parameters = new SqlParameterCollection (this);
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Command text to execute.")]
		[DefaultValue ("")]
		[RefreshProperties (RefreshProperties.All)]
		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		[DataSysDescription ("Time to wait for command to execute.")]
		[DefaultValue (30)]
		public int CommandTimeout {
			get { return commandTimeout;  }
			set { 
				if (commandTimeout < 0)
					throw new ArgumentException ("The property value assigned is less than 0.");
				commandTimeout = value; 
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("How to interpret the CommandText.")]
		[DefaultValue (CommandType.Text)]
		[RefreshProperties (RefreshProperties.All)]
		public CommandType CommandType	{
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("CommandType.TableDirect is not supported by the Mono SqlClient Data Provider.");
				commandType = value; 
			}
		}

		[DataCategory ("Behavior")]
		[DefaultValue (null)]
		[DataSysDescription ("Connection used by the command.")]
		public SqlConnection Connection {
			get { return connection; }
			set { 
				if (transaction != null && connection.Transaction != null && connection.Transaction.IsOpen)
					throw new InvalidOperationException ("The Connection property was changed while a transaction was in progress.");
				transaction = null;
				connection = value; 
			}
		}

		[Browsable (false)]
		[DefaultValue (true)]
		[DesignOnly (true)]
		public bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The parameters collection.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SqlParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return Connection.Tds; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is SqlConnection))
					throw new InvalidCastException ("The value was not a valid SqlConnection.");
				Connection = (SqlConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is SqlTransaction))
					throw new ArgumentException ();
				Transaction = (SqlTransaction) value; 
			}
		}

		[Browsable (false)]
		[DataSysDescription ("The transaction used by the command.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SqlTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}	

		[DataCategory ("Behavior")]
		[DataSysDescription ("When used by a DataAdapter.Update, how command results are applied to the current DataRow.")]
		[DefaultValue (UpdateRowSource.Both)]
		public UpdateRowSource UpdatedRowSource	{
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion // Fields

		#region Methods

		private string BuildCommand ()
		{
			string statementHandle = preparedStatements [commandText];
			if (statementHandle != null) {
				string proc = String.Format ("sp_execute {0}", statementHandle);	
				if (parameters.Count > 0)
					proc += ",";
				return BuildProcedureCall (proc, parameters);
			}

			if (commandType == CommandType.StoredProcedure)
				return BuildProcedureCall (commandText, parameters);

			string sql = String.Empty;
			if ((behavior & CommandBehavior.KeyInfo) > 0)
				sql += "SET FMTONLY OFF; SET NO_BROWSETABLE ON;";
			if ((behavior & CommandBehavior.SchemaOnly) > 0)
				sql += "SET FMTONLY ON;";
	
			switch (commandType) {
			case CommandType.Text :
				sql += commandText;
				break;
			default:
				throw new InvalidOperationException ("The CommandType was invalid.");
			}
			return BuildExec (sql);
		}

		private string BuildExec (string sql)
		{
			StringBuilder parms = new StringBuilder ();
			foreach (SqlParameter parameter in parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					parms.Append (" output");
			}

			SqlParameterCollection localParameters = new SqlParameterCollection (this);
			SqlParameter parm;
		
			localParameters.Add ("@P1", SqlDbType.NVarChar, sql.Length).Value = sql;

			if (parameters.Count > 0) 
				localParameters.Add ("@P2", SqlDbType.NVarChar, parms.ToString ().Length).Value = parms.ToString ();

			foreach (SqlParameter p in parameters)
				localParameters.Add ((SqlParameter) ((ICloneable) p).Clone ());

			return BuildProcedureCall ("sp_executesql", localParameters);
		}

		private string BuildPrepare ()
		{
			StringBuilder parms = new StringBuilder ();
			foreach (SqlParameter parameter in parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					parms.Append (" output");
			}

			SqlParameterCollection localParameters = new SqlParameterCollection (this);
			SqlParameter parm;
		
			parm = new SqlParameter ("@P1", SqlDbType.Int);
			parm.Direction = ParameterDirection.Output;
			localParameters.Add (parm);

			parm = new SqlParameter ("@P2", SqlDbType.NVarChar);
			parm.Value = parms.ToString ();
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			parm = new SqlParameter ("@P3", SqlDbType.NVarChar);
			parm.Value = commandText;
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			return BuildProcedureCall ("sp_prepare", localParameters);
		}

		private static string BuildProcedureCall (string procedure, SqlParameterCollection parameters)
		{
			StringBuilder parms = new StringBuilder ();
			StringBuilder declarations = new StringBuilder ();
			StringBuilder outParms = new StringBuilder ();
			StringBuilder set = new StringBuilder ();

			int index = 1;
			foreach (SqlParameter parameter in parameters) {
				string parmName = String.Format ("@P{0}", index);

				switch (parameter.Direction) {
				case ParameterDirection.Input :
					if (parms.Length > 0)
						parms.Append (", ");
					parms.Append (FormatParameter (parameter));
					break;
				case ParameterDirection.Output :
					if (parms.Length > 0)
						parms.Append (", ");
					parms.Append (parmName);
					parms.Append (" output");

					if (outParms.Length > 0) {
						outParms.Append (", ");
						declarations.Append (", ");
					}
					else {
						outParms.Append ("select ");
						declarations.Append ("declare ");
					}

					declarations.Append (parameter.Prepare (parmName));
					set.Append (String.Format ("set {0}=NULL\n", parmName));
					outParms.Append (parmName);
					break;
				default :
					throw new NotImplementedException ("Only support input and output parameters.");
				}
				index += 1;
			}
			if (declarations.Length > 0)
				declarations.Append ('\n');

			return String.Format ("{0}{1}{2} {3}\n{4}", declarations.ToString (), set.ToString (), procedure, parms.ToString (), outParms.ToString ());
		}

		public void Cancel () 
		{
			if (Connection == null || Connection.Tds == null)
				return;
			Connection.Tds.Cancel ();
		}

		internal void CloseDataReader (bool moreResults)
		{
			GetOutputParameters ();
			Connection.DataReader = null;

			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public SqlParameter CreateParameter () 
		{
			return new SqlParameter ();
		}

		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("SqlCommand DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));
			ValidateCommand ("DeriveParameters");

			SqlParameterCollection localParameters = new SqlParameterCollection (this);
			localParameters.Add ("@P1", SqlDbType.NVarChar, commandText.Length).Value = commandText;

			Connection.Tds.ExecuteQuery (BuildProcedureCall ("sp_procedure_params_rowset", localParameters));
			SqlDataReader reader = new SqlDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new SqlParameter (dbValues));
			}
			reader.Close ();	
		}

		public int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			string sql = String.Empty;
			int result = 0;

			if (Parameters.Count > 0)
				sql = BuildCommand ();
			else
				sql = CommandText;

			try {
				result = Connection.Tds.ExecuteNonQuery (sql, CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public SqlDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			this.behavior = behavior;

			try {
				Connection.Tds.ExecuteQuery (BuildCommand (), CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}
		
			Connection.DataReader = new SqlDataReader (this);
			return Connection.DataReader;
		}

		public object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
			try {
				Connection.Tds.ExecuteQuery (BuildCommand (), CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			if (!Connection.Tds.NextResult () || !Connection.Tds.NextRow ())
				return null;

			object result = Connection.Tds.ColumnValues [0];
			CloseDataReader (true);
			return result;
		}

		public XmlReader ExecuteXmlReader ()
		{
			ValidateCommand ("ExecuteXmlReader");

			try {
				Connection.Tds.ExecuteQuery (BuildCommand (), CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			SqlDataReader dataReader = new SqlDataReader (this);
			SqlXmlTextReader textReader = new SqlXmlTextReader (dataReader);
			XmlReader xmlReader = new XmlTextReader (textReader);
			return xmlReader;
		}

		[MonoTODO ("Include offset from SqlParameter for binary/string types.")]
		static string FormatParameter (SqlParameter parameter)
		{
			if (parameter.Value == null)
				return "NULL";

			switch (parameter.SqlDbType) {
				case SqlDbType.BigInt :
				case SqlDbType.Decimal :
				case SqlDbType.Float :
				case SqlDbType.Int :
				case SqlDbType.Money :
				case SqlDbType.Real :
				case SqlDbType.SmallInt :
				case SqlDbType.SmallMoney :
				case SqlDbType.TinyInt :
					return parameter.Value.ToString ();
				case SqlDbType.NVarChar :
				case SqlDbType.NChar :
					return String.Format ("N'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
				case SqlDbType.UniqueIdentifier :
					return String.Format ("0x{0}", ((Guid) parameter.Value).ToString ("N"));
				case SqlDbType.Bit:
					if (parameter.Value.GetType () == typeof (bool))
						return (((bool) parameter.Value) ? "0x1" : "0x0");
					return parameter.Value.ToString ();
				case SqlDbType.Image:
				case SqlDbType.Binary:
				case SqlDbType.VarBinary:
					return String.Format ("0x{0}", BitConverter.ToString ((byte[]) parameter.Value).Replace ("-", "").ToLower ());
				default:
					return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
		}

		private void GetOutputParameters ()
		{
			Connection.Tds.SkipToEnd ();

			IList list = Connection.Tds.ColumnValues;

			if (list != null && list.Count > 0) {
				int index = 0;
				foreach (SqlParameter parameter in parameters) {
					if (parameter.Direction != ParameterDirection.Input) {
						parameter.Value = list [index];
						index += 1;
					}
					if (index >= list.Count)
						break;
				}
			}
		}

		object ICloneable.Clone ()
		{
			return new SqlCommand (commandText, Connection);
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
			ValidateCommand ("Prepare");
			Connection.Tds.ExecuteNonQuery (BuildPrepare ());

			if (Connection.Tds.OutputParameters.Count == 0 || Connection.Tds.OutputParameters[0] == null)
				throw new Exception ("Could not prepare the statement.");

			preparedStatements [commandText] = ((int) Connection.Tds.OutputParameters [0]).ToString ();
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		private void ValidateCommand (string method)
		{
			if (Connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", method));
			if (Connection.Transaction != null && transaction != Connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (Connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.", method));
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			if (Connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
			if (Connection.XmlReader != null)
				throw new InvalidOperationException ("There is already an open XmlReader associated with this Connection which must be closed first.");
		}

		#endregion // Methods
	}
}
