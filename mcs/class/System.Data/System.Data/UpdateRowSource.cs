//
// System.Data.UpdateRowSource.cs
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
	/// Specifies how query command results are applied to the row being updated.
	/// </summary>
	[Serializable]
	public enum UpdateRowSource
	{
		None = 0,
		OutputParameters = 1,
		FirstReturnedRecord = 2,
		Both = 3
	}
}