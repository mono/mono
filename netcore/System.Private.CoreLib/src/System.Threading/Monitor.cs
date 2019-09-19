// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Threading
{
	public static class Monitor
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void Enter_icall (ref object obj, out Exception exception_handle);

		public static void Enter (object obj)
		{
			Exception exception; // internal temporary handle for native code, not always initialized
			Enter_icall (ref obj, out exception);
		}

		public static void Enter (object obj, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));

			ReliableEnter (obj, ref lockTaken);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void Exit_icall (ref object obj);

		public static void Exit (object obj)
		{
			Exit_icall (ref obj);
		}

		public static bool TryEnter (object obj)
		{
			bool lockTaken = false;
			TryEnter (obj, 0, ref lockTaken);
			return lockTaken;
		}

		public static void TryEnter (object obj, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));

			ReliableEnterTimeout (obj, 0, ref lockTaken);
		}

		public static bool TryEnter (object obj, int millisecondsTimeout)
		{
			bool lockTaken = false;
			TryEnter (obj, millisecondsTimeout, ref lockTaken);
			return lockTaken;
		}

		static int MillisecondsTimeoutFromTimeSpan (TimeSpan timeout)
		{
			long tm = (long) timeout.TotalMilliseconds;
			if (tm < -1 || tm > (long) int.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
			return (int) tm;
		}

		public static bool TryEnter (object obj, TimeSpan timeout)
		{
			return TryEnter (obj, MillisecondsTimeoutFromTimeSpan (timeout));
		}

		public static void TryEnter (object obj, int millisecondsTimeout, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));
			ReliableEnterTimeout (obj, millisecondsTimeout, ref lockTaken);
		}

		public static void TryEnter(object obj, TimeSpan timeout, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));
			ReliableEnterTimeout (obj, MillisecondsTimeoutFromTimeSpan (timeout), ref lockTaken);
		}

		public static bool IsEntered (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));
			return IsEnteredNative (obj);
		}

		public static bool Wait (object obj, int millisecondsTimeout, bool exitContext)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));
			return ObjWait (exitContext, millisecondsTimeout, obj);
		}

		public static bool Wait (object obj, TimeSpan timeout, bool exitContext) => Wait (obj, MillisecondsTimeoutFromTimeSpan (timeout), exitContext);

		public static bool Wait (object obj, int millisecondsTimeout) => Wait (obj, millisecondsTimeout, false);

		public static bool Wait(object obj, TimeSpan timeout) => Wait (obj, MillisecondsTimeoutFromTimeSpan (timeout), false);

		public static bool Wait(object obj) => Wait (obj, Timeout.Infinite, false);

		public static void Pulse (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));
			ObjPulse (obj);
		}

		public static void PulseAll (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));
			ObjPulseAll (obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool Monitor_test_synchronised (ref object obj);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void Monitor_pulse (ref object obj);

		static void ObjPulse (object obj)
		{
			if (!Monitor_test_synchronised (ref obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			Monitor_pulse (ref obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void Monitor_pulse_all (ref object obj);

		static void ObjPulseAll (object obj)
		{
			if (!Monitor_test_synchronised (ref fobj))
				throw new SynchronizationLockException ("Object is not synchronized");

			Monitor_pulse_all (ref obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool Monitor_wait (ref object obj, int ms);

		static bool ObjWait (bool exitContext, int millisecondsTimeout, object obj)
		{
			if (millisecondsTimeout < 0 && millisecondsTimeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			if (!Monitor_test_synchronised (ref obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			return Monitor_wait (ref obj, millisecondsTimeout);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void try_enter_with_atomic_var (ref object obj, int millisecondsTimeout, ref bool lockTaken, out Exception exception_handle);

		static void ReliableEnterTimeout (object obj, int timeout, ref bool lockTaken)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));

			if (timeout < 0 && timeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException (nameof (timeout));

			Exception exception; // internal temporary handle for native code, not always initialized
			try_enter_with_atomic_var (ref obj, timeout, ref lockTaken, out exception);
		}

		static void ReliableEnter (object obj, ref bool lockTaken)
		{
			ReliableEnterTimeout (obj, (int) Timeout.Infinite, ref lockTaken);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool Monitor_test_owner (ref object obj);

		static bool IsEnteredNative (object obj)
		{
			return Monitor_test_owner (ref obj);
		}
		
		public static long LockContentionCount
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
	}
}
