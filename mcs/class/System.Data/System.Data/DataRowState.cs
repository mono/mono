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
	[Flags]
	[Serializable]
	public enum DataRowState
	{
		Detached = 1,
		Unchanged = 2,
		Added = 4,
		Deleted = 8,
		Modified = 16
	}

}
