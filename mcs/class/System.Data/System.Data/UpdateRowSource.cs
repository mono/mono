//
// System.Data.UpdateRowSource.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies how query command results are applied to the row being updated.
	/// </summary>
	public enum UpdateRowSource
	{
		Both,
		FirstReturnedRecord,
		None,
		OutputParameters
	}
}