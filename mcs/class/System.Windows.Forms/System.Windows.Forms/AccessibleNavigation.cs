//
// System.Windows.Forms.AccessibleNavigation.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms
{

	/// <summary>
	/// Specifies a values for navigating among accessible objects.
	/// </summary>

	[Serializable]
	public enum AccessibleNavigation
	{
		Down,
		FirstChild,
		LastChild,
		Left,
		Next,
		Previous,
		Right,
		Up
	}
}