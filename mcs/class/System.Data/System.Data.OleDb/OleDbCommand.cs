//
// System.Data.OleDb.OleDbCommand
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace System.Data.OleDb
{
	/// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	public sealed class OleDbCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		string commandText;
		int timeout;
		CommandType commandType;
		OleDbConnection connection;
		OleDbParameterCollection parameters;
		OleDbTransaction transaction;
		bool designTimeVisible;
		OleDbDataReader dataReader;
		CommandBehavior behavior;
		ArrayList gdaCommands;
		ArrayList gdaResults;

		#endregion // Fields

		#region Constructors

		public OleDbCommand ()
	        {
			commandText = String.Empty;
			timeout = 30; // default timeout per .NET
			commandType = CommandType.Text;
			connection = null;
			parameters = new OleDbParameterCollection ();
			transaction = null;
			designTimeVisible = false;
			dataReader = null;
			behavior = CommandBehavior.Default;
			gdaCommands = new ArrayList ();
			gdaResults = new ArrayList ();
		}

		public OleDbCommand (string cmdText)
			: this ()
		{
			CommandText = cmdText;
		}

		public OleDbCommand (string cmdText, OleDbConnection connection)
			: this (cmdText)
		{
			Connection = connection;
		}

		public OleDbCommand (string cmdText, OleDbConnection connection, OleDbTransaction transaction)
			: this (cmdText, connection)
		{
			this.transaction = transaction;
		}

		#endregion // Constructors

		#region Properties
	
		public string CommandText 
		{
			get { return commandText; }
			set { 
				string[] queries = value.Split (new Char[] {';'});
				gdaCommands.Clear ();

				foreach (string query in queries) 
					gdaCommands.Add (libgda.gda_command_new (query, 0, 0));

				commandText = value; 
			}
		}

		public int CommandTimeout {
			get { return timeout; }
			set { timeout = value; }
		}

		public CommandType CommandType { 
			get { return commandType; }
			set { commandType = value; }
		}

		public OleDbConnection Connection { 
			get { return connection; }
			set { connection = value; }
		}

		public bool DesignTimeVisible { 
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		public OleDbParameterCollection Parameters {
			get { return parameters; }
			set { parameters = value; }
		}

		public OleDbTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		public UpdateRowSource UpdatedRowSource { 
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { Connection = (OleDbConnection) value; }
		}

		IDataParameterCollection IDbCommand.Parameters  {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction  {
			get { return Transaction; }
			set { Transaction = (OleDbTransaction) value; }
		}

		internal ArrayList GdaResults {
			get { return gdaResults; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public void Cancel () 
		{
			throw new NotImplementedException ();
		}

		public OleDbParameter CreateParameter ()
		{
			return new OleDbParameter ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		public int ExecuteNonQuery ()
		{
			if (connection == null)
				throw new InvalidOperationException ();
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			// FIXME: a third check is mentioned in .NET docs

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			return libgda.gda_connection_execute_non_query (gdaConnection, (IntPtr) gdaCommands[0], gdaParameterList);
		}

		public OleDbDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public OleDbDataReader ExecuteReader (CommandBehavior behavior)
		{
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException ();

			this.behavior = behavior;

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			foreach (IntPtr gdaCommand in gdaCommands) 
				GdaResults.Add (libgda.gda_connection_execute_single_command (gdaConnection, gdaCommand, gdaParameterList));

			dataReader = new OleDbDataReader (this);

			dataReader.NextResult ();

			return dataReader;
		}

		[MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		IDbDataParameter IDbCommand.CreateParameter ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();	
		}

		public void ResetCommandTimeout ()
		{
			timeout = 30;
		}

		#endregion

		#region Internal Methods

		// only meant to be used between OleDbConnectioin,
		// OleDbCommand, and OleDbDataReader
		internal void OpenReader (OleDbDataReader reader) 
		{
			connection.OpenReader (reader);
		}

		#endregion

	}
}
