//
// System.Runtime.CompilerServices.IUnknownConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[Serializable]
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class IUnknownConstantAttribute : CustomConstantAttribute
	{
		public IUnknownConstantAttribute ()
		{
		}

		public override object Value {
			get { return null; } // this is correct.
		}
	}
}
