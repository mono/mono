//
// System.Runtime.InteropServices.AutomationProxyAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Interface)]
	public sealed class AutomationProxyAttribute : Attribute
	{
		bool val;
		
		public AutomationProxyAttribute (bool val)
		{
			this.val = val;
		}

		public bool Value {
			get { return val; }
		}
	}
}
