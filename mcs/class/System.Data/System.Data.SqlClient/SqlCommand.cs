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

		int commandTimeout;
		bool designTimeVisible;
		string commandText;

		CommandType commandType;
		SqlConnection connection;
		SqlTransaction transaction;

		SqlParameterCollection parameters = new SqlParameterCollection ();

		// SqlDataReader state data for ExecuteReader()
		private SqlDataReader dataReader = null;
		private string[] queries = null;
		private int currentQuery = -1;
		private CommandBehavior cmdBehavior = CommandBehavior.Default;

		Hashtable procedureCache = new Hashtable ();

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
			this.designTimeVisible = false;
			this.commandTimeout = 30;
		}

		#endregion // Constructors

		#region Properties

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

		public SqlConnection Connection {
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

		public SqlParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return connection.Tds; }
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

		public SqlTransaction Transaction {
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
			if (connection.Tds.Errors.Count > 0)
				throw SqlException.FromTdsError (connection.Tds.Errors);
		}

		internal void CloseDataReader (bool moreResults)
		{
			while (moreResults)
				moreResults = connection.Tds.NextResult ();

			if (connection.Tds.OutputParameters.Count > 0)
			{
				Console.WriteLine ("Parameters found!");
				foreach (object o in connection.Tds.OutputParameters)
					Console.WriteLine (o);
			}
		}

		public SqlParameter CreateParameter () 
		{
			return new SqlParameter ();
		}

		public int ExecuteNonQuery ()
		{
			if (connection == null)
				throw new InvalidOperationException ("ExecuteNonQuery requires a Connection object to continue.");
			if (connection.Transaction != null && transaction != connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.");
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			int result = connection.Tds.ExecuteNonQuery (FormatQuery (commandText, commandType, parameters));
			if (connection.Tds.Errors.Count > 0)
				throw SqlException.FromTdsError (connection.Tds.Errors);
			return result;
		}

		public SqlDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public SqlDataReader ExecuteReader (CommandBehavior behavior)
		{
			if (connection == null)
				throw new InvalidOperationException ("ExecuteReader requires a Connection object to continue.");
			if (connection.Transaction != null && transaction != connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException ("ExecuteReader requires an open Connection object to continue. This connection is closed.");
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");

			string sql;
			if (procedureCache.ContainsKey (commandText))
				sql = FormatQuery ((string) procedureCache[commandText], CommandType.StoredProcedure, parameters);
			else
				sql = FormatQuery (commandText, commandType, parameters);

			connection.Tds.ExecuteQuery (sql);
			connection.DataReaderOpen = true;
			return new SqlDataReader (this);
		}

		[MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlReader ExecuteXmlReader ()
		{
			throw new NotImplementedException ();
		}

		static string FormatParameter (SqlParameter parameter)
		{
			if (parameter.Value == null)
				return "NULL";

			switch (parameter.SqlDbType) {
				case SqlDbType.BigInt :
				case SqlDbType.Bit :
				case SqlDbType.Decimal :
				case SqlDbType.Float :
				case SqlDbType.Int :
				case SqlDbType.Money :
				case SqlDbType.Real :
				case SqlDbType.SmallInt :
				case SqlDbType.SmallMoney :
				case SqlDbType.TinyInt :
					return parameter.Value.ToString ();
				default:
					return String.Format ("'{0}'", parameter.Value.ToString ().Replace ("'", "''"));
			}
		}

		static string FormatQuery (string commandText, CommandType commandType, SqlParameterCollection parameters)
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

				foreach (SqlParameter parameter in parameters) {
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

						declarations.Append (GetFormalParameterName (parameter));
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

		static string GetFormalParameterName (SqlParameter parameter)
		{
			StringBuilder result = new StringBuilder ();
			result.Append (parameter.ParameterName);
			result.Append (" ");
			result.Append (parameter.SqlDbType.ToString ());

			switch (parameter.SqlDbType) {
			case SqlDbType.Image :
			case SqlDbType.NVarChar :
			case SqlDbType.VarBinary :
			case SqlDbType.VarChar :
				if (parameter.Size == 0)
					throw new InvalidOperationException ("All variable length parameters must have an explicitly set non-zero size.");
				result.Append ("(");
				result.Append (parameter.Size.ToString ());
				result.Append (")");
				break;
			case SqlDbType.Decimal :
			case SqlDbType.Money :
			case SqlDbType.SmallMoney :
				result.Append ("(");
				result.Append (parameter.Precision.ToString ());
				result.Append (",");
				result.Append (parameter.Scale.ToString ());
				result.Append (")");
				break;
			default:
				break;
			}

			return result.ToString ();
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

			foreach (SqlParameter parameter in parameters) {
				if (prependComma)
					procedureString.Append (", ");
				else
					prependComma = true;
				procedureString.Append (GetFormalParameterName (parameter));
				if (parameter.Direction == ParameterDirection.Output)
					procedureString.Append (" OUT");
			}
				
			procedureString.Append (") AS ");
			procedureString.Append (commandText);
			connection.Tds.ExecuteNonQuery (procedureString.ToString ());
			procedureCache[commandText] = procedureName;
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		#endregion // Methods
	}
}
