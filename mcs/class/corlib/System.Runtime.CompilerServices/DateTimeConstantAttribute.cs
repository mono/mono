//
// System.Runtime.CompilerServices.DateTimeConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[Serializable]
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class DateTimeConstantAttribute : CustomConstantAttribute
	{
		long ticks;
		
		public DateTimeConstantAttribute (long ticks)
		{
			this.ticks = ticks;
		}

		public override object Value {
			get { return ticks; }
		}
	}
}
