//
// Mono.Data.SybaseClient.SybaseTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient;
using System;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseTransaction : MarshalByRefObject, IDbTransaction, IDisposable
	{
		#region Fields

		SybaseConnection connection;
		IsolationLevel isolationLevel;
		bool isOpen;

		#endregion

		#region Constructors

		internal SybaseTransaction (SybaseConnection connection, IsolationLevel isolevel)
		{
			SetIsolationLevel (connection, isolevel);
			this.connection = connection;
			this.isolationLevel = isolevel;
			isOpen = true;
		}

		#endregion // Constructors

		#region Properties

		public SybaseConnection Connection {
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

		static void SetIsolationLevel (SybaseConnection connection, IsolationLevel isolevel)
		{
			string commandText = "SET TRANSACTION ISOLATION LEVEL ";

			switch (isolevel) {
			case IsolationLevel.Chaos :
				commandText += "CHAOS";
				break;
			case IsolationLevel.ReadCommitted :
				commandText += "READ COMMITTED";
				break;
			case IsolationLevel.ReadUncommitted :
				commandText += "READ UNCOMMITTED";
				break;
			case IsolationLevel.RepeatableRead :
				commandText += "REPEATABLE READ";
				break;
			case IsolationLevel.Serializable :
				commandText += "SERIALIZABLE";
				break;
			default :
				return;
			}
			connection.Tds.ExecuteNonQuery (commandText);
		}

		public void Commit ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.ExecuteNonQuery ("IF @@TRANCOUNT>0 COMMIT TRAN");
			isOpen = false;
		}		

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Rollback ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.ExecuteNonQuery ("IF @@TRANCOUNT>0 ROLLBACK TRAN");
			isOpen = false;
		}

		public void Rollback (string transactionName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.ExecuteNonQuery (String.Format ("IF @@TRANCOUNT > 0 ROLLBACK TRAN {0}", transactionName));
			isOpen = false;
		}

		public void Save (string savePointName)
		{
			if (!isOpen)
				throw new InvalidOperationException ("The Transaction was not open.");
			connection.Tds.ExecuteNonQuery (String.Format ("SAVE TRAN {0}", savePointName));
		}

		#endregion // Methods
	}
}
