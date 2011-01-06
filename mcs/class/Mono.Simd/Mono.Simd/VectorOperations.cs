// VectorOperations.cs
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
namespace Mono.Simd
{
	public static class VectorOperations
	{
		/* ==== Bitwise operations ==== */

		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector4f AndNot (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *a = (int*)&v1;
			int *b = (int*)&v2;
			int *c = (int*)&res;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c = ~*a & *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d AndNot (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			int *a = (int*)&v1;
			int *b = (int*)&v2;
			int *c = (int*)&res;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c = ~*a & *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector4ui ArithmeticRightShift (this Vector4ui v1, int amount)
		{
			Vector4ui res = new Vector4ui ();
			uint *a = &v1.x;
			uint *b = &res.x;
			for (int i = 0; i < 4; ++i)
				*b++ = (uint)((int)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us ArithmeticRightShift (this Vector8us va, int amount)
		{
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (ushort)((short)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2l LogicalRightShift (this Vector2l v1, int amount)
		{
			return new Vector2l ((long)((ulong)(v1.x) >> amount), (long)((ulong)(v1.y) >> amount));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i LogicalRightShift (this Vector4i v1, int amount)
		{
			Vector4i res = new Vector4i ();
			int *a = &v1.x;
			int *b = &res.x;
			for (int i = 0; i < 4; ++i)
				*b++ = (int)((uint)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s LogicalRightShift (this Vector8s va, int amount)
		{
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &res.v0;
			for (int i = 0; i < 8; ++i)
				*b++ = (short)((ushort)(*a++) >> amount);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe int ExtractByteMask (this Vector16sb va) {
			int res = 0;
			sbyte *a = (sbyte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe int ExtractByteMask (this Vector16b va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}


		/* ==== Math operations ==== */

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s AddWithSaturation (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (System.Math.Min (*a++ + *b++, short.MaxValue), short.MinValue); 
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us AddWithSaturation (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Min (*a++ + *b++, ushort.MaxValue);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb AddWithSaturation (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (System.Math.Min (*a++ + *b++, sbyte.MaxValue), sbyte.MinValue);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b AddWithSaturation (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) System.Math.Min (*a++ + *b++, byte.MaxValue);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s SubtractWithSaturation (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (System.Math.Min (*a++ - *b++, short.MaxValue), short.MinValue); ;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us SubtractWithSaturation (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Max (*a++ - *b++, 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb SubtractWithSaturation (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (System.Math.Min (*a++ - *b++, sbyte.MaxValue), sbyte.MinValue);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b SubtractWithSaturation (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) System.Math.Max (*a++ - *b++, 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s MultiplyStoreHigh (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short)((int)*a++ * (int)*b++ >> 16);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us MultiplyStoreHigh (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort)((uint)*a++ * (uint)*b++ >> 16);
			return res;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us SumOfAbsoluteDifferences (this Vector16b va, Vector16sb vb) {
			Vector8us res = new Vector8us ();
			byte *a = &va.v0;
			sbyte *b = (sbyte*)&vb;

			int tmp = 0;
			for (int i = 0; i < 8; ++i)
				tmp += System.Math.Abs ((int)*a++ - (int)*b++);
			res.V0 = (ushort)tmp;

			tmp = 0;
			for (int i = 0; i < 8; ++i)
				tmp += System.Math.Abs ((int)*a++ - (int)*b++);
			res.V4 = (ushort)tmp;

			return res;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Sqrt (this Vector4f v1)
		{
			return new Vector4f ((float)System.Math.Sqrt ((float)v1.x),
								(float)System.Math.Sqrt ((float)v1.y),
								(float)System.Math.Sqrt ((float)v1.z),
								(float)System.Math.Sqrt ((float)v1.w));
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f InvSqrt (this Vector4f v1)
		{
			return new Vector4f ((float)(1.0 / System.Math.Sqrt ((float)v1.x)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.y)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.z)),
								(float)(1.0 / System.Math.Sqrt ((float)v1.w)));
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Reciprocal (this Vector4f v1)
		{
			return new Vector4f (1.0f / v1.x, 1.0f / v1.y, 1.0f / v1.z, 1.0f / v1.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d Sqrt (this Vector2d v1)
		{
			return new Vector2d (System.Math.Sqrt (v1.x),
								System.Math.Sqrt (v1.y));
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us Average (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) ((*a++ + *b++ + 1) >> 1);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b Average (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) ((*a++ + *b++ + 1) >> 1);
			return res;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Max (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Max (v1.x, v2.x),
								System.Math.Max (v1.y, v2.y),
								System.Math.Max (v1.z, v2.z),
								System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d Max (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (System.Math.Max (v1.x, v2.x),
								System.Math.Max (v1.y, v2.y));
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4i Max (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i (System.Math.Max (v1.x, v2.x), System.Math.Max (v1.y, v2.y), System.Math.Max (v1.z, v2.z), System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static Vector4ui Max (this Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Max (v1.x, v2.x), System.Math.Max (v1.y, v2.y), System.Math.Max (v1.z, v2.z), System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s Max (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Max (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static unsafe Vector8us Max (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Max (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Min (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Min (v1.x, v2.x),
								System.Math.Min (v1.y, v2.y),
								System.Math.Min (v1.z, v2.z),
								System.Math.Min (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb Max (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Max (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b Max (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) System.Math.Max(*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d Min (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (System.Math.Min (v1.x, v2.x),
								System.Math.Min (v1.y, v2.y));
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector4i Min (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i (System.Math.Min (v1.x, v2.x), System.Math.Min (v1.y, v2.y), System.Math.Min (v1.z, v2.z), System.Math.Min (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static Vector4ui Min (this Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (System.Math.Min (v1.x, v2.x), System.Math.Min (v1.y, v2.y), System.Math.Min (v1.z, v2.z), System.Math.Min (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s Min (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) System.Math.Min (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static unsafe Vector8us Min (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) System.Math.Min (*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb Min (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) System.Math.Min(*a++, *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b Min (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) System.Math.Min(*a++, *b++);
			return res;
		}


		/* ==== Horizontal operations ==== */

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f HorizontalAdd (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x + v1.y, v1.z + v1.w, v2.x + v2.y, v2.z + v2.w);
		}
		[Acceleration (AccelMode.SSE3)]
		public static Vector2d HorizontalAdd (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x + v1.y, v2.x + v2.y);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f HorizontalSub (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v1.y, v1.z - v1.w, v2.x - v2.y, v2.z - v2.w);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d HorizontalSub (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x - v1.y, v2.x - v2.y);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f AddSub (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v2.x, v1.y + v2.y, v1.z - v2.z, v1.w + v2.w);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d AddSub (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x - v2.x, v1.y + v2.y);
		}

		/* ==== Compare methods ==== */

		/*Same as a == b. */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareEqual (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x == v2.x ? -1 : 0;
			*c++ = v1.y == v2.y ? -1 : 0;
			*c++ = v1.z == v2.z ? -1 : 0;
			*c = v1.w == v2.w ? -1 : 0;
			return res;
		}

		/*Same as a == b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareEqual (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x == v2.x ? -1 : 0;
			*c = v1.y == v2.y ? -1 : 0;
			return res;
		}

		[Acceleration (AccelMode.SSE41)]
		public static Vector2l CompareEqual (this Vector2l v1, Vector2l v2)
		{
			return new Vector2l ((long)(v1.x ==  v2.x ? -1 : 0), (long)(v1.y ==  v2.y ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static Vector2ul CompareEqual (this Vector2ul v1, Vector2ul v2)
		{
			return new Vector2ul ((ulong)(v1.x ==  v2.x ? -1 : 0), (ulong)(v1.y ==  v2.y ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i CompareEqual (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i ((int)(v1.x ==  v2.x ? -1 : 0), (int)(v1.y ==  v2.y ? -1 : 0), (int)(v1.z ==  v2.z ? -1 : 0), (int)(v1.w ==  v2.w ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static Vector4ui CompareEqual (this Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui ((uint)(v1.x ==  v2.x ? -1 : 0), (uint)(v1.y ==  v2.y ? -1 : 0), (uint)(v1.z ==  v2.z ? -1 : 0), (uint)(v1.w ==  v2.w ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s CompareEqual (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us CompareEqual (this Vector8us va, Vector8us vb) {
			Vector8us res = new Vector8us ();
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			ushort *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (ushort) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb CompareEqual (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b CompareEqual (this Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte) (*a++ == *b++ ? -1 : 0);
			return res;
		}

		/*Same as a < b. */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareLessThan (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x < v2.x ? -1 : 0;
			*c++ = v1.y < v2.y ? -1 : 0;
			*c++ = v1.z < v2.z ? -1 : 0;
			*c = v1.w < v2.w ? -1 : 0;
			return res;
		}

		/*Same as a < b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareLessThan (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x < v2.x ? -1 : 0;
			*c = v1.y < v2.y ? -1 : 0;
			return res;
		}

		/*Same as a <= b. */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareLessEqual (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x <= v2.x ? -1 : 0;
			*c++ = v1.y <= v2.y ? -1 : 0;
			*c++ = v1.z <= v2.z ? -1 : 0;
			*c = v1.w <= v2.w ? -1 : 0;
			return res;
		}

		/*Same as a <= b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareLessEqual (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x <= v2.x ? -1 : 0;
			*c = v1.y <= v2.y ? -1 : 0;
			return res;
		}

		[Acceleration (AccelMode.SSE42)]
		public static Vector2l CompareGreaterThan (this Vector2l v1, Vector2l v2)
		{
			return new Vector2l ((long)(v1.x > v2.x ? -1 : 0), (long)(v1.y >  v2.y ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i CompareGreaterThan (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i ((int)(v1.x > v2.x ? -1 : 0), (int)(v1.y >  v2.y ? -1 : 0), (int)(v1.z >  v2.z ? -1 : 0), (int)(v1.w >  v2.w ? -1 : 0));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s CompareGreaterThan (this Vector8s va, Vector8s vb) {
			Vector8s res = new Vector8s ();
			short *a = &va.v0;
			short *b = &vb.v0;
			short *c = &res.v0;
			for (int i = 0; i < 8; ++i)
				*c++ = (short) (*a++ > *b++ ? -1 : 0);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb CompareGreaterThan (this Vector16sb va, Vector16sb vb) {
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte) (*a++ > *b++ ? -1 : 0);
			return res;
		}

		/*Same float.IsNaN (a) || float.IsNaN (b). */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareUnordered (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = float.IsNaN (v1.x) || float.IsNaN (v2.x) ? -1 : 0;
			*c++ = float.IsNaN (v1.y) || float.IsNaN (v2.y) ? -1 : 0;
			*c++ = float.IsNaN (v1.z) || float.IsNaN (v2.z) ? -1 : 0;
			*c = float.IsNaN (v1.w) || float.IsNaN (v2.w) ? -1 : 0;
			return res;
		}

		/*Same double.IsNaN (a) || double.IsNaN (b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareUnordered (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = double.IsNaN (v1.x) || double.IsNaN (v2.x) ? -1 : 0;
			*c = double.IsNaN (v1.y) || double.IsNaN (v2.y) ? -1 : 0;
			return res;
		}

		/*Same as a != b. */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotEqual (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x != v2.x ? -1 : 0;
			*c++ = v1.y != v2.y ? -1 : 0;
			*c++ = v1.z != v2.z ? -1 : 0;
			*c = v1.w != v2.w ? -1 : 0;
			return res;
		}

		/*Same as a != b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotEqual (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x != v2.x ? -1 : 0;
			*c = v1.y != v2.y ? -1 : 0;
			return res;
		}

		/*Same as !(a < b). */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotLessThan (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x < v2.x ? 0 : -1;
			*c++ = v1.y < v2.y ? 0 : -1;
			*c++ = v1.z < v2.z ? 0 : -1;
			*c = v1.w < v2.w ? 0 : -1;
			return res;
		}

		/*Same as !(a < b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotLessThan (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x < v2.x ? 0 : -1;
			*c = v1.y < v2.y ? 0 : -1;
			return res;
		}

		/*Same as !(a <= b). */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotLessEqual (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = v1.x <= v2.x ? 0 : -1;
			*c++ = v1.y <= v2.y ? 0 : -1;
			*c++ = v1.z <= v2.z ? 0 : -1;
			*c = v1.w <= v2.w ? 0 : -1;
			return res;
		}

		/*Same as !(a <= b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotLessEqual (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x <= v2.x ? 0 : -1;
			*c = v1.y <= v2.y ? 0 : -1;
			return res;
		}

		/*Same !float.IsNaN (a) && !float.IsNaN (b). */
		[Acceleration (AccelMode.SSE1)]
		public unsafe static Vector4f CompareOrdered (this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f ();
			int *c = (int*)&res;
			*c++ = !float.IsNaN (v1.x) && !float.IsNaN (v2.x) ? -1 : 0;
			*c++ = !float.IsNaN (v1.y) && !float.IsNaN (v2.y) ? -1 : 0;
			*c++ = !float.IsNaN (v1.z) && !float.IsNaN (v2.z) ? -1 : 0;
			*c = !float.IsNaN (v1.w) && !float.IsNaN (v2.w) ? -1 : 0;
			return res;
		}

		/*Same !double.IsNaN (a) && !double.IsNaN (b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareOrdered (this Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = !double.IsNaN (v1.x) && !double.IsNaN (v2.x) ? -1 : 0;
			*c = !double.IsNaN (v1.y) && !double.IsNaN (v2.y) ? -1 : 0;
			return res;
		}


		/* ==== Data shuffling ==== */

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f InterleaveHigh (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d InterleaveHigh (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f InterleaveLow (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x, v2.x, v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d InterleaveLow (this Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x, v2.x);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d Duplicate (this Vector2d v1)
		{
			return new Vector2d (v1.x, v1.x);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f DuplicateLow (this Vector4f v1)
		{
			return new Vector4f (v1.x, v1.x, v1.z, v1.z);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f DuplicateHigh (this Vector4f v1)
		{
			return new Vector4f (v1.y, v1.y, v1.w, v1.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l UnpackLow (this Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x, v2.x);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static Vector2ul UnpackLow (this Vector2ul v1, Vector2ul v2)
		{
			return new Vector2ul (v1.x, v2.x);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i UnpackLow (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.x, v2.x, v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static Vector4ui UnpackLow (this Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.x, v2.x, v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s UnpackLow (this Vector8s va, Vector8s vb)
		{
			return new Vector8s (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us UnpackLow (this Vector8us va, Vector8us vb)
		{
			return new Vector8us (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb UnpackLow (this Vector16sb va, Vector16sb vb)
		{
			return new Vector16sb (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3, va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b UnpackLow (this Vector16b va, Vector16b vb)
		{
			return new Vector16b (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3, va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l UnpackHigh (this Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static Vector2ul UnpackHigh (this Vector2ul v1, Vector2ul v2)
		{
			return new Vector2ul (v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector4i UnpackHigh (this Vector4i v1, Vector4i v2)
		{
			return new Vector4i (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static Vector4ui UnpackHigh (this Vector4ui v1, Vector4ui v2)
		{
			return new Vector4ui (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s UnpackHigh (this Vector8s va, Vector8s vb)
		{
			return new Vector8s (va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us UnpackHigh (this Vector8us va, Vector8us vb)
		{
			return new Vector8us (va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb UnpackHigh (this Vector16sb va, Vector16sb vb)
		{
			return new Vector16sb (va.v8, vb.v8, va.v9, vb.v9, va.v10, vb.v10, va.v11, vb.v11, va.v12, vb.v12, va.v13, vb.v13, va.v14, vb.v14, va.v15, vb.v15);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b UnpackHigh (this Vector16b va, Vector16b vb)
		{
			return new Vector16b (va.v8, vb.v8, va.v9, vb.v9, va.v10, vb.v10, va.v11, vb.v11, va.v12, vb.v12, va.v13, vb.v13, va.v14, vb.v14, va.v15, vb.v15);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4f Shuffle (this Vector4f v1, Vector4f v2, ShuffleSel sel)
		{
			float *p1 = (float*)&v1;
			float *p2 = (float*)&v2;
			int idx = (int)sel;
			return new Vector4f (*(p1 + ((idx >> 0) & 0x3)), *(p1 + ((idx >> 2) & 0x3)), *(p2 + ((idx >> 4) & 0x3)), *(p2 + ((idx >> 6) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i Shuffle (this Vector4i v1, Vector4i v2, ShuffleSel sel)
		{
			int *p1 = (int*)&v1;
			int *p2 = (int*)&v2;
			int idx = (int)sel;
			return new Vector4i (*(p1 + ((idx >> 0) & 0x3)), *(p1 + ((idx >> 2) & 0x3)), *(p2 + ((idx >> 4) & 0x3)), *(p2 + ((idx >> 6) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4ui Shuffle (this Vector4ui v1, Vector4ui v2, ShuffleSel sel)
		{
			uint *p1 = (uint*)&v1;
			uint *p2 = (uint*)&v2;
			int idx = (int)sel;
			return new Vector4ui (*(p1 + ((idx >> 0) & 0x3)), *(p1 + ((idx >> 2) & 0x3)), *(p2 + ((idx >> 4) & 0x3)), *(p2 + ((idx >> 6) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d Shuffle (this Vector2d v1, Vector2d v2, int sel)
		{
			double *p1 = (double*)&v1;
			double *p2 = (double*)&v2;
			return new Vector2d (*(p1 + ((sel >> 0) & 0x3)), *(p2 + ((sel >> 2) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2l Shuffle (this Vector2l v1, Vector2l v2, int sel)
		{
			long *p1 = (long*)&v1;
			long *p2 = (long*)&v2;
			return new Vector2l (*(p1 + ((sel >> 0) & 0x3)), *(p2 + ((sel >> 2) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2ul Shuffle (this Vector2ul v1, Vector2ul v2, int sel)
		{
			ulong *p1 = (ulong*)&v1;
			ulong *p2 = (ulong*)&v2;
			return new Vector2ul (*(p1 + ((sel >> 0) & 0x3)), *(p2 + ((sel >> 2) & 0x3))); 
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4f Shuffle (this Vector4f v1, ShuffleSel sel)
		{
			float *ptr = (float*)&v1;
			int idx = (int)sel;
			return new Vector4f (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector4i Shuffle (this Vector4i v1, ShuffleSel sel)
		{
			int *ptr = (int*)&v1;
			int idx = (int)sel;
			return new Vector4i (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector4ui Shuffle (this Vector4ui v1, ShuffleSel sel)
		{
			uint *ptr = (uint*)&v1;
			int idx = (int)sel;
			return new Vector4ui (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s ShuffleHigh (this Vector8s va, ShuffleSel sel)
		{
			short *ptr = ((short*)&va) + 4;
			int idx = (int)sel;
			return new Vector8s (va.v0, va.v1, va.v2, va.v3, *(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us ShuffleHigh (this Vector8us va, ShuffleSel sel)
		{
			ushort *ptr = ((ushort*)&va) + 4;
			int idx = (int)sel;
			return new Vector8us (va.v0, va.v1, va.v2, va.v3, *(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)));
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s ShuffleLow (this Vector8s va, ShuffleSel sel)
		{
			short *ptr = ((short*)&va);
			int idx = (int)sel;
			return new Vector8s (*(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)), va.v4, va.v5, va.v6, va.v7);
		}

		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8us ShuffleLow (this Vector8us va, ShuffleSel sel)
		{
			ushort *ptr = ((ushort*)&va);
			int idx = (int)sel;
			return new Vector8us (*(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)), va.v4, va.v5, va.v6, va.v7);
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE41)]
		public static unsafe Vector8us PackWithUnsignedSaturation (this Vector4i va, Vector4i vb) {
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
		public static unsafe Vector16b PackWithUnsignedSaturation (this Vector8s va, Vector8s vb) {
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

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8s PackWithSignedSaturation (this Vector4i va, Vector4i vb) {
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

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb PackWithSignedSaturation (this Vector8s va, Vector8s vb) {
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

		/* This function performs a packusdw, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE41)]
		[CLSCompliant (false)]
		public static unsafe Vector8us SignedPackWithUnsignedSaturation (this Vector4ui va, Vector4ui vb) {
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

		/*This function performs a packuswb, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16b SignedPackWithUnsignedSaturation (this Vector8us va, Vector8us vb) {
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
		
		/* This function performs a packssdw, which treats the source as a signed value*/
		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector8s SignedPackWithSignedSaturation (this Vector4ui va, Vector4ui vb) {
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

		/*This function performs a packsswb, which treats the source as a signed value */
		[Acceleration (AccelMode.SSE2)]
		[CLSCompliant (false)]
		public static unsafe Vector16sb SignedPackWithSignedSaturation (this Vector8us va, Vector8us vb) {
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
	}
}
