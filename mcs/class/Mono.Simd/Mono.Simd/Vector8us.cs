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
	[Obsolete ("Use the types in the System.Numerics.Vectors namespace")]
	[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 16)]
	[CLSCompliant(false)]
	public struct Vector8us
	{
		[ FieldOffset(0) ]
		internal ushort v0;
		[ FieldOffset(2) ]
		internal ushort v1;
		[ FieldOffset(4) ]
		internal ushort v2;
		[ FieldOffset(6) ]
		internal ushort v3;
		[ FieldOffset(8) ]
		internal ushort v4;
		[ FieldOffset(10) ]
		internal ushort v5;
		[ FieldOffset(12) ]
		internal ushort v6;
		[ FieldOffset(14) ]
		internal ushort v7;

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
		
		public Vector8us (ushort us)
		{
			this.v0 = us;
			this.v1 = us;
			this.v2 = us;
			this.v3 = us;
			this.v4 = us;
			this.v5 = us;
			this.v6 = us;
			this.v7 = us;
		}

		public ushort V0 { get { return v0; } set { v0 = value; } }
		public ushort V1 { get { return v1; } set { v1 = value; } }
		public ushort V2 { get { return v2; } set { v2 = value; } }
		public ushort V3 { get { return v3; } set { v3 = value; } }
		public ushort V4 { get { return v4; } set { v4 = value; } }
		public ushort V5 { get { return v5; } set { v5 = value; } }
		public ushort V6 { get { return v6; } set { v6 = value; } }
		public ushort V7 { get { return v7; } set { v7 = value; } }

		public static Vector8us Identity
		{
			get { return  new Vector8us (1); }
		}

		public static Vector8us Zero
		{
			get { return  new Vector8us (0); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe ushort this [int index]
		{
			get {
				if ((index | 0x7) != 0x7) //index < 0 || index > 7
					throw new ArgumentOutOfRangeException ("index");
				fixed (ushort *v = &v0) {
					return * (v + index);
				}
			}
			set {
				if ( (index | 0x7) != 0x7) //index < 0 || index > 7
					throw new ArgumentOutOfRangeException ("index");
				fixed (ushort *v = &v0) {
					* (v + index) = value;
				}
			}
		}

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
			uint *a = (uint*) &va.v0;
			uint *b = (uint*) &vb.v0;
			uint *c = (uint*) &res.v0;
			*c++ = *a++ & *b++;
			*c++ = *a++ & *b++;
			*c++ = *a++ & *b++;
			*c = *a & *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator | (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			uint *a = (uint*) &va.v0;
			uint *b = (uint*) &vb.v0;
			uint *c = (uint*) &res.v0;
			*c++ = *a++ | *b++;
			*c++ = *a++ | *b++;
			*c++ = *a++ | *b++;
			*c = *a | *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector8us operator ^ (Vector8us va, Vector8us vb)
		{
			Vector8us res = new Vector8us ();
			uint *a = (uint*) &va.v0;
			uint *b = (uint*) &vb.v0;
			uint *c = (uint*) &res.v0;
			*c++ = *a++ ^ *b++;
			*c++ = *a++ ^ *b++;
			*c++ = *a++ ^ *b++;
			*c = *a ^ *b;
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public unsafe static bool operator ==(Vector8us va, Vector8us vb)
		{
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			for (int i = 0; i < 8; ++i)
				if (*a++ != *b++)
					return false;
			return true;
		}

		[Acceleration (AccelMode.SSE2)]
		public unsafe static bool operator !=(Vector8us va, Vector8us vb)
		{
			ushort *a = &va.v0;
			ushort *b = &vb.v0;
			for (int i = 0; i < 8; ++i)
				if (*a++ != *b++)
					return true;
			return false;
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
		
		public override string ToString()
		{
			return "<" + v0 + ", " + v1 + ", " + v2 + ", " + v3 + ", " +
					v4 + ", " + v5 + ", " + v6 + ", " + v7 + ">"; 
		}
	}
}
