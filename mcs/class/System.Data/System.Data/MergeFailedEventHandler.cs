//
// System.Data.MergeFailedEventHandler.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents the method that will handle the MergeFailed event.
	/// </summary>
	[Serializable]
	public delegate void MergeFailedEventHandler(object sender, MergeFailedEventArgs e);

}