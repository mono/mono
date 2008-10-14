//
// RangeAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
using System.ComponentModel;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false)]
	public class RangeAttribute : ValidationAttribute
	{
		public RangeAttribute (double minimum, double maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public RangeAttribute (int minimum, int maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public RangeAttribute (Type type, string minimum, string maximum)
		{
			OperandType = type;
			Minimum = minimum;
			Maximum = maximum;
		}

		public object Maximum { get; private set; }
		public object Minimum { get; private set; }
		public Type OperandType { get; private set; }

		[MonoTODO]
		public override string FormatErrorMessage (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsValid (object value)
		{
			throw new NotImplementedException ();
		}
	}
}
