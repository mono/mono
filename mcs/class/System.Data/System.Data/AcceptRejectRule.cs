//
// System.Data.AcceptRejectRule.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	
	/// <summary>
	/// Determines the action that occurs when the AcceptChanges or RejectChanges method is invoked on a DataTable with a ForeignKeyConstraint.
	/// </summary>
	public enum AcceptRejectRule
	{
		Cascade,
		None
	}

}