namespace System.Threading
{
	partial class ThreadPool
	{
		static void EnsureInitialized ()
		{
		}

		internal static bool RequestWorkerThread ()
		{
			throw new NotImplementedException ();
		}

		internal static bool KeepDispatching(int startTickCount)
		{
			throw new NotImplementedException ();
		}

		internal static void NotifyWorkItemProgress ()
		{
		}

		static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state,
			 uint millisecondsTimeOutInterval, bool executeOnlyOnce, bool compressStack)
		{
			throw new NotImplementedException ();
		}

		internal static void ReportThreadStatus (bool isWorking)
		{
		}

		internal static bool NotifyWorkItemComplete ()
		{
			throw new NotImplementedException ();
		}

		public static bool BindHandle (System.IntPtr osHandle) { throw null; }
		public static bool BindHandle (System.Runtime.InteropServices.SafeHandle osHandle) { throw null; }
		public static void GetAvailableThreads (out int workerThreads, out int completionPortThreads) { throw null; }
		public static void GetMaxThreads (out int workerThreads, out int completionPortThreads) { throw null; }
		public static void GetMinThreads (out int workerThreads, out int completionPortThreads) { throw null; }
		public static bool SetMaxThreads(int workerThreads, int completionPortThreads) { throw null; }
		public static bool SetMinThreads(int workerThreads, int completionPortThreads) { throw null; }
		public static unsafe bool UnsafeQueueNativeOverlapped(System.Threading.NativeOverlapped* overlapped) { throw null; }
	}
}