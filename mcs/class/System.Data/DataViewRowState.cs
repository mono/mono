//
// System.Data.DataViewRowState.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Describes the version of data in a DataRow.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	public enum DataViewRowState
	{
		Added,
		CurrentRows,
		Deleted,
		ModifiedCurrent,
		ModifiedOriginal,
		None,
		OriginalRows,
		Unchanged
	}
}