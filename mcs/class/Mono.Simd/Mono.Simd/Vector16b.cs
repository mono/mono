// Vector4u.cs
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
	public struct Vector16b
	{
		private byte v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15;
		public Vector16b (byte v0, byte v1, byte v2, byte v3, byte v4, byte v5, byte v6, byte v7, byte v8, byte v9, byte v10, byte v11, byte v12, byte v13, byte v14, byte v15)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
			this.v4 = v4;
			this.v5 = v5;
			this.v6 = v6;
			this.v7 = v7;
			this.v8 = v8;
			this.v9 = v9;
			this.v10 = v10;
			this.v11 = v11;
			this.v12 = v12;
			this.v13 = v13;
			this.v14 = v14;
			this.v15 = v15;		}

		public byte V0 { get { return v0; } set { v0 = value; } }
		public byte V1 { get { return v1; } set { v1 = value; } }
		public byte V2 { get { return v2; } set { v2 = value; } }
		public byte V3 { get { return v3; } set { v3 = value; } }
		public byte V4 { get { return v4; } set { v4 = value; } }
		public byte V5 { get { return v5; } set { v5 = value; } }
		public byte V6 { get { return v6; } set { v6 = value; } }
		public byte V7 { get { return v7; } set { v7 = value; } }
		public byte V8 { get { return v8; } set { v8 = value; } }
		public byte V9 { get { return v9; } set { v9 = value; } }
		public byte V10 { get { return v10; } set { v10 = value; } }
		public byte V11 { get { return v11; } set { v11 = value; } }
		public byte V12 { get { return v12; } set { v12 = value; } }
		public byte V13 { get { return v13; } set { v13 = value; } }
		public byte V14 { get { return v14; } set { v14 = value; } }
		public byte V15 { get { return v15; } set { v15 = value; } }

		public static unsafe Vector16b operator + (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ + *b++);
			return res;
		}

		public static unsafe Vector16b operator - (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ - *b++);
			return res;
		}

		/*
		 * NOTE: Thou pmullw states it does signed multiplication, it works for unsigned numbers
		 * if only the lower part is considered and the flags disregarded.
		 */
		public static unsafe Vector16b operator * (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ * (*b++));
			return res;
		}

		public static unsafe Vector16b operator >> (Vector16b va, int amount)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &res.v0;
			for (int i = 0; i < 16; ++i)
				*b++ = (byte)(*a++ >> amount);
			return res;
		}


		public static unsafe Vector16b operator << (Vector16b va, int amount)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &res.v0;
			for (int i = 0; i < 16; ++i)
				*b++ = (byte)(*a++ << amount);
			return res;
		}

		public static unsafe Vector16b operator & (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ & *b++);
			return res;
		}

		public static unsafe Vector16b operator | (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ | *b++);
			return res;
		}

		public static unsafe Vector16b operator ^ (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (byte)(*a++ ^ *b++);
			return res;
		}

		public static unsafe Vector16b UnpackLow (Vector16b va, Vector16b vb)
		{
			return new Vector16b (va.v0, vb.v0, va.v1, vb.v1, va.v2, vb.v2, va.v3, vb.v3, va.v4, vb.v4, va.v5, vb.v5, va.v6, vb.v6, va.v7, vb.v7);
		}

		public static unsafe Vector16b UnpackHigh (Vector16b va, Vector16b vb)
		{
			return new Vector16b (va.v8, vb.v8, va.v9, vb.v9, va.v10, vb.v10, va.v11, vb.v11, va.v12, vb.v12, va.v13, vb.v13, va.v14, vb.v14, va.v15, vb.v15);
		}

		/*
		 * XXX Maybe this method doesn't make sense.
		 * C# doesn't support explicit shifting mode, it is defined by the signed of the type.
		 */
		public static unsafe Vector16b ShiftRightArithmetic (Vector16b va, int amount)
		{
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &res.v0;
			for (int i = 0; i < 16; ++i)
				*b++ = (byte)(((uint)(*a++)) >> amount);
			return res;
		}

		public static unsafe Vector16b AddWithSaturation (Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i) {
				int r = (int)*a++ + (int)*b++;
				*c++ = (byte)(r > byte.MaxValue ? byte.MaxValue : r);
			}
			return res;
		}

		public static unsafe Vector16b SubWithSaturation (Vector16b va, Vector16b vb) {
			Vector16b res = new Vector16b ();
			byte *a = &va.v0;
			byte *b = &vb.v0;
			byte *c = &res.v0;
			for (int i = 0; i < 16; ++i) {
				int r = (int)*a++ - (int)*b++;
				*c++ = (byte)(r < 0 ? 0 : r);
			}
			return res;
		}

		static byte Sat8 (byte v) {
			return (byte)(v > byte.MaxValue ? byte.MaxValue : v);
		}

		/*
         * NOTE: Thou packuswb states that it works with signed words, it works as expected with unsgined ones.
		 */
		public static unsafe Vector16b PackWithUnsignedSaturation (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
			byte *r = (byte*)&res;
			byte *a = &va.v0;
			byte *b = &vb.v0;
			int i;
			for (i = 0; i < 16; ++i)
				*r++ = Sat8 (*a++);
			for (i = 0; i < 16; ++i)
				*r++ = Sat8 (*b++);

			return res;
		}

		public static unsafe explicit operator Vector16b(Vector4f v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}

		public static unsafe explicit operator Vector16b(Vector8us v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}

		public static Vector16b LoadAligned (ref Vector16b v)
		{
			return v;
		}

		public static void StoreAligned (ref Vector16b res, Vector16b val)
		{
			res = val;
		}
	}
}
