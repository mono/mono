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
	[Obsolete ("Use the types in the System.Numerics.Vectors namespace")]
	[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 16)]
	[CLSCompliant(false)]
	public struct Vector4ui
	{
		[ FieldOffset(0) ]
		internal uint x;
		[ FieldOffset(4) ]
		internal uint y;
		[ FieldOffset(8) ]
		internal uint z;
		[ FieldOffset(12) ]
		internal uint w;

		public uint X { get { return x; } set { x = value; } }
		public uint Y { get { return y; } set { y = value; } }
		public uint Z { get { return z; } set { z = value; } }
		public uint W { get { return w; } set { w = value; } }

		public static Vector4ui Identity
		{
			get { return  new Vector4ui (1); }
		}

		public static Vector4ui Zero
		{
			get { return  new Vector4ui (0); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe uint this [int index]
		{
			get {
				if ((index | 0x3) != 0x3) //index < 0 || index > 3
					throw new ArgumentOutOfRangeException ("index");
				fixed (uint *v = &x) {
					return * (v + index);
				}
			}
			set {
				if ( (index | 0x3) != 0x3) //index < 0 || index > 3
					throw new ArgumentOutOfRangeException ("index");
				fixed (uint *v = &x) {
					* (v + index) = value;
				}
			}
		}

		public Vector4ui (uint x, uint y, uint z, uint w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
		
		public Vector4ui (uint ui)
		{
			this.x = ui;
			this.y = ui;
			this.z = ui;
			this.w = ui;
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
		public static bool operator ==(Vector4ui v1, Vector4ui v2)
		{
			return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z && v1.w == v2.w;
		}

		[Acceleration (AccelMode.SSE2)]
		public static bool operator !=(Vector4ui v1, Vector4ui v2)
		{
			return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z || v1.w != v2.w;
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
		
		public override string ToString()
		{
			return "<" + x + ", " + y + ", " + z + ", " + w + ">"; 
		}
	}
}
