//
// Mono.Data.SybaseClient.SybaseCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (monodanmorg@yahoo.com))
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002, 2008
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

namespace Mono.Data.SybaseClient {
#if NET_2_0
	public sealed class SybaseCommand : DbCommand, IDbCommand, ICloneable
#else
	public sealed class SybaseCommand : Component, IDbCommand, ICloneable
#endif // NET_2_0
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
		SybaseParameterCollection parameters;
		string preparedStatement = null;

		#endregion // Fields

		#region Constructors

		public SybaseCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SybaseCommand (string commandText) 
			: this (commandText, null, null)
		{
		}

		public SybaseCommand (string commandText, SybaseConnection connection) 
			: this (commandText, connection, null)
		{
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

		private SybaseCommand(string commandText, SybaseConnection connection, SybaseTransaction transaction, CommandType commandType, UpdateRowSource updatedRowSource, bool designTimeVisible, int commandTimeout, SybaseParameterCollection parameters)
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = commandType;
			this.updatedRowSource = updatedRowSource;
			this.designTimeVisible = designTimeVisible;
			this.commandTimeout = commandTimeout;
			this.parameters = new SybaseParameterCollection(this);
			for (int i = 0;i < parameters.Count;i++)
				this.parameters.Add(((ICloneable)parameters[i]).Clone());
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		string CommandText {
			get { return commandText; }
			set { 
				if (value != commandText && preparedStatement != null)
					Unprepare ();
				commandText = value; 
			}
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		int CommandTimeout {
			get { return commandTimeout;  }
			set { 
				if (commandTimeout < 0)
					throw new ArgumentException ("The property value assigned is less than 0.");
				commandTimeout = value; 
			}
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		CommandType CommandType	{
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentException ("CommandType.TableDirect is not supported by the Mono SybaseClient Data Provider.");
				commandType = value; 
			}
		}

		public
#if NET_2_0
		new
#endif //NET_2_0
		SybaseConnection Connection {
			get { return connection; }
			set { 
				if (transaction != null && connection.Transaction != null && connection.Transaction.IsOpen)
					throw new InvalidOperationException ("The Connection property was changed while a transaction was in progress.");
				transaction = null;
				connection = value; 
			}
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		public
#if NET_2_0
		new
#endif //NET_2_0
		SybaseParameterCollection Parameters {
			get { return parameters; }
		}

		internal MDTP.Tds Tds {
			get { return Connection.Tds; }
		}

#if !NET_2_0
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
#endif // !NET_2_0

		public new SybaseTransaction Transaction {
			get {
				if (transaction != null && !transaction.IsOpen)
					transaction = null;
				return transaction;
			}
			set
			{
#if ONLY_1_1
				if (connection != null && connection.DataReader != null)
					throw new InvalidOperationException ("The connection is busy fetching data.");
#endif
				transaction = value;
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		UpdateRowSource UpdatedRowSource	{
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion // Fields

		#region Methods

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Cancel () 
		{
			if (Connection == null || Connection.Tds == null)
				return;
			Connection.Tds.Cancel ();
		}

#if NET_2_0
		public SybaseCommand Clone ()
		{
			return new SybaseCommand (commandText, connection, transaction, commandType, updatedRowSource, designTimeVisible, commandTimeout, parameters);
		}
#endif // NET_2_0

		internal void CloseDataReader (bool moreResults)
		{
			GetOutputParameters ();
			Connection.DataReader = null;

			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public new SybaseParameter CreateParameter () 
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

			string sql = "sp_procedure_params_rowset";

			Connection.Tds.ExecProc (sql, localParameters.MetaParameters, 0, true);

			SybaseDataReader reader = new SybaseDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new SybaseParameter (dbValues));
			}
			reader.Close ();	
		}

		private void Execute (CommandBehavior behavior, bool wantResults)
		{
			Tds.RecordsAffected = -1; 
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			if (preparedStatement == null) {
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
			else 
				Connection.Tds.ExecPrepared (preparedStatement, parms, CommandTimeout, wantResults);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			int result = 0;

			try {
				Execute (CommandBehavior.Default, false);
				result = Tds.RecordsAffected;
			}
			catch (TdsTimeoutException e) {
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public new SybaseDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public new SybaseDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			try {
				Execute (behavior, true);
			}
			catch (TdsTimeoutException e) {
				throw SybaseException.FromTdsInternalException ((TdsInternalException) e);
			}
			Connection.DataReader = new SybaseDataReader (this);
			return Connection.DataReader;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0 
		object ExecuteScalar ()
		{
			ValidateCommand ("ExecuteScalar");
			try {
				Execute (CommandBehavior.Default, true);
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

		internal void GetOutputParameters ()
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
			return new SybaseCommand (commandText, connection, transaction, commandType, updatedRowSource, designTimeVisible, commandTimeout, parameters);
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

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Prepare ()
		{
#if NET_2_0
			if (Connection == null)
				throw new NullReferenceException ();
#endif

			if (CommandType == CommandType.StoredProcedure || CommandType == CommandType.Text && Parameters.Count == 0)
				return;

			ValidateCommand ("Prepare");
			if (CommandType == CommandType.Text) 
				preparedStatement = Connection.Tds.Prepare (CommandText, Parameters.MetaParameters);
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		private void Unprepare ()
		{
			Connection.Tds.Unprepare (preparedStatement); 
			preparedStatement = null;
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
#if NET_2_0
		protected override DbParameter CreateDbParameter ()
		{
			return CreateParameter ();
		}

		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		protected override DbConnection DbConnection {
			get { return Connection; }
			set { Connection = (SybaseConnection) value; }
		}

		protected override DbParameterCollection DbParameterCollection {
			get { return Parameters; }
		}

		protected override DbTransaction DbTransaction {
			get { return Transaction; }
			set { Transaction = (SybaseTransaction) value; }
		}
#endif // NET_2_0

		#endregion // Methods
	}
}
