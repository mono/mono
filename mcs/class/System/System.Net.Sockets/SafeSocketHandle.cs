//
// System.Net.Sockets.SafeSocketHandle
//
// Authors:
//	Marcos Henrich  <marcos.henrich@xamarin.com>
//

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace System.Net.Sockets {

	sealed class SafeSocketHandle : SafeHandleZeroOrMinusOneIsInvalid {

		List<Thread> blocking_threads;
		Dictionary<Thread, StackTrace> threads_stacktraces;

		bool in_cleanup;

		const int SOCKET_CLOSED = 10004;

		const int ABORT_RETRIES = 10;
		static bool THROW_ON_ABORT_RETRIES = Environment.GetEnvironmentVariable("MONO_TESTS_IN_PROGRESS") == "yes";

		public SafeSocketHandle (IntPtr preexistingHandle, bool ownsHandle) : base (ownsHandle)
		{
			SetHandle (preexistingHandle);

			if (THROW_ON_ABORT_RETRIES)
				threads_stacktraces = new Dictionary<Thread, StackTrace> ();
		}

		// This is just for marshalling
		internal SafeSocketHandle () : base (true)
		{
		}

		protected override bool ReleaseHandle ()
		{
			int error = 0;

			Socket.Blocking_icall (handle, false, out error);
#if FULL_AOT_DESKTOP
			/* It's only for platforms that do not have working syscall abort mechanism, like WatchOS and TvOS */
			Socket.Shutdown_icall (handle, SocketShutdown.Both, out error);
#endif

			if (blocking_threads != null) {
				lock (blocking_threads) {
					int abort_attempts = 0;
					while (blocking_threads.Count > 0) {
						if (abort_attempts++ >= ABORT_RETRIES) {
							if (THROW_ON_ABORT_RETRIES) {
								StringBuilder sb = new StringBuilder ();
								sb.AppendLine ("Could not abort registered blocking threads before closing socket.");
								foreach (var thread in blocking_threads) {
									sb.AppendLine ("Thread StackTrace:");
									sb.AppendLine (threads_stacktraces[thread].ToString ());
								}
								sb.AppendLine ();

								throw new Exception (sb.ToString ());
							}

							// Attempts to close the socket safely failed.
							// We give up, and close the socket with pending blocking system calls.
							// This should not occur, nonetheless if it does this avoids an endless loop.
							break;
						}

						/*
						* This method can be called by the DangerousRelease inside RegisterForBlockingSyscall
						* When this happens blocking_threads contains the current thread.
						* We can safely close the socket and throw SocketException in RegisterForBlockingSyscall
						* before the blocking system call.
						*/
						if (blocking_threads.Count == 1 && blocking_threads[0] == Thread.CurrentThread)
							break;

						// abort registered threads
						foreach (var t in blocking_threads)
							Socket.cancel_blocking_socket_operation (t);

						// Sleep so other threads can resume
						in_cleanup = true;
						Monitor.Wait (blocking_threads, 100);
					}
				}
			}

			Socket.Close_icall (handle, out error);

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
					if (THROW_ON_ABORT_RETRIES)
						threads_stacktraces.Add (Thread.CurrentThread, new StackTrace (true));
				}
				if (release)
					DangerousRelease ();

				// Handle can be closed by DangerousRelease
				if (IsClosed)
					throw new SocketException (SOCKET_CLOSED);
			}
		}

		/* This must be called from a finally block! */
		public void UnRegisterForBlockingSyscall ()
		{
			//If this NRE, we're in deep problems because Register Must have
			lock (blocking_threads) {
				var current = Thread.CurrentThread;
				blocking_threads.Remove (current);
				if (THROW_ON_ABORT_RETRIES) {
					if (blocking_threads.IndexOf (current) == -1)
						threads_stacktraces.Remove (current);
				}

				if (in_cleanup && blocking_threads.Count == 0)
					Monitor.Pulse (blocking_threads);
			}
		}
	}
}

