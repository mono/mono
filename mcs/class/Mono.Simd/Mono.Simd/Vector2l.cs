// Vector2l.cs
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
	public struct Vector2l
	{
		private long x;
		private long y;

		public long X { get { return x; } set { x = value; } }
		public long Y { get { return y; } set { y = value; } }

		public Vector2l (long x, long y)
		{
			this.x = x;
			this.y = y;
		}

		public static Vector2l operator + (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x + v2.x, v1.y + v2.y);
		}

		public static Vector2l operator - (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x - v2.x, v1.y - v2.y);
		}

		public static unsafe Vector2l operator << (Vector2l v1, int amount)
		{
			return new Vector2l (v1.x << amount, v1.y << amount);
		}

		public static Vector2l operator & (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x & v2.x, v1.y & v2.y);
		}

		public static Vector2l operator | (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x | v2.x, v1.y | v2.y);
		}

		public static Vector2l operator ^ (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x ^ v2.x, v1.y ^ v2.y);
		}

		public static Vector2l UnpackLow (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x, v2.x);
		}

		public static Vector2l UnpackHigh (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.y, v2.y);
		}

		public static unsafe Vector2l ShiftRightLogic (Vector2l v1, int amount)
		{
			return new Vector2l ((long)((ulong)(v1.x) >> amount), (long)((ulong)(v1.y) >> amount));
		}

		public static unsafe long ExtractByteMask (Vector2l va) {
			int res = 0;
			byte *a = (byte*)&va;
			for (int i = 0; i < 16; ++i)
				res |= (*a++ & 0x80) >> 7 << i;
			return res;
		}

		/*Requires SSE 4.1*/
		public static Vector2l CompareEqual (Vector2l v1, Vector2l v2)
		{
			return new Vector2l ((long)(v1.x ==  v2.x ? -1 : 0), (long)(v1.y ==  v2.y ? -1 : 0));
		}

		/*Requires SSE 4.1*/
		public static Vector2l CompareGreaterThan (Vector2l v1, Vector2l v2)
		{
			return new Vector2l ((long)(v1.x > v2.x ? -1 : 0), (long)(v1.y >  v2.y ? -1 : 0));
		}

  		public static unsafe explicit operator Vector4f (Vector2l v1)
		{
			Vector4f* p = (Vector4f*)&v1;
			return *p;
		}

 		[CLSCompliant(false)]
 		public static unsafe explicit operator Vector8us (Vector2l v1)
		{
			Vector8us* p = (Vector8us*)&v1;
			return *p;
		}

		[CLSCompliant(false)]
  		public static unsafe explicit operator Vector16b (Vector2l v1)
		{
			Vector16b* p = (Vector16b*)&v1;
			return *p;
		}

		public static Vector2l LoadAligned (ref Vector2l v)
		{
			return v;
		}

		public static void StoreAligned (ref Vector2l res, Vector2l val)
		{
			res = val;
		}

		[CLSCompliant(false)]
		public static unsafe Vector2l LoadAligned (Vector2l *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
		public static unsafe void StoreAligned (Vector2l *res, Vector2l val)
		{
			*res = val;
		}
	}
}
