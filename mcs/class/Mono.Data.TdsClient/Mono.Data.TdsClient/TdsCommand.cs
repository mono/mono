//
// Mono.Data.TdsClient.TdsCommand.cs
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

using Mono.Data.Tds;
using Mono.Data.Tds.Protocol;
using MDTP = Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Data.TdsClient {
	public sealed class TdsCommand : Component, IDbCommand, ICloneable
	{
		#region Fields

		bool disposed = false;
		int commandTimeout;
		bool designTimeVisible;
		string commandText;
		CommandType commandType;
		TdsConnection connection;
		TdsTransaction transaction;
		UpdateRowSource updatedRowSource;
		CommandBehavior behavior = CommandBehavior.Default;
		TdsParameterCollection parameters;

		#endregion // Fields

		#region Constructors

		public TdsCommand() 
			: this (String.Empty, null, null)
		{
		}

		public TdsCommand (string commandText) 
			: this (commandText, null, null)
		{
		}

		public TdsCommand (string commandText, TdsConnection connection) 
			: this (commandText, connection, null)
		{
		}

		public TdsCommand (string commandText, TdsConnection connection, TdsTransaction transaction) 
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;

			this.designTimeVisible = false;
			this.commandTimeout = 30;
			parameters = new TdsParameterCollection (this);
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
					throw new ArgumentException ("CommandType.TableDirect is not supported by the Mono TdsClient Data Provider.");
				commandType = value; 
			}
		}

		public TdsConnection Connection {
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

		public TdsParameterCollection Parameters {
			get { return parameters; }
		}

		internal MDTP.Tds Tds {
			get { return Connection.Tds; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is TdsConnection))
					throw new InvalidCastException ("The value was not a valid TdsConnection.");
				Connection = (TdsConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is TdsTransaction))
					throw new ArgumentException ();
				Transaction = (TdsTransaction) value; 
			}
		}

		public TdsTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}	

		public UpdateRowSource UpdatedRowSource	{
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion // Fields

		#region Methods

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

		public TdsParameter CreateParameter () 
		{
			return new TdsParameter ();
		}

		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("TdsCommand DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));
			ValidateCommand ("DeriveParameters");

			TdsParameterCollection localParameters = new TdsParameterCollection (this);
			localParameters.Add ("@P1", TdsType.NVarChar, commandText.Length).Value = commandText;

			string sql = "sp_procedure_params_rowset";

			Connection.Tds.ExecProc (sql, localParameters.MetaParameters, 0, true);

			TdsDataReader reader = new TdsDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new TdsParameter (dbValues));
			}
			reader.Close ();	
		}

		private void Execute (CommandBehavior behavior, bool wantResults)
		{
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			bool schemaOnly = ((CommandBehavior & CommandBehavior.SchemaOnly) > 0);
			bool keyInfo = ((CommandBehavior & CommandBehavior.SchemaOnly) > 0);

			StringBuilder sql1 = new StringBuilder ();
			StringBuilder sql2 = new StringBuilder ();

			if (schemaOnly || keyInfo)
				sql1.Append ("SET FMTONLY OFF;");
			if (keyInfo) {
				sql1.Append ("SET NO_BROWSETABLE ON;");
				sql2.Append ("SET NO_BROWSETABLE OFF;");
			}
			if (schemaOnly) {
				sql1.Append ("SET FMTONLY ON;");
				sql2.Append ("SET FMTONLY OFF;");
			}

			switch (CommandType) {
			case CommandType.StoredProcedure:
				if (keyInfo || schemaOnly)
					Connection.Tds.Execute (sql1.ToString ());
				Connection.Tds.ExecProc (CommandText, parms, CommandTimeout, wantResults);
				if (keyInfo || schemaOnly)
					Connection.Tds.Execute (sql2.ToString ());
				break;
			case CommandType.Text:
				string sql = String.Format ("{0}{1}{2}", sql1.ToString (), CommandText, sql2.ToString ());
				Connection.Tds.Execute (sql, parms, CommandTimeout, wantResults);
				break;
			}
		}

		public int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			int result = 0;

			try {
				Execute (CommandBehavior.Default, false);
			}
			catch (TdsTimeoutException e) {
				throw TdsException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public TdsDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public TdsDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			try {
				Execute (behavior, true);
			}
			catch (TdsTimeoutException e) {
				throw TdsException.FromTdsInternalException ((TdsInternalException) e);
			}
			Connection.DataReader = new TdsDataReader (this);
			return Connection.DataReader;
		}

		public object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
			try {
				Execute (CommandBehavior.Default, true);
			}
			catch (TdsTimeoutException e) {
				throw TdsException.FromTdsInternalException ((TdsInternalException) e);
			}

			if (!Connection.Tds.NextResult () || !Connection.Tds.NextRow ())
				return null;

			object result = Connection.Tds.ColumnValues [0];
			CloseDataReader (true);
			return result;
		}

		private void GetOutputParameters ()
		{
			Connection.Tds.SkipToEnd ();

			IList list = Connection.Tds.ColumnValues;

			if (list != null && list.Count > 0) {
				int index = 0;
				foreach (TdsParameter parameter in parameters) {
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
			return new TdsCommand (commandText, Connection);
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
			throw new NotSupportedException ("TdsClient does not support PREPARE.");
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
