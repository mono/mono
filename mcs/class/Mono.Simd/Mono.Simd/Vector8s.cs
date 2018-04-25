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
	[Obsolete ("Use the types in the System.Numerics.Vectors namespace")]
	[StructLayout(LayoutKind.Explicit, Pack = 0, Size = 16)]
	public struct Vector8s
	{
		[ FieldOffset(0) ]
		internal short v0;
		[ FieldOffset(2) ]
		internal short v1;
		[ FieldOffset(4) ]
		internal short v2;
		[ FieldOffset(6) ]
		internal short v3;
		[ FieldOffset(8) ]
		internal short v4;
		[ FieldOffset(10) ]
		internal short v5;
		[ FieldOffset(12) ]
		internal short v6;
		[ FieldOffset(14) ]
		internal short v7;

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
		
		public Vector8s (short s)
		{
			this.v0 = s;
			this.v1 = s;
			this.v2 = s;
			this.v3 = s;
			this.v4 = s;
			this.v5 = s;
			this.v6 = s;
			this.v7 = s;
		}

		public short V0 { get { return v0; } set { v0 = value; } }
		public short V1 { get { return v1; } set { v1 = value; } }
		public short V2 { get { return v2; } set { v2 = value; } }
		public short V3 { get { return v3; } set { v3 = value; } }
		public short V4 { get { return v4; } set { v4 = value; } }
		public short V5 { get { return v5; } set { v5 = value; } }
		public short V6 { get { return v6; } set { v6 = value; } }
		public short V7 { get { return v7; } set { v7 = value; } }

		public static Vector8s Identity
		{
			get { return  new Vector8s (1); }
		}

		public static Vector8s Zero
		{
			get { return  new Vector8s (0); }
		}

		public static Vector8s MinusOne
		{
			get { return new Vector8s (-1); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe short this [int index]
		{
			get {
				if ((index | 0x7) != 0x7) //index < 0 || index > 7
					throw new ArgumentOutOfRangeException ("index");
				fixed (short *v = &v0) {
					return * (v + index);
				}
			}
			set {
				if ( (index | 0x7) != 0x7) //index < 0 || index > 7
					throw new ArgumentOutOfRangeException ("index");
				fixed (short *v = &v0) {
					* (v + index) = value;
				}
			}
		}

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
		public static unsafe Vector8s operator | (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
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
		public static unsafe Vector8s operator ^ (Vector8s va, Vector8s vb)
		{
			Vector8s res = new Vector8s ();
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
		public unsafe static bool operator ==(Vector8s va, Vector8s vb)
		{
			short *a = &va.v0;
			short *b = &vb.v0;
			for (int i = 0; i < 8; ++i)
				if (*a++ != *b++)
					return false;
			return true;
		}

		[Acceleration (AccelMode.SSE2)]
		public unsafe static bool operator !=(Vector8s va, Vector8s vb)
		{
			short *a = &va.v0;
			short *b = &vb.v0;
			for (int i = 0; i < 8; ++i)
				if (*a++ != *b++)
					return true;
			return false;
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
		
		public override string ToString()
		{
			return "<" + v0 + ", " + v1 + ", " + v2 + ", " + v3 + ", " +
					v4 + ", " + v5 + ", " + v6 + ", " + v7 + ">"; 
		}
	}
}
