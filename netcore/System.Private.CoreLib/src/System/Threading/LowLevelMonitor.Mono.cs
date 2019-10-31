// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Threading
{
	//
	// In mono, the normal Monitor class is implemented in native code, so this class
	// can just wrap it.

    internal sealed partial class LowLevelMonitor : IDisposable
    {
		object monitor;

        public LowLevelMonitor()
        {
			monitor = new object ();
		}

        private void DisposeCore()
        {
			monitor = null;
        }

        private void AcquireCore()
        {
			Monitor.Enter (monitor);
        }

        private void ReleaseCore()
        {
			Monitor.Exit (monitor);
        }

        private void WaitCore()
        {
			Monitor.Wait (monitor);
        }

        private bool WaitCore(int timeoutMilliseconds)
        {
            Debug.Assert(timeoutMilliseconds >= -1);

            if (timeoutMilliseconds < 0) {
                WaitCore();
                return true;
            }

			return Monitor.Wait (monitor, timeoutMilliseconds);
        }

        private void Signal_ReleaseCore()
        {
			Monitor.Pulse (monitor);
			Monitor.Exit (monitor);
        }
    }
}
