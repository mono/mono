//
// System.Data.DataColumnChangeEventHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents the method that will handle the the ColumnChanging event.
	/// </summary>
	[Serializable]
	public delegate void DataColumnChangeEventHandler(object sender, DataColumnChangeEventArgs e);

}