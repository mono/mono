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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// COMPLETE

using System.Runtime.InteropServices;
using System.Drawing;

namespace System.Windows.Forms {
	[ComVisible(true)]
	public class MouseEventArgs : EventArgs {
		private MouseButtons	buttons;
		private int		clicks;
		private int		delta;
		private int		x;
		private int		y;

		#region Public Constructors
		public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta) {
			this.buttons=button;
			this.clicks=clicks;
			this.delta=delta;
			this.x=x;
			this.y=y;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public MouseButtons Button {
			get {
				return this.buttons;
			}
		}

		public int Clicks {
			get {
				return this.clicks;
			}
		}

		public int Delta {
			get {
				return this.delta;
			}
		}

		public int X {
			get {
				return this.x;
			}
		}

		public int Y {
			get {
				return this.y;
			}
		}

		public Point Location {
			get {
				return new Point (this.x, this.y);
			}
		}
		#endregion	// Public Instance Properties
	}
}
