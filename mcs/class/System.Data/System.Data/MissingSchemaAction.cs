//
// System.Data.MissingSchemaAction.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Specifies the action to take when adding data to the DataSet and the required DataTable or DataColumn is missing.
	/// </summary>
	public enum MissingSchemaAction
	{
		Add,
		AddWithKey,
		Error,
		Ignore
	}
}