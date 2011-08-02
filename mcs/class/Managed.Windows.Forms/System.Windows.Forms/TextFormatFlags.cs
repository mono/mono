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
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//



namespace System.Windows.Forms {
	
	[FlagsAttribute()]
	public enum TextFormatFlags {
		Left = 0,
		Top = 0,
		Default = 0,
		GlyphOverhangPadding = 0,
		HorizontalCenter = 1,
		Right = 2,
		VerticalCenter = 4,
		Bottom = 8,
		WordBreak = 16,
		SingleLine = 32,
		ExpandTabs = 64,
		NoClipping = 256,
		ExternalLeading = 512,
		NoPrefix = 2048,
		Internal = 4096,
		TextBoxControl = 8192,
		PathEllipsis = 16384,
		EndEllipsis = 32768,
		ModifyString = 65536,
		RightToLeft = 131072,
		WordEllipsis = 262144,
		NoFullWidthCharacterBreak = 524288,
		HidePrefix = 1048576,
		PrefixOnly = 2097152,
		PreserveGraphicsClipping = 16777216,
		PreserveGraphicsTranslateTransform = 33554432,
		NoPadding = 268435456,
		LeftAndRightPadding = 536870912
	}
}