//
// System.Runtime.InteropServices.VARFLAGS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Runtime.InteropServices
{
#if NET_2_0
	[Obsolete]
#else
	[ComVisible (false)]
#endif
	[Serializable]
	[Flags]
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
