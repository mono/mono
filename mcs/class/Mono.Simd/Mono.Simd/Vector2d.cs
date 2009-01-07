// Vector2d.cs
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
	public struct Vector2d
	{
		[ FieldOffset(0) ]
		internal double x;
		[ FieldOffset(8) ]
		internal double y;

		public double X { get { return x; } set { x = value; } }
		public double Y { get { return y; } set { y = value; } }

		public Vector2d (double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d operator & (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			int *a = (int*)&v1;
			int *b = (int*)&v2;
			int *c = (int*)&res;
			*c++ = *a++ & *b++;
			*c++ = *a++ & *b++;
			*c++ = *a++ & *b++;
			*c = *a & *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d operator | (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			int *a = (int*)&v1;
			int *b = (int*)&v2;
			int *c = (int*)&res;
			*c++ = *a++ | *b++;
			*c++ = *a++ | *b++;
			*c++ = *a++ | *b++;
			*c = *a | *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d operator ^ (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			int *a = (int*)&v1;
			int *b = (int*)&v2;
			int *c = (int*)&res;
			*c++ = *a++ ^ *b++;
			*c++ = *a++ ^ *b++;
			*c++ = *a++ ^ *b++;
			*c = *a ^ *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d operator + (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x + v2.x, v1.y + v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d operator - (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x - v2.x, v1.y - v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d operator * (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x * v2.x, v1.y * v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d operator / (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x / v2.x, v1.y / v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2d AndNot (Vector2d v1, Vector2d v2)
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
		public static Vector2d Max (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (System.Math.Max (v1.x, v2.x),
								System.Math.Max (v1.y, v2.y));
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d Min (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (System.Math.Min (v1.x, v2.x),
								System.Math.Min (v1.y, v2.y));
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d HorizontalAdd (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x + v1.y, v2.x + v2.y);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d AddSub (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x - v2.x, v1.y + v2.y);
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d HorizontalSub (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x - v1.y, v2.x - v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d InterleaveHigh (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.y, v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2d InterleaveLow (Vector2d v1, Vector2d v2)
		{
			return new Vector2d (v1.x, v2.x);
		}

		/*Same as a == b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareEqual (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x == v2.x ? -1 : 0;
			*c = v1.y == v2.y ? -1 : 0;
			return res;
		}

		/*Same as a < b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareLessThan (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x < v2.x ? -1 : 0;
			*c = v1.y < v2.y ? -1 : 0;
			return res;
		}

		/*Same as a <= b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareLessEqual (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x <= v2.x ? -1 : 0;
			*c = v1.y <= v2.y ? -1 : 0;
			return res;
		}

		/*Same double.IsNaN (a) || double.IsNaN (b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareUnordered (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = double.IsNaN (v1.x) || double.IsNaN (v2.x) ? -1 : 0;
			*c = double.IsNaN (v1.y) || double.IsNaN (v2.y) ? -1 : 0;
			return res;
		}

		/*Same as a != b. */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotEqual (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x != v2.x ? -1 : 0;
			*c = v1.y != v2.y ? -1 : 0;
			return res;
		}

		/*Same as !(a < b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotLessThan (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x < v2.x ? 0 : -1;
			*c = v1.y < v2.y ? 0 : -1;
			return res;
		}

		/*Same as !(a <= b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareNotLessEqual (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = v1.x <= v2.x ? 0 : -1;
			*c = v1.y <= v2.y ? 0 : -1;
			return res;
		}

		/*Same !double.IsNaN (a) && !double.IsNaN (b). */
		[Acceleration (AccelMode.SSE2)]
		public unsafe static Vector2d CompareOrdered (Vector2d v1, Vector2d v2)
		{
			Vector2d res = new Vector2d ();
			long *c = (long*)&res;
			*c++ = !double.IsNaN (v1.x) && !double.IsNaN (v2.x) ? -1 : 0;
			*c = !double.IsNaN (v1.y) && !double.IsNaN (v2.y) ? -1 : 0;
			return res;
		}

		[Acceleration (AccelMode.SSE3)]
		public static Vector2d Duplicate (Vector2d v1)
		{
			return new Vector2d (v1.x, v1.x);
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector2d v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector2d v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector2ul (Vector2d v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector2d v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui (Vector2d v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector2d v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector8us (Vector2d v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector16sb (Vector2d v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector2d v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}


		[Acceleration (AccelMode.SSE1)]
		public static Vector2d LoadAligned (ref Vector2d v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector2d res, Vector2d val)
		{
			res = val;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector2d LoadAligned (Vector2d *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector2d *res, Vector2d val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector2d res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector2d res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector2d res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector2d res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector2d *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector2d *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector2d *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector2d *res)
		{
		}
	}
}
