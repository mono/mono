//
// CreditCardAttribute.cs
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
using System.Linq;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsageAttribute (AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class CreditCardAttribute : DataTypeAttribute
	{
		private const string DefaultErrorMessage = "The {0} field is not a valid credit card number.";

		public CreditCardAttribute ()
			: base(DataType.CreditCard)
		{
			// XXX: There is no .ctor accepting Func<string> on DataTypeAttribute.. :?
			base.ErrorMessage = DefaultErrorMessage;
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			if (string.IsNullOrEmpty(value as string))
				return false;

			// Remove any invalid characters..
			var creditCardNumber = (value as string).Replace("-", "").Replace(" ", "");

			if (creditCardNumber.Any (x => !Char.IsDigit (x)))
				return false;

			// Performan a Luhn-based check against credit card number.
			//
			// See: http://en.wikipedia.org/wiki/Luhn_algorithm
			// See: http://rosettacode.org/wiki/Luhn_test_of_credit_card_numbers
			// See: http://www.codeproject.com/Tips/515367/Validate-credit-card-number-with-Mod-10-algorithm

			int sumOfDigits = creditCardNumber.Where((e) => e >= '0' && e <= '9')
				.Reverse()
				.Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2))
				.Sum((e) => e / 10 + e % 10);

			return sumOfDigits % 10 == 0;
		}
	}
}

#endif
