//
// System.Runtime.InteropServices.SafeHandle
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace System.Runtime.InteropServices
{
	[MonoTODO]
	public abstract class SafeHandle : CriticalFinalizerObject, IDisposable {
		protected IntPtr handle;

		[MonoTODO]
		protected SafeHandle (IntPtr invalidHandleValue, bool ownsHandle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Close () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DangerousAddRef (ref bool success) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IntPtr DangerousGetHandle () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DangerousRelease () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Dispose () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetHandleAsInvalid () {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void Dispose (bool disposing) {
			throw new NotImplementedException ();
		}

		protected abstract bool ReleaseHandle ();

		[MonoTODO]
		protected void SetHandle (IntPtr handle) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsClosed {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract bool IsInvalid {
			get;
		}
	}
}
#endif
