// 
// OracleCommand.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;

namespace System.Data.OracleClient {
	public class OracleCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		bool disposed = false;
		CommandBehavior behavior;
		string commandText;
		CommandType commandType;
		OracleConnection connection;
		bool designTimeVisible;
		OracleParameterCollection parameters;
		OracleTransaction transaction;
		UpdateRowSource updatedRowSource;
		
		IntPtr statementHandle;
		OciStatementType statementType;

		#endregion // Fields

		#region Constructors

		public OracleCommand ()
			: this (String.Empty, null, null)
		{
		}

		public OracleCommand (string commandText)
			: this (commandText, null, null)
		{
		}

		public OracleCommand (string commandText, OracleConnection connection)
			: this (commandText, connection, null)
		{
		}

		public OracleCommand (string commandText, OracleConnection connection, OracleTransaction tx)
		{
			this.commandText = commandText;
			this.connection = connection;
			this.transaction = tx;
			this.commandType = CommandType.Text;
			this.updatedRowSource = UpdateRowSource.Both;
			this.designTimeVisible = false;
                        parameters = new OracleParameterCollection (this);

		}

		#endregion // Constructors

		#region Properties

		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public CommandType CommandType {
			get { return commandType; }
			set { commandType = value; }
		}

		public OracleConnection Connection {
			get { return connection; }
			set { connection = value; }
		}

		public bool DesignTimeVisible {
			get { return designTimeVisible; }
			set { designTimeVisible = value; }
		}

		int IDbCommand.CommandTimeout {
			get { throw new InvalidOperationException (); }
			set { throw new InvalidOperationException (); }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is OracleConnection))
					throw new InvalidCastException ("The value was not a valid OracleConnection.");
				Connection = (OracleConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { 
				if (!(value is OracleTransaction))
					throw new ArgumentException ();
				Transaction = (OracleTransaction) value; 
			}
		}

		public OracleParameterCollection Parameters {
			get { return parameters; }
		}

		public OracleTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		public UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set { updatedRowSource = value; }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Clone ()
		{
			throw new NotImplementedException ();
		}

		/*
		[MonoTODO("Still need to Dispose correctly")]
		public new void Dispose () 
		{	
			if (!disposed) {
				if (statementHandle != IntPtr.Zero) {
					OciGlue.OCIHandleFree (statementHandle, OciHandleType.Statement);
					statementHandle = IntPtr.Zero;
				}
				//base.Dispose ();
			}
		}
		
		[MonoTODO("still need to Finalize correctly")]
		~OracleCommand() {
			Dispose();
		}
		*/

		internal void CloseDataReader ()
		{
			Connection.DataReader = null;
			if ((behavior & CommandBehavior.CloseConnection) != 0)
				Connection.Close ();
		}

		public OracleParameter CreateParameter ()
		{
			return new OracleParameter ();
		}

		public int ExecuteNonQuery () 
		{
			int rowsAffected = -1;

			ValidateCommand ("ExecuteNonQuery");
			statementHandle = Connection.Oci.PrepareStatement (CommandText);
			statementType = Connection.Oci.GetStatementType (statementHandle);
			Connection.Oci.ExecuteStatement (statementHandle, statementType);

			return rowsAffected;
		}

		[MonoTODO]
		public int ExecuteOracleNonQuery (out OracleString rowid)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ExecuteOracleScalar ()
		{
			throw new NotImplementedException ();
		}

		public OracleDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		[MonoTODO]
		public OracleDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ExecuteScalar ()
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

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

		private void ValidateCommand (string method)
		{
			if (Connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", method));
			if (Connection.Transaction != null && Transaction != Connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (Connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("{0} requires an open Connection object to continue. This connection is closed.", method));
			if (CommandText == String.Empty || CommandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			if (Connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
		}

		#endregion // Methods
	}
}
