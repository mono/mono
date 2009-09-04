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
	public class MemoryMappedViewAccessor : IDisposable {

		object monitor;
		IntPtr mmap_addr;
		ulong mmap_size;
		SafeMemoryMappedViewHandle handle;

		internal MemoryMappedViewAccessor (FileStream file, long offset, long size, MemoryMappedFileAccess access) {
			monitor = new Object ();
			if (Environment.OSVersion.Platform < PlatformID.Unix)
				throw new NotImplementedException ("Not implemented on windows.");
			else
				CreatePosix (file, offset, size, access);
		}

		unsafe void CreatePosix (FileStream file, long offset, long size, MemoryMappedFileAccess access) {
			long fsize = file.Length;

			if (size == 0 || size > fsize)
				size = fsize;

			int offset_diff;

			MemoryMappedFile.MapPosix (file, offset, size, access, out mmap_addr, out offset_diff, out mmap_size);

			handle = new SafeMemoryMappedViewHandle ((IntPtr)((long)mmap_addr + offset_diff), size);
		}

		public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle {
			get {
				return handle;
			}
		}

		public void Dispose () {
			Dispose (true);
		}

		public void Dispose (bool disposing) {
			lock (monitor) {
				if (mmap_addr != (IntPtr)(-1)) {
					MemoryMappedFile.UnmapPosix (mmap_addr, mmap_size);
					mmap_addr = (IntPtr)(-1);
				}
			}
		}
	}
}

#endif