//
// Unsafe.cs: Compile only stubs for Unsafe implementation
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2017 Microsoft Corporation (http://microsoft.com)
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

#if BIT64
using nuint = System.UInt64;
using nint = System.Int64;
#else
using nuint = System.UInt32;
using nint = System.Int32;
#endif

namespace System.Runtime.CompilerServices
{
	unsafe static partial class Unsafe
	{
		public static ref T Add<T> (ref T source, int elementOffset)
		{
			throw new NotImplementedException ();
		}
		
		public static ref T Add<T> (ref T source, System.IntPtr elementOffset)
		{
			throw new NotImplementedException ();
		}

		public unsafe static void* Add<T> (void* source, int elementOffset)
		{
			throw new NotImplementedException ();
		}

		public static ref T AddByteOffset<T> (ref T source, System.IntPtr byteOffset)
		{
			throw new NotImplementedException ();
		}
		
		public static bool AreSame<T> (ref T left, ref T right)
		{
			throw new NotImplementedException ();
		}
		
		public static T As<T> (object o) where T : class
		{
			throw new NotImplementedException ();
		}
		
		public static ref TTo As<TFrom, TTo> (ref TFrom source)
		{
			throw new NotImplementedException ();
		}

		public unsafe static void* AsPointer<T> (ref T value)
		{
			throw new NotImplementedException ();
		}
		
		public unsafe static ref T AsRef<T> (void* source)
		{
			throw new NotImplementedException ();
		}
		
		public static System.IntPtr ByteOffset<T> (ref T origin, ref T target)
		{
			throw new NotImplementedException ();
		}

		public static void CopyBlock (ref byte destination, ref byte source, uint byteCount)
		{
			throw new NotImplementedException ();
		}

		public static void InitBlockUnaligned (ref byte startAddress, byte value, uint byteCount)
		{
			throw new NotImplementedException ();
		}

		public unsafe static void InitBlockUnaligned (void* startAddress, byte value, uint byteCount)
		{
			throw new NotImplementedException ();
		}

		public unsafe static T Read<T> (void* source)
		{
			throw new NotImplementedException ();
		}

		public unsafe static T ReadUnaligned<T> (void* source)
		{
			throw new NotImplementedException ();
		}

		public static T ReadUnaligned<T> (ref byte source)
		{
			throw new NotImplementedException ();
		}

		public static int SizeOf<T> ()
		{
			throw new NotImplementedException ();
		}

		public static ref T Subtract<T> (ref T source, int elementOffset)
		{
			throw new NotImplementedException ();
		}

		public static void WriteUnaligned<T> (ref byte destination, T value)
		{
			throw new NotImplementedException ();
		}

		public static void WriteUnaligned<T>(void* destination, T value)
		{
			throw new NotImplementedException ();
		}

		public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
		{
			throw new NotImplementedException ();
		}

		public static bool IsAddressLessThan<T>(ref T left, ref T right)
		{
			throw new NotImplementedException ();
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		unsafe internal static ref T AddByteOffset<T> (ref T source, nuint byteOffset)
		{
			return ref AddByteOffset (ref source, (IntPtr)(void*)byteOffset);
		}
	}
}
