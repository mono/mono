//
// System.Data.Rule.cs
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
	/// Indicates the action that occurs when a ForeignKeyConstraint is enforced.
	/// </summary>
	[Serializable]
	public enum Rule
	{
		None = 0,
		Cascade = 1,
		SetNull = 2,
		SetDefault = 3
	}
}