//
// System.Data.SqlClient.SqlConnection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents an open connection to a SQL data source
	/// </summary>
	public class SqlConnection : IDBConnection
	{
		SqlTransaction BeginTransaction();

		SqlTransaction BeginTransaction(IsolationLevel il);

		void ChangeDatabase(string databaseName);

		void Close();

		SqlCommand CreateCommand();

		void Open();


		string ConnectionString{get; set;}

		int ConnectionTimeout{get;}

		string Database{get;}

		ConnectionState State{get;}

	}
}
