using System;
using System.Collections.Generic;

namespace Mono.Debugger
{
	[Flags]
	public enum InvokeOptions {
		None = 0,
		/*
		 * Disable breakpoints on the thread doing the invoke
		 */
		DisableBreakpoints = 1
	}
}
