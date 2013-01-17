//
// RetryConditionHeaderValueTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Net.Http.Headers;

namespace MonoTests.System.Net.Http.Headers
{
	[TestFixture]
	public class RetryConditionHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new RetryConditionHeaderValue (TimeSpan.MaxValue);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new RetryConditionHeaderValue (new DateTimeOffset (DateTime.Today));
			Assert.AreEqual (value, new RetryConditionHeaderValue (new DateTimeOffset (DateTime.Today)), "#1");
			Assert.AreNotEqual (value, new RetryConditionHeaderValue (new DateTimeOffset ()), "#2");

			value = new RetryConditionHeaderValue (new TimeSpan (300));
			Assert.AreEqual (value, new RetryConditionHeaderValue (new TimeSpan (300)), "#4");
			Assert.AreNotEqual (value, new RetryConditionHeaderValue (new TimeSpan (44)), "#5");
		}

		[Test]
		public void Parse ()
		{
			var res = RetryConditionHeaderValue.Parse ("144");
			Assert.IsNull (res.Date, "#1");
			Assert.AreEqual (new TimeSpan (0, 0, 144), res.Delta, "#2");
			Assert.AreEqual ("144", res.ToString (), "#3");

			res = RetryConditionHeaderValue.Parse ("Fri, 31 Dec 1999 23:59:59 GMT");
			Assert.AreEqual (new DateTimeOffset (1999, 12, 31, 23, 59, 59, 0, TimeSpan.Zero), res.Date, "#11");
			Assert.IsNull (res.Delta, "#12");
			Assert.AreEqual ("Fri, 31 Dec 1999 23:59:59 GMT", res.ToString (), "#13");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				RetryConditionHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				RetryConditionHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				RetryConditionHeaderValue.Parse ("a");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new RetryConditionHeaderValue (new TimeSpan (5000));
			Assert.IsNull (value.Date, "#1");
			Assert.AreEqual (new TimeSpan (5000), value.Delta, "#2");

			value = new RetryConditionHeaderValue (new DateTimeOffset (DateTime.Today));
			Assert.AreEqual (new DateTimeOffset (DateTime.Today), value.Date, "#3");
			Assert.IsNull (value.Delta, "#4");
		}

		[Test]
		public void TryParse ()
		{
			RetryConditionHeaderValue res;
			Assert.IsTrue (RetryConditionHeaderValue.TryParse ("124", out res), "#1");
			Assert.IsNull (res.Date, "#2");
			Assert.AreEqual (new TimeSpan (0, 2, 4), res.Delta, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			RetryConditionHeaderValue res;
			Assert.IsFalse (RetryConditionHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
