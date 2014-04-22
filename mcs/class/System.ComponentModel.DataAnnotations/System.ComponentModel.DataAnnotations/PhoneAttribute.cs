//
// PhoneAttribute.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//      Pablo Ruiz García <pablo.ruiz@gmail.com>
//
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
// Copyright (C) 2013 Pablo Ruiz García
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
using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class PhoneAttribute : DataTypeAttribute
	{
		private const string DefaultErrorMessage = "The {0} field is not a valid phone number.";
		private const string _regexStr = @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$";
		private static Regex _regex = new Regex (_regexStr, RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public PhoneAttribute ()
			: base(DataType.PhoneNumber)
		{
			// XXX: There is no .ctor accepting Func<string> on DataTypeAttribute.. :?
			base.ErrorMessage = DefaultErrorMessage;
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			if (value is string)
			{
				var str = value as string;
				return !string.IsNullOrEmpty(str) ? _regex.IsMatch(str) : false;
			}

			return false;
		}
	}
}

#endif
