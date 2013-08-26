//
// MinLengthAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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

#if NET_4_5

using System;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class MinLengthAttribute : ValidationAttribute
	{
		private const string DefaultErrorMessage = "The field {0} must be a string or array type with a minimum length of '{1}'.";
		private const string InvalidLengthErrorMessage = "MinLengthAttribute must have a Length value that is zero or greater.";

		public MinLengthAttribute ()
		{
		}

		public MinLengthAttribute (int length)
		{
			Length = length;
		}

		public int Length { get; private set; }

		public override string FormatErrorMessage (string name)
		{
			return string.Format (ErrorMessageString, name, Length);
		}

		public override bool IsValid (object value)
		{
			if (this.Length < 0)
				throw new InvalidOperationException (InvalidLengthErrorMessage);

			if (value != null) {

				if (value is string) {
					return (value as string).Length >= this.Length;
				}

				if (value is Array) {
					return (value as Array).Length >= this.Length;
				}

				// NOTE: from my tests, MS.NET does not support IEnumerable as value. :(
			}

			return true;
		}
	}
}

#endif
