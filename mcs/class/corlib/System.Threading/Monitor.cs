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
#if FEATURE_REMOTING
using System.Runtime.Remoting.Contexts;
#endif
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading
{
	public static partial class Monitor
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Monitor_test_synchronised(object obj);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Monitor_pulse(object obj);

		static void ObjPulse(Object obj)
		{
			if (!Monitor_test_synchronised (obj))
				throw new SynchronizationLockException("Object is not synchronized");

			Monitor_pulse (obj);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Monitor_pulse_all(object obj);

		static void ObjPulseAll(Object obj)
		{
			if (!Monitor_test_synchronised (obj))
				throw new SynchronizationLockException("Object is not synchronized");

			Monitor_pulse_all (obj);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Monitor_wait(object obj, int ms);

		static bool ObjWait(bool exitContext, int millisecondsTimeout, Object obj)
		{
			if (millisecondsTimeout < 0 && millisecondsTimeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");
			if (!Monitor_test_synchronised (obj))
				throw new SynchronizationLockException ("Object is not synchronized");

			try {
#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.ExitContext ();
#endif

				return Monitor_wait (obj, millisecondsTimeout);
			} finally {
#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.EnterContext ();
#endif
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void try_enter_with_atomic_var (object obj, int millisecondsTimeout, ref bool lockTaken);

		static void ReliableEnterTimeout(Object obj, int timeout, ref bool lockTaken)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			if (timeout < 0 && timeout != (int) Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			try_enter_with_atomic_var (obj, timeout, ref lockTaken);
		}

		static void ReliableEnter(Object obj, ref bool lockTaken)
		{
			ReliableEnterTimeout (obj, (int) Timeout.Infinite, ref lockTaken);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Monitor_test_owner (object obj);

		static bool IsEnteredNative(Object obj)
		{
			return Monitor_test_owner (obj);
		}
		
#if NETCORE
		public static long LockContentionCount => throw new PlatformNotSupportedException ();
#endif
	}
}
