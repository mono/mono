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
		XFromX,
		XFromY,
		XFromZ,
		XFromW,

		YFromX = 0x00,
		YFromY = 0x04,
		YFromZ = 0x08,
		YFromW = 0x0C,

		ZFromX = 0x00,
		ZFromY = 0x10,
		ZFromZ = 0x20,
		ZFromW = 0x30,

		WFromX = 0x00,
		WFromY = 0x40,
		WFromZ = 0x80,
		WFromW = 0xC0,

		/*Expand a single element into all elements*/
		ExpandX = XFromX | YFromX | ZFromX | WFromX,
		ExpandY = XFromY | YFromY | ZFromY | WFromY,
		ExpandZ = XFromZ | YFromZ | ZFromZ | WFromZ,
		ExpandW = XFromW | YFromW | ZFromW | WFromW,

		/*Expand a pair of elements (x,y,z,w) -> (x,x,y,y)*/
		ExpandXY = XFromX | YFromX | ZFromY | WFromY,
		ExpandZW = XFromZ | YFromZ | ZFromW | WFromW,

		/*Expand interleaving elements (x,y,z,w) -> (x,y,x,y)*/
		ExpandInterleavedXY = XFromX | YFromY | ZFromX | WFromY,
		ExpandInterleavedZW = XFromZ | YFromW | ZFromZ | WFromW,

		/*Rotate elements*/
		RotateRight = XFromY | YFromZ | ZFromW | WFromX,
		RotateLeft = XFromW | YFromX | ZFromY | WFromZ,

		/*Swap order*/
		Swap = XFromW | YFromZ | ZFromY | WFromX,
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

		/*
		The sel argument must be a value combination of ShuffleSel flags.
		*/
		public static Vector4f Shuffle (Vector4f v1, ShuffleSel sel)
		{
			return new Vector4f (v1.select (0, sel), v1.select (1, sel), v1.select (2, sel), v1.select (3, sel));
		}

		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui(Vector4f v)
		{
			Vector4ui* p = (Vector4ui*)&v;
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
