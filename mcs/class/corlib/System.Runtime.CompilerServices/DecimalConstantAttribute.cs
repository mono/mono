//
// System.Runtime.CompilerServices.DecimalConstantAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian Inc.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace System.Runtime.CompilerServices {

	[Serializable] 
#if !NET_2_0
	[CLSCompliant (false)]
#endif
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter,
			 Inherited=false)]
	public sealed class DecimalConstantAttribute : Attribute
	{
		byte scale;
		bool sign;
		int hi;
		int mid;
		int low;

		[CLSCompliant (false)]
		public DecimalConstantAttribute (byte scale, byte sign, uint hi, uint mid, uint low)
		{
			this.scale = scale;
			this.sign  = Convert.ToBoolean (sign);
			this.hi    = (int) hi;
			this.mid   = (int) mid;
			this.low   = (int) low;
		}

#if NET_2_0
		public DecimalConstantAttribute (byte scale, byte sign, int hi, int mid, int low)
		{
			this.scale = scale;
			this.sign  = Convert.ToBoolean (sign);
			this.hi    = hi;
			this.mid   = mid;
			this.low   = low;
		}
#endif

		public Decimal Value {
			get { return new Decimal (low, mid, hi, sign, scale); }
		}
	}
}
