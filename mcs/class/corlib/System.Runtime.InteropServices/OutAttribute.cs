//
// System.Runtime.InteropServices.OutAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Parameter, Inherited=false)]
	public sealed class OutAttribute : Attribute {

		public OutAttribute ()
		{
		}
	}
}
