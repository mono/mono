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
    /*
    TODO
            Accessors
            Basic ops
            vector x scalar ops
            Single uint constructor (expand 4x)
            Remaining SSE ops (add, sub, mul)
    */
	[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
	public struct Vector4u
	{
		private uint x;
		private uint y;
		private uint z;
		private uint w;

		public uint X { get { return x; } set { x = value; } }
		public uint Y { get { return y; } set { y = value; } }
		public uint Z { get { return z; } set { z = value; } }
		public uint W { get { return w; } set { w = value; } }

		public Vector4u (uint x, uint y, uint z, uint w)
		{
			this.x = x;
			this.y = x;
			this.z = z;
			this.w = w;
		}

		public static Vector4u operator & (Vector4u v1, Vector4u v2)
		{
			return new Vector4u (v1.x & v2.x, v1.y & v2.y, v1.z & v2.z, v1.w & v2.w);
		}

		public static Vector4u operator | (Vector4u v1, Vector4u v2)
		{
			return new Vector4u (v1.x | v2.x, v1.y | v2.y, v1.z | v2.z, v1.w | v2.w);
		}

		public static Vector4u operator ^ (Vector4u v1, Vector4u v2)
		{
			return new Vector4u (v1.x ^ v2.x, v1.y ^ v2.y, v1.z ^ v2.z, v1.w ^ v2.w);
		}

  		public static unsafe explicit operator Vector4f (Vector4u v1)
		{
			Vector4f* p = (Vector4f*)&v1;
			return *p;
		}
	}
}
