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
//     CER-like behavior is implemented for Close and DangerousAddRef
//     via the try/finally uninterruptible pattern in case of async
//     exceptions like ThreadAbortException.
//
// On implementing SafeHandles:
//     http://blogs.msdn.com/bclteam/archive/2005/03/15/396335.aspx
//
// Issues:
//
//     TODO: Although DangerousAddRef has been implemented, I need to
//     find out whether the runtime performs the P/Invoke if the
//     handle has been disposed already.
//
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Runtime.InteropServices
{
	public abstract class SafeHandle : CriticalFinalizerObject, IDisposable {
		//
		// Warning: the offset of handle is mapped inside the runtime
		// if you move this, you must updated the runtime definition of
		// MonoSafeHandle
		//
		protected IntPtr handle;
		IntPtr invalid_handle_value;
		int refcount = 0;
		bool owns_handle;
		
#if NET_2_1
		protected SafeHandle ()
		{
			throw new NotImplementedException ();
		}
#endif
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		protected SafeHandle (IntPtr invalidHandleValue, bool ownsHandle)
		{
			invalid_handle_value = invalidHandleValue;
			owns_handle = ownsHandle;
			refcount = 1;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Close ()
		{
			if (refcount == 0)
				throw new ObjectDisposedException (GetType ().FullName);

			int newcount = 0, current = 0;
			bool registered = false;
			RuntimeHelpers.PrepareConstrainedRegions ();
			try {
				do {
					current = refcount;
					newcount = current-1;

					// perform changes in finally to avoid async interruptions
					try {}
					finally {
						if (Interlocked.CompareExchange (ref refcount, newcount, current) == current)
							registered = true;
					}
				} while (!registered);
			} finally {
				if (registered && newcount == 0 && owns_handle && !IsInvalid){
					ReleaseHandle ();
					handle = invalid_handle_value;
					refcount = -1;
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
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public void DangerousAddRef (ref bool success)
		{
			if (refcount <= 0)
				throw new ObjectDisposedException (GetType ().FullName);

			bool registered = false;
			int newcount, current;
			do {
				current = refcount;
				newcount = current + 1;
				
				if (current <= 0){
					//
					// In MS, calling sf.Close () followed by a call
					// to P/Invoke with SafeHandles throws this, but
					// am left wondering: when would "success" be
					// set to false?
					//
					throw new ObjectDisposedException (GetType ().FullName);
				}

				// perform changes in finally to avoid async interruptions
				RuntimeHelpers.PrepareConstrainedRegions ();
				try {}
				finally {
					if (Interlocked.CompareExchange (ref refcount, newcount, current) == current)
						registered = success = true;
				}
			} while (!registered);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public IntPtr DangerousGetHandle ()
		{
			if (refcount <= 0){
				throw new ObjectDisposedException (GetType ().FullName);
			}

			return handle;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void DangerousRelease ()
		{
			if (refcount <= 0)
				throw new ObjectDisposedException (GetType ().FullName);

			int newcount, current;
			do {
				current = refcount;
				newcount = current-1;
			} while (Interlocked.CompareExchange (ref refcount, newcount, current) != current);

			if (newcount == 0 && owns_handle && !IsInvalid){
				ReleaseHandle ();
				handle = invalid_handle_value;
			}
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		//
		// See documentation, this invalidates the handle without
		// closing it.
		//
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void SetHandleAsInvalid ()
		{
			handle = invalid_handle_value;
		}
		
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		protected abstract bool ReleaseHandle ();

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		protected void SetHandle (IntPtr handle)
		{
			this.handle = handle;
		}

		public bool IsClosed {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get {
				return refcount <= 0;
			}
		}

		public abstract bool IsInvalid {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			get;
		}

		~SafeHandle ()
		{
			if (owns_handle && !IsInvalid){
				ReleaseHandle ();
				handle = invalid_handle_value;
			}
		}
	}
}
