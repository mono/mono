//
// System.Runtime.InteropServices.InAttribute.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Parameter)]
	public sealed class InAttribute : Attribute {
		public InAttribute () {
		}
	}
}
