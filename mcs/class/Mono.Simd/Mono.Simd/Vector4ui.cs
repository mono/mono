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

		/*
		 * Requires SSE 4.1
		 */
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

		public static unsafe Vector4ui Max (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Max (v1.x, v2.x), System.Math.Max (v1.y, v2.y), System.Math.Max (v1.z, v2.z), System.Math.Max (v1.w, v2.w));
		}

		public static unsafe Vector4ui Min (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Min (v1.x, v2.x), System.Math.Min (v1.y, v2.y), System.Math.Min (v1.z, v2.z), System.Math.Min (v1.w, v2.w));
		}

		public static unsafe int ExtractByteMask (Vector4ui va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		public static unsafe Vector4ui Shuffle (Vector4ui v1, ShuffleSel sel)
		{
			uint *ptr = (uint*)&v1;
			int idx = (int)sel;
			return new Vector4ui (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		public static unsafe Vector4ui CompareEqual (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui ((uint)(v1.x ==  v2.x ? -1 : 0), (uint)(v1.y ==  v2.y ? -1 : 0), (uint)(v1.z ==  v2.z ? -1 : 0), (uint)(v1.w ==  v2.w ? -1 : 0));
		}
		/* This function performs a packusdw, which treats the source as a signed value
		 *
		 * Requires SSE 4.1
		 */
		public static unsafe Vector8us SignedPackWithUnsignedSaturation (Vector4ui va, Vector4ui vb) {
			Vector8us res = new Vector8us ();
			int *a = (int*)&va;
			int *b = (int*)&vb;
			ushort *c = (ushort*)&res;
			for (int i = 0; i < 4; ++i)
				*c++ = (ushort)System.Math.Max (0, System.Math.Min (*a++, ushort.MaxValue));
			for (int i = 0; i < 4; ++i)
				*c++ = (ushort)System.Math.Max (0, System.Math.Min (*b++, ushort.MaxValue));
			return res;
		}

  		public static unsafe explicit operator Vector4f (Vector4ui v1)
		{
			Vector4f* p = (Vector4f*)&v1;
			return *p;
		}

  		public static unsafe explicit operator Vector8us (Vector4ui v1)
		{
			Vector8us* p = (Vector8us*)&v1;
			return *p;
		}

  		public static unsafe explicit operator Vector16b (Vector4ui v1)
		{
			Vector16b* p = (Vector16b*)&v1;
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
