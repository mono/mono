//
// System.Data.Rule.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Indicates the action that occurs when a ForeignKeyConstraint is enforced.
	/// </summary>
	public enum Rule
	{
		Cascade,
		None,
		SetDefault,
		SetNull
	}
}