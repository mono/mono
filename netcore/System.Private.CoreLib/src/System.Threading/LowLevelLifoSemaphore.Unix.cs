// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Internal.Runtime.CompilerServices;

namespace System.Threading
{
	// FIXME: LowLevelLifoSemaphore should be uninterruptible
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

		private bool WaitCore (int timeoutMs)
		{
			WaitEntry wait_entry = new WaitEntry ();

			lock (mutex) {
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

			lock (wait_entry.condition) {
				if (!wait_entry.signaled) {
					Monitor.Wait (wait_entry.condition, timeoutMs, false);
				}
			}
	
			lock (mutex) {
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

			return wait_entry.signaled;
		}

		private void ReleaseCore (int count)
		{
			lock (mutex) {
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
		}
	}
}
