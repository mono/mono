//
// RuntimeImports.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2018  Microsoft Corporation
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

using System.Runtime.CompilerServices;

#if BIT64
using nuint = System.UInt64;
#else
using nuint = System.UInt32;
#endif

namespace System.Runtime
{
	static partial class RuntimeImports
	{
		internal static unsafe void RhZeroMemory (ref byte b, nuint byteLength)
		{
			fixed (byte* bytePointer = &b) {
				ZeroMemory (bytePointer, (uint)byteLength);
			}
		}

		internal static unsafe void RhZeroMemory (IntPtr p, UIntPtr byteLength)
		{
			ZeroMemory ((void*)p, (uint) byteLength);
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern unsafe void ZeroMemory (void* p, uint byteLength);

		[MethodImpl (MethodImplOptions.InternalCall)]
		internal static extern unsafe void Memmove (byte* dest, byte* src, uint len);

		[MethodImpl (MethodImplOptions.InternalCall)]
		internal static extern unsafe void Memmove_wbarrier (byte* dest, byte* src, uint len, IntPtr type_handle);
	}
}
