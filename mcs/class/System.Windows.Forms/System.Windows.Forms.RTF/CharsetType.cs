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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//	Karl Scowen		(contact@scowencomputers.co.nz)
//
//

// COMPLETE

namespace System.Windows.Forms.RTF {

#if RTF_LIB
	public
#else
	internal
#endif
	enum CharsetType : byte {
		ANSI		= 0,
		General		= 1,
		Symbol		= 2,
		Mac			= 77,
		Shift_Jis	= 128,
		Hangul		= 129,
		Johab		= 130,
		GB2312		= 134,
		Big5		= 136,
		Greek		= 161,
		Turkish		= 162,
		Vietnamese	= 163,
		Hebrew		= 177,
		Arabic		= 178,
		Arabic_Traditional = 179,
		Arabic_user = 180,
		Hebrew_user = 181,
		Baltic		= 186,
		Russian		= 204,
		Thai		= 222,
		Eastern_European = 238,
		PC_437		= 254,
		OEM			= 255
	}
}
