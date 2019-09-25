// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Threading
{
	public static class Monitor
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern void Enter (object obj);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		public static void Enter (object obj, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));

			ReliableEnter (obj, ref lockTaken);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void Exit_icall (ref object obj);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
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

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static void ObjPulse (object obj)
		{
			if (!Monitor_test_synchronised (ref obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			Monitor_pulse (ref obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void Monitor_pulse_all (ref object obj);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static void ObjPulseAll (object obj)
		{
			if (!Monitor_test_synchronised (ref obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			Monitor_pulse_all (ref obj);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static bool Monitor_wait (ref object obj, int ms, bool allowInterruption);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static bool ObjWait (bool exitContext, int millisecondsTimeout, object obj)
		{
			if (millisecondsTimeout < 0 && millisecondsTimeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			if (!Monitor_test_synchronised (ref obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			bool allowInterruption = true;
			return Monitor_wait (ref obj, millisecondsTimeout, allowInterruption);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void try_enter_with_atomic_var (ref object obj, int millisecondsTimeout, bool allowInterruption, ref bool lockTaken);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static void ReliableEnterTimeout (object obj, int timeout, ref bool lockTaken)
		{
			if (obj == null)
				throw new ArgumentNullException (nameof (obj));

			if (timeout < 0 && timeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException (nameof (timeout));

			bool allowInterruption = true;
			try_enter_with_atomic_var (ref obj, timeout, allowInterruption, ref lockTaken);
		}

		static void ReliableEnter (object obj, ref bool lockTaken)
		{
			ReliableEnterTimeout (obj, (int) Timeout.Infinite, ref lockTaken);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static bool Monitor_test_owner (ref object obj);

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		static bool IsEnteredNative (object obj)
		{
			return Monitor_test_owner (ref obj);
		}
		
		public extern static long LockContentionCount
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
	}
}
