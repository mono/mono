//
// System.Data.SqlClient.SqlTransaction.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc. 2002
//
using System;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a transaction to be performed on a SQL database.
	/// </summary>
	// public sealed class SqlTransaction : MarshalByRefObject,
	//	IDbTransaction, IDisposable
	public sealed class SqlTransaction : IDbTransaction
	{
		#region Fields

		private SqlConnection conn = null;
		//        How do you get/set the 
		//        IsolationLevel in PostgreSQL?
		private IsolationLevel isolationLevel =	
			IsolationLevel.ReadCommitted;
		// There are two IsolationLevel's for PostgreSQL:
		//    ReadCommitted and Serializable, 
		// but ReadCommitted is the default 
		
		#endregion
               
		#region Public Methods

		[MonoTODO]
		public void Commit ()
		{
			IntPtr pgResult;
			ExecStatusType execStatus;

			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, 
					"COMMIT");
			/* FIXME: check result and emit 
			 * exceptions on errors 
			 */
			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);

			String cmdStatus;
			cmdStatus = PostgresLibrary.
				PQcmdStatus(pgResult);

			Console.WriteLine("*** Command Status: " +
				cmdStatus);

			PostgresLibrary.PQclear (pgResult);
		}		

		[MonoTODO]
		public void Rollback()
		{
			IntPtr pgResult;
			ExecStatusType execStatus;

			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, 
					"ROLLBACK");
			/* FIXME: check result and emit 
			 * exceptions on errors 
			 */
			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);

			String cmdStatus;
			cmdStatus = PostgresLibrary.
				PQcmdStatus(pgResult);

			Console.WriteLine("*** Command Status: " +
				cmdStatus);

			PostgresLibrary.PQclear (pgResult);
		}

		#endregion // Public Methods

		#region Internal Methods to System.Data.dll Assembly

		internal void Begin()
		{
			IntPtr pgResult;
			ExecStatusType execStatus;

			pgResult = PostgresLibrary.
				PQexec (conn.PostgresConnection, 
					"BEGIN");
			/* FIXME: check result and emit 
			 * exceptions on errors 
			 */
			execStatus = PostgresLibrary.
				PQresultStatus (pgResult);

			String cmdStatus;
			cmdStatus = PostgresLibrary.
				PQcmdStatus(pgResult);

			Console.WriteLine("*** Command Status: " +
				cmdStatus);

			PostgresLibrary.PQclear (pgResult);
		}

		internal void SetIsolationLevel(IsolationLevel isoLevel)
		{
			this.isolationLevel = isoLevel;
		}

		internal void SetConnection(SqlConnection connection)
		{
			this.conn = connection;
		}

		#endregion // Internal Methods to System.Data.dll Assembly

		#region Properties

		IDbConnection IDbTransaction.Connection	{
			get { 
				return Connection; 
			}
		}

		public SqlConnection Connection	{
			get { 
				return conn; 
			}
		}

		public IsolationLevel IsolationLevel {
			get { 
				return isolationLevel; 
			}
		}

		[MonoTODO]
		public void Dispose()
		{
			// FIXME: need to properly release resources
		}
	
		#endregion // Properties

	}
}
