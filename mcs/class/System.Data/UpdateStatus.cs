//
// System.Data.UpdateStatus.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the action to take with regard to the current and remaining rows during an Update.
	/// </summary>
	public enum UpdateStatus
	{
		Continue,
		ErrorsOccurred,
		SkipAllRemainingRows,
		SkipCurrentRow
	}
}