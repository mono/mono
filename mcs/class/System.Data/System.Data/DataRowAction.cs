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
	[Flags]
	[Serializable]
	public enum DataRowAction
	{
		Nothing = 0,
		Delete = 1,
		Change = 2,
		Rollback = 4,
		Commit = 8,
		Add = 16
	}

}