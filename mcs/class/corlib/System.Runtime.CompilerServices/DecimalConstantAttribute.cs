//
// System.Runtime.CompilerServices.DecimalConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

using System;

namespace System.Runtime.CompilerServices {

	[Serializable] [CLSCompliant (false)]
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter)]
	public sealed class DecimalConstantAttribute : Attribute
	{
		byte scale;
		bool sign;
		int hi;
		int mid;
		int low;
		
		public DecimalConstantAttribute (byte scale, byte sign, uint hi, uint mid, uint low)
		{
			this.scale = scale;
			this.sign  = Convert.ToBoolean (sign);
			this.hi    = (int) hi;
			this.mid   = (int) mid;
			this.low   = (int) low;
		}

		public Decimal Value {
			get { return new Decimal (low, mid, hi, sign, scale); }
		}
	}
}
