//
// System.Data.IDBConnection.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace System.Data
{
	/// <summary>
	/// Represents an open connection to a data source, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDbConnection : IDisposable
	{
		IDbTransaction BeginTransaction();

		IDbTransaction BeginTransaction(IsolationLevel il);

		void ChangeDatabase(string databaseName);

		void Close();

		IDbCommand CreateCommand();

		void Open();


		string ConnectionString{get; set;}

		int ConnectionTimeout{get;}

		string Database{get;}

		ConnectionState State{get;}

	}
}
