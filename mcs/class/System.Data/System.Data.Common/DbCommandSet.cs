//
// System.Data.Common.DbCommandSet
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
	public abstract class DbCommandSet : IDisposable
	{
		#region Constructors

		protected DbCommandSet ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int CommandCount { get; }
		public abstract int CommandTimeout { get; set; }

		public DbConnection Connection {
			get { return DbConnection; }
			set { DbConnection = value; }
		}

		protected abstract DbConnection DbConnection { get; set; }
		protected abstract DbTransaction DbTransaction { get; set; }

		public DbTransaction Transaction {
			get { return DbTransaction; }
		}

		#endregion // Properties

		#region Methods

		public abstract void Append (DbCommand command);
		public abstract void Cancel ();
		public abstract void Clear ();
		public abstract void CopyToParameter (int commandIndex, int parameterIndex, DbParameter destination);
		public abstract void CopyToParameter (int commandIndex, string parameterName, DbParameter destination);
		public abstract void CopyToParameterCollection (int commandIndex, DbParameterCollection destination);
		public abstract void Dispose ();
		public abstract DbDataReader ExecuteDbDataReader (CommandBehavior behavior);
		public abstract int ExecuteNonQuery ();

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

		public abstract int GetParameterCount (int commandIndex);
		
		#endregion // Methods

	}
}

#endif
