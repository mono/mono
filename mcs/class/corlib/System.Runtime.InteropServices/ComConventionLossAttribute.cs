//
// System.Runtime.InteropServices.ComConventionLossAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.All)]
	public sealed class ComConventionLossAttribute : Attribute
	{
		public ComConventionLossAttribute ()
		{
		}
	}
	
}
