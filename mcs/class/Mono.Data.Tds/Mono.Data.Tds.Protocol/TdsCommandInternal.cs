//
// Mono.Data.TdsClient.Internal.TdsCommandInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient.Internal {
        internal sealed class TdsCommandInternal : Component, ICloneable, IDbCommand
	{
		#region Fields

		string commandText;
		int commandTimeout;
		CommandType commandType;
		TdsConnectionInternal connection;
		TdsTransactionInternal transaction;

		#endregion // Fields

		#region Constructors

		public TdsCommandInternal ()
		{
			commandText = String.Empty;
			connection = null;
			transaction = null;
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

		public TdsConnectionInternal Connection {
			get { return connection; }
			set { connection = value; }
		}

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { 
				if (!(value is TdsConnectionInternal))
					throw new ArgumentException ();
				Connection = (TdsConnectionInternal) value; 
			}
		}

		[MonoTODO]
		IDataParameterCollection IDbCommand.Parameters {
			get { throw new NotImplementedException (); }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set {
				if (!(value is TdsTransactionInternal))
					throw new ArgumentException ();
				Transaction = (TdsTransactionInternal) value;
			}
		}

		public TdsTransactionInternal Transaction {
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
		public IDbDataParameter CreateParameter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int ExecuteNonQuery ()
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
/*
		[MonoTODO]
		public TdsDataReaderInternal ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		public TdsDataReaderInternal ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}
*/

		[MonoTODO]
		public object ExecuteScalar ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

                #endregion // Methods
	}
}
