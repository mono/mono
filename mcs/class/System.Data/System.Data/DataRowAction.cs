//
// System.Data.DataRowAction.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Describes the action taken on a DataRow.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	public enum DataRowAction
	{
		Add,
		Change,
		Commit,
		Delete,
		Nothing,
		Rollback
	}

}