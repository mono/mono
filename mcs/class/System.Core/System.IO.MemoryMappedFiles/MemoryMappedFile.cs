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

using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace System.IO.MemoryMappedFiles
{
	internal static class MemoryMapImpl {
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern IntPtr OpenFileInternal (string path, FileMode mode, string mapName, out long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, out int error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern IntPtr OpenHandleInternal (IntPtr handle, string mapName, out long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, out int error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void CloseMapping (IntPtr handle);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void Flush (IntPtr file_handle);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void ConfigureHandleInheritability (IntPtr handle, HandleInheritability inheritability);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool Unmap (IntPtr mmap_handle);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int MapInternal (IntPtr handle, long offset, ref long size, MemoryMappedFileAccess access, out IntPtr mmap_handle, out IntPtr base_address);

		internal static void Map (IntPtr handle, long offset, ref long size, MemoryMappedFileAccess access, out IntPtr mmap_handle, out IntPtr base_address)
		{
			int error = MapInternal (handle, offset, ref size, access, out mmap_handle, out base_address);
			if (error != 0)
				throw CreateException (error, "<none>");
		}

		static Exception CreateException (int error, string path) {
			switch (error){
			case 1:
				return new ArgumentException ("A positive capacity must be specified for a Memory Mapped File backed by an empty file.");
			case 2:
				return new ArgumentOutOfRangeException ("The capacity may not be smaller than the file size.");
			case 3:
				return new FileNotFoundException (path);
			case 4:
				return new IOException ("The file already exists");
			case 5:
				return new PathTooLongException ();
			case 6:
				return new IOException ("Could not open file");
			case 7:
				return new ArgumentException ("Capacity must be bigger than zero for non-file mappings");
			case 8:
				return new ArgumentException ("Invalid FileMode value.");
			case 9:
				return new IOException ("Could not map file");
			default:
				return new IOException ("Failed with unknown error code " + error);
			}
		}

		internal static IntPtr OpenFile (string path, FileMode mode, string mapName, out long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options)
		{
			int error = 0;
			IntPtr res = OpenFileInternal (path, mode, mapName, out capacity, access, options, out error);
			if (error != 0)
				throw CreateException (error, path);
			return res;
		}

		internal static IntPtr OpenHandle (IntPtr handle, string mapName, out long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options)
		{
			int error = 0;
			IntPtr res = OpenHandleInternal (handle, mapName, out capacity, access, options, out error);
			if (error != 0)
				throw CreateException (error, "<none>");
			return res;
		}
	}


	public class MemoryMappedFile : IDisposable {
		MemoryMappedFileAccess fileAccess;
		string name;
		long fileCapacity;

		//
		// We allow the use of either the FileStream/keepOpen combo
		// or a Unix file descriptor.  This way we avoid the dependency on
		// Mono's io-layer having the Unix file descriptors mapped to
		// the same io-layer handle
		//
		FileStream stream;
		bool keepOpen;
		IntPtr handle;

		public static MemoryMappedFile CreateFromFile (string path)
		{
			return CreateFromFile (path, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode)
		{
			long capacity = 0;
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");
			if (mode == FileMode.Append)
				throw new ArgumentException ("mode");

			IntPtr handle = MemoryMapImpl.OpenFile (path, mode, null, out capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages);

			return new MemoryMappedFile () {
				handle = handle,
				fileAccess = MemoryMappedFileAccess.ReadWrite,
				fileCapacity = capacity
			};
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName)
		{
			return CreateFromFile (path, mode, mapName, 0, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName, long capacity)
		{
			return CreateFromFile (path, mode, mapName, capacity, MemoryMappedFileAccess.ReadWrite);
		}

		public static MemoryMappedFile CreateFromFile (string path, FileMode mode, string mapName, long capacity, MemoryMappedFileAccess access)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path.Length == 0)
				throw new ArgumentException ("path");
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if (mode == FileMode.Append)
				throw new ArgumentException ("mode");			
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");

			IntPtr handle = MemoryMapImpl.OpenFile (path, mode, mapName, out capacity, access, MemoryMappedFileOptions.DelayAllocatePages);
			
			return new MemoryMappedFile () {
				handle = handle,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity
			};
		}

		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access,
							       HandleInheritability inheritability,
							       bool leaveOpen)
		{
			if (fileStream == null)
				throw new ArgumentNullException ("fileStream");
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if ((!MonoUtil.IsUnix && capacity == 0 && fileStream.Length == 0) || (capacity > fileStream.Length))
				throw new ArgumentException ("capacity");

			IntPtr handle = MemoryMapImpl.OpenHandle (fileStream.Handle, mapName, out capacity, access, MemoryMappedFileOptions.DelayAllocatePages);
			
			MemoryMapImpl.ConfigureHandleInheritability (handle, inheritability);
				
			return new MemoryMappedFile () {
				handle = handle,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity,

				stream = fileStream,
				keepOpen = leaveOpen
			};
		}

		[MonoLimitation ("memoryMappedFileSecurity is currently ignored")]
		public static MemoryMappedFile CreateFromFile (FileStream fileStream, string mapName, long capacity, MemoryMappedFileAccess access,
							       MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability,
							       bool leaveOpen)
		{
			if (fileStream == null)
				throw new ArgumentNullException ("fileStream");
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if ((!MonoUtil.IsUnix && capacity == 0 && fileStream.Length == 0) || (capacity > fileStream.Length))
				throw new ArgumentException ("capacity");

			IntPtr handle = MemoryMapImpl.OpenHandle (fileStream.Handle, mapName, out capacity, access, MemoryMappedFileOptions.DelayAllocatePages);
			
			MemoryMapImpl.ConfigureHandleInheritability (handle, inheritability);
				
			return new MemoryMappedFile () {
				handle = handle,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity,

				stream = fileStream,
				keepOpen = leaveOpen
			};
		}


		static MemoryMappedFile CoreShmCreate (string mapName, long capacity, MemoryMappedFileAccess access,
							  MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity,
							  HandleInheritability inheritability, FileMode mode)
		{
			if (mapName != null && mapName.Length == 0)
				throw new ArgumentException ("mapName");
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");

			IntPtr handle = MemoryMapImpl.OpenFile (null, mode, mapName, out capacity, access, options);
			
			return new MemoryMappedFile () {
				handle = handle,
				fileAccess = access,
				name = mapName,
				fileCapacity = capacity
			};			
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity)
		{
			return CreateNew (mapName, capacity, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, null, HandleInheritability.None);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access) 
		{
			return CreateNew (mapName, capacity, access, MemoryMappedFileOptions.DelayAllocatePages, null, HandleInheritability.None);
		}

		[MonoLimitation ("Named mappings scope is process local; options is ignored")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, HandleInheritability inheritability)
		{
			return CreateNew (mapName, capacity, access, options, null, inheritability);
		}

		[MonoLimitation ("Named mappings scope is process local; options and memoryMappedFileSecurity are ignored")]
		public static MemoryMappedFile CreateNew (string mapName, long capacity, MemoryMappedFileAccess access,
							  MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity,
							  HandleInheritability inheritability)
		{
			return CoreShmCreate (mapName, capacity, access, options, memoryMappedFileSecurity, inheritability, FileMode.CreateNew);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity)
		{
			return CreateOrOpen (mapName, capacity, MemoryMappedFileAccess.ReadWrite);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access)
		{
			return CreateOrOpen (mapName, capacity, access, MemoryMappedFileOptions.DelayAllocatePages, null, HandleInheritability.None);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, HandleInheritability inheritability)
		{
			return CreateOrOpen (mapName, capacity, access, options, null, inheritability);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile CreateOrOpen (string mapName, long capacity, MemoryMappedFileAccess access, MemoryMappedFileOptions options, MemoryMappedFileSecurity memoryMappedFileSecurity, HandleInheritability inheritability)
		{
			return CoreShmCreate (mapName, capacity, access, options, memoryMappedFileSecurity, inheritability, FileMode.OpenOrCreate);
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile OpenExisting (string mapName)
		{
			throw new NotImplementedException ();
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile OpenExisting (string mapName, MemoryMappedFileRights desiredAccessRights)
		{
			throw new NotImplementedException ();
		}

		[MonoLimitation ("Named mappings scope is process local")]
		public static MemoryMappedFile OpenExisting (string mapName, MemoryMappedFileRights desiredAccessRights, HandleInheritability inheritability)
		{
			throw new NotImplementedException ();
		}

		public MemoryMappedViewStream CreateViewStream ()
		{
			return CreateViewStream (0, 0);//FIXME this is wrong
		}

		public MemoryMappedViewStream CreateViewStream (long offset, long size)
		{
			return CreateViewStream (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		public MemoryMappedViewStream CreateViewStream (long offset, long size, MemoryMappedFileAccess access)
		{
			var view = MemoryMappedView.Create (handle, offset, size, access);
			return new MemoryMappedViewStream (view);
		}

		public MemoryMappedViewAccessor CreateViewAccessor ()
		{
			return CreateViewAccessor (0, 0);
		}

		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size)
		{
			return CreateViewAccessor (offset, size, MemoryMappedFileAccess.ReadWrite);
		}

		public MemoryMappedViewAccessor CreateViewAccessor (long offset, long size, MemoryMappedFileAccess access)
		{
			var view = MemoryMappedView.Create (handle, offset, size, access);
			return new MemoryMappedViewAccessor (view);
		}

		MemoryMappedFile ()
		{
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing){
				if (stream != null){
					if (keepOpen == false)
						stream.Close ();
					stream = null;
				}
				if (handle != IntPtr.Zero) {
					MemoryMapImpl.CloseMapping (handle);
					handle = IntPtr.Zero;
				}
			}
		}

		[MonoTODO]
		public MemoryMappedFileSecurity GetAccessControl ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetAccessControl (MemoryMappedFileSecurity memoryMappedFileSecurity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SafeMemoryMappedFileHandle SafeMemoryMappedFileHandle {
			get {
				throw new NotImplementedException ();
			}
		}

		// This converts a MemoryMappedFileAccess to a FileAccess. MemoryMappedViewStream and
		// MemoryMappedViewAccessor subclass UnmanagedMemoryStream and UnmanagedMemoryAccessor, which both use
		// FileAccess to determine whether they are writable and/or readable.
		internal static FileAccess GetFileAccess (MemoryMappedFileAccess access) {

			if (access == MemoryMappedFileAccess.Read) {
				return FileAccess.Read;
			}
			if (access == MemoryMappedFileAccess.Write) {
				return FileAccess.Write;
			}
			else if (access == MemoryMappedFileAccess.ReadWrite) {
				return FileAccess.ReadWrite;
			}
			else if (access == MemoryMappedFileAccess.CopyOnWrite) {
				return FileAccess.ReadWrite;
			}
			else if (access == MemoryMappedFileAccess.ReadExecute) {
				return FileAccess.Read;
			}
			else if (access == MemoryMappedFileAccess.ReadWriteExecute) {
				return FileAccess.ReadWrite;
			}

			// If we reached here, access was invalid.
			throw new ArgumentOutOfRangeException ("access");
		}
	}
}

