//
// RangeHeaderValueTest.cs
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
using System.Linq;

namespace MonoTests.System.Net.Http.Headers
{
	[TestFixture]
	public class RangeHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new RangeHeaderValue (null, null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new RangeHeaderValue (long.MinValue, null);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new RangeHeaderValue (10, 1);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new RangeHeaderValue (4, null);
			Assert.AreEqual (value, new RangeHeaderValue (4, null), "#1");
			Assert.AreNotEqual (value, new RangeHeaderValue (4, 5), "#2");
			Assert.AreNotEqual (value, new RangeHeaderValue (), "#3");

			value = new RangeHeaderValue (2, 4);
			Assert.AreEqual (value, new RangeHeaderValue (2, 4), "#4");
			Assert.AreNotEqual (value, new RangeHeaderValue (2, null), "#5");
			Assert.AreNotEqual (value, new RangeHeaderValue (2, 3), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = RangeHeaderValue.Parse ("bytes=2-40");
			Assert.AreEqual ("bytes", res.Unit, "#1");
			Assert.AreEqual (2, res.Ranges.First ().From, "#2");
			Assert.AreEqual (40, res.Ranges.First ().To, "#3");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				RangeHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				RangeHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				RangeHeaderValue.Parse ("5-6");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				RangeHeaderValue.Parse ("bytes=");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new RangeHeaderValue (3, 9);
			Assert.AreEqual ("bytes", value.Unit, "#1");
			Assert.AreEqual (3, value.Ranges.First ().From, "#2");
			Assert.AreEqual (9, value.Ranges.First ().To, "#3");

			value = new RangeHeaderValue ();
			Assert.AreEqual ("bytes", value.Unit, "#4");
			Assert.AreEqual (0, value.Ranges.Count, "#5");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var value = new RangeHeaderValue ();
			try {
				value.Unit = "";
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			RangeHeaderValue res;
			Assert.IsTrue (RangeHeaderValue.TryParse ("bytes=4-33", out res), "#1");
			Assert.AreEqual ("bytes", res.Unit, "#2");
			Assert.AreEqual (4, res.Ranges.First ().From, "#3");
			Assert.AreEqual (33, res.Ranges.First ().To, "#4");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			RangeHeaderValue res;
			Assert.IsFalse (RangeHeaderValue.TryParse ("bytes=4,33", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
