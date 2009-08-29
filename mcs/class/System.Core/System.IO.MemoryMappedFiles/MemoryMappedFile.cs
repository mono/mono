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
			return CreateViewStream (0, 0, MemoryMappedFileAccess.ReadWrite);
		}

		[MonoTODO]
		public MemoryMappedViewStream CreateViewStream (long offset, long size, MemoryMappedFileAccess access) {
			throw new NotImplementedException ();
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
	}
}

#endif