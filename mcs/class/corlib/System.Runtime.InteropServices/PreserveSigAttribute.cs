// System.Runtime.InteropServices.PreserveSigAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class PreserveSigAttribute : Attribute
	{
		public PreserveSigAttribute ()
		{
		}
	}
}
