//
// System.Data.FillErrorEventHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents the method that will handle the FillError event.
	/// </summary>
	[Serializable]
	public delegate void FillErrorEventHandler(object sender, FillErrorEventArgs e);

}