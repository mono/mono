//
// Mono.Data.TdsClient.TdsTransaction.cs
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
        public class TdsTransaction : Component, ICloneable, IDbTransaction
	{
		#region Fields

		internal TdsTransactionInternal transaction;

		#endregion // Fields

		#region Constructors

		public TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
		{
			transaction = new TdsTransactionInternal (connection.connection, isolevel);
		}

		#endregion // Constructors

		#region Properties

		IDbConnection IDbTransaction.Connection {
			get { return ((IDbTransaction) transaction).Connection; }
		}

		IsolationLevel IDbTransaction.IsolationLevel {
			get { return ((IDbTransaction) transaction).IsolationLevel; }
		}

		#endregion // Properties

                #region Methods

		public void Commit()
		{
			transaction.Commit ();
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }


		public void Rollback ()
		{
			transaction.Rollback ();
		}

                #endregion // Methods
	}
}
