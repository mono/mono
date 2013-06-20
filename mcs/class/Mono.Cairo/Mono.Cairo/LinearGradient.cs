//
// Mono.Cairo.LinearGradient.cs
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

	public class LinearGradient : Gradient
	{
		internal LinearGradient (IntPtr handle, bool owned) : base (handle, owned)
		{
		}

		public LinearGradient (double x0, double y0, double x1, double y1)
			: base (NativeMethods.cairo_pattern_create_linear (x0, y0, x1, y1), true)
		{
		}

		public PointD[] LinearPoints {
			get {
				double x0, y0, x1, y1;
				PointD[] points = new PointD [2];

				NativeMethods.cairo_pattern_get_linear_points (Handle, out x0, out y0, out x1, out y1);

				points[0] = new PointD (x0, y0);
				points[1] = new PointD (x1, y1);
				return points;
			}
		}
	}
}

