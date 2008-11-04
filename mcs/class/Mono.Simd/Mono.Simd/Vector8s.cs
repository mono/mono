// Vector8s.cs
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
	public struct Vector8s
	{
		private short v0, v1, v2, v3, v4, v5, v6, v7;
		public Vector8s (short v0, short v1, short v2, short v3, short v4, short v5, short v6, short v7)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
			this.v5 = v5;
			this.v6 = v6;
			this.v7 = v7;
		}

		public short V0 { get { return v0; } set { v0 = value; } }
		public short V1 { get { return v1; } set { v1 = value; } }
		public short V2 { get { return v2; } set { v2 = value; } }
		public short V3 { get { return v3; } set { v3 = value; } }
		public short V4 { get { return v4; } set { v4 = value; } }
		public short V5 { get { return v5; } set { v5 = value; } }
		public short V6 { get { return v6; } set { v6 = value; } }
		public short V7 { get { return v7; } set { v7 = value; } }

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator + (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ + *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator - (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ - *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator * (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ * (*b++));
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator >> (Vector8s va, int amount)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (short)(*a++ >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator << (Vector8s va, int amount)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (short)(*a++ << amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator & (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ & *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator | (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ | *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s operator ^ (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)(*a++ ^ *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s UnpackLow (Vector8s va, Vector8s vb)
		{
			return new Vector8s (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s UnpackHigh (Vector8s va, Vector8s vb)
		{
			return new Vector8s (va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s ShiftRightLogic (Vector8s va, int amount)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (short)((ushort)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s AddWithSaturation (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (System.Math.Min (*a++ + *b++, short.MaxValue), short.MinValue); 
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s SubWithSaturation (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (System.Math.Min (*a++ - *b++, short.MaxValue), short.MinValue); ;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s Max (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s Min (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Min (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe int ExtractByteMask (Vector8s va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s ShuffleHigh (Vector8s va, ShuffleSel sel)
		{
			short *ptr = ((short*)&va) + 4;
			int idx = (int)sel;
			return new Vector8s (va.v0, va.v1, va.v2, va.v3, *(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s ShuffleLow (Vector8s va, ShuffleSel sel)
		{
			short *ptr = ((short*)&va);
			int idx = (int)sel;
			return new Vector8s (*(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)), va.v4, va.v5, va.v6, va.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s CompareEqual (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s CompareGreaterThan (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) (*a++ > *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s MultiplyStoreHigh (Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)((int)*a++ * (int)*b++ >> 16);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b PackWithUnsignedSaturation (Vector8s va, Vector8s vb) {
			Vector16b res = new Vector16b ();
			short *a = (short*)&va;
			short *b = (short*)&vb;
			byte *c = (byte*)&res;
			for (int i = 0; i < 8; ++i)
				*c++ = (byte)System.Math.Max (0, System.Math.Min ((int)*a++, byte.MaxValue));
			for (int i = 0; i < 8; ++i)
				*c++ = (byte)System.Math.Max (0, System.Math.Min ((int)*b++, byte.MaxValue));
			return res;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb PackWithSignedSaturation (Vector8s va, Vector8s vb) {
			Vector16sb res = new Vector16sb ();
			short *a = (short*)&va;
			short *b = (short*)&vb;
			sbyte *c = (sbyte*)&res;
			for (int i = 0; i < 8; ++i)
				*c++ = (sbyte)System.Math.Max (System.Math.Min ((int)*a++, sbyte.MaxValue), sbyte.MinValue);
			for (int i = 0; i < 8; ++i)
				*c++ = (sbyte)System.Math.Max (System.Math.Min ((int)*b++, sbyte.MaxValue), sbyte.MinValue);
			return res;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2d (Vector8s v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector8s v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector8s v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector2ul (Vector8s v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector8s v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui (Vector8s v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector8us (Vector8s v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector16sb (Vector8s v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector8s v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}


		[Acceleration (AccelMode.SSE1)]
		public static Vector8s LoadAligned (ref Vector8s v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector8s res, Vector8s val)
		{
			res = val;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector8s LoadAligned (Vector8s *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector8s *res, Vector8s val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector8s res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector8s res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector8s res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector8s res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector8s *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector8s *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector8s *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector8s *res)
		{
		}
	}
}
