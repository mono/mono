//
// StringLengthAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Marek Habersack <grendel@twistedcode.net>
//
// Copyright (C) 2008-2010 Novell Inc. http://novell.com
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
	public class StringLengthAttribute : ValidationAttribute
	{
		public int MaximumLength { get; private set; }
#if NET_4_0
		public int MinimumLength { get; set; }
#endif
		public StringLengthAttribute (int maximumLength)
			: base (GetDefaultErrorMessage)
		{
#if !NET_4_0
			if (maximumLength < 0)
				throw new ArgumentOutOfRangeException ("maximumLength", String.Format ("Actual value was {0}", maximumLength));
#endif
			MaximumLength = maximumLength;
		}

		static string GetDefaultErrorMessage ()
		{
			return "The field {0} must be a string with a maximum length of {1}.";
		}

		public override string FormatErrorMessage (string name)
		{
			return String.Format (ErrorMessageString, name, MaximumLength);
		}

		public override bool IsValid (object value)
		{
			if (value == null)
				return true;

			string str = (string)value;
			int max = MaximumLength;
#if NET_4_0
			int min = MinimumLength;

			// LAMESPEC: documented to throw ArgumentOutOfRangeException
			if (max < 0)
				throw new InvalidOperationException ("The maximum length must be a nonnegative integer.");

			if (min > max)
				throw new InvalidOperationException (
					String.Format ("The maximum value '{0}' must be greater than or equal to the minimum value '{1}'.",
						       max, min)
				);

			int len = str.Length;
			return len <= max && len >= min;
#else			
			return str.Length <= max;
#endif
		}
	}
}
