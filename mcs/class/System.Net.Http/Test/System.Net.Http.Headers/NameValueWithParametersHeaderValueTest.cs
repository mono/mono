//
// NameValueWithParametersHeaderValueTest.cs
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
	public class NameValueWithParametersHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new NameValueWithParametersHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new NameValueHeaderValue (" ", null);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new NameValueWithParametersHeaderValue ("ab");
			Assert.AreEqual (value, new NameValueWithParametersHeaderValue ("ab"), "#1");
			Assert.AreEqual (value, new NameValueWithParametersHeaderValue ("AB"), "#2");
			Assert.AreNotEqual (value, new NameValueWithParametersHeaderValue ("AA"), "#3");

			var second = new NameValueWithParametersHeaderValue ("AB");
			second.Parameters.Add (new NameValueHeaderValue ("pv"));

			Assert.AreNotEqual (value, second, "#4");

			value.Parameters.Add (new NameValueHeaderValue ("pv"));
			Assert.AreEqual (value, second, "#5");
		}

		[Test]
		public void Parse ()
		{
			var res = NameValueWithParametersHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Name, "#1");
			Assert.AreEqual ("c", res.ToString (), "#1b");
			Assert.IsNull (res.Value, "#2");

			res = NameValueWithParametersHeaderValue.Parse ("a=2 ; b = 555");
			Assert.AreEqual ("a", res.Name, "#3");
			Assert.AreEqual ("a=2; b=555", res.ToString (), "#3b");
			Assert.AreEqual ("2", res.Value, "#4");
			Assert.AreEqual ("b", res.Parameters.First().Name, "#5");
			Assert.AreEqual (1, res.Parameters.Count, "#6");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				NameValueWithParametersHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				NameValueWithParametersHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				NameValueWithParametersHeaderValue.Parse ("a=1;");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new NameValueWithParametersHeaderValue ("s", "p");
			Assert.AreEqual ("s", value.Name, "#1");
			Assert.AreEqual ("p", value.Value, "#2");

			value = new NameValueWithParametersHeaderValue ("s");
			Assert.AreEqual ("s", value.Name, "#3");
			Assert.IsNull (value.Value, "#4");

			value.Value = "bb";
			Assert.AreEqual ("bb", value.Value, "#5");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var value = new NameValueWithParametersHeaderValue ("s");
			try {
				value.Value = "   ";
				Assert.Fail ("#1");
			} catch (FormatException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			NameValueWithParametersHeaderValue res;
			Assert.IsTrue (NameValueWithParametersHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Name, "#2");
			Assert.IsNull (res.Value, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			NameValueWithParametersHeaderValue res;
			Assert.IsFalse (NameValueWithParametersHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
