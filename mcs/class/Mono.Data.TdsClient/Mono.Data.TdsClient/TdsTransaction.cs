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

		[System.MonoTODO]
		public void Commit()
		{
                        throw new NotImplementedException ();
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		[System.MonoTODO]
		public void Rollback ()
		{
                        throw new NotImplementedException ();
		}

                #endregion // Methods
	}
}
