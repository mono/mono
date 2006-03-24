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
//
//

// COMPLETE

namespace System.Windows.Forms.RTF {
	internal enum Major {
		// Group class
		BeginGroup	= 0,
		EndGroup	= 1,

		// Control
		Version		= 0,
		DefFont		= 1,
		CharSet		= 2,

		Destination	= 3,
		FontFamily	= 4,
		ColorName	= 5,
		SpecialChar	= 6,
		StyleAttr	= 7,
		DocAttr		= 8,
		SectAttr	= 9,
		TblAttr		= 10,
		ParAttr		= 11,
		CharAttr	= 12,
		PictAttr	= 13,
		BookmarkAttr	= 14,
		NeXTGrAttr	= 15,
		FieldAttr	= 16,
		TOCAttr		= 17,
		PosAttr		= 18,
		ObjAttr		= 19,
		FNoteAttr	= 20,
		KeyCodeAttr	= 21,
		ACharAttr	= 22,
		FontAttr	= 23,
		FileAttr	= 24,
		FileSource	= 25,
		DrawAttr	= 26,
		IndexAttr	= 27,
		Unicode		= 28
	}
}
