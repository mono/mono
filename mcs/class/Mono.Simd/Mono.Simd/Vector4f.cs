// Vector4f.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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

namespace Mono.Simd
{
	public enum ShuffleSel
	{
		X1 = 0x00,
		Y1 = 0x01,
		Z1 = 0x02,
		W1 = 0x03,

		X2 = 0x00,
		Y2 = 0x04,
		Z2 = 0x08,
		W2 = 0x0C,

		X3 = 0x00,
		Y3 = 0x10,
		Z3 = 0x20,
		W3 = 0x30,

		X4 = 0x00,
		Y4 = 0x40,
		Z4 = 0x80,
		W4 = 0xC0,
	};

/*
	TODO:
        Unary - (implemented as mulps [-1,-1,-1,-1])
        Abs (implemented as pand [7fffffff,...] )
        Comparison functions
        Mask extraction function
        Setters
        vector x float ops
        Single float constructor (expand it to the 4 positions)
		Replace Shuffle with less bug prone methods
*/

	[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
	public struct Vector4f
	{
		private float x;
		private float y;
		private float z;
		private float w;

		public float X { get { return x; } set { x = value; } }
		public float Y { get { return y; } set { y = value; } }
		public float Z { get { return z; } set { z = value; } }
		public float W { get { return w; } set { w = value; } }

		public Vector4f (float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public static Vector4f operator + (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}

		public static Vector4f operator - (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}

		public static Vector4f operator * (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}

		public static Vector4f operator / (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x / v2.x, v1.y / v2.y, v1.z / v2.z, v1.w / v2.w);
		}

		public static Vector4f Sqrt (Vector4f v1)
		{
			return new Vector4f ((float)System.Math.Sqrt ((float)v1.x),
								(float)System.Math.Sqrt ((float)v1.y),
								(float)System.Math.Sqrt ((float)v1.z),
								(float)System.Math.Sqrt ((float)v1.w));
		}

		public static Vector4f InvSqrt (Vector4f v1)
		{
			return new Vector4f ((float)(1.0 / System.Math.Sqrt ((float)v1.x)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.y)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.z)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.w)));
		}

		public static Vector4f Max (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Max (v1.x, v2.x),
								System.Math.Max (v1.y, v2.y),
								System.Math.Max (v1.z, v2.z),
								System.Math.Max (v1.w, v2.w));
		}

		public static Vector4f Min (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Min (v1.x, v2.x),
								System.Math.Min (v1.y, v2.y),
								System.Math.Min (v1.z, v2.z),
								System.Math.Min (v1.w, v2.w));
		}

		public static Vector4f HorizontalAdd (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x + v1.y, v1.z + v1.w, v2.x + v2.y, v2.z + v2.w);
		}

		public static Vector4f AddSub (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v2.x, v1.y + v2.y, v1.z - v2.z, v1.w + v2.w);
		}

		public static Vector4f HorizontalSub (Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v1.y, v1.z - v1.w, v2.x - v2.y, v2.z - v2.w);
		}

		float select (int pos, ShuffleSel sel) {
			int idx = (((int)sel) >> (pos * 2)) & 0x3;
			switch (idx) {
			case 0: return x;
			case 1: return y;
			case 2: return z;
			case 3: return w;
			}
			throw new Exception ("not reached"); 
		}

		public static Vector4f Shuffle (Vector4f v1, ShuffleSel sel)
		{
			return new Vector4f (v1.select (0, sel), v1.select (1, sel), v1.select (2, sel), v1.select (3, sel));
		}

		public static unsafe explicit operator Vector4u(Vector4f v)
		{
			Vector4u* p = (Vector4u*)&v;
			return *p;
		}

		public static Vector4f LoadAligned (ref Vector4f v)
		{
			return v;
		}

		public static void StoreAligned (ref Vector4f res, Vector4f val)
		{
			res = val;
		}

	}
}
