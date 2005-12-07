// DateTimeStyles.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:32:07 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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


namespace System.Globalization {

	[Flags]
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible(true)]
#endif
	public enum DateTimeStyles {
		None                 = 0x00000000,
		AllowLeadingWhite    = 0x00000001,
		AllowTrailingWhite   = 0x00000002,
		AllowInnerWhite      = 0x00000004,
		AllowWhiteSpaces     = AllowLeadingWhite | AllowTrailingWhite | AllowInnerWhite,
		NoCurrentDateDefault = 0x00000008,
		AdjustToUniversal    = 0x00000010,
#if NET_2_0
		AssumeLocal          = 0x00000020,
		AssumeUniversal      = 0x00000040,
		RoundtripKind        = 0x00000080,
#endif
	} // DateTimeStyles

} // System.Globalization
