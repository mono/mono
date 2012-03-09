// SpinLock.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

#if NET_4_0 || MOBILE

using System;
using System.Collections.Concurrent;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct TicketType {
		[FieldOffset(0)]
		public long TotalValue;
		[FieldOffset(0)]
		public int Value;
		[FieldOffset(4)]
		public int Users;
	}

	/* Implement the ticket SpinLock algorithm described on http://locklessinc.com/articles/locks/
	 * This lock is usable on both endianness.
	 * All the try/finally patterns in this class and various extra gimmicks compared to the original
	 * algorithm are here to avoid problems caused by asynchronous exceptions.
	 */
	[System.Diagnostics.DebuggerDisplay ("IsHeld = {IsHeld}")]
	[System.Diagnostics.DebuggerTypeProxy ("System.Threading.SpinLock+SystemThreading_SpinLockDebugView")]
	public struct SpinLock
	{
		TicketType ticket;

		int threadWhoTookLock;
		readonly bool isThreadOwnerTrackingEnabled;

		static Watch sw = Watch.StartNew ();

		ConcurrentOrderedList<int> stallTickets;

		public bool IsThreadOwnerTrackingEnabled {
			get {
				return isThreadOwnerTrackingEnabled;
			}
		}

		public bool IsHeld {
			get {
				// No need for barrier here
				long totalValue = ticket.TotalValue;
				return (totalValue >> 32) != (totalValue & 0xFFFFFFFF);
			}
		}

		public bool IsHeldByCurrentThread {
			get {
				if (isThreadOwnerTrackingEnabled)
					return IsHeld && Thread.CurrentThread.ManagedThreadId == threadWhoTookLock;
				else
					return IsHeld;
			}
		}

		public SpinLock (bool enableThreadOwnerTracking)
		{
			this.isThreadOwnerTrackingEnabled = enableThreadOwnerTracking;
			this.threadWhoTookLock = 0;
			this.ticket = new TicketType ();
			this.stallTickets = null;
		}

		[MonoTODO ("Not safe against async exceptions")]
		public void Enter (ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException ("lockTaken", "lockTaken must be initialized to false");
			if (isThreadOwnerTrackingEnabled && IsHeldByCurrentThread)
				throw new LockRecursionException ();

			int slot = -1;

			RuntimeHelpers.PrepareConstrainedRegions ();
			try {
				slot = Interlocked.Increment (ref ticket.Users) - 1;

				SpinWait wait = new SpinWait ();
				while (slot != ticket.Value) {
					wait.SpinOnce ();

					while (stallTickets != null && stallTickets.TryRemove (ticket.Value))
						++ticket.Value;
				}
			} finally {
				if (slot == ticket.Value) {
					lockTaken = true;
					threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
				} else if (slot != -1) {
					// We have been interrupted, initialize stallTickets
					if (stallTickets == null)
						Interlocked.CompareExchange (ref stallTickets, new ConcurrentOrderedList<int> (), null);
					stallTickets.TryAdd (slot);
				}
			}
		}

		public void TryEnter (ref bool lockTaken)
		{
			TryEnter (0, ref lockTaken);
		}

		public void TryEnter (TimeSpan timeout, ref bool lockTaken)
		{
			TryEnter ((int)timeout.TotalMilliseconds, ref lockTaken);
		}

		public void TryEnter (int millisecondsTimeout, ref bool lockTaken)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("milliSeconds", "millisecondsTimeout is a negative number other than -1");
			if (lockTaken)
				throw new ArgumentException ("lockTaken", "lockTaken must be initialized to false");
			if (isThreadOwnerTrackingEnabled && IsHeldByCurrentThread)
				throw new LockRecursionException ();

			long start = millisecondsTimeout == -1 ? 0 : sw.ElapsedMilliseconds;
			bool stop = false;

			do {
				while (stallTickets != null && stallTickets.TryRemove (ticket.Value))
					++ticket.Value;

				long u = ticket.Users;
				long totalValue = (u << 32) | u;
				long newTotalValue
					= BitConverter.IsLittleEndian ? (u << 32) | (u + 1) : ((u + 1) << 32) | u;
				
				RuntimeHelpers.PrepareConstrainedRegions ();
				try {}
				finally {
					lockTaken = Interlocked.CompareExchange (ref ticket.TotalValue, newTotalValue, totalValue) == totalValue;
				
					if (lockTaken) {
						threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
						stop = true;
					}
				}
	        } while (!stop && (millisecondsTimeout == -1 || (sw.ElapsedMilliseconds - start) < millisecondsTimeout));
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Exit ()
		{
			Exit (false);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public void Exit (bool useMemoryBarrier)
		{
			RuntimeHelpers.PrepareConstrainedRegions ();
			try {}
			finally {
				if (isThreadOwnerTrackingEnabled && !IsHeldByCurrentThread)
					throw new SynchronizationLockException ("Current thread is not the owner of this lock");

				threadWhoTookLock = int.MinValue;
				do {
					if (useMemoryBarrier)
						Interlocked.Increment (ref ticket.Value);
					else
						ticket.Value++;
				} while (stallTickets != null && stallTickets.TryRemove (ticket.Value));
			}
		}
	}
}
#endif
