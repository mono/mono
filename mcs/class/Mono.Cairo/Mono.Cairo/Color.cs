//
// Mono.Cairo.Context.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Miguel de Icaza (miguel@novell.com)
//   Hisham Mardam Bey (hisham.mardambey@gmail.com)
//   Alp Toker (alp@atoker.com)
//
// (C) Ximian Inc, 2003.
// (C) Novell Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
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

namespace Cairo {

	public struct Color
	{
		public Color(double r, double g, double b) : this (r, g, b, 1.0)
		{
		}

		public Color(double r, double g, double b, double a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		double r, g, b, a;

		public double R {
			get { return r; }
			set { r = value; }
		}

		public double G {
			get { return g; }
			set { g = value; }
		}

		public double B {
			get { return b; }
			set { b = value; }
		}

		public double A {
			get { return a; }
			set { a = value; }
		}
	}
}
