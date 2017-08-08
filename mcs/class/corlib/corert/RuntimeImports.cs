namespace System.Runtime
{
	using System.Runtime.InteropServices;
	using System.Runtime.CompilerServices;

	public static class RuntimeImports
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern unsafe int RhCompatibleReentrantWaitAny(int alertable, int timeout, int count, IntPtr* handles);

		internal static unsafe int RhCompatibleReentrantWaitAny(bool alertable, int timeout, int count, IntPtr* handles)
		{
			return RhCompatibleReentrantWaitAny(alertable ? 1 : 0, timeout, count, handles);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern static int RhHandleAllocInternal(object obj, int handle, GCHandleType type);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern static object RhHandleGet(int handle);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool RhSetThreadExitCallback(IntPtr pCallback);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool RhSpinWaitInternal ();

		// Busy spin for the given number of iterations.
		internal static void RhSpinWait (int iterations)
		{
			if (iterations <= 0)
			{
				return;
			}

			while (iterations-- < 0)
			{
				RhSpinWaitInternal ();
			}
		}

		// Free handle.
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void RhHandleFree(IntPtr handle);

		// Yield the cpu to another thread ready to process, if one is available.
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern int RhYieldInternal();
		internal static bool RhYield() { return (RhYieldInternal() != 0); }

		internal static IntPtr RhHandleAlloc(Object value, GCHandleType type)
		{
			IntPtr h = (IntPtr)RhHandleAllocInternal(value, 0, type);
			if (h == IntPtr.Zero)
				throw new OutOfMemoryException();
			return h;
		}

		// Get object reference from handle.
		internal static object RhHandleGet(IntPtr handle)
		{
			return RhHandleGet((int)handle);
		}
	}
}