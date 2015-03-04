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
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

namespace System.Windows.Forms {

	[Flags]
	public enum AccessibleStates {
		None		= 0x00000000,
		Unavailable	= 0x00000001,
		Selected	= 0x00000002,
		Focused		= 0x00000004,
		Pressed		= 0x00000008,
		Checked		= 0x00000010,
		Mixed		= 0x00000020,
		Indeterminate	= 0x00000020,
		ReadOnly	= 0x00000040,
		HotTracked	= 0x00000080,
		Default		= 0x00000100,
		Expanded	= 0x00000200,
		Collapsed	= 0x00000400,
		Busy		= 0x00000800,
		Floating	= 0x00001000,
		Marqueed	= 0x00002000,
		Animated	= 0x00004000,
		Invisible	= 0x00008000,
		Offscreen	= 0x00010000,
		Sizeable	= 0x00020000,
		Moveable	= 0x00040000,
		SelfVoicing	= 0x00080000,
		Focusable	= 0x00100000,
		Selectable	= 0x00200000,
		Linked		= 0x00400000,
		Traversed	= 0x00800000,
		MultiSelectable	= 0x01000000,
		ExtSelectable	= 0x02000000,
		AlertLow	= 0x04000000,
		AlertMedium	= 0x08000000,
		AlertHigh	= 0x10000000,
		Protected	= 0x20000000,
		[Obsolete]
		Valid		= 0x3FFFFFFF,
		HasPopup	= 0x40000000
	}
}
