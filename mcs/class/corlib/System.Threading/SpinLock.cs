// SpinLock.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

#if NET_4_0
namespace System.Threading
{
	// Implement the ticket SpinLock algorithm described on http://locklessinc.com/articles/locks/
	// This lock is usable on both endianness
	// TODO: some 32 bits platform apparently doesn't support CAS with 64 bits value
	internal static class SpinLockHelpers
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

		internal static void EnterLock (ref TicketType ticket)
		{
			int slot = Interlocked.Increment (ref ticket.Users) - 1;

			SpinWait wait;
			while (slot != ticket.Value)
				wait.SpinOnce ();
		}

		internal static void ReleaseLock (ref TicketType ticket, bool flush)
		{
			if (flush)
				Interlocked.Increment (ref ticket.Value);
			else
				ticket.Value++;
		}

		internal static bool TryEnterLock (ref TicketType ticket)
		{
			long u = ticket.Users;
			long totalValue = (u << 32) | u;
			long newTotalValue
				= BitConverter.IsLittleEndian ? (u << 32) | (u + 1) : ((u + 1) << 32) | u;

			return Interlocked.CompareExchange (ref ticket.TotalValue, newTotalValue, totalValue) == totalValue;
		}

		internal static bool IsLockHeld (ref TicketType ticket)
		{
			// No need for barrier here
			long totalValue = ticket.TotalValue;
			return (totalValue >> 32) != (totalValue & 0xFFFFFFFF);
		}
	}

	public struct SpinLock
	{
		SpinLockHelpers.TicketType tickets;

		int threadWhoTookLock;
		readonly bool isThreadOwnerTrackingEnabled;

		public bool IsThreadOwnerTrackingEnabled {
			get {
				return isThreadOwnerTrackingEnabled;
			}
		}

		public bool IsHeld {
			get {
				return SpinLockHelpers.IsLockHeld (ref tickets);
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

		public SpinLock (bool trackId)
		{
			this.isThreadOwnerTrackingEnabled = trackId;
			this.threadWhoTookLock = 0;
			this.tickets = new SpinLockHelpers.TicketType ();
		}

		[MonoTODO("This method is not rigorously correct. Need CER treatment")]
		public void Enter (ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException ("lockTaken", "lockTaken must be initialized to false");
			if (isThreadOwnerTrackingEnabled && IsHeldByCurrentThread)
				throw new LockRecursionException ();

			SpinLockHelpers.EnterLock (ref tickets);
			lockTaken = true;
			
			threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
		}

		[MonoTODO("This method is not rigorously correct. Need CER treatment")]
		public void TryEnter (ref bool lockTaken)
		{
			TryEnter (0, ref lockTaken);
		}

		[MonoTODO("This method is not rigorously correct. Need CER treatment")]
		public void TryEnter (TimeSpan timeout, ref bool lockTaken)
		{
			TryEnter ((int)timeout.TotalMilliseconds, ref lockTaken);
		}

		[MonoTODO("This method is not rigorously correct. Need CER treatment")]
		public void TryEnter (int milliSeconds, ref bool lockTaken)
		{
			if (milliSeconds < -1)
				throw new ArgumentOutOfRangeException ("milliSeconds", "millisecondsTimeout is a negative number other than -1");
			if (lockTaken)
				throw new ArgumentException ("lockTaken", "lockTaken must be initialized to false");
			if (isThreadOwnerTrackingEnabled && IsHeldByCurrentThread)
				throw new LockRecursionException ();

			Watch sw = Watch.StartNew ();

			do {
				if (lockTaken = SpinLockHelpers.TryEnterLock (ref tickets)) {
					threadWhoTookLock = Thread.CurrentThread.ManagedThreadId;
					break;
				}
			} while (milliSeconds == -1 || (milliSeconds > 0 && sw.ElapsedMilliseconds < milliSeconds));
		}

		public void Exit ()
		{
			Exit (false);
		}

		public void Exit (bool flushReleaseWrites)
		{
			if (isThreadOwnerTrackingEnabled && !IsHeldByCurrentThread)
				throw new SynchronizationLockException ("Current thread is not the owner of this lock");

			threadWhoTookLock = int.MinValue;
			SpinLockHelpers.ReleaseLock (ref tickets, flushReleaseWrites);
		}
	}

	// Wraps a SpinLock in a reference when we need to pass
	// around the lock
	internal class SpinLockWrapper
	{
		public SpinLock Lock = new SpinLock (false);
	}
}
#endif
