//
// System.Runtime.InteropServices.ComUnregisterFunctionAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method, Inherited=false)]
	public sealed class ComUnregisterFunctionAttribute : Attribute
	{
		public ComUnregisterFunctionAttribute ()
		{
		}
	}
}
