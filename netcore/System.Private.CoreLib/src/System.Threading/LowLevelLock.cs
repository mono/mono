// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Threading
{
	// FIXME: LowLevelLock should be uninterruptible
	internal class LowLevelLock : IDisposable
	{
		public void Dispose () {}
		public bool TryAcquire() => Monitor.TryEnter (this);
		public void Acquire() => Monitor.Enter (this);
		public void Release() => Monitor.Exit (this);

		[Conditional("DEBUG")]
		public void VerifyIsLocked() {}
	}
}
