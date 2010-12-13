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
//


// COMPLETE

using System.ComponentModel;

namespace System.Windows.Forms {

	[Flags]
	public enum ControlStyles {
		ContainerControl	= 0x00000001,
		UserPaint		= 0x00000002,
		Opaque			= 0x00000004,
		ResizeRedraw		= 0x00000010,
		FixedWidth		= 0x00000020,
		FixedHeight		= 0x00000040,
		StandardClick		= 0x00000100,
		Selectable		= 0x00000200,
		UserMouse		= 0x00000400,
		SupportsTransparentBackColor	= 0x00000800,
		StandardDoubleClick	= 0x00001000,
		AllPaintingInWmPaint	= 0x00002000,
		CacheText		= 0x00004000,
		EnableNotifyMessage	= 0x00008000,

		[EditorBrowsable (EditorBrowsableState.Never)]
		DoubleBuffer		= 0x00010000,

		OptimizedDoubleBuffer	= 0x00020000,
		UseTextForAccessibility	= 0x00040000
	}
}
