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
	public static class ArrayExtensions
	{
		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4ui GetVector (this uint[] array, int offset)
		{
			return new Vector4ui (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetValue (this uint[] array, Vector4ui val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}
	}
}
