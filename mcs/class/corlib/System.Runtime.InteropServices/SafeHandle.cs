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
// Notes:
//     This code is only API complete, but it lacks the runtime support
//     for CriticalFinalizerObject and any P/Invoke wrapping that might
//     happen.
//
//     For details, see:
//     http://blogs.msdn.com/cbrumme/archive/2004/02/20/77460.aspx
//

#if NET_2_0
using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace System.Runtime.InteropServices
{
	public abstract class SafeHandle : CriticalFinalizerObject, IDisposable {
		object handle_lock = new object ();
		protected IntPtr handle;
		IntPtr invalid_handle_value;
		int refcount = 0;
		bool owns_handle;
		
		protected SafeHandle (IntPtr invalidHandleValue, bool ownsHandle)
		{
			invalid_handle_value = invalidHandleValue;
			owns_handle = ownsHandle;
			refcount = 1;
		}

		public void Close ()
		{
			lock (handle_lock){
				refcount--;
				if (refcount == 0){
					ReleaseHandle ();
					handle = invalid_handle_value;
				}
			}
		}

		//
		// I do not know when we could not be able to increment the
		// reference count and set success to false.   It might just
		// be a convention used for the following code pattern:
		//
		// bool release = false
		// try { x.DangerousAddRef (ref release); ... }
		// finally { if (release) x.DangerousRelease (); }
		//
		public void DangerousAddRef (ref bool success)
		{
			lock (handle_lock){
				refcount++;
				success = true;
			}
		}

		public IntPtr DangerousGetHandle ()
		{
			return handle;
		}

		public void DangerousRelease ()
		{
			lock (handle_lock){
				refcount--;
				if (refcount == 0)
					ReleaseHandle ();
			}
		}

		public virtual void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		//
		// See documentation, this invalidates the handle without
		// closing it.
		//
		public void SetHandleAsInvalid ()
		{
			handle = invalid_handle_value;
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing)
				Close ();
			else {
				//
				// The docs say `never call this with disposing=false',
				// the question is whether:
				//   * The runtime will ever call Dipose(false) for SafeHandles (special runtime case)
				//   * Whether we should just call ReleaseHandle regardless?
				// 
			}
		}

		protected abstract bool ReleaseHandle ();

		protected void SetHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		public bool IsClosed {
			get {
				return refcount == 0;
			}
		}

		public abstract bool IsInvalid {
			get;
		}
	}
}
#endif
