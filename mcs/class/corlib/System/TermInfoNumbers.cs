//
// System.TermInfoNumbers
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
#if MONO_FEATURE_CONSOLE

// These values are taken from 'man 5 terminfo' and /usr/include/term.h.
// They are the indexes for the numeric capabilities in a terminfo file.
namespace System {
	enum TermInfoNumbers {
		Columns,		// 0
		InitTabs,
		Lines,
		LinesOfMemory,
		MagicCookieGlitch,
		PaddingBaudRate,
		VirtualTerminal,
		WidthStatusLine,
		NumLabels,
		LabelHeight,
		LabelWidth,
		MaxAttributes,
		MaximumWindows,
		MaxColors,
		MaxPairs,
		NoColorVideo,
		BufferCapacity,
		DotVertSpacing,
		DotHorzSpacing,
		MaxMicroAddress,
		MaxMicroJump,
		MicroColSize,
		MicroLineSize,
		NumberOfPins,
		OutputResChar,
		OutputResLine,
		OutputResHorzInch,
		OutputResVertInch,
		PrintRate,
		WideCharSize,
		Buttons,
		BitImageEntwining,
		BitImageType,		// 32
		Last
	}
}
#endif

