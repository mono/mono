//
// System.Runtime.InteropServices.ComImportAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Class |
			 AttributeTargets.Interface)]
	public sealed class ComImportAttribute : Attribute
	{
		public ComImportAttribute ()
		{
		}
	}
}
