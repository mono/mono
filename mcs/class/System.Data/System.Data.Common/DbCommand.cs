//
// System.Data.Common.DbCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public abstract class DbCommand : Component, IDbCommand, IDisposable
	{
		protected DbCommand ()
		{
		}

		#region Properties

		public abstract string CommandText { get; set; }
		public abstract int CommandTimeout { get; set; }
		public abstract CommandType CommandType { get; set; }

		public DbConnection Connection {
			get { return DbConnection; }
			set { DbConnection = value; }
		}

		protected abstract DbConnection DbConnection { get; set; }
		protected abstract DbParameterCollection DbParameterCollection { get; set; }
		protected abstract DbTransaction DbTransaction { get; set; }
		public abstract bool DesignTimeVisible { get; set; }

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { Connection = (DbConnection) value; }
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { Transaction = (DbTransaction) value; }
		}

		[MonoTODO]
		public virtual DbCommandOptionalFeatures OptionalFeatures { 
			get { throw new NotImplementedException (); }
		}

		public DbParameterCollection Parameters {
			get { return DbParameterCollection; }
		}

		public DbTransaction Transaction {
			get { return DbTransaction; }
			set { DbTransaction = value; }
		}

		public abstract UpdateRowSource UpdatedRowSource { get; set; }

		#endregion // Properties

		#region Methods

		public abstract void Cancel ();
		protected abstract DbParameter CreateDbParameter ();

		public DbParameter CreateParameter ()
		{
			return CreateDbParameter ();
		}

		protected abstract DbDataReader ExecuteDbDataReader (CommandBehavior behavior);

		[MonoTODO]
		protected virtual DbDataReader ExecuteDbPageReader (CommandBehavior behavior, int startRecord, int maxRecords)
		{
			throw new NotImplementedException ();
		}

		public abstract int ExecuteNonQuery ();
		public DbDataReader ExecutePageReader (CommandBehavior behavior, int startRecord, int maxRecords)
		{
			return ExecuteDbPageReader (behavior, startRecord, maxRecords);
		}

		[MonoTODO]
		public DbDataReader ExecuteReader ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DbDataReader ExecuteReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		public abstract object ExecuteScalar ();

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

		public abstract void Prepare ();
		
		#endregion // Methods

	}
}

#endif
