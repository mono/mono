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
		public static Vector2d GetVector (this double[] array, int offset)
		{
			return new Vector2d (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector2l GetVector (this long[] array, int offset)
		{
			return new Vector2l (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector2ul GetVector (this ulong[] array, int offset)
		{
			return new Vector2ul (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4f GetVector (this float[] array, int offset)
		{
			return new Vector4f (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4i GetVector (this int[] array, int offset)
		{
			return new Vector4i (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4ui GetVector (this uint[] array, int offset)
		{
			return new Vector4ui (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector8s GetVector (this short[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 8)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector8s*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector8us GetVector (this ushort[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 8)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector8us*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector16sb GetVector (this sbyte[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 16)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector16sb*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector16b GetVector (this byte[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 16)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector16b*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this double[] array, Vector2d val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this long[] array, Vector2l val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this ulong[] array, Vector2ul val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this float[] array, Vector4f val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this int[] array, Vector4i val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this uint[] array, Vector4ui val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this short[] array, Vector8s val, int offset)
		{
			array [offset + 0] = val.V0;
			array [offset + 1] = val.V1;
			array [offset + 2] = val.V2;
			array [offset + 3] = val.V3;
			array [offset + 4] = val.V4;
			array [offset + 5] = val.V5;
			array [offset + 6] = val.V6;
			array [offset + 7] = val.V7;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this ushort[] array, Vector8us val, int offset)
		{
			array [offset + 0] = val.V0;
			array [offset + 1] = val.V1;
			array [offset + 2] = val.V2;
			array [offset + 3] = val.V3;
			array [offset + 4] = val.V4;
			array [offset + 5] = val.V5;
			array [offset + 6] = val.V6;
			array [offset + 7] = val.V7;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this sbyte[] array, Vector16sb val, int offset)
		{
			for (int i = 0; i < 16; ++i)
				array [offset + i] = val [i];
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVector (this byte[] array, Vector16b val, int offset)
		{
			for (int i = 0; i < 16; ++i)
				array [offset + i] = val [i];
		}


		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector2d GetVectorAligned (this double[] array, int offset)
		{
			return new Vector2d (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector2l GetVectorAligned (this long[] array, int offset)
		{
			return new Vector2l (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector2ul GetVectorAligned (this ulong[] array, int offset)
		{
			return new Vector2ul (array [offset], array [offset + 1]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4f GetVectorAligned (this float[] array, int offset)
		{
			return new Vector4f (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4i GetVectorAligned (this int[] array, int offset)
		{
			return new Vector4i (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector4ui GetVectorAligned (this uint[] array, int offset)
		{
			return new Vector4ui (array [offset], array [offset + 1], array [offset + 2], array [offset + 3]);
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector8s GetVectorAligned (this short[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 8)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector8s*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector8us GetVectorAligned (this ushort[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 8)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector8us*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector16sb GetVectorAligned (this sbyte[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 16)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector16sb*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static Vector16b GetVectorAligned (this byte[] array, int offset)
		{
			if (offset < 0 || offset > array.Length - 16)
				throw new IndexOutOfRangeException ();
			unsafe {
				fixed (void *ptr = &array[offset]) {
					return *(Vector16b*)ptr;
				}
			}
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this double[] array, Vector2d val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this long[] array, Vector2l val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this ulong[] array, Vector2ul val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this float[] array, Vector4f val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this int[] array, Vector4i val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this uint[] array, Vector4ui val, int offset)
		{
			array [offset + 0] = val.X;
			array [offset + 1] = val.Y;
			array [offset + 2] = val.Z;
			array [offset + 3] = val.W;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this short[] array, Vector8s val, int offset)
		{
			array [offset + 0] = val.V0;
			array [offset + 1] = val.V1;
			array [offset + 2] = val.V2;
			array [offset + 3] = val.V3;
			array [offset + 4] = val.V4;
			array [offset + 5] = val.V5;
			array [offset + 6] = val.V6;
			array [offset + 7] = val.V7;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this ushort[] array, Vector8us val, int offset)
		{
			array [offset + 0] = val.V0;
			array [offset + 1] = val.V1;
			array [offset + 2] = val.V2;
			array [offset + 3] = val.V3;
			array [offset + 4] = val.V4;
			array [offset + 5] = val.V5;
			array [offset + 6] = val.V6;
			array [offset + 7] = val.V7;
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this sbyte[] array, Vector16sb val, int offset)
		{
			for (int i = 0; i < 16; ++i)
				array [offset + i] = val [i];
		}

		[Acceleration (AccelMode.SSE1)]
		[CLSCompliant(false)]
		public static void SetVectorAligned (this byte[] array, Vector16b val, int offset)
		{
			for (int i = 0; i < 16; ++i)
				array [offset + i] = val [i];
		}

		public static bool IsAligned<T> (this T[] vect, int index) where T : struct
		{
			int size = Marshal.SizeOf (typeof (T));
			return size * index % 16 == 0;
		}
	}
}
