namespace System.Threading
{
	[CLSCompliant(false)]
	public unsafe delegate void IOCompletionCallback(uint errorCode, // Error code
									   uint numBytes, // No. of bytes transferred 
									   NativeOverlapped* pOVERLAP // ptr to OVERLAP structure
									   );

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
	}
}