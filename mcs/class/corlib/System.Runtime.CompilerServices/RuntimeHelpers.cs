// System.Runtime.CompilerServices.RuntimeHelpers
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc. 2001

namespace System.Runtime.CompilerServices
{
	public sealed class RuntimeHelpers
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void InitializeArray (Array array, RuntimeFieldHandle fldHandle);

	}
}
