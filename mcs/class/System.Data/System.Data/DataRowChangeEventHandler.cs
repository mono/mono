//
// System.Data.DataRowChangeEventHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents the method that will handle the RowChanging, RowChanged, RowDeleting, and RowDeleted events of a DataTable.
	/// </summary>
	[Serializable]
	public delegate void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e);

}