//
// Mono.Data.TdsClient.TdsTransaction.cs
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
        public class TdsTransaction : Component, ICloneable, IDbTransaction
	{
		#region Fields

		TdsConnection connection;
		IsolationLevel isolationLevel;
		bool isOpen;

		#endregion // Fields

		#region Constructors

		internal TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;

			connection.Tds.ExecuteNonQuery ("BEGIN TRAN");
			isOpen = true;
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

		public bool IsOpen {	
			get { return isOpen; }
		}

		#endregion // Properties

                #region Methods

		public void Commit ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.ExecuteNonQuery ("IF @@TRANCOUNT>0 COMMIT TRAN");
			isOpen = false;
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		public void Rollback ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.ExecuteNonQuery ("IF @@TRANCOUNT>0 ROLLBACK TRAN");
			isOpen = false;
		}

		public void Save (string savePointName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.ExecuteNonQuery (String.Format ("SAVE TRAN {0}", savePointName));
		}

                #endregion // Methods
	}
}
