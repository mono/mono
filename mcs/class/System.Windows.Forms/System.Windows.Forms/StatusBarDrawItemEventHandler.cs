//
// System.Windows.Forms.StatusBarDrawItemEventHandler.cs
//
// Authors:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (DENNISH@Raytek.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents the method that will handle the DrawItem event of a StatusBar.
	/// </summary>
	[Serializable]
	public delegate void StatusBarDrawItemEventHandler(object sender, StatusBarDrawItemEventArgs e);
}