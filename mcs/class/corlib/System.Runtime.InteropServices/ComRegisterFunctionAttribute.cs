//
// System.Runtime.InteropServices.ComRegisterFunctionAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method, Inherited=false)]
	public sealed class ComRegisterFunctionAttribute : Attribute
	{
		public ComRegisterFunctionAttribute ()
		{
		}
	}
}
