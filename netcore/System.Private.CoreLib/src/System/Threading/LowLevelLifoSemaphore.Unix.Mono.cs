// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Runtime.CompilerServices;

namespace System.Threading
{
	internal unsafe sealed partial class LowLevelLifoSemaphore : IDisposable
	{
		struct WaitEntry
		{
			public object condition;
			public bool signaled;
			public void* previous;
			public void* next;
		}

		private object mutex;
		private void* head;
		private uint pending_signals;

		private void Create (int maximumSignalCount)
		{
			mutex = new object();
			head = null;
			pending_signals = 0;
		}

		public void Dispose ()
		{
		}

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		private bool WaitCore (int timeoutMs)
		{
			WaitEntry wait_entry = new WaitEntry ();
			bool mutexLocked = false;
			bool waitEntryLocked = false;
			bool allowInterruption = false;

			try {
				Monitor.try_enter_with_atomic_var (ref mutex, Timeout.Infinite, allowInterruption, ref mutexLocked);

				if (pending_signals > 0) {
					--pending_signals;
					return true;
				}

				wait_entry.condition = new object();
				wait_entry.previous = null;
				wait_entry.next = head;
				if (head != null) {
					Unsafe.AsRef<WaitEntry> (head).previous = Unsafe.AsPointer<WaitEntry> (ref wait_entry);
				}
				head = Unsafe.AsPointer<WaitEntry> (ref wait_entry);
			}
			finally {
				if (mutexLocked) {
					Monitor.Exit (mutex);
				}
			}

			try {
				Monitor.try_enter_with_atomic_var (ref wait_entry.condition, Timeout.Infinite, allowInterruption, ref waitEntryLocked);
				if (!wait_entry.signaled) {
					Monitor.Monitor_wait (ref wait_entry.condition, timeoutMs, allowInterruption);
				}
			}
			finally {
				if (waitEntryLocked)
					Monitor.Exit (wait_entry.condition);
			}
	
			mutexLocked = false;
			try {
				Monitor.try_enter_with_atomic_var (ref mutex, Timeout.Infinite, allowInterruption, ref mutexLocked);

				if (!wait_entry.signaled) {
					if (head == Unsafe.AsPointer<WaitEntry> (ref wait_entry)) {
						head = wait_entry.next;
					}
					if (wait_entry.next != null) {
						Unsafe.AsRef<WaitEntry> (wait_entry.next).previous = wait_entry.previous;
					}
					if (wait_entry.previous != null) {
						Unsafe.AsRef<WaitEntry> (wait_entry.previous).next = wait_entry.next;
					}
				}
			}
			finally {
				if (mutexLocked) {
					Monitor.Exit (mutex);
				}
			}

			return wait_entry.signaled;
		}

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		private void ReleaseCore (int count)
		{
			bool allowInterruption = false;
			bool mutexLocked = false;
			try {
				Monitor.try_enter_with_atomic_var (ref mutex, Timeout.Infinite, allowInterruption, ref mutexLocked);
				while (count > 0) {
					if (head != null) {
						ref WaitEntry wait_entry = ref Unsafe.AsRef<WaitEntry> (head);
						head = wait_entry.next;
						if (head != null) {
							Unsafe.AsRef<WaitEntry> (head).previous = null;
						}
						wait_entry.previous = null;
						wait_entry.next = null;
						lock (wait_entry.condition)
						{
							wait_entry.signaled = true;
							Monitor.Pulse (wait_entry.condition);
						}
						--count;
					} else {
						pending_signals += (uint)count;
						count = 0;
					}
				}
			}
			finally {
				if (mutexLocked) {
					Monitor.Exit (mutex);
				}
			}
		}
	}
}
