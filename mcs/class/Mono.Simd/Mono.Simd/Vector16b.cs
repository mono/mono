// Vector16b.cs
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
	[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
	public struct Vector16b
	{
		[ FieldOffset(0) ]
		internal byte v0;
		[ FieldOffset(1) ]
		internal byte v1;
		[ FieldOffset(2) ]
		internal byte v2;
		[ FieldOffset(3) ]
		internal byte v3;
		[ FieldOffset(4) ]
		internal byte v4;
		[ FieldOffset(5) ]
		internal byte v5;
		[ FieldOffset(6) ]
		internal byte v6;
		[ FieldOffset(7) ]
		internal byte v7;
		[ FieldOffset(8) ]
		internal byte v8;
		[ FieldOffset(9) ]
		internal byte v9;
		[ FieldOffset(10) ]
		internal byte v10;
		[ FieldOffset(11) ]
		internal byte v11;
		[ FieldOffset(12) ]
		internal byte v12;
		[ FieldOffset(13) ]
		internal byte v13;
		[ FieldOffset(14) ]
		internal byte v14;
		[ FieldOffset(15) ]
		internal byte v15;
		
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
			this.v15 = v15;
		}
		
		public Vector16b (byte b)
		{
			this.v0 = b;
			this.v1 = b;
			this.v2 = b;
			this.v3 = b;
			this.v4 = b;
			this.v5 = b;
			this.v6 = b;
			this.v7 = b;
			this.v8 = b;
			this.v9 = b;
			this.v10 = b;
			this.v11 = b;
			this.v12 = b;
			this.v13 = b;
			this.v14 = b;
			this.v15 = b;
		}

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

		public static Vector16b One
		{
			get {return new Vector16b (1); }
		}

		public static Vector16b Zero
		{
			get {return new Vector16b (0); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe byte this [int index]
		{
			get {
				if ((index | 0xF) != 0xF) //index < 0 || index > 15
					throw new ArgumentOutOfRangeException ("index");
				fixed (byte *v = &v0) {
					return * (v + index);
				}
			}
			set {
				if ( (index | 0xF) != 0xF) //index < 0 || index > 15
					throw new ArgumentOutOfRangeException ("index");
				fixed (byte *v = &v0) {
					* (v + index) = value;
				}
			}
		}

		[Acceleration (AccelMode.SSE2)]
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

		[Acceleration (AccelMode.SSE2)]
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

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16b operator & (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
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
		public static unsafe Vector16b operator | (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
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
		public static unsafe Vector16b operator ^ (Vector16b va, Vector16b vb)
		{
			Vector16b res = new Vector16b ();
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
		public unsafe static bool operator ==(Vector16b va, Vector16b vb)
		{
			byte *a = &va.v0;
			byte *b = &vb.v0;
			for (int i = 0; i < 16; ++i)
				if (*a++ != *b++)
					return false;
			return true;
		}

		[Acceleration (AccelMode.SSE2)]
		public unsafe static bool operator !=(Vector16b va, Vector16b vb)
		{
			byte *a = &va.v0;
			byte *b = &vb.v0;
			for (int i = 0; i < 16; ++i)
				if (*a++ != *b++)
					return true;
			return false;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2d (Vector16b v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector16b v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector16b v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector2ul (Vector16b v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector16b v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector4ui (Vector16b v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector16b v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector8us (Vector16b v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe explicit operator Vector16sb (Vector16b v)
		{
			Vector16sb* p = (Vector16sb*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector16b LoadAligned (ref Vector16b v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector16b res, Vector16b val)
		{
			res = val;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector16b LoadAligned (Vector16b *v)
		{
			return *v;
		}

		[CLSCompliant(false)]
		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector16b *res, Vector16b val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector16b res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector16b res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector16b res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector16b res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector16b *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector16b *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector16b *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector16b *res)
		{
		}
		
		public override string ToString()
		{
			return "<" + v0 + ", " + v1 + ", " + v2 + ", " + v3 + ", " +
					v4 + ", " + v5 + ", " + v6 + ", " + v7 + ", " + 
					+ v8 + ", " + v9 + ", " + v10 + ", " + v11 + ", " +
					v12 + ", " + v13 + ", " + v14 + ", " + v15 + ">"; 
		}
	}
}
