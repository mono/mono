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

		int commandTimeout;
		bool designTimeVisible;
		string commandText;

		CommandType commandType;
		SybaseConnection connection;
		SybaseTransaction transaction;

		SybaseParameterCollection parameters = new SybaseParameterCollection ();
		private CommandBehavior behavior = CommandBehavior.Default;

		NameValueCollection procedureCache = new NameValueCollection ();

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
			this.designTimeVisible = false;
			this.commandTimeout = 30;
		}

		#endregion // Constructors

		#region Properties

		internal CommandBehavior CommandBehavior {
			get { return behavior; }
		}

		public string CommandText {
			get { return CommandText; }
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
			[MonoTODO ("Validate")]
			set { commandType = value; }
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
			get { return connection.Tds; }
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

		[MonoTODO]
		public UpdateRowSource UpdatedRowSource	{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Fields

		#region Methods

		public void Cancel () 
		{
			if (connection == null || connection.Tds == null)
				return;
			connection.Tds.Cancel ();
			connection.CheckForErrors ();
		}

		internal void CloseDataReader (bool moreResults)
		{
			while (moreResults)
				moreResults = connection.Tds.NextResult ();

			if (connection.Tds.OutputParameters.Count > 0) {
				int index = 0;
				foreach (SybaseParameter parameter in parameters) {
					if (parameter.Direction != ParameterDirection.Input)
						parameter.Value = connection.Tds.OutputParameters[index];
					index += 1;
					if (index >= connection.Tds.OutputParameters.Count)
						break;
				}
			}
			connection.DataReaderOpen = false;
			if ((behavior & CommandBehavior.CloseConnection) != 0)
				connection.Close ();
		}

		public SybaseParameter CreateParameter () 
		{
			return new SybaseParameter ();
		}

		public int ExecuteNonQuery ()
		{
			int result = connection.Tds.ExecuteNonQuery (ValidateQuery ("ExecuteNonQuery"));
			connection.CheckForErrors ();
			return result;
		}

		public SybaseDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public SybaseDataReader ExecuteReader (CommandBehavior behavior)
		{
			this.behavior = behavior;
			connection.Tds.ExecuteQuery (ValidateQuery ("ExecuteReader"));
			connection.CheckForErrors ();
			connection.DataReaderOpen = true;
			return new SybaseDataReader (this);
		}

		public object ExecuteScalar ()
		{
			connection.Tds.ExecuteQuery (ValidateQuery ("ExecuteScalar"));

			bool moreResults = connection.Tds.NextResult ();
			connection.CheckForErrors ();

			if (!moreResults)
				return null;

			moreResults = connection.Tds.NextRow ();
			connection.CheckForErrors ();

			if (!moreResults)
				return null;

			object result = connection.Tds.ColumnValues[0];
			CloseDataReader (true);
			return result;
		}

		static string FormatParameter (SybaseParameter parameter)
		{
			if (parameter.Value == null)
				return "NULL";

			switch (parameter.SybaseType) {
				case SybaseType.BigInt :
				case SybaseType.Bit :
				case SybaseType.Decimal :
				case SybaseType.Float :
				case SybaseType.Int :
				case SybaseType.Money :
				case SybaseType.Real :
				case SybaseType.SmallInt :
				case SybaseType.SmallMoney :
				case SybaseType.TinyInt :
					return parameter.Value.ToString ();
				default:
					return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
		}

		static string FormatQuery (string commandText, CommandType commandType, SybaseParameterCollection parameters)
		{
			StringBuilder result = new StringBuilder ();

			switch (commandType) {
			case CommandType.Text :
				return commandText;
			case CommandType.TableDirect :
				return String.Format ("SELECT * FROM {0}", commandText);
			case CommandType.StoredProcedure :

				StringBuilder parms = new StringBuilder ();
				StringBuilder declarations = new StringBuilder ();

				foreach (SybaseParameter parameter in parameters) {
					switch (parameter.Direction) {
					case ParameterDirection.Input :
						if (parms.Length > 0)
							result.Append (",");
						parms.Append (FormatParameter (parameter));
						break;
					case ParameterDirection.Output :
						if (parms.Length > 0)
							parms.Append (",");
						parms.Append (parameter.ParameterName);
						parms.Append (" OUT");

						if (declarations.Length == 0)
							declarations.Append ("DECLARE ");
						else
							declarations.Append (",");

						declarations.Append (parameter.Prepare ());
						break;
					default :
						throw new NotImplementedException ("Only support input and output parameters.");
					}
				}
				result.Append (declarations.ToString ());
				result.Append (" EXEC ");
				result.Append (commandText);
				result.Append (" ");
				result.Append (parms);
				return result.ToString ();
			default:
				throw new InvalidOperationException ("The CommandType was not recognized.");
			}
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
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

		void IDisposable.Dispose ()
		{
			Dispose (true);
		}

		public void Prepare ()
		{
			bool prependComma = false;
			Guid uniqueId = Guid.NewGuid ();
			string procedureName = String.Format ("#mono#{0}", uniqueId.ToString ("N"));
			StringBuilder procedureString = new StringBuilder ();

			procedureString.Append ("CREATE PROC ");
			procedureString.Append (procedureName);
			procedureString.Append (" (");

			foreach (SybaseParameter parameter in parameters) {
				if (prependComma)
					procedureString.Append (", ");
				else
					prependComma = true;
				procedureString.Append (parameter.Prepare ());
				if (parameter.Direction == ParameterDirection.Output)
					procedureString.Append (" OUT");
			}
				
			procedureString.Append (") AS ");
			procedureString.Append (commandText);
			string cmdText = FormatQuery (procedureName, CommandType.StoredProcedure, parameters);
			connection.Tds.ExecuteNonQuery (procedureString.ToString ());
			procedureCache[commandText] = cmdText;
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		string ValidateQuery (string methodName)
		{
			if (connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", methodName));
			if (connection.Transaction != null && transaction != connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.", methodName));
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");

			string sql = procedureCache[commandText];
			if (sql == null)
				sql = FormatQuery (commandText, commandType, parameters);
		
			if ((behavior & CommandBehavior.KeyInfo) != 0)
				sql += " FOR BROWSE";

			return sql;
		}

		#endregion // Methods
	}
}
