//
// SafeBuffer.cs
//
// Authors:
//	Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009, Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace System.Runtime.InteropServices
{
	public abstract class SafeBuffer : SafeHandleZeroOrMinusOneIsInvalid, IDisposable {
		ulong byte_length;
		unsafe byte *last_byte;
		bool inited;

		protected SafeBuffer (bool ownsHandle) : base (ownsHandle)
		{
		}

		[CLSCompliant (false)]
		public void Initialize (ulong numBytes)
		{
			if (numBytes == 0)
				throw new ArgumentOutOfRangeException ("numBytes");

			inited = true;
			byte_length = numBytes;
			unsafe {
				last_byte = (byte *) (((byte *) handle) + numBytes);
			}
		}

		[CLSCompliant (false)]
		public void Initialize (uint numElements, uint sizeOfEachElement)
		{
			Initialize (numElements * sizeOfEachElement);
		}

		[CLSCompliant (false)]
		public void Initialize<T> (uint numElements) where T : struct
		{
			Initialize (numElements, (uint)Marshal.SizeOf (typeof (T)));
		}

		[CLSCompliant (false)]
		public unsafe void AcquirePointer (ref byte* pointer) {
			if (!inited)
				throw new InvalidOperationException ();
			bool success = false;

			DangerousAddRef (ref success);
			if (success)
				pointer = (byte*)handle;
		}

		public void ReleasePointer () {
			if (!inited)
				throw new InvalidOperationException ();
			DangerousRelease ();
		}

		[CLSCompliant (false)]
		public ulong ByteLength {
			get {
				return byte_length;
			}
		}

		[CLSCompliant (false)]
		public T Read<T> (ulong byteOffset) where T : struct
		{
			if (!inited)
				throw new InvalidOperationException ();

			unsafe {
				byte *source = (((byte *) handle) + byteOffset);
				if (source >= last_byte || source + Marshal.SizeOf (typeof (T)) > last_byte){
					throw new ArgumentException ("byteOffset");
				}

				return (T) Marshal.PtrToStructure ((IntPtr) source, typeof (T));
			}
		}

		[CLSCompliant (false)]
		public void ReadArray<T> (ulong byteOffset, T[] array, int index, int count) where T : struct {
			if (!inited)
				throw new InvalidOperationException ();

			unsafe {
				int size = Marshal.SizeOf (typeof (T)) * count;
				byte *source = (((byte *) handle) + byteOffset);
				if (source >= last_byte || source + size > last_byte)
					throw new ArgumentException ("byteOffset");
				
				Marshal.copy_from_unmanaged ((IntPtr) source, index, array, count);
			}
		}

		[CLSCompliant (false)]
		public void Write<T> (ulong byteOffset, T value) where T : struct {
			if (!inited)
				throw new InvalidOperationException ();

			unsafe {
				byte *target = (((byte *) handle) + byteOffset);
				if (target >= last_byte || target + Marshal.SizeOf (typeof (T)) > last_byte)
					throw new ArgumentException ("byteOffset");

				Marshal.StructureToPtr (value, (IntPtr) target, false);
			}
		}

		[CLSCompliant (false)]
		public void WriteArray<T> (ulong byteOffset, T[] array, int index, int count) where T : struct
		{
			if (!inited)
				throw new InvalidOperationException ();

			unsafe {
				byte *target = ((byte *) handle) + byteOffset;
				int size = Marshal.SizeOf (typeof (T)) * count;
				if (target >= last_byte || target + size > last_byte)
					throw new ArgumentException ("would overrite");
				
				Marshal.copy_to_unmanaged (array, index, (IntPtr) target, count);
			}
		}
	}
}

#endif