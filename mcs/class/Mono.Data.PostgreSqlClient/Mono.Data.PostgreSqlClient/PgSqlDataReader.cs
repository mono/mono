//
// System.Data.SqlClient.SqlDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// (C) Ximian, Inc
//

namespace System.Data.SqlClient
{
	/// <summary>
	/// Provides a means of reading one or more forward-only streams of result sets obtained by executing a command at a SQL database.
	/// </summary>
	public interface SqlDataReader : IDataReader
	{
		void Close();
		
		DataTable GetSchemaTable();
		
		bool NextResult();

		bool Read();

		int Depth{get;}

		bool IsClosed{get;}

		int RecordsAffected{get;}


	}
}
