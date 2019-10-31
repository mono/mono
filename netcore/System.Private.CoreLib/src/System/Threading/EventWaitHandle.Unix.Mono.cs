// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	partial class EventWaitHandle
	{
		public bool Set ()
		{
			SafeWaitHandle handle = ValidateHandle (out bool release);

			try {
				WaitSubsystem.SetEvent (handle.DangerousGetHandle ());
				return true;
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		public bool Reset ()
		{
			SafeWaitHandle handle = ValidateHandle (out bool release);

			try {
				WaitSubsystem.ResetEvent (handle.DangerousGetHandle ());
				return true;
			} finally {
				if (release)
					handle.DangerousRelease ();
			}
		}

		void CreateEventCore (bool initialState, EventResetMode mode, string name, out bool createdNew)
		{
			if (name != null)
				throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);

			SafeWaitHandle = WaitSubsystem.NewEvent (initialState, mode);
			createdNew = true;
		}

		static OpenExistingResult OpenExistingWorker (string name, out EventWaitHandle result)
		{
			throw new PlatformNotSupportedException (SR.PlatformNotSupported_NamedSynchronizationPrimitives);
		}

		internal static bool Set (SafeWaitHandle waitHandle)
		{
			bool release = false;
			try {
				waitHandle.DangerousAddRef (ref release);
				WaitSubsystem.SetEvent (waitHandle.DangerousGetHandle ());
				return true;
			} finally {
				if (release)
					waitHandle.DangerousRelease ();
			}
		}

		SafeWaitHandle ValidateHandle (out bool success)
		{
			// The field value is modifiable via the public <see cref="WaitHandle.SafeWaitHandle"/> property, save it locally
			// to ensure that one instance is used in all places in this method
			SafeWaitHandle waitHandle = SafeWaitHandle;
			if (waitHandle.IsInvalid)
				throw new InvalidOperationException ();

			success = false;
			waitHandle.DangerousAddRef (ref success);
			return waitHandle;
		}
	}
}
