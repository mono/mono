// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	partial class Mutex
	{
		Mutex (IntPtr handle) => Handle = handle;

		public void ReleaseMutex ()
		{
			SafeWaitHandle waitHandle = SafeWaitHandle;
			if (waitHandle.IsInvalid)
				ThrowInvalidHandleException ();

			bool success = false;
			waitHandle.DangerousAddRef (ref success);
			try {
				WaitSubsystem.ReleaseMutex (waitHandle.DangerousGetHandle ());
			}finally {
				if (success)
					waitHandle.DangerousRelease ();
			}
		}

		void CreateMutexCore (bool initiallyOwned, string name, out bool createdNew)
		{
			if (name != null)
				throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);

			SafeWaitHandle = WaitSubsystem.NewMutex(initiallyOwned);
			createdNew = true;
		}

		static OpenExistingResult OpenExistingWorker (string name, out Mutex result) 
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));
			result = null;
			if ((name.Length == 0) ||
				(name.Length > 260)) {
				return OpenExistingResult.NameInvalid;
			}

			throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);
		}
	}
}
