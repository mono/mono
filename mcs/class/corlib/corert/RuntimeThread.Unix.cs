using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Internal.Runtime.Augments
{
	public sealed partial class RuntimeThread
	{
		private RuntimeThread ()
		{
			_waitedSafeWaitHandles = new WaitHandleArray<SafeWaitHandle>(elementInitializer: null);
			_threadState = (int)ThreadState.Unstarted;
			_priority = ThreadPriority.Normal;
			_lock = new Lock();

			_waitInfo = new WaitSubsystem.ThreadWaitInfo(this);

			PlatformSpecificInitialize();
		}
	}
}