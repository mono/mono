//
// System.Data.IDbDataAdapter.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents a set of command-related properties that are used to fill the DataSet and update a data source, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDbDataAdapter : IDataAdapter
	{
		IDbCommand DeleteCommand{get; set;}

		IDbCommand InsertCommand{get; set;}

		IDbCommand SelectCommand{get; set;}

		IDbCommand UpdateCommand{get; set;}
	}
}