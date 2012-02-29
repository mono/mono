//
// ContentRangeHeaderValueTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
	public class ContentRangeHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new ContentRangeHeaderValue (-1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new ContentRangeHeaderValue (10, 5);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new ContentRangeHeaderValue (0, 1, 0);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new ContentRangeHeaderValue (-1, 15);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new ContentRangeHeaderValue (8);
			Assert.AreEqual (value, new ContentRangeHeaderValue (8), "#1");
			Assert.AreNotEqual (value, new ContentRangeHeaderValue (8, 30), "#2");

			value = new ContentRangeHeaderValue (5, 30, 100);
			Assert.AreEqual (value, new ContentRangeHeaderValue (5, 30, 100), "#3");
			Assert.AreNotEqual (value, new ContentRangeHeaderValue (5, 30, 101), "#4");
			Assert.AreNotEqual (value, new ContentRangeHeaderValue (5, 30), "#5");
			Assert.AreNotEqual (value, new ContentRangeHeaderValue (5, 30, 100) {
				Unit = "g"
			}, "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = ContentRangeHeaderValue.Parse ("bytes  0 - 499/ 1234");
			Assert.AreEqual (0, res.From, "#1");
			Assert.AreEqual (499, res.To, "#2");
			Assert.AreEqual (1234, res.Length, "#3");
			Assert.AreEqual ("bytes 0-499/1234", res.ToString (), "#4");

			res = ContentRangeHeaderValue.Parse ("bytes  */ 8");
			Assert.IsNull (res.From, "#11");
			Assert.IsNull (res.To, "#12");
			Assert.AreEqual (8, res.Length, "#13");
			Assert.AreEqual ("bytes */8", res.ToString (), "#14");

			res = ContentRangeHeaderValue.Parse ("by  */*");
			Assert.IsNull (res.From, "#21");
			Assert.IsNull (res.To, "#22");
			Assert.IsNull (res.Length, "#23");
			Assert.AreEqual ("by */*", res.ToString (), "#24");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				ContentRangeHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				ContentRangeHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				ContentRangeHeaderValue.Parse ("a b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				ContentRangeHeaderValue.Parse ("bytes 10/");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new ContentRangeHeaderValue (4);
			Assert.IsNull (value.From, "#1");
			Assert.IsTrue (value.HasLength, "#2");
			Assert.IsFalse (value.HasRange, "#3");
			Assert.AreEqual (4, value.Length, "#4");
			Assert.IsNull (value.To, "#5");
			Assert.AreEqual ("bytes", value.Unit, "#6");

			value = new ContentRangeHeaderValue (1, 10, 20);
			value.Unit = "mu";
			Assert.AreEqual (1, value.From, "#11");
			Assert.IsTrue (value.HasLength, "#12");
			Assert.IsTrue (value.HasRange, "#13");
			Assert.AreEqual (20, value.Length, "#14");
			Assert.AreEqual (10, value.To, "#15");
			Assert.AreEqual ("mu", value.Unit, "#16");
		}

		[Test]
		public void TryParse ()
		{
			ContentRangeHeaderValue res;
			Assert.IsTrue (ContentRangeHeaderValue.TryParse ("b 1-10/*", out res), "#1");
			Assert.AreEqual (1, res.From, "#2");
			Assert.AreEqual (10, res.To, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			ContentRangeHeaderValue res;
			Assert.IsFalse (ContentRangeHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
