//
// System.Runtime.InteropServices.IMPLTYPEFLAGS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Flags, ComVisible (false)]
	public enum IMPLTYPEFLAGS
	{
		IMPLTYPEFLAG_FDEFAULT = 1,
		IMPLTYPEFLAG_FSOURCE = 2,
		IMPLTYPEFLAG_FRESTRICTED = 4,
		IMPLTYPEFLAG_FDEFAULTVTABLE = 8
	}
}
