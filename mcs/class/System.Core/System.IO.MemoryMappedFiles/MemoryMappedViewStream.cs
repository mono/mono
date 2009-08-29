//
// MemoryMappedViewStream.cs
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
using Mono.Unix.Native;

namespace System.IO.MemoryMappedFiles
{
	public sealed class MemoryMappedViewStream : UnmanagedMemoryStream {

		static int pagesize;

		IntPtr mmap_addr;
		ulong mmap_size;
		object monitor;

		internal MemoryMappedViewStream (FileStream file, long offset, long size, MemoryMappedFileAccess access) {
			monitor = new Object ();
			if (Environment.OSVersion.Platform < PlatformID.Unix)
				throw new NotImplementedException ("Not implemented on windows.");
			else
				CreateStreamPosix (file, offset, size, access);
		}

		unsafe void CreateStreamPosix (FileStream file, long offset, long size, MemoryMappedFileAccess access) {
			if (pagesize == 0)
				pagesize = Syscall.getpagesize ();

			long fsize = file.Length;

			if (size == 0 || size > fsize)
				size = fsize;
			
			// Align offset
			long real_offset = offset & ~(pagesize - 1);

			int offset_diff = (int)(offset - real_offset);

			// FIXME: Need to determine the unix fd for the file, Handle is only
			// equal to it by accident
			mmap_size = (ulong)size;
			mmap_addr = Syscall.mmap (IntPtr.Zero, mmap_size, MmapProts.PROT_READ, MmapFlags.MAP_SHARED, (int)file.Handle, real_offset);
			if (mmap_addr == (IntPtr)(-1))
				throw new IOException ("mmap failed for " + file + "(" + offset + ", " + size + ")");

			FileAccess faccess;

			switch (access) {
			case MemoryMappedFileAccess.ReadWrite:
				faccess = FileAccess.ReadWrite;
				break;
			case MemoryMappedFileAccess.Read:
				faccess = FileAccess.Read;
				break;
			case MemoryMappedFileAccess.Write:
				faccess = FileAccess.Write;
				break;
			default:
				throw new NotImplementedException ("access mode " + access + " not supported.");
			}
			Initialize ((byte*)mmap_addr + offset_diff, size, size, faccess);
		}
		 
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			lock (monitor) {
				if (mmap_addr != (IntPtr)(-1)) {
					int err = Syscall.munmap (mmap_addr, mmap_size);
					if (err != 0)
						/* This shouldn't happen */
						throw new IOException ("munmap failed for address " + mmap_addr + ", size=" + mmap_size);
					mmap_addr = (IntPtr)(-1);
				}
			}
		}

	}
}

#endif