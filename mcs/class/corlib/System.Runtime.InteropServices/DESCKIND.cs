
// System.Runtime.InteropServices/DESCKIND.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[Serializable]
	[ComVisible(false)]
	public enum DESCKIND {
		DESCKIND_NONE = 0,
		DESCKIND_FUNCDESC = 1,
		DESCKIND_VARDESC = 2,
		DESCKIND_TYPECOMP = 3,
		DESCKIND_IMPLICITAPPOBJ = 4,
		DESCKIND_MAX = 5
	}
}

