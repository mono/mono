//
// System.IO.UnmanagedMemoryAccessor.cs
//
// Author:
//  Zoltan Varga (vargaz@gmail.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace System.IO
{
	public class UnmanagedMemoryAccessor : IDisposable {
		SafeBuffer buffer;
		long offset;
		long capacity;
		FileAccess access;

		[MonoTODO]
		public UnmanagedMemoryAccessor () {
			throw new NotImplementedException ();
		}

		public UnmanagedMemoryAccessor (SafeBuffer buffer, long offset, long capacity) : this (buffer, offset, capacity, FileAccess.ReadWrite) {
		}

		public UnmanagedMemoryAccessor (SafeBuffer buffer, long offset, long capacity, FileAccess access) {
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset");
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			if (offset + capacity < 0)
				throw new InvalidOperationException ();
			this.buffer = buffer;
			this.offset = offset;
			this.capacity = capacity;
			this.access = access;
		}

		public void Dispose () {
			Dispose (true);
		}

		public void Dispose (bool disposing) {
		}
	}
}

#endif