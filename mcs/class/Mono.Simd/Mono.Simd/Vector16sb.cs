// Vector16sb.cs
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
	[CLSCompliant(false)]
	public struct Vector16sb
	{
		[ FieldOffset(0) ]
		internal sbyte v0;
		[ FieldOffset(1) ]
		internal sbyte v1;
		[ FieldOffset(2) ]
		internal sbyte v2;
		[ FieldOffset(3) ]
		internal sbyte v3;
		[ FieldOffset(4) ]
		internal sbyte v4;
		[ FieldOffset(5) ]
		internal sbyte v5;
		[ FieldOffset(6) ]
		internal sbyte v6;
		[ FieldOffset(7) ]
		internal sbyte v7;
		[ FieldOffset(8) ]
		internal sbyte v8;
		[ FieldOffset(9) ]
		internal sbyte v9;
		[ FieldOffset(10) ]
		internal sbyte v10;
		[ FieldOffset(11) ]
		internal sbyte v11;
		[ FieldOffset(12) ]
		internal sbyte v12;
		[ FieldOffset(13) ]
		internal sbyte v13;
		[ FieldOffset(14) ]
		internal sbyte v14;
		[ FieldOffset(15) ]
		internal sbyte v15;
		
		public Vector16sb (sbyte v0, sbyte v1, sbyte v2, sbyte v3, sbyte v4, sbyte v5, sbyte v6, sbyte v7, sbyte v8, sbyte v9, sbyte v10, sbyte v11, sbyte v12, sbyte v13, sbyte v14, sbyte v15)
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
		
		public Vector16sb (sbyte sb)
		{
			this.v0 = sb;
			this.v1 = sb;
			this.v2 = sb;
			this.v3 = sb;
			this.v4 = sb;
			this.v5 = sb;
			this.v6 = sb;
			this.v7 = sb;
			this.v8 = sb;
			this.v9 = sb;
			this.v10 = sb;
			this.v11 = sb;
			this.v12 = sb;
			this.v13 = sb;
			this.v14 = sb;
			this.v15 = sb;
		}

		public sbyte V0 { get { return v0; } set { v0 = value; } }
		public sbyte V1 { get { return v1; } set { v1 = value; } }
		public sbyte V2 { get { return v2; } set { v2 = value; } }
		public sbyte V3 { get { return v3; } set { v3 = value; } }
		public sbyte V4 { get { return v4; } set { v4 = value; } }
		public sbyte V5 { get { return v5; } set { v5 = value; } }
		public sbyte V6 { get { return v6; } set { v6 = value; } }
		public sbyte V7 { get { return v7; } set { v7 = value; } }
		public sbyte V8 { get { return v8; } set { v8 = value; } }
		public sbyte V9 { get { return v9; } set { v9 = value; } }
		public sbyte V10 { get { return v10; } set { v10 = value; } }
		public sbyte V11 { get { return v11; } set { v11 = value; } }
		public sbyte V12 { get { return v12; } set { v12 = value; } }
		public sbyte V13 { get { return v13; } set { v13 = value; } }
		public sbyte V14 { get { return v14; } set { v14 = value; } }
		public sbyte V15 { get { return v15; } set { v15 = value; } }

		public static Vector16sb One
		{
			get { return new Vector16sb (1); }
		}

		public static Vector16sb Zero
		{
			get { return new Vector16sb (0); }
		}

		public static Vector16sb MinusOne
		{
			get { return new Vector16sb (-1); }
		}

		[System.Runtime.CompilerServices.IndexerName ("Component")]
		public unsafe sbyte this [int index]
		{
			get {
				if ((index | 0xF) != 0xF) //index < 0 || index > 15
					throw new ArgumentOutOfRangeException ("index");
				fixed (sbyte *v = &v0) {
					return *(v + index);
				}
			}
			set {
				if ( (index | 0xF) != 0xF) //index < 0 || index > 15
					throw new ArgumentOutOfRangeException ("index");
				fixed (sbyte *v = &v0) {
					*(v + index) = value;
				}
			}
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb operator + (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ + *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb operator - (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			sbyte *c = &res.v0;
			for (int i = 0; i < 16; ++i)
				*c++ = (sbyte)(*a++ - *b++);
			return res;
		}

		[Acceleration (AccelMode.SSE2)]
		public static unsafe Vector16sb operator & (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
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
		public static unsafe Vector16sb operator | (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
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
		public static unsafe Vector16sb operator ^ (Vector16sb va, Vector16sb vb)
		{
			Vector16sb res = new Vector16sb ();
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
		public unsafe static bool operator ==(Vector16sb va, Vector16sb vb)
		{
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			for (int i = 0; i < 16; ++i)
				if (*a++ != *b++)
					return false;
			return true;
		}

		[Acceleration (AccelMode.SSE2)]
		public unsafe static bool operator !=(Vector16sb va, Vector16sb vb)
		{
			sbyte *a = &va.v0;
			sbyte *b = &vb.v0;
			for (int i = 0; i < 16; ++i)
				if (*a++ != *b++)
					return true;
			return false;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2d (Vector16sb v)
		{
			Vector2d* p = (Vector2d*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4f (Vector16sb v)
		{
			Vector4f* p = (Vector4f*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2l (Vector16sb v)
		{
			Vector2l* p = (Vector2l*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector2ul (Vector16sb v)
		{
			Vector2ul* p = (Vector2ul*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4i (Vector16sb v)
		{
			Vector4i* p = (Vector4i*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector4ui (Vector16sb v)
		{
			Vector4ui* p = (Vector4ui*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8s (Vector16sb v)
		{
			Vector8s* p = (Vector8s*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector8us (Vector16sb v)
		{
			Vector8us* p = (Vector8us*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe explicit operator Vector16b (Vector16sb v)
		{
			Vector16b* p = (Vector16b*)&v;
			return *p;
		}

		[Acceleration (AccelMode.SSE1)]
		public static Vector16sb LoadAligned (ref Vector16sb v)
		{
			return v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static void StoreAligned (ref Vector16sb res, Vector16sb val)
		{
			res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe Vector16sb LoadAligned (Vector16sb *v)
		{
			return *v;
		}

		[Acceleration (AccelMode.SSE1)]
		public static unsafe void StoreAligned (Vector16sb *res, Vector16sb val)
		{
			*res = val;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporalAllCacheLevels (ref Vector16sb res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal1stLevelCache (ref Vector16sb res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchTemporal2ndLevelCache (ref Vector16sb res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void PrefetchNonTemporal (ref Vector16sb res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporalAllCacheLevels (Vector16sb *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal1stLevelCache (Vector16sb *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchTemporal2ndLevelCache (Vector16sb *res)
		{
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static unsafe void PrefetchNonTemporal (Vector16sb *res)
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
