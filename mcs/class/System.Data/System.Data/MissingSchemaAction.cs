//
// System.Data.MissingSchemaAction.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System;

namespace mono.System.Data
{
	/// <summary>
	/// Specifies the action to take when adding data to the DataSet and the required DataTable or DataColumn is missing.
	/// </summary>
	[Serializable]
	public enum MissingSchemaAction
	{
		Add = 1,
		Ignore = 2,
		Error = 3,
		AddWithKey = 4
	}
}