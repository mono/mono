//
// NameValueHeaderValueTest.cs
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
	public class NameValueHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new NameValueHeaderValue (null);
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
			var value = new NameValueHeaderValue ("ab");
			Assert.AreEqual (value, new NameValueHeaderValue ("ab"), "#1");
			Assert.AreEqual (value, new NameValueHeaderValue ("AB"), "#2");
			Assert.AreNotEqual (value, new NameValueHeaderValue ("AA"), "#3");
			Assert.AreEqual (value, new NameValueHeaderValue ("AB", ""), "#3-1");

			value = new NameValueHeaderValue ("ab", "DD");
			Assert.AreEqual (value, new NameValueHeaderValue ("Ab", "DD"), "#4");
			Assert.AreNotEqual (value, new NameValueHeaderValue ("AB"), "#5");
			Assert.AreEqual (value, new NameValueHeaderValue ("Ab", "dd"), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = NameValueHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Name, "#1");
			Assert.IsNull (res.Value, "#1a");

			res = NameValueHeaderValue.Parse ("c = 1");
			Assert.AreEqual ("c", res.Name, "#2");
			Assert.AreEqual ("1", res.Value, "#2a");
			Assert.AreEqual ("c=1", res.ToString (), "#2b");

			res = NameValueHeaderValue.Parse ("c = \"1\"");
			Assert.AreEqual ("c", res.Name, "#3");
			Assert.AreEqual ("\"1\"", res.Value, "#3a");
			Assert.AreEqual ("c=\"1\"", res.ToString (), "#3b");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				NameValueHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				NameValueHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				NameValueHeaderValue.Parse ("a;b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				NameValueHeaderValue.Parse ("c = 1;");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new NameValueHeaderValue ("s", "p");
			Assert.AreEqual ("s", value.Name, "#1");
			Assert.AreEqual ("p", value.Value, "#2");

			value = new NameValueHeaderValue ("s");
			Assert.AreEqual ("s", value.Name, "#3");
			Assert.IsNull (value.Value, "#4");

			value.Value = "bb";
			Assert.AreEqual ("bb", value.Value, "#5");

			value.Value = null;
		}

		[Test]
		public void Properties_Invalid ()
		{
			var value = new NameValueHeaderValue ("s");
			try {
				value.Value = "   ";
				Assert.Fail ("#1");
			} catch (FormatException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			NameValueHeaderValue res;
			Assert.IsTrue (NameValueHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Name, "#2");
			Assert.IsNull (res.Value, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			NameValueHeaderValue res;
			Assert.IsFalse (NameValueHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
