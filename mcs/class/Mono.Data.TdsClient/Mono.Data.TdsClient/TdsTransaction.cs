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

		TdsConnection connection;
		IsolationLevel isolationLevel;

		#endregion // Fields

		#region Constructors

		public TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
			Begin ();
		}

		#endregion // Constructors

		#region Properties

		TdsConnection Connection {
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

		private void Begin ()
		{
			connection.AllocateTds ().BeginTransaction ();
		}

		public void Commit ()
		{
			connection.AllocateTds ().CommitTransaction ();
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
		public void Rollback ()
		{
			connection.AllocateTds ().RollbackTransaction ();
		}

		public void Save (string savePointName)
		{
			connection.AllocateTds ().SaveTransaction (savePointName);
		}

                #endregion // Methods
	}
}
