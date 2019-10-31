// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading
{
	partial class WaitHandle
	{
		private static int WaitOneCore(IntPtr handle, int millisecondsTimeout) =>
			WaitSubsystem.Wait(handle, millisecondsTimeout, true);

		internal static int WaitMultipleIgnoringSyncContext(Span<IntPtr> handles, bool waitAll, int millisecondsTimeout) =>
			WaitSubsystem.Wait(handles, waitAll, millisecondsTimeout);

		private static int SignalAndWaitCore(IntPtr handleToSignal, IntPtr handleToWaitOn, int millisecondsTimeout) =>
			WaitSubsystem.SignalAndWait(handleToSignal, handleToWaitOn, millisecondsTimeout);

		// FIXME: Move to shared
		internal static int WaitAny (ReadOnlySpan<WaitHandle> waitHandles, int millisecondsTimeout) =>
			WaitMultiple (waitHandles, false, millisecondsTimeout);

		internal static void ThrowInvalidHandleException ()
		{
			var ex = new InvalidOperationException (SR.InvalidOperation_InvalidHandle);
			ex.HResult = HResults.E_HANDLE;
			throw ex;
		}
	}
}
