//
// System.Threading.Monitor.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading
{
	[ComVisible (true)]
	public static class Monitor
	{
		// Grabs the mutex on object 'obj', with a maximum
		// wait time 'ms' but doesn't block - if it can't get
		// the lock it returns false, true if it can
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_try_enter(object obj, int ms);

		// Enter/Exit are implemented directly as icalls for performance reasons

		// Acquires the mutex on object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void Enter(object obj);

		// Releases the mutex on object 'obj'
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void Exit(object obj);

		// Signals one of potentially many objects waiting on
		// object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Monitor_pulse(object obj);

		// Checks whether object 'obj' is currently synchronised
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_test_synchronised(object obj);

		public static void Pulse(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			Monitor_pulse(obj);
		}

		// Signals all of potentially many objects waiting on
		// object 'obj'
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Monitor_pulse_all(object obj);

		public static void PulseAll(object obj) {
			if(obj==null) {
				throw new ArgumentNullException("obj");
			}
			if(Monitor_test_synchronised(obj)==false) {
				throw new SynchronizationLockException("Object is not synchronized");
			}

			Monitor_pulse_all(obj);
		}

		public static bool TryEnter (object obj)
		{
			return TryEnter (obj, 0);
		}

		public static bool TryEnter (object obj, int millisecondsTimeout)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (millisecondsTimeout == Timeout.Infinite) {
				Enter (obj);
				return true;
			}

			if (millisecondsTimeout < 0)
				throw new ArgumentException ("negative value for millisecondsTimeout", "millisecondsTimeout");
			
			return Monitor_try_enter (obj, millisecondsTimeout);
		}

		public static bool TryEnter (object obj, TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");
			
			return TryEnter (obj, (int) ms);
		}

		// Waits for a signal on object 'obj' with maximum
		// wait time 'ms'. Returns true if the object was
		// signalled, false if it timed out
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Monitor_wait(object obj, int ms);

		public static bool Wait (object obj)
		{
			return Wait (obj, Timeout.Infinite);
		}

		public static bool Wait (object obj, int millisecondsTimeout)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout", "timeout out of range");

			if (!Monitor_test_synchronised (obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			return Monitor_wait (obj, millisecondsTimeout);
		}

		public static bool Wait (object obj, TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");

			return Wait (obj, (int) ms);
		}

		public static bool Wait(object obj, int millisecondsTimeout, bool exitContext) {
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return Wait (obj, millisecondsTimeout);
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		public static bool Wait(object obj, TimeSpan timeout, bool exitContext) {
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return Wait (obj, timeout);
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

#if NET_4_0 || MOONLIGHT
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void try_enter_with_atomic_var (object obj, int millisecondsTimeout, ref bool lockTaken);

		public static void Enter (object obj, ref bool lockTaken)
		{
			TryEnter (obj, Timeout.Infinite, ref lockTaken);
		}

		public static void TryEnter (object obj, ref bool lockTaken)
		{
			TryEnter (obj, 0, ref lockTaken);
		}

		public static void TryEnter (object obj, TimeSpan timeout, ref bool lockTaken)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");
			TryEnter (obj, (int)ms, ref lockTaken);
		}

		public static void TryEnter (object obj, int millisecondsTimeout, ref bool lockTaken)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			if (lockTaken)
				throw new ArgumentException ("lockTaken");

			if (millisecondsTimeout < 0 && millisecondsTimeout != Timeout.Infinite)
				throw new ArgumentException ("negative value for millisecondsTimeout", "millisecondsTimeout");

			try_enter_with_atomic_var (obj, millisecondsTimeout, ref lockTaken);
		}		

#endif
	}
}
