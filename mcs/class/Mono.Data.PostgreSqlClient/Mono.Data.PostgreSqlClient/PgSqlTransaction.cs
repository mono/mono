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
		// FIXME: is the IsolationLevel correct for PostgreSQL?
		private IsolationLevel isolationLevel =
			IsolationLevel.ReadCommitted | 
			IsolationLevel.Serializable;
		
		#endregion
               
		#region Public Methods

		[MonoTODO]
		public void Commit ()
		{
			IntPtr pgResult;

			pgResult = PostgresLibrary.
					PQexec (conn.PostgresConnection, 
						"COMMIT");
			/* FIXME: check result and emit 
			 * exceptions on errors 
			 */
			PostgresLibrary.PQclear (pgResult);
		}		

		[MonoTODO]
		public void Rollback()
		{
			IntPtr pgResult;

			pgResult = PostgresLibrary.
					PQexec (conn.PostgresConnection, 
						"ROLLBACK");
			/* FIXME: check result and emit 
			 * exceptions on errors
			 */
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

		#endregion

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
	
		#endregion

	}
}
