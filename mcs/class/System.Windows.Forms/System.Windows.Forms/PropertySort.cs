//
// System.Windows.Forms.PropertySort.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	public enum PropertySort
	{
		Alphabetical = 1,
		Categorized = 2,
		CategorizedAlphabetical = 3,
		NoSort = 0
	}
}
