//
// Mono.Data.TdsClient.Internal.TdsTransactionInternal.cs
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
        internal class TdsTransactionInternal : Component, ICloneable, IDbTransaction
	{
		#region Fields

		TdsConnectionInternal connection;
		IsolationLevel isolationLevel;

		#endregion // Fields

		#region Constructors

		public TdsTransactionInternal (TdsConnectionInternal connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
		}

		#endregion // Constructors

		#region Properties

		TdsConnectionInternal Connection {
			get { return connection; }
		}

		IDbConnection IDbTransaction.Connection {
			get { return Connection; }
		}

		IsolationLevel IDbTransaction.IsolationLevel {
			get { return isolationLevel; }
		}

		#endregion // Properties

                #region Methods

		public void Commit()
		{
			throw new NotImplementedException ();
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }


		public void Rollback ()
		{
			throw new NotImplementedException ();
		}

                #endregion // Methods
	}
}
