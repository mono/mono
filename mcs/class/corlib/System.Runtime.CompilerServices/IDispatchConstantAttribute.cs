//
// System.Runtime.CompilerServices.IDispatchConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[Serializable]
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter)]
	public class IDispatchConstantAttribute : CustomConstantAttribute
	{
		public IDispatchConstantAttribute ()
		{
		}

		public override object Value {
			get { return null; } // this is correct.
		}
	}
}
