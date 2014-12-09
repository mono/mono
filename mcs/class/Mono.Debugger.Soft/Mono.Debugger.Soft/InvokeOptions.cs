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
		SingleThreaded = 2,
		/*
		 * Return the changed receiver when invoking
		 * a valuetype method.
		 */
		ReturnOutThis = 4,
		/*
		 * Return the values of out arguments
		 */
		ReturnOutArgs = 8,
		/*
		 * Do a virtual invoke
		 * Since protocol version 2.37
		 */
		Virtual = 16
	}
}
