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
		TdsInternal tds;
		int updateCount;

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

		[System.MonoTODO]
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
		public IDbDataParameter CreateParameter ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		public int ExecuteNonQuery ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		IDataReader IDbCommand.ExecuteReader ()
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO]
		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}
/*
		[System.MonoTODO]
		public TdsDataReaderInternal ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		public TdsDataReaderInternal ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}
*/

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

		[System.MonoTODO]
		private bool GetMoreResults (TdsInternal tds, bool allowTdsRelease)
		{
			return false;
		}

		[System.MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();
		}

		internal void SkipToEnd ()
		{
			if (tds != null)
				while (GetMoreResults (tds, false) || updateCount != 1);
		}

                #endregion // Methods
	}
}
