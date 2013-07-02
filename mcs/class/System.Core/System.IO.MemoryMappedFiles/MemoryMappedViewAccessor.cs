//
// MemoryMappedViewAccessor.cs
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

namespace System.IO.MemoryMappedFiles
{
	public sealed class MemoryMappedViewAccessor : UnmanagedMemoryAccessor, IDisposable {
		int file_handle;
		IntPtr mmap_addr;
		SafeMemoryMappedViewHandle handle;

		internal MemoryMappedViewAccessor (int file_handle, long offset, long size, MemoryMappedFileAccess access)
		{
			this.file_handle = file_handle;
			Create (offset, size, access);
		}

		static FileAccess ToFileAccess (MemoryMappedFileAccess access)
		{
			switch (access){
			case MemoryMappedFileAccess.CopyOnWrite:
			case MemoryMappedFileAccess.ReadWrite:
			case MemoryMappedFileAccess.ReadWriteExecute:
				return FileAccess.ReadWrite;
				
			case MemoryMappedFileAccess.Write:
				return FileAccess.Write;
				
			case MemoryMappedFileAccess.ReadExecute:
			case MemoryMappedFileAccess.Read:
			default:
				return FileAccess.Read;
			}
		}
		
		unsafe void Create (long offset, long size, MemoryMappedFileAccess access)
		{
			int offset_diff;

			MemoryMapImpl.Map (file_handle, offset, ref size, access, out mmap_addr, out offset_diff);

			handle = new SafeMemoryMappedViewHandle ((IntPtr)((long)mmap_addr + offset_diff), size);
			Initialize (handle, 0, size, ToFileAccess (access));
		}

		public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle {
			get {
				return handle;
			}
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		void IDisposable.Dispose () {
			Dispose (true);
		}

		public void Flush ()
		{
			MemoryMapImpl.Flush (file_handle);
		}
	}
}

#endif
