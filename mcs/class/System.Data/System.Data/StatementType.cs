//
// System.Data.StatementType.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace mono.System.Data
{
	/// <summary>
	/// Specifies the type of SQL query to be used by the OleDbRowUpdatedEventArgs, OleDbRowUpdatingEventArgs, SqlRowUpdatedEventArgs, or SqlRowUpdatingEventArgs class.
	/// </summary>
	[Serializable]
	public enum StatementType
	{
		Select = 0,
		Insert = 1,
		Update = 2,
		Delete = 3
	}
}