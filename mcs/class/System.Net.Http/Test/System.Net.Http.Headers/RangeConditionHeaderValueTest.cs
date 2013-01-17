//
// RangeConditionHeaderValueTest.cs
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
	public class RangeConditionHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new RangeConditionHeaderValue (null as string);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new RangeConditionHeaderValue ("a");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new RangeConditionHeaderValue (null as EntityTagHeaderValue);
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new RangeConditionHeaderValue ("\"abc\"");
			Assert.AreEqual (value, new RangeConditionHeaderValue ("\"abc\""), "#1");
			Assert.AreNotEqual (value, new RangeConditionHeaderValue ("\"AbC\""), "#2");

			value = new RangeConditionHeaderValue (DateTimeOffset.MinValue);
			Assert.AreEqual (value, new RangeConditionHeaderValue (DateTimeOffset.MinValue), "#3");
			Assert.AreNotEqual (value, new RangeConditionHeaderValue (DateTimeOffset.MaxValue), "#4");
			Assert.AreNotEqual (value, new RangeConditionHeaderValue ("\"AbC\""), "#5");
		}

		[Test]
		public void Parse ()
		{
			var res = RangeConditionHeaderValue.Parse ("\"c\"");
			Assert.AreEqual ("\"c\"", res.EntityTag.Tag, "#1");
			Assert.IsFalse (res.EntityTag.IsWeak, "#2");
			Assert.IsNull (res.Date, "#3");
			Assert.AreEqual ("\"c\"", res.ToString (), "#4");

			res = RangeConditionHeaderValue.Parse ("W/\"\"");
			Assert.AreEqual ("\"\"", res.EntityTag.Tag, "#11");
			Assert.IsTrue (res.EntityTag.IsWeak, "#12");
			Assert.IsNull (res.Date, "#13");
			Assert.AreEqual ("W/\"\"", res.ToString (), "#14");


			res = RangeConditionHeaderValue.Parse ("Sun Nov 6 08:49:37 1994");
			Assert.IsNull (res.EntityTag, "#21");
			Assert.AreEqual (new DateTimeOffset (1994, 11, 6, 8, 49, 37, 0, TimeSpan.Zero), res.Date, "#22");
			Assert.AreEqual ("Sun, 06 Nov 1994 08:49:37 GMT", res.ToString (), "#23");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				RangeConditionHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				RangeConditionHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				RangeConditionHeaderValue.Parse ("a b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new RangeConditionHeaderValue ("\"b\"");
			Assert.AreEqual (new EntityTagHeaderValue ("\"b\""), value.EntityTag, "#1");
			Assert.IsNull (value.Date, "#2");

			var dto = new DateTimeOffset (20000, new TimeSpan ());
			value = new RangeConditionHeaderValue (dto);
			Assert.AreEqual (null, value.EntityTag, "#3");
			Assert.AreEqual (dto, value.Date, "#4");
		}

		[Test]
		public void TryParse ()
		{
			RangeConditionHeaderValue res;
			Assert.IsTrue (RangeConditionHeaderValue.TryParse ("\"\"", out res), "#1");
			Assert.AreEqual ("\"\"", res.EntityTag.Tag, "#2");
			Assert.IsNull (res.Date, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			RangeConditionHeaderValue res;
			Assert.IsFalse (RangeConditionHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
