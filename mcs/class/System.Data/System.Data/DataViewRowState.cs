//
// System.Data.DataViewRowState.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Describes the version of data in a DataRow.
	/// This enumeration has a FlagsAttribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[EditorAttribute("Microsoft.VSDesigner.Data.Design.DataViewRowStateEditor, "+Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+Consts.AssemblySystem_Drawing )]
	[Serializable]
	public enum DataViewRowState
	{
		None = 0,
		Unchanged = 2,
		Added = 4,
		Deleted = 8,
		ModifiedCurrent = 16,
		CurrentRows = 22,
		ModifiedOriginal = 32,
		OriginalRows = 42
	}
}
