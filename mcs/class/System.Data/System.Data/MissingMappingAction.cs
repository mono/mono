//
// System.Data.MissingMappingAction.cs
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
	/// Determines the action that occurs when a mapping is missing from a source table or a source column.
	/// </summary>
	[Serializable]
	public enum MissingMappingAction
	{
		Passthrough = 1,
		Ignore = 2,
		Error = 3
	}
}