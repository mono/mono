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
	/// Represents a transaction to be performed at a SQL database.
	/// </summary>
	public interface SqlTransaction : IDbTransaction
	{
		void Commit();

		void Rollback();
		
		SqlConnection Connection{get;}

		IsolationLevel IsolationLevel{get;}
	}	
}
