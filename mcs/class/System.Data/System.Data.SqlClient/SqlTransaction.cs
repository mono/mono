//
// System.Data.SqlClient.SqlTransaction.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a transaction to be performed on a SQL database.
	/// </summary>
	public class SqlTransaction : IDbTransaction
	{
		protected SqlConnection connection = null;

		public SqlTransaction (SqlConnection cnc) {
			connection = cnc;
		}

		[DllImport("pq")]
                public static extern IntPtr PQexec(IntPtr pgconn, String sql);
		public static extern int PQresultStatus(IntPtr pgres);
		public static extern void PQclear(IntPtr pgres);

		public void Commit () {
			IntPtr pgres;

			pgres = PQexec (pgconn, "COMMIT");
			/* FIXME: check result and emit exceptions on errors? */
			PQclear (pgres);
		}

		[DllImport("pq")]
                public static extern IntPtr PQexec(IntPtr pgconn, String sql);
		public static extern int PQresultStatus(IntPtr pgres);
		public static extern void PQclear(IntPtr pgres);

		void Rollback() {
			IntPtr pgres;

			pgres = PQexec (pgconn, "ROLLBACK");
			/* FIXME: check result and emit exceptions on errors? */
			PQclear (pgres);
		}

		public SqlConnection Connection {
			get { return connection; }
		}

		[MonoTODO]
		public IsolationLevel IsolationLevel {
			get { throw new NotImplementedException (); }
		}

	}
}
