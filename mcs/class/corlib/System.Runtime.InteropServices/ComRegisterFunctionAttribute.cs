//
// System.Runtime.InteropServices.ComRegisterFunctionAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class ComRegisterFunctionAttribute : Attribute
	{
		public ComRegisterFunctionAttribute ()
		{
		}
	}
}
