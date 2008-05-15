//
// Mono.Cairo.Glyph.cs
//
// Authors: Duncan Mak (duncan@ximian.com)
//          Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
// (C) Ximian, Inc. 2003
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
using System.Runtime.InteropServices;

namespace Cairo
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Glyph
	{
		internal long index;
		internal double x;
		internal double y;
		
		public Glyph (long index, double x, double y)
		{
			this.index = index;
			this.x = x;
			this.y = y;
		}
		
		public long Index {
			get { return index; }
			set { index = value; }
		}
		
		public double X {
			get { return x; }
			set { x = value; }
		}
		
		public double Y {
			get { return y; }
			set { y = value; }
		}

		public override bool Equals (object obj)
		{
			if (obj is Glyph)
				return this == (Glyph)obj;
			return false;
		}

		public override int GetHashCode ()
		{
			return (int) Index ^ (int) X ^ (int) Y;
		}

		internal static IntPtr GlyphsToIntPtr (Glyph[] glyphs)
		{
			int size = Marshal.SizeOf (glyphs[0]);
			IntPtr dest = Marshal.AllocHGlobal (size * glyphs.Length);
			long pos = dest.ToInt64 ();
			for (int i = 0; i < glyphs.Length; i++, pos += size)
				Marshal.StructureToPtr (glyphs[i], (IntPtr) pos, false);
			return dest;
		}

		public static bool operator == (Glyph glyph, Glyph other)
		{
			return glyph.Index == other.Index && glyph.X == other.X && glyph.Y == other.Y;
		}

		public static bool operator != (Glyph glyph, Glyph other)
		{
			return !(glyph == other);
		}
	}
}
