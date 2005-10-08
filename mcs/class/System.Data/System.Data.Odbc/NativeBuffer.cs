//
// NativeBuffer.cs: class for handling native pointers.
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//   Sureshkumar T <tsureshkumar@novell.com>  2004.
//
// Copyright (C) Brian Ritchie, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Data;
using System.Data.Common;

using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
        sealed class NativeBuffer : IDisposable
	{
		private IntPtr _ptr;
		private int _length;
		private bool disposed;

		public NativeBuffer ()
		{
		}

		public IntPtr Handle
		{
			get { return _ptr; }
			set { _ptr = value; }
		}

		public int Size
		{
			get { return _length; }
		}

		public void AllocBuffer (int length)
		{
			FreeBuffer ();
			_ptr = Marshal.AllocCoTaskMem (length);
			_length = length;
		}

		public void FreeBuffer ()
		{
			if (_ptr == IntPtr.Zero)
				return;

			Marshal.FreeCoTaskMem (_ptr);
			_length = 0;
			_ptr = IntPtr.Zero;
		}

		public void EnsureAlloc (int length)
		{
			if (Size == length && _ptr != IntPtr.Zero)
				return;
			
			AllocBuffer (length);
		}

		public void Dispose (bool disposing)
		{
			if (disposed)
				return;
			FreeBuffer ();
			_ptr = IntPtr.Zero;
			disposed = true;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~NativeBuffer ()
		{
			Dispose (false);
		}

		public static implicit operator IntPtr (NativeBuffer buf)
		{
			return buf.Handle;
		}
	}
}
