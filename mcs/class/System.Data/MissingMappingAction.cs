//
// System.Data.MissingMappingAction.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Determines the action that occurs when a mapping is missing from a source table or a source column.
	/// </summary>
	public enum MissingMappingAction
	{
		Error,
		Ignore,
		Passthrough
	}
}