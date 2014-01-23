//
// Mono.Cairo.Gradient.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//         Hisham Mardam Bey (hisham.mardambey@gmail.com)
// (C) Ximian Inc, 2004.
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;

namespace Cairo {

	public class Gradient : Pattern
	{
		protected Gradient (IntPtr handle, bool owned) : base (handle, owned)
		{
		}

		[Obsolete]
		protected Gradient ()
		{
		}

		public int ColorStopCount {
			get {
				int cnt;
				NativeMethods.cairo_pattern_get_color_stop_count (Handle, out cnt);
				return cnt;
			}
		}

		public Status AddColorStop (double offset, Color c)
		{
			NativeMethods.cairo_pattern_add_color_stop_rgba (Handle, offset, c.R, c.G, c.B, c.A);
			return Status;
		}

		public Status AddColorStopRgb (double offset, Color c)
		{
			NativeMethods.cairo_pattern_add_color_stop_rgb (Handle, offset, c.R, c.G, c.B);
			return Status;
		}
	}
}

