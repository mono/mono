//
// Mono.Data.TdsClient.TdsTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.TdsClient {
	public sealed class TdsTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Fields

		bool disposed = false;

		TdsConnection connection;
		IsolationLevel isolationLevel;
		bool isOpen;

		#endregion

		#region Constructors

		internal TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;
			isOpen = true;
		}

		#endregion // Constructors

		#region Properties

		public TdsConnection Connection {
			get { return connection; }
		}

		internal bool IsOpen {
			get { return isOpen; }
		}

		public IsolationLevel IsolationLevel {
			get { return isolationLevel; }
		}
		
		IDbConnection IDbTransaction.Connection	{
			get { return Connection; }
		}

		#endregion // Properties
               
		#region Methods

		public void Commit ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.Execute ("COMMIT TRANSACTION");
			connection.Transaction = null;
			isOpen = false;
		}		

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing)
					Rollback ();
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
			Rollback (String.Empty);
		}

		public void Rollback (string transactionName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.Execute (String.Format ("ROLLBACK TRANSACTION {0}", transactionName));
			isOpen = false;
		}

		public void Save (string savePointName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.Execute (String.Format ("SAVE TRANSACTION {0}", savePointName));
		}

		#endregion // Methods
	}
}
