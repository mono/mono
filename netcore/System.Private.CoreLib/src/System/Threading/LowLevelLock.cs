// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Threading
{
	// This class provides implementation of uninterruptible lock for internal
	// use by thread pool.
	internal class LowLevelLock : IDisposable
	{
		public void Dispose ()
		{
		}

		// Do not inline, to ensure icall handles are references to locals and native code can omit barriers.
		//
		[MethodImplAttribute (MethodImplOptions.NoInlining)]
		bool AcquireInternal (int timeout)
		{
			bool lockTaken = false;
			bool allowInterruption = false;
			object self = this;
			Monitor.try_enter_with_atomic_var (ref self, timeout, allowInterruption, ref lockTaken);
			return lockTaken;
		}

		public bool TryAcquire ()
		{
			int timeout_zero = 0;
			return AcquireInternal (timeout_zero);
		}

		public void Acquire ()
		{
			AcquireInternal (Timeout.Infinite);
		}

		public void Release()
		{
			Monitor.Exit (this);
		}

		[Conditional("DEBUG")]
		public void VerifyIsLocked()
		{
			Debug.Assert (Monitor.IsEntered (this));
		}
	}
}
