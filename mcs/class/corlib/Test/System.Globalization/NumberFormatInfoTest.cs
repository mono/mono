//
// NumberFormatInfoTest.cs
//
// Authors:
//     Marek Safar (marek.safar@gmail.com)
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


using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class NumberFormatInfoTest
	{
		[Test]
		public void CurrencyDecimalDigits ()
		{
			CultureInfo c;
			
			c = CultureInfo.GetCultureInfo ("id-ID");
			Assert.AreEqual (0, c.NumberFormat.CurrencyDecimalDigits, "#1");

			c = CultureInfo.GetCultureInfo ("is-IS");
			Assert.AreEqual (0, c.NumberFormat.CurrencyDecimalDigits, "#2");

			c = CultureInfo.InvariantCulture;
			Assert.AreEqual (2, c.NumberFormat.CurrencyDecimalDigits, "#3");
		}

		[Test]
		public void AllCulturesCanParseNegativeNumber ()
		{
			foreach (var c in CultureInfo.GetCultures (CultureTypes.AllCultures))
			{
				int.Parse ("-1", c);
			}
		}
	}
}
