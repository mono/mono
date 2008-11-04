// Vector8us.cs
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
	public struct Vector8us
	{
		private ushort v0, v1, v2, v3, v4, v5, v6, v7;
		public Vector8us (ushort v0, ushort v1, ushort v2, ushort v3, ushort v4, ushort v5, ushort v6, ushort v7)
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

		public ushort V0 { get { return v0; } set { v0 = value; } }
		public ushort V1 { get { return v1; } set { v1 = value; } }
		public ushort V2 { get { return v2; } set { v2 = value; } }
		public ushort V3 { get { return v3; } set { v3 = value; } }
		public ushort V4 { get { return v4; } set { v4 = value; } }
		public ushort V5 { get { return v5; } set { v5 = value; } }
		public ushort V6 { get { return v6; } set { v6 = value; } }
		public ushort V7 { get { return v7; } set { v7 = value; } }

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator + (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ + *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator - (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ - *b++);
			return res;
		}

		/*
		 * NOTE: Thou pmullw states it does signed multiplication, it works for unsigned numbers
		 * if only the lower part is considered and the flags disregarded.
		 */
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator * (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ * (*b++));
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator >> (Vector8us va, int amount)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (ushort)(*a++ >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator << (Vector8us va, int amount)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (ushort)(*a++ << amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator & (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ & *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator | (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ | *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator ^ (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)(*a++ ^ *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us UnpackLow (Vector8us va, Vector8us vb)
		{
			return new Vector8us (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us UnpackHigh (Vector8us va, Vector8us vb)
		{
			return new Vector8us (va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us ShiftRightArithmetic (Vector8us va, int amount)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (ushort)((short)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us AddWithSaturation (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Min (*a++ + *b++, ushort.MaxValue);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us SubWithSaturation (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Max (*a++ - *b++, 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us Average (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) ((*a++ + *b++ + 1) >> 1);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector8us Max (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Max (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector8us Min (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Min (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe int ExtractByteMask (Vector8us va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us ShuffleHigh (Vector8us va, ShuffleSel sel)
		{
			ushort *ptr = ((ushort*)&va) + 4;
			int idx = (int)sel;
			return new Vector8us (va.v0, va.v1, va.v2, va.v3, *(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us ShuffleLow (Vector8us va, ShuffleSel sel)
		{
			ushort *ptr = ((ushort*)&va);
			int idx = (int)sel;
			return new Vector8us (*(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)), va.v4, va.v5, va.v6, va.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us CompareEqual (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us MultiplyStoreHigh (Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)((uint)*a++ * (uint)*b++ >> 16);
			return res;
		}

		/*This function performs a packuswb, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b SignedPackWithUnsignedSaturation (Vector8us va, Vector8us vb) {
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

		/*This function performs a packsswb, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb SignedPackWithSignedSaturation (Vector8us va, Vector8us vb) {
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
		public static unsafe explicit operator Vector2d (Vector8us v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector8us v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector8us v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2ul (Vector8us v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector8us v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4ui (Vector8us v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector8us v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16sb (Vector8us v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector8us v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}


		[Acceleration (AccelMode.SSE1)]
		public static Vector8us LoadAligned (ref Vector8us v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector8us res, Vector8us val)
		{
			res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector8us LoadAligned (Vector8us *v)
		{
			return *v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector8us *res, Vector8us val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector8us res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector8us res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector8us res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector8us res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector8us *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector8us *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector8us *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector8us *res)
		{
		}
	}
}
