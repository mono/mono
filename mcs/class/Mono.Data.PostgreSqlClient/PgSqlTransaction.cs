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

		public SqlTransaction (SqlConnection cnc)
		{
			connection = cnc;
		}

		[MonoTODO]
		public void Commit ()
		{
			throw new NotImplementedException ();
		}		

		[MonoTODO]
		void Rollback()
		{
			throw new NotImplementedException ();
		}

		public SqlConnection Connection
		{
			get { return connection; }
		}

		[MonoTODO]
		public IsolationLevel IsolationLevel
		{
			get { throw new NotImplementedException (); }
		}

	}
}
