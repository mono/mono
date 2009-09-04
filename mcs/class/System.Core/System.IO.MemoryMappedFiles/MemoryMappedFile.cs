//
// MemoryMappedFile.cs
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
using Mono.Unix.Native;

namespace System.IO.MemoryMappedFiles
{
	public class MemoryMappedFile : IDisposable {

		FileStream stream;

		[MonoTODO]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream) {
			if (fileStream == null)
				throw new ArgumentNullException ("fileStream");
			return new MemoryMappedFile () { stream = fileStream };
		}

		[MonoTODO]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName) {
			throw new NotImplementedException ();
		}		

		[MonoTODO]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity) {
			throw new NotImplementedException ();
		}		

		[MonoTODO]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access) {
			throw new NotImplementedException ();
		}		

		/*
		[MonoTODO]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability, bool leaveOpen) {
			throw new NotImplementedException ();
		}
		*/	

		[MonoTODO]
			public static MemoryMappedFile CreateNew (string mapName, long capacity) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
			public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access) {
			throw new NotImplementedException ();
		}

		/*
		[MonoTODO]
			public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability handleInheritability) {
			throw new NotImplementedException ();
		}
		*/

		[MonoTODO]
			public static MemoryMappedFile CreateOrOpen (string mapName, long capacity) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
			public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access) {
			throw new NotImplementedException ();
		}

		/*
		[MonoTODO]
			public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability handleInheritability) {
			throw new NotImplementedException ();
		}
		*/

		public MemoryMappedViewStream CreateViewStream () {
			return CreateViewStream (0, 0);
		}

		public MemoryMappedViewStream CreateViewStream (long offset, long size) {
			return CreateViewStream (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		[MonoTODO]
		public MemoryMappedViewStream CreateViewStream (long offset, long size, MemoryMappedFileAccess access) {
			return new MemoryMappedViewStream (stream, offset, size, access);
		}

		public MemoryMappedViewAccessor CreateViewAccessor () {
			return CreateViewAccessor (0, 0);
		}

		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size) {
			return CreateViewAccessor (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		[MonoTODO]
		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size, MemoryMappedFileAccess access) {
			return new MemoryMappedViewAccessor (stream, offset, size, access);
		}

		MemoryMappedFile () {
		}

		[MonoTODO]
		public void Dispose () {
		}

		[MonoTODO]
		public SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle {
			get {
				throw new NotImplementedException ();
			}
		}

		static int pagesize;

		internal static unsafe void MapPosix (FileStream file, long offset, long size, MemoryMappedFileAccess access, out IntPtr map_addr, out int offset_diff, out ulong map_size) {
			if (pagesize == 0)
				pagesize = Syscall.getpagesize ();

			long fsize = file.Length;

			if (size == 0 || size > fsize)
				size = fsize;
			
			// Align offset
			long real_offset = offset & ~(pagesize - 1);

			offset_diff = (int)(offset - real_offset);

			// FIXME: Need to determine the unix fd for the file, Handle is only
			// equal to it by accident
			map_size = (ulong)size;
			map_addr = Syscall.mmap (IntPtr.Zero, map_size, MmapProts.PROT_READ, MmapFlags.MAP_SHARED, (int)file.Handle, real_offset);
			if (map_addr == (IntPtr)(-1))
				throw new IOException ("mmap failed for " + file + "(" + offset + ", " + size + ")");
		}

		internal static void UnmapPosix (IntPtr map_addr, ulong map_size) {
			int err = Syscall.munmap (map_addr, map_size);
			if (err != 0)
				/* This shouldn't happen */
				throw new IOException ("munmap failed for address " + map_addr + ", size=" + map_size);
		}

	}
}

#endif