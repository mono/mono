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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ColorDepth.cs,v $
// Revision 1.2  2004/08/15 23:23:56  ravindra
// Added attribute.
//
// Revision 1.1  2004/07/15 20:05:28  pbartok
// - Implemented ImageList and ImageList.ImageCollection classes
// - Added ColorDepth enumeration
// - Updated SWF VS.Net project
//
//
//

// COMPLETE

namespace System.Windows.Forms {
	[Serializable]
	public enum ColorDepth {
		Depth4Bit	= 4,
		Depth8Bit	= 8,
		Depth16Bit	= 16,
		Depth24Bit	= 24,
		Depth32Bit	= 32
	}
}
