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
		internal SolidPattern (IntPtr handle) : base (handle)
		{
		}

		public SolidPattern (Color color)
		{
			pattern = NativeMethods.cairo_pattern_create_rgba (color.R, color.G, color.B, color.A);
		}

		public SolidPattern (double r, double g, double b)
		{
			pattern = NativeMethods.cairo_pattern_create_rgb (r, g, b);
		}

		public SolidPattern (double r, double g, double b, double a)
		{
			NativeMethods.cairo_pattern_create_rgba (r, g, b, a);
		}

		public SolidPattern (Color color, bool solid)
		{
			if (solid)
				pattern = NativeMethods.cairo_pattern_create_rgb (color.R, color.G, color.B);
			else
				pattern = NativeMethods.cairo_pattern_create_rgba (color.R, color.G, color.B, color.A);
		}

		public Color Color {
                        get {
				double red, green, blue, alpha;

				NativeMethods.cairo_pattern_get_rgba  (pattern, out red, out green, out blue, out alpha);
				return new Color (red, green, blue, alpha);
                        }
                }
	}
}

