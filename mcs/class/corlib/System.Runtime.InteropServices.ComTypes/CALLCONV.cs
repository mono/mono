//
// System.Runtime.InteropServices.ComTypes.CALLCONV.cs
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
namespace System.Runtime.InteropServices.ComTypes
{
	[Serializable]
	public enum CALLCONV
	{
		CC_CDECL = 1,
		CC_PASCAL = 2,
		CC_MSCPASCAL = 2,
		CC_MACPASCAL = 3,
		CC_STDCALL = 4,
		CC_RESERVED = 5,
		CC_SYSCALL = 6,
		CC_MPWCDECL = 7,
		CC_MPWPASCAL = 8,
		CC_MAX = 9
	}
}
#endif
