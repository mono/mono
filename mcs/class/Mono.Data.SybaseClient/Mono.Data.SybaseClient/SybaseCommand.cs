//
// Mono.Data.SybaseClient.SybaseCommand.cs
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

namespace Mono.Data.SybaseClient {
	public sealed class SybaseCommand : Component, IDbCommand, ICloneable
	{
		#region Fields

		bool disposed = false;

		int commandTimeout;
		bool designTimeVisible;
		string commandText;

		CommandType commandType;
		SybaseConnection connection;
		SybaseTransaction transaction;
		UpdateRowSource updatedRowSource;

		CommandBehavior behavior = CommandBehavior.Default;
		NameValueCollection preparedStatements = new NameValueCollection ();
		SybaseParameterCollection parameters;

		#endregion // Fields

		#region Constructors

		public SybaseCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SybaseCommand (string commandText) 
			: this (commandText, null, null)
		{
			commandText = commandText;
		}

		public SybaseCommand (string commandText, SybaseConnection connection) 
			: this (commandText, connection, null)
		{
			Connection = connection;
		}

		public SybaseCommand (string commandText, SybaseConnection connection, SybaseTransaction transaction) 
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;

			this.designTimeVisible = false;
			this.commandTimeout = 30;
			parameters = new SybaseParameterCollection (this);
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public int CommandTimeout {
			get { return commandTimeout;  }
			set { 
				if (commandTimeout < 0)
					throw new ArgumentException ("The property value assigned is less than 0.");
				commandTimeout = value; 
			}
		}

		public CommandType CommandType	{
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("CommandType.TableDirect is not supported by the Mono SybaseClient Data Provider.");
				commandType = value; 
			}
		}

		public SybaseConnection Connection {
			get { return connection; }
			set { 
				if (transaction != null && connection.Transaction != null && connection.Transaction.IsOpen)
					throw new InvalidOperationException ("The Connection property was changed while a transaction was in progress.");
				transaction = null;
				connection = value; 
			}
		}

		public bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		public SybaseParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return Connection.Tds; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is SybaseConnection))
					throw new InvalidCastException ("The value was not a valid SybaseConnection.");
				Connection = (SybaseConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is SybaseTransaction))
					throw new ArgumentException ();
				Transaction = (SybaseTransaction) value; 
			}
		}

		public SybaseTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}	

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

		[MonoTODO ("This throws a SybaseException.")]
		private string BuildExec (string sql)
		{
			StringBuilder declare = new StringBuilder ();
			StringBuilder assign = new StringBuilder ();

			sql = sql.Replace ("'", "''");
			foreach (SybaseParameter parameter in parameters) {
				declare.Append ("declare ");
				declare.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					declare.Append (" output");
				declare.Append ('\n');
				assign.Append (String.Format ("select {0}={1}\n", parameter.ParameterName, FormatParameter (parameter)));
			}

			return String.Format ("{0}{1}{2}", declare.ToString (), assign.ToString (), sql);
		}

		private string BuildPrepare ()
		{
			StringBuilder parms = new StringBuilder ();
			foreach (SybaseParameter parameter in parameters) {
				if (parms.Length > 0)
					parms.Append (", ");
				parms.Append (parameter.Prepare (parameter.ParameterName));
				if (parameter.Direction == ParameterDirection.Output)
					parms.Append (" output");
			}

			SybaseParameterCollection localParameters = new SybaseParameterCollection (this);
			SybaseParameter parm;
		
			parm = new SybaseParameter ("@P1", SybaseType.Int);
			parm.Direction = ParameterDirection.Output;
			localParameters.Add (parm);

			parm = new SybaseParameter ("@P2", SybaseType.NVarChar);
			parm.Value = parms.ToString ();
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			parm = new SybaseParameter ("@P3", SybaseType.NVarChar);
			parm.Value = commandText;
			parm.Size = ((string) parm.Value).Length;
			localParameters.Add (parm);

			return BuildProcedureCall ("sp_prepare", localParameters);
		}

		private static string BuildProcedureCall (string procedure, SybaseParameterCollection parameters)
		{
			StringBuilder parms = new StringBuilder ();
			StringBuilder declarations = new StringBuilder ();
			StringBuilder outParms = new StringBuilder ();
			StringBuilder set = new StringBuilder ();

			int index = 1;
			foreach (SybaseParameter parameter in parameters) {
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

		public SybaseParameter CreateParameter () 
		{
			return new SybaseParameter ();
		}

		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("SybaseCommand DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));
			ValidateCommand ("DeriveParameters");

			SybaseParameterCollection localParameters = new SybaseParameterCollection (this);
			localParameters.Add ("@P1", SybaseType.NVarChar, commandText.Length).Value = commandText;

			Connection.Tds.ExecuteQuery (BuildProcedureCall ("sp_procedure_params_rowset", localParameters));
			SybaseDataReader reader = new SybaseDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new SybaseParameter (dbValues));
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
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public SybaseDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public SybaseDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			this.behavior = behavior;

			try {
				Connection.Tds.ExecuteQuery (BuildCommand (), CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
			}
		
			Connection.DataReader = new SybaseDataReader (this);
			return Connection.DataReader;
		}

		public object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
			try {
				Connection.Tds.ExecuteQuery (BuildCommand (), CommandTimeout);
			}
			catch (TdsTimeoutException e) {
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
			}

			if (!Connection.Tds.NextResult () || !Connection.Tds.NextRow ())
				return null;

			object result = Connection.Tds.ColumnValues [0];
			CloseDataReader (true);
			return result;
		}

		[MonoTODO ("Include offset from SybaseParameter for binary/string types.")]
		static string FormatParameter (SybaseParameter parameter)
		{
			if (parameter.Value == null)
				return "NULL";

			switch (parameter.SybaseType) {
				case SybaseType.BigInt :
				case SybaseType.Decimal :
				case SybaseType.Float :
				case SybaseType.Int :
				case SybaseType.Money :
				case SybaseType.Real :
				case SybaseType.SmallInt :
				case SybaseType.SmallMoney :
				case SybaseType.TinyInt :
					return parameter.Value.ToString ();
				case SybaseType.NVarChar :
				case SybaseType.NChar :
					return String.Format ("N'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
				case SybaseType.UniqueIdentifier :
					return String.Format ("0x{0}", ((Guid) parameter.Value).ToString ("N"));
				case SybaseType.Bit:
					if (parameter.Value.GetType () == typeof (bool))
						return (((bool) parameter.Value) ? "0x1" : "0x0");
					return parameter.Value.ToString ();
				case SybaseType.Image:
				case SybaseType.Binary:
				case SybaseType.VarBinary:
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
				foreach (SybaseParameter parameter in parameters) {
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
			return new SybaseCommand (commandText, Connection);
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
		}

		#endregion // Methods
	}
}
