//
// System.Data.SqlClient.SqlDataAdapter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Represents a set of command-related properties that are used to fill the DataSet and update a data source, all this from a SQL database.
	/// </summary>
	public class SqlDataAdapter : IDbDataAdapter
	{
		SqlCommand DeleteCommand{get; set;}

		SqlCommand InsertCommand{get; set;}

		SqlCommand SelectCommand{get; set;}

		SqlCommand UpdateCommand{get; set;}
	}
}
