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
			return connection.Tds.ExecuteNonQuery (FormatQuery (commandText, commandType));
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
			connection.Tds.ExecuteQuery (FormatQuery (commandText, commandType));
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

		static string FormatQuery (string commandText, CommandType commandType)
		{
			switch (commandType) {
			case CommandType.Text :
				return commandText;
			case CommandType.TableDirect :
				return String.Format ("SELECT * FROM {0}", commandText);
			case CommandType.StoredProcedure :
				return String.Format ("EXEC {0}", commandText);
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

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

		public void ResetCommandTimeout ()
		{
			commandTimeout = 30;
		}

		#endregion // Methods
	}
}
