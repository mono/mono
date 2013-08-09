//
// Mono.Cairo.Pattern.cs
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

	public class SolidPattern : Pattern
	{
		internal SolidPattern (IntPtr handle, bool owned) : base (handle, owned)
		{
		}

		public SolidPattern (Color color)
			: base (NativeMethods.cairo_pattern_create_rgba (color.R, color.G, color.B, color.A), true)
		{
		}

		public SolidPattern (double r, double g, double b)
			: base (NativeMethods.cairo_pattern_create_rgb (r, g, b), true)
		{
		}

		public SolidPattern (double r, double g, double b, double a)
			: base (NativeMethods.cairo_pattern_create_rgba (r, g, b, a), true)
		{
		}

		public SolidPattern (Color color, bool solid)
			: base (solid
					? NativeMethods.cairo_pattern_create_rgb (color.R, color.G, color.B)
					: NativeMethods.cairo_pattern_create_rgba (color.R, color.G, color.B, color.A),
				true)
		{
		}

		public Color Color {
			get {
				double red, green, blue, alpha;
				NativeMethods.cairo_pattern_get_rgba  (Handle, out red, out green, out blue, out alpha);
				return new Color (red, green, blue, alpha);
			}
		}
	}
}

