
using System;

namespace Mono.Compiler
{
	public struct InstalledRuntimeCode
	{
#pragma warning disable 169
		IntPtr handle; /* This is the actual pointer to the domain-allocated InstalledRuntimeCode */
#pragma warning restore 169
	}
}
