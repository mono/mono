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


		/* ==== Math operations ==== */

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

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Max (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Max (v1.x, v2.x),
								System.Math.Max (v1.y, v2.y),
								System.Math.Max (v1.z, v2.z),
								System.Math.Max (v1.w, v2.w));
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f Min (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (System.Math.Min (v1.x, v2.x),
								System.Math.Min (v1.y, v2.y),
								System.Math.Min (v1.z, v2.z),
								System.Math.Min (v1.w, v2.w));
		}


		/* ==== Horizontal operations ==== */

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f HorizontalAdd (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x + v1.y, v1.z + v1.w, v2.x + v2.y, v2.z + v2.w);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f AddSub (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v2.x, v1.y + v2.y, v1.z - v2.z, v1.w + v2.w);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector4f HorizontalSub (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x - v1.y, v1.z - v1.w, v2.x - v2.y, v2.z - v2.w);
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


		/* ==== Data shuffling ==== */

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f InterleaveHigh (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.z, v2.z, v1.w, v2.w);
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector4f InterleaveLow (this Vector4f v1, Vector4f v2)
		{
			return new Vector4f (v1.x, v2.x, v1.y, v2.y);
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
		public static unsafe Vector4f Shuffle (this Vector4f v1, ShuffleSel sel)
		{
			float *ptr = (float*)&v1;
			int idx = (int)sel;
			return new Vector4f (*(ptr + ((idx >> 0) & 0x3)),*(ptr + ((idx >> 2) & 0x3)),*(ptr + ((idx >> 4) & 0x3)),*(ptr + ((idx >> 6) & 0x3)));
		}
	}
}