//
// Mono.Data.TdsClient.TdsCommand.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
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
		{
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
			set { connection = value; }
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

		public TdsTransaction Transaction {
			get { return transaction; }
			set { transaction = value; }
		}

		[System.MonoTODO]
		public UpdateRowSource UpdatedRowSource {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

                #region Methods

		[System.MonoTODO]
		public void Cancel ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		TdsParameter CreateParameter ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		public int ExecuteNonQuery ()
		{
			throw new NotImplementedException ();
		}

		public TdsDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		[System.MonoTODO]
		public TdsDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
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

		[System.MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

		/*
		internal void SkipToEnd ()
		{
			if (tds != null)
				while (GetMoreResults (tds, false) || updateCount != 1);
		}
		*/

                #endregion // Methods
	}
}
