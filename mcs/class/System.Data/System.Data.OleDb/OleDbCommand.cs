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
using System.Runtime.InteropServices;

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
		IntPtr gdaCommand;

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
			gdaCommand = IntPtr.Zero;
		}

		public OleDbCommand (string cmdText) : this ()
		{
			CommandText = cmdText;
		}

		public OleDbCommand (string cmdText, OleDbConnection connection)
			: this (cmdText)
		{
			Connection = connection;
		}

		public OleDbCommand (string cmdText,
				     OleDbConnection connection,
				     OleDbTransaction transaction) : this (cmdText, connection)
		{
			this.transaction = transaction;
		}

		#endregion // Constructors

		#region Properties
	
		public string CommandText 
		{
			get {
				return commandText;
			}
			set { 
				commandText = value;
			}
		}

		public int CommandTimeout {
			get {
				return timeout;
			}
			set {
				timeout = value;
			}
		}

		public CommandType CommandType { 
			get {
				return commandType;
			}
			set {
				commandType = value;
			}
		}

		public OleDbConnection Connection { 
			get {
				return connection;
			}
			set {
				connection = value;
			}
		}

		public bool DesignTimeVisible { 
			get {
				return designTimeVisible;
			}
			set {
				designTimeVisible = value;
			}
		}

		public OleDbParameterCollection Parameters {
			get {
				return parameters;
			}
			set {
				parameters = value;
			}
		}

		public OleDbTransaction Transaction {
			get {
				return transaction;
			}
			set {
				transaction = value;
			}
		}

		public UpdateRowSource UpdatedRowSource { 
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		IDbConnection IDbCommand.Connection {
			get {
				return Connection;
			}
			set {
				Connection = (OleDbConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters  {
			get {
				return Parameters;
			}
		}

		IDbTransaction IDbCommand.Transaction  {
			get {
				return Transaction;
			}
			set {
				Transaction = (OleDbTransaction) value;
			}
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

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		private void SetupGdaCommand ()
		{
			GdaCommandType type;
			
			switch (commandType) {
			case CommandType.TableDirect :
				type = GdaCommandType.Table;
				break;
			case CommandType.StoredProcedure :
				type = GdaCommandType.Procedure;
				break;
			case CommandType.Text :
			default :
				type = GdaCommandType.Sql;
				break;
			}
			
			if (gdaCommand != IntPtr.Zero) {
				libgda.gda_command_set_text (gdaCommand, commandText);
				libgda.gda_command_set_command_type (gdaCommand, type);
			} else {
				gdaCommand = libgda.gda_command_new (commandText, type, 0);
			}

			//libgda.gda_command_set_transaction 
		}
		
		public int ExecuteNonQuery ()
		{
			if (connection == null)
				throw new InvalidOperationException ("connection == null");
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("State == Closed");
			// FIXME: a third check is mentioned in .NET docs

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			SetupGdaCommand ();
			return libgda.gda_connection_execute_non_query (gdaConnection,
									(IntPtr) gdaCommand,
									gdaParameterList);
		}

		public OleDbDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		public OleDbDataReader ExecuteReader (CommandBehavior behavior)
		{
			ArrayList results = new ArrayList ();
			IntPtr rs_list;
			GdaList glist_node;

			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException ("State != Open");

			this.behavior = behavior;

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			/* execute the command */
			SetupGdaCommand ();
			rs_list = libgda.gda_connection_execute_command (
				gdaConnection,
				gdaCommand,
				gdaParameterList);
			if (rs_list != IntPtr.Zero) {
				glist_node = (GdaList) Marshal.PtrToStructure (rs_list, typeof (GdaList));

				while (glist_node != null) {
					results.Add (glist_node.data);
					if (glist_node.next == IntPtr.Zero)
						break;

					glist_node = (GdaList) Marshal.PtrToStructure (glist_node.next,
										       typeof (GdaList));
				}
				dataReader = new OleDbDataReader (this, results);
				dataReader.NextResult ();
			}

			return dataReader;
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
		
		public object ExecuteScalar ()
		{
			SetupGdaCommand ();
			OleDbDataReader reader = ExecuteReader ();
			if (reader == null) {
				return null;
			}
			if (!reader.Read ()) {
				reader.Close ();
				return null;
			}
			object o = reader.GetValue (0);
			reader.Close ();
			return o;
		}

		[MonoTODO]
		object ICloneable.Clone ()
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
	}
}
