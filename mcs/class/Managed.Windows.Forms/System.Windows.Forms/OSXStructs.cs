// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software",, to deal in the Software without restriction, including
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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@customerdna.com>
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// Mac OSX Version
namespace System.Windows.Forms {

	internal enum ThemeCursor : uint {
		kThemeArrowCursor = 0,
		kThemeCopyArrowCursor = 1,
		kThemeAliasArrowCursor = 2,
		kThemeContextualMenuArrowCursor = 3,
		kThemeIBeamCursor = 4,
		kThemeCrossCursor = 5,
		kThemePlusCursor = 6,
		kThemeWatchCursor = 7,
		kThemeClosedHandCursor = 8,
		kThemeOpenHandCursor = 9,
		kThemePointingHandCursor = 10,
		kThemeCountingUpHandCursor = 11,
		kThemeCountingDownHandCursor = 12,
		kThemeCountingUpAndDownHandCursor = 13,
		kThemeSpinningCursor = 14,
		kThemeResizeLeftCursor = 15,
		kThemeResizeRightCursor = 16,
		kThemeResizeLeftRightCursor = 17,
		kThemeNotAllowedCursor = 18
	}
	
	internal struct CGSize {
		public float width;
		public float height;

		public CGSize (int w, int h) {
			this.width = (float)w;
			this.height = (float)h;
		}
	}

	internal struct QDPoint {
		public short y;
		public short x;

		public QDPoint (short x, short y) {
			this.x = x;
			this.y = y;
		}
	}
	internal struct CGPoint {
		public float x;
		public float y;

		public CGPoint (int x, int y) {
			this.x = (float)x;
			this.y = (float)y;
		}
	}

	internal struct HIRect {
		public CGPoint origin;
		public CGSize size;

		public HIRect (int x, int y, int w, int h) {
			this.origin = new CGPoint (x, y);
			this.size = new CGSize (w, h);
		}
	}

	internal struct HIViewID {
		public uint type;
		public uint id;

		public HIViewID (uint type, uint id) {
			this.type = type;
			this.id = id;
		}
	}
	
	internal struct EventTypeSpec
        {
		public UInt32 eventClass;
		public UInt32 eventKind;

		public EventTypeSpec (UInt32 eventClass, UInt32 eventKind)
		{
			this.eventClass = eventClass;
			this.eventKind = eventKind;
		}
	}
	
	internal struct CarbonEvent
        {
		public IntPtr hWnd;
		public IntPtr evt;

		public CarbonEvent (IntPtr hWnd, IntPtr evt)
		{
			this.hWnd = hWnd;
			this.evt = evt;
		}
	}
	
	internal struct RGBColor
	{
		public short red;
		public short green;
		public short blue;
	}

	internal struct Rect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct OSXCaret
	{
		internal Timer timer;
		internal IntPtr hwnd;
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int visible;
		internal bool on;
		internal bool paused;
	}

	internal struct OSXHover {
		internal Timer timer;
		internal IntPtr hwnd;
		internal int x;
		internal int y;
		internal int interval;
	}

	internal struct CGAffineTransform
	{
		internal float a;
		internal float b;
		internal float c;
		internal float d;
		internal float tx;
		internal float ty;
	}
}	
