//
// AuthenticationHeaderValueTest.cs
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
	public class AuthenticationHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new AuthenticationHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new AuthenticationHeaderValue (" ", null);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new AuthenticationHeaderValue ("ab");
			Assert.AreEqual (value, new AuthenticationHeaderValue ("ab"), "#1");
			Assert.AreEqual (value, new AuthenticationHeaderValue ("AB"), "#2");
			Assert.AreNotEqual (value, new AuthenticationHeaderValue ("AA"), "#3");

			value = new AuthenticationHeaderValue ("ab", "DD");
			Assert.AreEqual (value, new AuthenticationHeaderValue ("Ab", "DD"), "#4");
			Assert.AreNotEqual (value, new AuthenticationHeaderValue ("AB"), "#5");
			Assert.AreNotEqual (value, new AuthenticationHeaderValue ("Ab", "dd"), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = AuthenticationHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Scheme, "#1");
			Assert.IsNull (res.Parameter, "#2");
			Assert.AreEqual ("c", res.ToString (), "#3");

			res = AuthenticationHeaderValue.Parse ("ss   p=3 , q = \"vvv\"");

			Assert.AreEqual ("ss", res.Scheme, "#11");
			Assert.AreEqual ("p=3 , q = \"vvv\"", res.Parameter, "#12");
			Assert.AreEqual ("ss p=3 , q = \"vvv\"", res.ToString (), "#13");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				AuthenticationHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				AuthenticationHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				AuthenticationHeaderValue.Parse ("a;b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new AuthenticationHeaderValue ("s", "p");
			Assert.AreEqual ("s", value.Scheme, "#1");
			Assert.AreEqual ("p", value.Parameter, "#2");

			value = new AuthenticationHeaderValue ("s");
			Assert.AreEqual ("s", value.Scheme, "#3");
			Assert.IsNull (value.Parameter, "#4");
		}

		[Test]
		public void TryParse ()
		{
			AuthenticationHeaderValue res;
			Assert.IsTrue (AuthenticationHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Scheme, "#2");
			Assert.IsNull (res.Parameter, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			AuthenticationHeaderValue res;
			Assert.IsFalse (AuthenticationHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
