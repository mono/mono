using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Internal.Runtime.Augments
{
	public sealed partial class RuntimeThread
	{
		private readonly WaitSubsystem.ThreadWaitInfo _waitInfo;
		
		private RuntimeThread ()
		{
			_waitedSafeWaitHandles = new WaitHandleArray<SafeWaitHandle>(elementInitializer: null);
            _threadState = (int)ThreadState.Unstarted;
            _priority = ThreadPriority.Normal;
            _lock = new Lock();
            
			PlatformSpecificInitialize();
		}

		private void PlatformSpecificInitialize ()
		{
			throw new NotImplementedException ();
		}

		private void PlatformSpecificInitializeExistingThread ()
		{
			throw new NotImplementedException ();
		}

		internal SafeWaitHandle[] RentWaitedSafeWaitHandleArray (int requiredCapacity)
		{
			throw new NotImplementedException ();
		}

		internal void ReturnWaitedSafeWaitHandleArray (SafeWaitHandle[] waitedSafeWaitHandles)
		{
			throw new NotImplementedException ();
		}

		private ThreadPriority GetPriorityLive ()
		{
			throw new NotImplementedException ();
		}

		private bool SetPriorityLive (ThreadPriority priority)
		{
			throw new NotImplementedException ();
		}

		private ThreadState GetThreadState ()
		{
			throw new NotImplementedException ();
		}

		private bool JoinInternal(int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}

		private bool CreateThread(GCHandle thisThreadHandle)
		{
			throw new NotImplementedException ();
		}

#if WIN_PLATFORM
		private static uint ThreadEntryPoint (IntPtr parameter)
		{
			throw new NotImplementedException ();
		}
#else
		private static IntPtr ThreadEntryPoint (IntPtr parameter)
		{ 
			throw new NotImplementedException ();
		}
#endif

		public void Interrupt ()
		{
			throw new NotImplementedException ();
		}

		internal static void UninterruptibleSleep0 ()
		{
			throw new NotImplementedException ();
		}

		private static void SleepInternal (int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}

		internal static bool ReentrantWaitsEnabled => throw new NotImplementedException ();

		internal static void SuppressReentrantWaits ()
		{
			throw new NotImplementedException ();
		}

		internal static void RestoreReentrantWaits ()
		{
			throw new NotImplementedException ();
		}
	}
}