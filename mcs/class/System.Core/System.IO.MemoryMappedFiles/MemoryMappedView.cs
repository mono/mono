//
// MemoryMappedView.cs
//
// Authors:
//	Marcos Henrich (marcos.henrich@gmail.com)
//
// Copyright (C) 2015, Xamarin, Inc
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
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace System.IO.MemoryMappedFiles
{
	internal class MemoryMappedView : IDisposable {
		private SafeMemoryMappedViewHandle m_viewHandle;
		private Int64 m_pointerOffset;
		private Int64 m_size;
		private MemoryMappedFileAccess m_access;

		[System.Security.SecurityCritical]
		private unsafe MemoryMappedView(SafeMemoryMappedViewHandle viewHandle, Int64 pointerOffset, 
											Int64 size, MemoryMappedFileAccess access) {

			m_viewHandle = viewHandle;
			m_pointerOffset = pointerOffset;
			m_size = size;
			m_access = access;
		}

		internal SafeMemoryMappedViewHandle ViewHandle {
			[System.Security.SecurityCritical]
			get {
				return m_viewHandle;
			}
		}

		internal Int64 PointerOffset {
			get {
				return m_pointerOffset;
			}
		}

		internal Int64 Size {
			get {
				return m_size;
			}
		}

		internal MemoryMappedFileAccess Access {
			get {
				return m_access;
			}
		}

		internal unsafe static MemoryMappedView Create (IntPtr handle, long offset, long size, MemoryMappedFileAccess access)
		{
			IntPtr base_address;
			IntPtr mmap_handle;

			MemoryMapImpl.Map (handle, offset, ref size, access, out mmap_handle, out base_address);

			var safe_handle = new SafeMemoryMappedViewHandle (mmap_handle, base_address, size);

			// MemoryMapImpl.Map returns a base_address to the offset so MemoryMappedView is initiated
			// no offset.
			return new MemoryMappedView (safe_handle, 0, size, access);
		}

		public void Flush (IntPtr capacity)
		{
			m_viewHandle.Flush ();
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (m_viewHandle != null && !m_viewHandle.IsClosed) {
				m_viewHandle.Dispose ();
			}
		}
 
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
 
		internal bool IsClosed {
			get {
				return (m_viewHandle == null || m_viewHandle.IsClosed);
			}
		}
	}
}

