//
// System.Windows.Forms.UICuesEventHandler.cs
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
	/// Represents a method that will handle the ChangeUICues event of a Control.
	/// </summary>
	[Serializable]
	public delegate void UICuesEventHandler(object sender, UICuesEventArgs e);
}