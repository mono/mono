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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@customerdna.com>
//

using System;

namespace System.Windows.Forms.CarbonInternal {
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
	
	internal struct EventTypeSpec {
		public UInt32 eventClass;
		public UInt32 eventKind;

		public EventTypeSpec (UInt32 eventClass, UInt32 eventKind)
		{
			this.eventClass = eventClass;
			this.eventKind = eventKind;
		}
	}
	
	internal struct CarbonEvent {
		public IntPtr hWnd;
		public IntPtr evt;

		public CarbonEvent (IntPtr hWnd, IntPtr evt)
		{
			this.hWnd = hWnd;
			this.evt = evt;
		}
	}
	
	internal struct RGBColor {
		public short red;
		public short green;
		public short blue;
	}

	internal struct Rect {
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct Caret {
		internal Timer Timer;
		internal IntPtr Hwnd;
		internal int X;
		internal int Y;
		internal int Width;
		internal int Height;
		internal int Visible;
		internal bool On;
		internal bool Paused;
	}

	internal struct Hover {
		internal Timer Timer;
		internal IntPtr Hwnd;
		internal int X;
		internal int Y;
		internal int Interval;
	}

	internal struct CGAffineTransform {
		internal float a;
		internal float b;
		internal float c;
		internal float d;
		internal float tx;
		internal float ty;
	}
	
	internal struct MouseTrackingRegionID {
		public uint signature;
		public uint id;
		
		public MouseTrackingRegionID (uint signature, uint id) {
			this.signature = signature;
			this.id = id;
		}
	}
	
	internal struct ProcessSerialNumber {
		public ulong highLongOfPSN;
		public ulong lowLongOfPSN;
	}
}	
