//
// System.Data.SqlClient.SqlCommand.cs
//
// Author:
//   Rodrigo Moya (rdorigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a SQL statement that is executed while connected to a SQL database.
	/// </summary>
	public class SqlCommand implements IDbCommand
	{
		void Cancel();
		
		SqlParameter CreateParameter();
		
		int ExecuteNonQuery();

		SqlDataReader ExecuteReader();

		SqlDataReader ExecuteReader(CommandBehavior behavior);

		object ExecuteScalar();

		void Prepare();


		string CommandText{get; set;}

		int CommandTimeout{get; set;}

		CommandType CommandType{get; set;}

		SqlConnection Connection{get; set;}

		SqlParameterCollection Parameters{get;}

		IDbTransaction Transaction{get; set;}

		UpdateRowSource UpdatedRowSource{get; set;}
	}
}
