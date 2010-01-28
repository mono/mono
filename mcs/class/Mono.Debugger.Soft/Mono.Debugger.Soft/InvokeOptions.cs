using System;
using System.Collections.Generic;

namespace Mono.Debugger.Soft
{
	[Flags]
	public enum InvokeOptions {
		None = 0,
		/*
		 * Disable breakpoints on the thread doing the invoke
		 */
		DisableBreakpoints = 1,
		/*
		 * Only resume the target thread during the invoke
		 */
		SingleThreaded = 2
	}
}
