//
// System.Net.Sockets.SafeSocketHandle
//
// Authors:
//	Marcos Henrich  <marcos.henrich@xamarin.com>
//

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace System.Net.Sockets {

	sealed class SafeSocketHandle : SafeHandleZeroOrMinusOneIsInvalid {

		List<Thread> blocking_threads;
		const int ABORT_RETRIES = 10;

		public SafeSocketHandle (IntPtr preexistingHandle, bool ownsHandle) : base (ownsHandle)
		{
			SetHandle (preexistingHandle);
		}

		// This is just for marshalling
		internal SafeSocketHandle () : base (true)
		{
		}

		protected override bool ReleaseHandle ()
		{
			int error = 0;
			
			Socket.Blocking_internal (handle, false, out error);
	
			if (blocking_threads != null) {
				int abort_attempts = 0;
				while (blocking_threads.Count > 0) {
					if (abort_attempts++ >= ABORT_RETRIES) {
#if DEBUG
						throw new Exception ("Could not abort registered blocking threads before closing socket.");
#else
						// Attempts to close the socket safely failed.
						// We give up, and close the socket with pending blocking system calls.
						// This should not occur, nonetheless if it does this avoids an endless loop.
						break;
#endif
					}

					AbortRegisteredThreads ();
					// Sleep so other threads can resume
					Thread.Sleep (1);
				}
			}

			Socket.Close_internal (handle, out error);

			return error == 0;
		}

		public void RegisterForBlockingSyscall ()
		{
			if (blocking_threads == null)
				Interlocked.CompareExchange (ref blocking_threads, new List<Thread> (), null);
			
			bool release = false;
			try {
				DangerousAddRef (ref release);
			} finally {
				/* We must use a finally block here to make this atomic. */
				lock (blocking_threads) {
					blocking_threads.Add (Thread.CurrentThread);
				}
				if (release)
					DangerousRelease ();
			}
		}

		/* This must be called from a finally block! */
		public void UnRegisterForBlockingSyscall ()
		{
			//If this NRE, we're in deep problems because Register Must have
			lock (blocking_threads) {
				blocking_threads.Remove (Thread.CurrentThread);
			}
		}

		void AbortRegisteredThreads () {
			if (blocking_threads == null)
				return;

			lock (blocking_threads) {
				foreach (var t in blocking_threads)
					Socket.cancel_blocking_socket_operation (t);
			}
		}
	}
}

