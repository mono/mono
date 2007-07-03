//
// System.Runtime.InteropServices.ComTypes.FUNCFLAGS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Kazuki Oikawa (kazuki@panicode.com)
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
#if NET_2_0
using System;

namespace System.Runtime.InteropServices.ComTypes
{
	[Flags, Serializable]
	public enum FUNCFLAGS
	{
		FUNCFLAG_FRESTRICTED = 1,
		FUNCFLAG_FSOURCE = 2,
		FUNCFLAG_FBINDABLE = 4,
		FUNCFLAG_FREQUESTEDIT = 8,
		FUNCFLAG_FDISPLAYBIND = 16,
		FUNCFLAG_FDEFAULTBIND = 32,
		FUNCFLAG_FHIDDEN = 64,
		FUNCFLAG_FUSESGETLASTERROR = 128,
		FUNCFLAG_FDEFAULTCOLLELEM = 256,
		FUNCFLAG_FUIDEFAULT = 512,
		FUNCFLAG_FNONBROWSABLE = 1024,
		FUNCFLAG_FREPLACEABLE = 2048,
		FUNCFLAG_FIMMEDIATEBIND = 4096
	}
}
#endif
