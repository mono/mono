//
// System.Runtime.InteropServices.VARFLAGS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[ComVisible (false), Flags]
	public enum VARFLAGS
	{
		VARFLAG_FREADONLY = 1,
		VARFLAG_FSOURCE = 2,
		VARFLAG_FBINDABLE = 4,
		VARFLAG_FREQUESTEDIT = 8,
		VARFLAG_FDISPLAYBIND = 16,
		VARFLAG_FDEFAULTBIND = 32,
		VARFLAG_FHIDDEN = 64,
		VARFLAG_FRESTRICTED = 128,
		VARFLAG_FDEFAULTCOLLELEM = 256,
		VARFLAG_FUIDEFAULT = 512,
		VARFLAG_FNONBROWSABLE = 1024,
		VARFLAG_FREPLACEABLE = 2048,
		VARFLAG_FIMMEDIATEBIND = 4096
	}
}
