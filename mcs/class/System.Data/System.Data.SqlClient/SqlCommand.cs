//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//
// (C) Ximian, Inc 2002 http://www.ximian.com/
// (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Sql;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace System.Data.SqlClient {
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.SqlCommandDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("System.Drawing.Design.ToolboxItem, "+ Consts.AssemblySystem_Drawing)]
	[DefaultEventAttribute ("RecordsAffected")]
	public sealed class SqlCommand : DbCommand, IDbCommand, ICloneable
	{
		#region Fields

		const int DEFAULT_COMMAND_TIMEOUT = 30;

		int commandTimeout;
		bool designTimeVisible;
		string commandText;
		CommandType commandType;
		SqlConnection connection;
		SqlTransaction transaction;
		UpdateRowSource updatedRowSource;
		CommandBehavior behavior = CommandBehavior.Default;
		SqlParameterCollection parameters;
		string preparedStatement;
		bool disposed;
		SqlNotificationRequest notification;
		bool notificationAutoEnlist;

		#endregion // Fields

		#region Constructors

		public SqlCommand() 
			: this (String.Empty, null, null)
		{
		}

		public SqlCommand (string cmdText)
			: this (cmdText, null, null)
		{
		}

		public SqlCommand (string cmdText, SqlConnection connection)
			: this (cmdText, connection, null)
		{
		}

		public SqlCommand (string cmdText, SqlConnection connection, SqlTransaction transaction) 
		{
			this.commandText = cmdText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;

			this.commandTimeout = DEFAULT_COMMAND_TIMEOUT;
			notificationAutoEnlist = true;
			designTimeVisible = true;
			parameters = new SqlParameterCollection (this);
		}

		private SqlCommand(string commandText, SqlConnection connection, SqlTransaction transaction, CommandType commandType, UpdateRowSource updatedRowSource, bool designTimeVisible, int commandTimeout, SqlParameterCollection parameters)
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = transaction;
			this.commandType = commandType;
			this.updatedRowSource = updatedRowSource;
			this.designTimeVisible = designTimeVisible;
			this.commandTimeout = commandTimeout;
			this.parameters = new SqlParameterCollection(this);
			for (int i = 0;i < parameters.Count;i++)
				this.parameters.Add(((ICloneable)parameters[i]).Clone());
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		[DefaultValue ("")]
		[EditorAttribute ("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RefreshProperties (RefreshProperties.All)]
		public
		override
		string CommandText {
			get {
				if (commandText == null)
					return string.Empty;
				return commandText;
			}
			set {
				if (value != commandText && preparedStatement != null)
					Unprepare ();
				commandText = value;
			}
		}

		public
		override
		int CommandTimeout {
			get { return commandTimeout; }
			set { 
				if (value < 0)
					throw new ArgumentException ("The property value assigned is less than 0.",
						"CommandTimeout");
				commandTimeout = value; 
			}
		}

		[DefaultValue (CommandType.Text)]
		[RefreshProperties (RefreshProperties.All)]
		public
		override
		CommandType CommandType {
			get { return commandType; }
			set { 
				if (value == CommandType.TableDirect)
					throw new ArgumentOutOfRangeException ("CommandType.TableDirect is not supported " +
						"by the Mono SqlClient Data Provider.");

				ExceptionHelper.CheckEnumValue (typeof (CommandType), value);
				commandType = value; 
			}
		}

		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]		
		public
		new
		SqlConnection Connection {
			get { return connection; }
			set
			{
				connection = value;
			}
		}

		[Browsable (false)]
		[DefaultValue (true)]
		[DesignOnly (true)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public
		override
		bool DesignTimeVisible {
			get { return designTimeVisible; } 
			set { designTimeVisible = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public
		new
		SqlParameterCollection Parameters {
			get { return parameters; }
		}

		internal Tds Tds {
			get { return Connection.Tds; }
		}


		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new SqlTransaction Transaction {
			get {
				if (transaction != null && !transaction.IsOpen)
					transaction = null;
				return transaction;
			}
			set
			{
				transaction = value;
			}
		}

		[DefaultValue (UpdateRowSource.Both)]
		public
		override
		UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (UpdateRowSource), value);
				updatedRowSource = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SqlNotificationRequest Notification {
			get { return notification; }
			set { notification = value; }
		}

		[DefaultValue (true)]
		public bool NotificationAutoEnlist {
			get { return notificationAutoEnlist; }
			set { notificationAutoEnlist = value; }
		}
		#endregion // Fields

		#region Methods

		public
		override
		void Cancel ()
		{
			if (Connection == null || Connection.Tds == null)
				return;
			Connection.Tds.Cancel ();
		}

		public SqlCommand Clone ()
		{
			return new SqlCommand (commandText, connection, transaction, commandType, updatedRowSource, designTimeVisible, commandTimeout, parameters);
		}

		internal void CloseDataReader ()
		{
			if (Connection != null) {
				Connection.DataReader = null;

				if ((behavior & CommandBehavior.CloseConnection) != 0)
					Connection.Close ();

				if (Tds != null)
					Tds.SequentialAccess = false;
			}

			// Reset the behavior
			behavior = CommandBehavior.Default;
		}

		public new SqlParameter CreateParameter ()
		{
			return new SqlParameter ();
		}

		private string EscapeProcName (string name, bool schema)
		{
			string procName;
			string tmpProcName = name.Trim ();
			int procNameLen = tmpProcName.Length;
			char[] brkts = new char [] {'[', ']'};
			bool foundMatching = false;
			int start = 0, count = procNameLen;
			int sindex = -1, eindex = -1;
			
			// We try to match most of the "brackets" combination here, however
			// there could be other combinations that may generate a different type
			// of exception in MS.NET
			
			if (procNameLen > 1) {
				if ((sindex = tmpProcName.IndexOf ('[')) <= 0)
					foundMatching = true;
				else
					foundMatching = false;
			
				if (foundMatching == true && sindex > -1) {
					eindex = tmpProcName.IndexOf (']');
					if (sindex > eindex && eindex != -1) {
						foundMatching = false;
					} else if (eindex == procNameLen-1) {
						if (tmpProcName.IndexOfAny (brkts, 1, procNameLen-2) != -1) {
							foundMatching = false;
						} else {
							start = 1;
							count = procNameLen - 2;
						}
					} else if (eindex == -1 && schema) {
						foundMatching = true;
					} else {
						foundMatching = false;
					}
				}
			
				if (foundMatching)
					procName = tmpProcName.Substring (start, count);
				else
					throw new ArgumentException (String.Format ("SqlCommand.CommandText property value is an invalid multipart name {0}, incorrect usage of quotes", CommandText));
			} else {
				procName = tmpProcName;
			}
			
			return procName;
		}
		internal void DeriveParameters ()
		{
			if (commandType != CommandType.StoredProcedure)
				throw new InvalidOperationException (String.Format ("SqlCommand DeriveParameters only supports CommandType.StoredProcedure, not CommandType.{0}", commandType));
			ValidateCommand ("DeriveParameters", false);

			string procName = CommandText;
			string schemaName = String.Empty;
			int dotPosition = procName.LastIndexOf ('.');

			// Procedure name can be: [database].[user].[procname]
			if (dotPosition >= 0) {
				schemaName = procName.Substring (0, dotPosition);
				procName = procName.Substring (dotPosition + 1);
				if ((dotPosition = schemaName.LastIndexOf ('.')) >= 0)
					schemaName = schemaName.Substring (dotPosition + 1);
			}
			
			procName = EscapeProcName (procName, false);
			schemaName = EscapeProcName (schemaName, true);
			
			SqlParameterCollection localParameters = new SqlParameterCollection (this);
			localParameters.Add ("@procedure_name", SqlDbType.NVarChar, procName.Length).Value = procName;
			if (schemaName.Length > 0)
				localParameters.Add ("@procedure_schema", SqlDbType.NVarChar, schemaName.Length).Value = schemaName;
			
			string sql = "sp_procedure_params_rowset";

			try {
				Connection.Tds.ExecProc (sql, localParameters.MetaParameters, 0, true);
			} catch (TdsTimeoutException ex) {
				Connection.Tds.Reset ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			} catch (TdsInternalException ex) {
				Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
			}

			SqlDataReader reader = new SqlDataReader (this);
			parameters.Clear ();
			object[] dbValues = new object[reader.FieldCount];

			while (reader.Read ()) {
				reader.GetValues (dbValues);
				parameters.Add (new SqlParameter (dbValues));
			}
			reader.Close ();

			if (parameters.Count == 0)
				throw new InvalidOperationException ("Stored procedure '" + procName + "' does not exist.");
		}

		private void Execute (bool wantResults)
		{
			int index = 0;
			Connection.Tds.RecordsAffected = -1;
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			foreach (TdsMetaParameter param in parms) {
				param.Validate (index++);
			}

			if (preparedStatement == null) {
				bool schemaOnly = ((behavior & CommandBehavior.SchemaOnly) > 0);
				bool keyInfo = ((behavior & CommandBehavior.KeyInfo) > 0);

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
					try {
						if (keyInfo || schemaOnly)
							Connection.Tds.Execute (sql1.ToString ());
						Connection.Tds.ExecProc (CommandText, parms, CommandTimeout, wantResults);
						if (keyInfo || schemaOnly)
							Connection.Tds.Execute (sql2.ToString ());
					} catch (TdsTimeoutException ex) {
						// If it is a timeout exception there can be many reasons:
						// 1) Network is down/server is down/not reachable
						// 2) Somebody has an exclusive lock on Table/DB
						// In any of these cases, don't close the connection. Let the user do it
						Connection.Tds.Reset ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					} catch (TdsInternalException ex) {
						Connection.Close ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					}
					break;
				case CommandType.Text:
					string sql;
					if (sql2.Length > 0) {
						sql = String.Format ("{0}{1};{2}", sql1.ToString (), CommandText, sql2.ToString ());
					} else {
						sql = String.Format ("{0}{1}", sql1.ToString (), CommandText);
					}
					try {
						Connection.Tds.Execute (sql, parms, CommandTimeout, wantResults);
					} catch (TdsTimeoutException ex) {
						Connection.Tds.Reset ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					} catch (TdsInternalException ex) {
						Connection.Close ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					}
					break;
				}
			}
			else {
				try {
					Connection.Tds.ExecPrepared (preparedStatement, parms, CommandTimeout, wantResults);
				} catch (TdsTimeoutException ex) {
					Connection.Tds.Reset ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				} catch (TdsInternalException ex) {
					Connection.Close ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				}
			}
		}

		public
		override
		int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery", false);
			int result = 0;
			behavior = CommandBehavior.Default;

			try {
				Execute (false);
				result = Connection.Tds.RecordsAffected;
			} catch (TdsTimeoutException e) {
				Connection.Tds.Reset ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			GetOutputParameters ();
			return result;
		}

		public new SqlDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public new SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader", false);
			if ((behavior & CommandBehavior.SingleRow) != 0)
				behavior |= CommandBehavior.SingleResult;
			this.behavior = behavior;
			if ((behavior & CommandBehavior.SequentialAccess) != 0)
				Tds.SequentialAccess = true;
			try {
				Execute (true);
				Connection.DataReader = new SqlDataReader (this);
				return Connection.DataReader;
			} catch {
				if ((behavior & CommandBehavior.CloseConnection) != 0)
					Connection.Close ();
				throw;
			}
		}

		[MonoTODO]
		public new Task<SqlDataReader> ExecuteReaderAsync ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new Task<SqlDataReader> ExecuteReaderAsync (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new Task<SqlDataReader> ExecuteReaderAsync (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new Task<SqlDataReader> ExecuteReaderAsync (CommandBehavior behavior, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Task<XmlReader> ExecuteXmlReaderAsync ()
		{
			throw new NotImplementedException ();
		}
 
		[MonoTODO]
		public Task<XmlReader> ExecuteXmlReaderAsync (CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		public
		override
		object ExecuteScalar ()
		{
			try {
				object result = null;
				ValidateCommand ("ExecuteScalar", false);
				behavior = CommandBehavior.Default;
				Execute (true);

				try {
					if (Connection.Tds.NextResult () && Connection.Tds.NextRow ())
						result = Connection.Tds.ColumnValues[0];

					if (commandType == CommandType.StoredProcedure) {
						Connection.Tds.SkipToEnd ();
						GetOutputParameters ();
					}
				} catch (TdsTimeoutException ex) {
					Connection.Tds.Reset ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				} catch (TdsInternalException ex) {
					Connection.Close ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				}

				return result;
			} finally {
				CloseDataReader ();
			}
		}

		public XmlReader ExecuteXmlReader ()
		{
			ValidateCommand ("ExecuteXmlReader", false);
			behavior = CommandBehavior.Default;
			try {
				Execute (true);
			} catch (TdsTimeoutException e) {
				Connection.Tds.Reset ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			SqlDataReader dataReader = new SqlDataReader (this);
			SqlXmlTextReader textReader = new SqlXmlTextReader (dataReader);
			XmlReader xmlReader = new XmlTextReader (textReader);
			return xmlReader;
		}

		internal void GetOutputParameters ()
		{
			IList list = Connection.Tds.OutputParameters;

			if (list != null && list.Count > 0) {

				int index = 0;
				foreach (SqlParameter parameter in parameters) {
					if (parameter.Direction != ParameterDirection.Input &&
					    parameter.Direction != ParameterDirection.ReturnValue) {
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
			return new SqlCommand (commandText, connection, transaction, commandType, updatedRowSource, designTimeVisible, commandTimeout, parameters);

		}


		protected override void Dispose (bool disposing)
		{
			if (disposed) return;
			if (disposing) {
				parameters.Clear();
				if (Connection != null)
					Connection.DataReader = null;
			}
			base.Dispose (disposing);
			disposed = true;
		}

		public
		override
		void Prepare ()
		{
			if (Connection == null)
				throw new NullReferenceException ();

			if (CommandType == CommandType.StoredProcedure || CommandType == CommandType.Text && Parameters.Count == 0)
				return;

			ValidateCommand ("Prepare", false);

			try {
				foreach (SqlParameter param in Parameters)
					param.CheckIfInitialized ();
			} catch (Exception e) {
				throw new InvalidOperationException ("SqlCommand.Prepare requires " + e.Message);
			}

			preparedStatement = Connection.Tds.Prepare (CommandText, Parameters.MetaParameters);
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = DEFAULT_COMMAND_TIMEOUT;
		}

		private void Unprepare ()
		{
			Connection.Tds.Unprepare (preparedStatement);
			preparedStatement = null;
		}

		private void ValidateCommand (string method, bool async)
		{
			if (Connection == null)
				throw new InvalidOperationException (String.Format ("{0}: A Connection object is required to continue.", method));
			if (Transaction == null && Connection.Transaction != null)
				throw new InvalidOperationException (String.Format (
					"{0} requires a transaction if the command's connection is in a pending transaction.",
					method));
			if (Transaction != null && Transaction.Connection != Connection)
				throw new InvalidOperationException ("The connection does not have the same transaction as the command.");
			if (Connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("{0} requires an open connection to continue. This connection is closed.", method));
			if (CommandText.Length == 0)
				throw new InvalidOperationException (String.Format ("{0}: CommandText has not been set for this Command.", method));
			if (Connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
			if (Connection.XmlReader != null)
				throw new InvalidOperationException ("There is already an open XmlReader associated with this Connection which must be closed first.");
			if (async && !Connection.AsyncProcessing)
				throw new InvalidOperationException ("This Connection object is not " + 
					"in Asynchronous mode. Use 'Asynchronous" +
					" Processing = true' to set it.");
		}

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
			set { Connection = (SqlConnection) value; }
		}

		protected override DbParameterCollection DbParameterCollection {
			get { return Parameters; }
		}

		protected override DbTransaction DbTransaction {
			get { return Transaction; }
			set { Transaction = (SqlTransaction) value; }
		}

		#endregion // Methods

		#region Asynchronous Methods

		internal IAsyncResult BeginExecuteInternal (CommandBehavior behavior, 
													bool wantResults,
													AsyncCallback callback, 
													object state)
		{
			IAsyncResult ar = null;
			Connection.Tds.RecordsAffected = -1;
			TdsMetaParameterCollection parms = Parameters.MetaParameters;
			if (preparedStatement == null) {
				bool schemaOnly = ((behavior & CommandBehavior.SchemaOnly) > 0);
				bool keyInfo = ((behavior & CommandBehavior.KeyInfo) > 0);

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
					string prolog = "";
					string epilog = "";
					if (keyInfo || schemaOnly)
						prolog = sql1.ToString ();
					if (keyInfo || schemaOnly)
						epilog = sql2.ToString ();
					try {
						ar = Connection.Tds.BeginExecuteProcedure (prolog,
										      epilog,
										      CommandText,
										      !wantResults,
										      parms,
										      callback,
										      state);
					} catch (TdsTimeoutException ex) {
						Connection.Tds.Reset ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					} catch (TdsInternalException ex) {
						Connection.Close ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					}
					break;
				case CommandType.Text:
					string sql = String.Format ("{0}{1};{2}", sql1.ToString (), CommandText, sql2.ToString ());
					try {
						if (wantResults)
							ar = Connection.Tds.BeginExecuteQuery (sql, parms, callback, state);
						else
							ar = Connection.Tds.BeginExecuteNonQuery (sql, parms, callback, state);
					} catch (TdsTimeoutException ex) {
						Connection.Tds.Reset ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					} catch (TdsInternalException ex) {
						Connection.Close ();
						throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
					}
					break;
				}
			}
			else {
				try {
					Connection.Tds.ExecPrepared (preparedStatement, parms, CommandTimeout, wantResults);
				} catch (TdsTimeoutException ex) {
					Connection.Tds.Reset ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				} catch (TdsInternalException ex) {
					Connection.Close ();
					throw SqlException.FromTdsInternalException ((TdsInternalException) ex);
				}
			}
			return ar;
		}

		internal void EndExecuteInternal (IAsyncResult ar)
		{
			SqlAsyncResult sqlResult = ( (SqlAsyncResult) ar);
			Connection.Tds.WaitFor (sqlResult.InternalResult);
			Connection.Tds.CheckAndThrowException (sqlResult.InternalResult);
		}

		public IAsyncResult BeginExecuteNonQuery ()
		{
			return BeginExecuteNonQuery (null, null);
		}

		public IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, object stateObject)
		{
			ValidateCommand ("BeginExecuteNonQuery", true);
			SqlAsyncResult ar = new SqlAsyncResult (callback, stateObject);
			ar.EndMethod = "EndExecuteNonQuery";
			ar.InternalResult = BeginExecuteInternal (CommandBehavior.Default, false, ar.BubbleCallback, ar);
			return ar;
		}

		public int EndExecuteNonQuery (IAsyncResult asyncResult)
		{
			ValidateAsyncResult (asyncResult, "EndExecuteNonQuery");
			EndExecuteInternal (asyncResult);

			int ret = Connection.Tds.RecordsAffected;

			GetOutputParameters ();
			((SqlAsyncResult) asyncResult).Ended = true;
			return ret;
		}

		public IAsyncResult BeginExecuteReader ()
		{
			return BeginExecuteReader (null, null, CommandBehavior.Default);
		}

		public IAsyncResult BeginExecuteReader (CommandBehavior behavior)
		{
			return BeginExecuteReader (null, null, behavior);
		}

		public IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject)
		{
			return BeginExecuteReader (callback, stateObject, CommandBehavior.Default);
		}

		public IAsyncResult BeginExecuteReader (AsyncCallback callback, object stateObject, CommandBehavior behavior)
		{
			ValidateCommand ("BeginExecuteReader", true);
			this.behavior = behavior;
			SqlAsyncResult ar = new SqlAsyncResult (callback, stateObject);
			ar.EndMethod = "EndExecuteReader";
			IAsyncResult tdsResult = BeginExecuteInternal (behavior, true, 
				ar.BubbleCallback, stateObject);
			ar.InternalResult = tdsResult;
			return ar;
		}

		public SqlDataReader EndExecuteReader (IAsyncResult asyncResult)
		{
			ValidateAsyncResult (asyncResult, "EndExecuteReader");
			EndExecuteInternal (asyncResult);
			SqlDataReader reader = null;
			try {
				reader = new SqlDataReader (this);
			} catch (TdsTimeoutException e) {
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			} catch (TdsInternalException e) {
				// if behavior is closeconnection, even if it throws exception
				// the connection has to be closed.
				if ((behavior & CommandBehavior.CloseConnection) != 0)
					Connection.Close ();
				throw SqlException.FromTdsInternalException ((TdsInternalException) e);
			}

			((SqlAsyncResult) asyncResult).Ended = true;
			return reader;
		}

		public IAsyncResult BeginExecuteXmlReader (AsyncCallback callback, object stateObject)
		{
			ValidateCommand ("BeginExecuteXmlReader", true);
			SqlAsyncResult ar = new SqlAsyncResult (callback, stateObject);
			ar.EndMethod = "EndExecuteXmlReader";
			ar.InternalResult = BeginExecuteInternal (behavior, true, 
				ar.BubbleCallback, stateObject);
			return ar;
		}

		public IAsyncResult BeginExecuteXmlReader ()
		{
			return BeginExecuteXmlReader (null, null);
		}
		

		public XmlReader EndExecuteXmlReader (IAsyncResult asyncResult)
		{
			ValidateAsyncResult (asyncResult, "EndExecuteXmlReader");
			EndExecuteInternal (asyncResult);
			SqlDataReader reader = new SqlDataReader (this);
			SqlXmlTextReader textReader = new SqlXmlTextReader (reader);
			XmlReader xmlReader = new XmlTextReader (textReader);
			((SqlAsyncResult) asyncResult).Ended = true;
			return xmlReader;
		}

		internal void ValidateAsyncResult (IAsyncResult ar, string endMethod)
		{
			if (ar == null)
				throw new ArgumentException ("result passed is null!");
			if (! (ar is SqlAsyncResult))
				throw new ArgumentException (String.Format ("cannot test validity of types {0}",
					ar.GetType ()));
			SqlAsyncResult result = (SqlAsyncResult) ar;
			if (result.EndMethod != endMethod)
				throw new InvalidOperationException (String.Format ("Mismatched {0} called for AsyncResult. " + 
					"Expected call to {1} but {0} is called instead.",
					endMethod, result.EndMethod));
			if (result.Ended)
				throw new InvalidOperationException (String.Format ("The method {0} cannot be called " + 
					"more than once for the same AsyncResult.", endMethod));
		}

		#endregion // Asynchronous Methods

		public event StatementCompletedEventHandler StatementCompleted;
	}
}
