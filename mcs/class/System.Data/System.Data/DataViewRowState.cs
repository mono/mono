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
	[Flags]
	[Serializable]
	public enum DataViewRowState
	{
		None = 0,
		Unchanged = 2,
		Added = 4,
		Deleted = 8,
		ModifiedCurrent = 16,
		CurrentRows = 22,
		ModifiedOriginal = 32,
		OriginalRows = 42
	}
}
