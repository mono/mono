//
// System.Web.UI.HtmlTextWriterStyle.cs
//
// Authors:
//	Leen Toelen (toelen@hotmail.com)
//	Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

namespace System.Web.UI{
	public enum HtmlTextWriterStyle {
		BackgroundColor,
		BackgroundImage,
		BorderCollapse,
		BorderColor,
		BorderStyle,
		BorderWidth,
		Color,
		FontFamily,
		FontSize,
		FontStyle,
		FontWeight,
		Height,
		TextDecoration,
		Width,
#if NET_2_0
		ListStyleImage = 14,
		ListStyleType = 15,
		Cursor = 16,
		Direction = 17,
		Display = 18,
		Filter = 19,
		FontVariant = 20,
		Left = 21,
		Margin = 22,
		MarginBottom = 23,
		MarginLeft = 24,
		MarginRight = 25,
		MarginTop = 26,
		Overflow = 27,
		OverflowX = 28,
		OverflowY = 29,
		Padding = 30,
		PaddingBottom = 31,
		PaddingLeft = 32,
		PaddingRight = 33,
		PaddingTop = 34,
		Position = 35,
		TextAlign = 36,
		TextOverflow = 37,
		Top = 38,
		Visibility = 39,
		WhiteSpace = 40,
		ZIndex = 41
#endif
	}
}

