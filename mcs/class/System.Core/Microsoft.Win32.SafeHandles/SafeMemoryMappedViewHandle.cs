//
// SafeMemoryMappedViewHandle.cs
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


using System;
using System.IO;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32.SafeHandles
{
	public sealed class SafeMemoryMappedViewHandle : SafeBuffer {
		IntPtr mmap_handle;

		internal SafeMemoryMappedViewHandle (IntPtr mmap_handle, IntPtr base_address, long size) : base (true) {
			this.mmap_handle = mmap_handle;
			this.handle = base_address;
			Initialize ((ulong)size);
		}

		internal void Flush () {
			MemoryMapImpl.Flush (this.mmap_handle);
		}

		protected override bool ReleaseHandle () {
			if (this.handle != (IntPtr) (-1))
				return MemoryMapImpl.Unmap (this.mmap_handle);
			throw new NotImplementedException ();
		}
	}
}

