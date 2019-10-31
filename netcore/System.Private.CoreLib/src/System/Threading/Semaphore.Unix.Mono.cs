// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace System.Threading
{
	partial class Semaphore
	{
		const int MAX_PATH = 260;

		Semaphore (SafeWaitHandle handle) => this.SafeWaitHandle = handle;

		int ReleaseCore (int releaseCount)
		{
			SafeWaitHandle waitHandle = SafeWaitHandle;
			if (waitHandle.IsInvalid)
				ThrowInvalidHandleException ();
			bool success = false;
			waitHandle.DangerousAddRef (ref success);
			try {
				return WaitSubsystem.ReleaseSemaphore (waitHandle.DangerousGetHandle (), releaseCount);
			} finally {
				if (success)
					waitHandle.DangerousRelease ();
			}
		}

		static OpenExistingResult OpenExistingWorker (string name, out Semaphore result)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));

			if (name.Length  == 0)
				throw new ArgumentException (SR.Argument_StringZeroLength, nameof (name));

			if (name.Length > MAX_PATH)
				throw new ArgumentException (SR.Argument_WaitHandleNameTooLong);

			throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);
		}

		void CreateSemaphoreCore (int initialCount, int maximumCount, string name, out bool createdNew)
		{
			if (name?.Length > MAX_PATH)
				throw new ArgumentException (SR.Argument_WaitHandleNameTooLong);

			if (name != null)
				throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);

			SafeWaitHandle = WaitSubsystem.NewSemaphore (initialCount, maximumCount);
			createdNew = true;
		}
	}
}
