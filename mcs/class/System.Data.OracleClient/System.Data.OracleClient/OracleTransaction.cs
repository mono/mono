//
// OracleTransaction.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;

namespace System.Data.OracleClient {
	public sealed class OracleTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Fields

		OracleConnection connection;
		IsolationLevel isolationLevel;
		bool disposed = false;
		bool isOpen;

		#endregion // Fields

		#region Constructors

		internal OracleTransaction (OracleConnection connection, IsolationLevel isolevel, IntPtr transactionHandle)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
			isOpen = true;
		}

		#endregion // Constructors

		#region Properties

		internal bool IsOpen {
			get { return isOpen; }
		}

		public OracleConnection Connection {
			get { return connection; }
		}

		public IsolationLevel IsolationLevel {
			get { return isolationLevel; }
		}

		IDbConnection IDbTransaction.Connection {
			get { return Connection; }
		}

		#endregion // Properties

		#region Methods

		public void Commit ()
		{
			Int32 status = Connection.Oci.CommitTransaction ();
			if (status != 0)
				throw new Exception("Error: Unable to connect: " + status.ToString() + ": " + Connection.Oci.CheckError(status));
			else {
				Connection.Transaction = null;
				isOpen = false;
			}
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Rollback ();
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public void Rollback ()
		{
			Int32 status = Connection.Oci.RollbackTransaction ();
			if (status != 0)
				throw new Exception("Error: Unable to connect: " + status.ToString() + ": " + Connection.Oci.CheckError(status));
			else {
				Connection.Transaction = null;
				isOpen = false;
			}
		}

		#endregion // Methods
	}
}
