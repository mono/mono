//
// Mono.Data.TdsClient.TdsCommand.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.Tds.Protocol;
using System;
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient {
        public class TdsCommand : Component, ICloneable, IDbCommand
	{
		#region Fields

		string commandText;
		int commandTimeout;
		CommandType commandType;
		TdsConnection connection;
		TdsParameterCollection parameters;
		TdsTransaction transaction;

		#endregion // Fields

		#region Constructors

		public TdsCommand ()
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
			this.transaction = transaction;
			this.commandType = CommandType.Text;
			this.connection = connection;
		}

		#endregion // Constructors

		#region Properties

		public string CommandText {
			get { return commandText; }
			set { commandText = value; }
		}

		public int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		public CommandType CommandType {
			get { return commandType; }
			set { commandType = value; }
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

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is TdsConnection)) 
					throw new ArgumentException ();
				Connection = (TdsConnection) value; 	
			}
		}

		IDataParameterCollection IDbCommand.Parameters {
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

		public TdsParameterCollection Parameters {
			get { return parameters; }
		}

		internal ITds Tds {
			get { return connection.Tds; }
		}

		public TdsTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		[MonoTODO]
		public UpdateRowSource UpdatedRowSource {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

                #region Methods

		[MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		TdsParameter CreateParameter ()
		{
			throw new NotImplementedException ();
		}

		public int ExecuteNonQuery ()
		{
			ValidateCommand ("ExecuteNonQuery");
			return connection.Tds.ExecuteNonQuery (FormatQuery (commandText, commandType));
		}

		public TdsDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		public TdsDataReader ExecuteReader (CommandBehavior behavior)
		{
			ValidateCommand ("ExecuteReader");
			connection.DataReader = new TdsDataReader (this);
			return connection.DataReader;
		}

		[MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		private static string FormatQuery (string commandText, CommandType commandType)
		{
			switch (commandType) {
			case CommandType.Text :
				return commandText;
			case CommandType.TableDirect :
				return String.Format ("select * from {0}", commandText);
			case CommandType.StoredProcedure :
				return String.Format ("exec {0}", commandText);
			}
			throw new InvalidOperationException ("Invalid command type");
		}

		[MonoTODO]
                object ICloneable.Clone()
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
			if (connection == null)
				throw new InvalidOperationException (String.Format ("{0} requires a Connection object to continue.", method));
			if (connection.Transaction != null && transaction != connection.Transaction)
				throw new InvalidOperationException ("The Connection object does not have the same transaction as the command object.");
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException (String.Format ("ExecuteNonQuery requires an open Connection object to continue. This connection is closed.", method));
			if (commandText == String.Empty || commandText == null)
				throw new InvalidOperationException ("The command text for this Command has not been set.");
			if (connection.DataReader != null)
				throw new InvalidOperationException ("There is already an open DataReader associated with this Connection which must be closed first.");
		}

                #endregion // Methods
	}
}
