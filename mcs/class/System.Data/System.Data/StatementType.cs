//
// System.Data.StatementType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the type of SQL query to be used by the OleDbRowUpdatedEventArgs, OleDbRowUpdatingEventArgs, SqlRowUpdatedEventArgs, or SqlRowUpdatingEventArgs class.
	/// </summary>
	public enum StatementType
	{
		Delete,
		Insert,
		Select,
		Update
	}
}