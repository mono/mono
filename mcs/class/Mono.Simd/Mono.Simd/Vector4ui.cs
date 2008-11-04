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

		[Acceleration (AccelMode.SSE2)]
		public static Vector4ui operator + (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4ui operator - (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4ui operator * (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui operator << (Vector4ui v1, int amount)
		{
			return new Vector4ui (v1.x << amount, v1.y << amount, v1.z << amount, v1.w << amount);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui operator >> (Vector4ui v1, int amount)
		{
			return new Vector4ui (v1.x >> amount, v1.y >> amount, v1.z >> amount, v1.w >> amount);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4ui operator & (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x & v2.x, v1.y & v2.y, v1.z & v2.z, v1.w & v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4ui operator | (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x | v2.x, v1.y | v2.y, v1.z | v2.z, v1.w | v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4ui operator ^ (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x ^ v2.x, v1.y ^ v2.y, v1.z ^ v2.z, v1.w ^ v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui UnpackLow (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x, v2.x, v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui UnpackHigh (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui ShiftRightArithmetic (Vector4ui v1, int amount)
		{
			Vector4ui res = new Vector4ui ();
			uint *a = &v1.x;
			uint *b = &res.x;
			for (int i = 0; i < 4; ++i)
				*b++ = (uint)((int)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector4ui Max (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Max (v1.x, v2.x), System.Math.Max (v1.y, v2.y), System.Math.Max (v1.z, v2.z), System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector4ui Min (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Min (v1.x, v2.x), System.Math.Min (v1.y, v2.y), System.Math.Min (v1.z, v2.z), System.Math.Min (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe int ExtractByteMask (Vector4ui va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui Shuffle (Vector4ui v1, ShuffleSel sel)
		{
			uint *ptr = (uint*)&v1;
			int idx = (int)sel;
			return new Vector4ui (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui CompareEqual (Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui ((uint)(v1.x ==  v2.x ? -1 : 0), (uint)(v1.y ==  v2.y ? -1 : 0), (uint)(v1.z ==  v2.z ? -1 : 0), (uint)(v1.w ==  v2.w ? -1 : 0));
		}

		/* This function performs a packusdw, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE41)]
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

		/* This function performs a packssdw, which treats the source as a signed value*/
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s SignedPackWithSignedSaturation (Vector4ui va, Vector4ui vb) {
			Vector8s res = new Vector8s ();
			int *a = (int*)&va;
			int *b = (int*)&vb;
			short *c = (short*)&res;
			for (int i = 0; i < 4; ++i)
				*c++ = (short)System.Math.Max (System.Math.Min ((int)*a++, short.MaxValue), short.MinValue);
			for (int i = 0; i < 4; ++i)
				*c++ = (short)System.Math.Max (System.Math.Min ((int)*b++, short.MaxValue), short.MinValue);
			return res;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2d (Vector4ui v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector4ui v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector4ui v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2ul (Vector4ui v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector4ui v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector4ui v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8us (Vector4ui v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16sb (Vector4ui v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector4ui v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4ui LoadAligned (ref Vector4ui v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector4ui res, Vector4ui val)
		{
			res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector4ui LoadAligned (Vector4ui *v)
		{
			return *v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector4ui *res, Vector4ui val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector4ui res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector4ui res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector4ui res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector4ui res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector4ui *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector4ui *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector4ui *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector4ui *res)
		{
		}
	}
}
