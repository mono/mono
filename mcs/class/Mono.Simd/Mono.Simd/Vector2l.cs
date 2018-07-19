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
	[Obsolete ("Use the types in the System.Numerics.Vectors namespace")]
	[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 16)]
	public struct Vector2l
	{
		[ FieldOffset(0) ]
		internal long x;
		[ FieldOffset(8) ]
		internal long y;

		public long X { get { return x; } set { x = value; } }
		public long Y { get { return y; } set { y = value; } }

		public static Vector2l One
		{
			get { return new Vector2l (1); }
		}

		public static Vector2l Zero
		{
			get { return new Vector2l (0); }
		}

		public static Vector2l MinusOne
		{
			get { return new Vector2l (-1); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe long this [int index]
		{
			get {
				if ((index | 0x1) != 0x1) //index < 0 || index > 1
					throw new ArgumentOutOfRangeException ("index");
				fixed (long *v = &x) {
					return * (v + index);
				}
			}
			set {
				if ( (index | 0x1) != 0x1) //index < 0 || index > 1
					throw new ArgumentOutOfRangeException ("index");
				fixed (long *v = &x) {
					* (v + index) = value;
				}
			}
		}

		public Vector2l (long x, long y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2l (long l)
		{
			this.x = l;
			this.y = l;
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l operator + (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x + v2.x, v1.y + v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l operator - (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x - v2.x, v1.y - v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector2l operator << (Vector2l v1, int amount)
		{
			return new Vector2l (v1.x << amount, v1.y << amount);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l operator & (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x & v2.x, v1.y & v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l operator | (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x | v2.x, v1.y | v2.y);
		}

		[Acceleration (AccelMode.SSE2)]
		public static Vector2l operator ^ (Vector2l v1, Vector2l v2)
		{
			return new Vector2l (v1.x ^ v2.x, v1.y ^ v2.y);
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2d (Vector2l v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector2l v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector2ul (Vector2l v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector2l v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui (Vector2l v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector2l v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector8us (Vector2l v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector16sb (Vector2l v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector2l v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}

 		[Acceleration (AccelMode.SSE1)]
		public static Vector2l LoadAligned (ref Vector2l v)
		{
			return v;
		}

 		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector2l res, Vector2l val)
		{
			res = val;
		}

		[CLSCompliant(false)]
 		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector2l LoadAligned (Vector2l *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
 		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector2l *res, Vector2l val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector2l res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector2l res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector2l res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector2l res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector2l *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector2l *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector2l *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector2l *res)
		{
		}
		
		public override string ToString()
		{
			return "<" + x + ", " + y + ">"; 
		}
	}
}
