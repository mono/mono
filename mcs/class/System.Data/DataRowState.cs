//
// System.Data.DataRowState.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Gets the state of a DataRow object.
	/// </summary>
	public enum DataRowState
	{
		Added,
		Deleted,
		Detached,
		Modified,
		Unchanged
	}

}