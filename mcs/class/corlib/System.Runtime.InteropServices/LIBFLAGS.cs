//
// System.Runtime.InteropServices.LIBFLAGS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Flags, ComVisible (false)]
	public enum LIBFLAGS
	{
		LIBFLAG_FRESTRICTED = 1,
		LIBFLAG_FCONTROL = 2,
		LIBFLAG_FHIDDEN = 4,
		LIBFLAG_FHASDISKIMAGE = 8
	}
}
