//
// System.Runtime.InteropServices.LCIDConversionAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Method)]
	public sealed class LCIDConversionAttribute : Attribute
	{
		int id;
		
		public LCIDConversionAttribute (int lcid)
		{
			id = lcid;
		}

		public int Value {
			get { return id; }
		}
	}
}
