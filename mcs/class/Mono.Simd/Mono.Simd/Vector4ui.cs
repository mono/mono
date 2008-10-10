// Vector4ui.cs
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
	[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
	[CLSCompliant(false)]
	public struct Vector4ui
	{
		private uint x;
		private uint y;
		private uint z;
		private uint w;

		public uint X { get { return x; } set { x = value; } }
		public uint Y { get { return y; } set { y = value; } }
		public uint Z { get { return z; } set { z = value; } }
		public uint W { get { return w; } set { w = value; } }

		public Vector4ui (uint x, uint y, uint z, uint w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public static Vector4ui operator + (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}

		public static Vector4ui operator - (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}

		public static Vector4ui operator * (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}

		public static unsafe Vector4ui operator << (Vector4ui v1, int amount)
		{
			return new Vector4ui (v1.x << amount, v1.y << amount, v1.z << amount, v1.w << amount);
		}

		public static unsafe Vector4ui operator >> (Vector4ui v1, int amount)
		{
			return new Vector4ui (v1.x >> amount, v1.y >> amount, v1.z >> amount, v1.w >> amount);
		}


		public static Vector4ui operator & (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x & v2.x, v1.y & v2.y, v1.z & v2.z, v1.w & v2.w);
		}

		public static Vector4ui operator | (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x | v2.x, v1.y | v2.y, v1.z | v2.z, v1.w | v2.w);
		}

		public static Vector4ui operator ^ (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x ^ v2.x, v1.y ^ v2.y, v1.z ^ v2.z, v1.w ^ v2.w);
		}

		public static unsafe Vector4ui UnpackLow (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x, v2.x, v1.y, v2.y);
		}

		public static unsafe Vector4ui UnpackHigh (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.z, v2.z, v1.w, v2.w);
		}

		public static unsafe Vector4ui ShiftRightArithmetic (Vector4ui v1, int amount)
		{
			Vector4ui res = new Vector4ui ();
			uint *a = &v1.x;
			uint *b = &res.x;
			for (int i = 0; i < 4; ++i)
				*b++ = (uint)((int)(*a++) >> amount);
			return res;
		}

		/*
         * NOTE: Thou packusdw states that it works with signed dwords, unsigned ones will do just fine.
		 */
		public static unsafe Vector8us PackWithUnsignedSaturation (Vector4ui va, Vector4ui vb)
		{
			Vector8us res = new Vector8us ();
			ushort *r = (ushort*)&res;
			uint *a = &va.x;
			uint *b = &vb.x;
			int i;
			for (i = 0; i < 4; ++i)
				*r++ = (ushort)System.Math.Min (*a++, ushort.MaxValue);
			for (i = 0; i < 4; ++i)
				*r++ = (ushort)System.Math.Min (*b++, ushort.MaxValue);

			return res;
		}

  		public static unsafe explicit operator Vector4f (Vector4ui v1)
		{
			Vector4f* p = (Vector4f*)&v1;
			return *p;
		}

		public static Vector4ui LoadAligned (ref Vector4ui v)
		{
			return v;
		}

		public static void StoreAligned (ref Vector4ui res, Vector4ui val)
		{
			res = val;
		}

	}
}
