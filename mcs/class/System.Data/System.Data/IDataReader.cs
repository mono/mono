//
// System.Data.IDataReader.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Provides a means of reading one or more forward-only streams of result sets obtained by executing a command at a data source, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDataReader  : IDisposable, IDataRecord
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