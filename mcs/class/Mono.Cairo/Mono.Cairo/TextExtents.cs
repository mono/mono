//
// Mono.Cairo.TextExtents.cs
//
// Authors:
//   Duncan Mak (duncan@ximian.com)
//   Hisham Mardam Bey (hisham.mardambey@gmail.com)
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
	[StructLayout (LayoutKind.Sequential)]
	public struct TextExtents
	{
		double xbearing;
		double ybearing;
		double width;
		double height;
		double xadvance;
		double yadvance;
		
		public double XBearing {
			get { return xbearing; }
			set { xbearing = value; }
		}
		
		public double YBearing {
			get { return ybearing; }
			set { ybearing = value; }
		}
		
		public double Width {
			get { return width; }
			set { width = value; }
		}
		
		public double Height {
			get { return height; }
			set { height = value; }
		}
		
		public double XAdvance {
			get { return xadvance; }
			set { xadvance = value; }
		}
		
		public double YAdvance {
			get { return yadvance; }
			set { yadvance = value; }
		}

		public override bool Equals (object obj)
		{
			if (obj is TextExtents)
				return this == (TextExtents)obj;
			return false;
		}

		public override int GetHashCode ()
		{
			return (int)XBearing ^ (int)YBearing ^ (int)Width ^ (int)Height ^ (int)XAdvance ^ (int)YAdvance;
		}

		public static bool operator == (TextExtents extents, TextExtents other)
		{
			return extents.XBearing == other.XBearing && extents.YBearing == other.YBearing && extents.Width == other.Width && extents.Height == other.Height && extents.XAdvance == other.XAdvance && extents.YAdvance == other.YAdvance;
		}

		public static bool operator != (TextExtents extents, TextExtents other)
		{
			return !(extents == other);
		}
	}
}
