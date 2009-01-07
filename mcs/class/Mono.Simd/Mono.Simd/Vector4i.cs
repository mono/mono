// Vector4i.cs
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
	[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 16)]
	public struct Vector4i
	{
		[ FieldOffset(0) ]
		internal int x;
		[ FieldOffset(4) ]
		internal int y;
		[ FieldOffset(8) ]
		internal int z;
		[ FieldOffset(12) ]
		internal int w;

		public int X { get { return x; } set { x = value; } }
		public int Y { get { return y; } set { y = value; } }
		public int Z { get { return z; } set { z = value; } }
		public int W { get { return w; } set { w = value; } }

		public Vector4i (int x, int y, int z, int w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i operator + (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i operator - (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4i operator * (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x * v2.x, v1.y * v2.y, v1.z * v2.z, v1.w * v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i operator << (Vector4i v1, int amount)
		{
			return new Vector4i (v1.x << amount, v1.y << amount, v1.z << amount, v1.w << amount);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i operator >> (Vector4i v1, int amount)
		{
			return new Vector4i (v1.x >> amount, v1.y >> amount, v1.z >> amount, v1.w >> amount);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i operator & (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x & v2.x, v1.y & v2.y, v1.z & v2.z, v1.w & v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i operator | (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x | v2.x, v1.y | v2.y, v1.z | v2.z, v1.w | v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i operator ^ (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x ^ v2.x, v1.y ^ v2.y, v1.z ^ v2.z, v1.w ^ v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i UnpackLow (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x, v2.x, v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i UnpackHigh (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i ShiftRightLogic (Vector4i v1, int amount)
		{
			Vector4i res = new Vector4i ();
			int *a = &v1.x;
			int *b = &res.x;
			for (int i = 0; i < 4; ++i)
				*b++ = (int)((uint)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4i Max (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (System.Math.Max (v1.x, v2.x), System.Math.Max (v1.y, v2.y), System.Math.Max (v1.z, v2.z), System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4i Min (Vector4i v1, Vector4i v2)
		{
			return new Vector4i (System.Math.Min (v1.x, v2.x), System.Math.Min (v1.y, v2.y), System.Math.Min (v1.z, v2.z), System.Math.Min (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe int ExtractByteMask (Vector4i va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i Shuffle (Vector4i v1, ShuffleSel sel)
		{
			int *ptr = (int*)&v1;
			int idx = (int)sel;
			return new Vector4i (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i CompareEqual (Vector4i v1, Vector4i v2)
		{
			return new Vector4i ((int)(v1.x ==  v2.x ? -1 : 0), (int)(v1.y ==  v2.y ? -1 : 0), (int)(v1.z ==  v2.z ? -1 : 0), (int)(v1.w ==  v2.w ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i CompareGreaterThan (Vector4i v1, Vector4i v2)
		{
			return new Vector4i ((int)(v1.x > v2.x ? -1 : 0), (int)(v1.y >  v2.y ? -1 : 0), (int)(v1.z >  v2.z ? -1 : 0), (int)(v1.w >  v2.w ? -1 : 0));
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector8us PackWithUnsignedSaturation (Vector4i va, Vector4i vb) {
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

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s PackWithSignedSaturation (Vector4i va, Vector4i vb) {
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
		public static unsafe explicit operator Vector2d (Vector4i v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector4i v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector4i v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector2ul (Vector4i v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui (Vector4i v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector4i v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector8us (Vector4i v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector16sb (Vector4i v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector4i v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}


		[Acceleration (AccelMode.SSE1)]
		public static Vector4i LoadAligned (ref Vector4i v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector4i res, Vector4i val)
		{
			res = val;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector4i LoadAligned (Vector4i *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector4i *res, Vector4i val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector4i res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector4i res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector4i res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector4i res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector4i *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector4i *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector4i *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector4i *res)
		{
		}
	}
}
