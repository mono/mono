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
		#region fields
		
		protected SqlConnection connection = null;

		#endregion
		
		#region constructors
		
		public SqlTransaction (SqlConnection cnc)
		{
			connection = cnc;
		}

		#endregion

		#region methods
		
		public void Commit ()
		{
			throw new NotImplementedException ();
		}		

		void Rollback()
		{
			throw new NotImplementedException ();
		}

		#endregion
		
		# region properties
		
		public SqlConnection Connection
		{
			get { return connection; }
		}

		public IsolationLevel IsolationLevel
		{
			get { throw new NotImplementedException (); }
		}

		#endregion
	}
}
