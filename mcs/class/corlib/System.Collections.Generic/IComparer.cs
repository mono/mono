//
// System.Collections.Generic.IComparer
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[CLSCompliant(false)]
	[ComVisible(false)]
	public interface IComparer<T> {
		int Compare(T x, T y);
	}
}
#endif
