//
// System.Data.UpdateStatus.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace System.Data
{
	/// <summary>
	/// Specifies the action to take with regard to the current and remaining rows during an Update.
	/// </summary>
	[Serializable]
	public enum UpdateStatus
	{
		Continue = 0,
		ErrorsOccurred = 1,
		SkipCurrentRow = 2,
		SkipAllRemainingRows = 3
	}
}