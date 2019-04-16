using System.Runtime.CompilerServices;

namespace System.Threading
{
	partial class Monitor
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern void Enter (object obj);

		public static void Enter (object obj, ref bool lockTaken)
		{
			if (lockTaken)
				throw new ArgumentException (SR.Argument_MustBeFalse, nameof (lockTaken));

			ReliableEnter (obj, ref lockTaken);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern void Exit (object obj);

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
	}
}
