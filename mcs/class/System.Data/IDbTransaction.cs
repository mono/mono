//
// System.Data.IDbTransaction.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents a transaction to be performed at a data source, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDbTransaction
	{

		void Commit()
		{
		}

		void Rollback()
		{
		}
		
		IsolationLevel IsolationLevel
		{
			get
			{
			}
		}
	}	
}