//
// ProductHeaderValueTest.cs
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
	public class ProductHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new ProductHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new ProductHeaderValue ("x", " ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new ProductHeaderValue ("/");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			new ProductHeaderValue ("aa", null);
		}

		[Test]
		public void Equals ()
		{
			var value = new ProductHeaderValue ("ab");
			Assert.AreEqual (value, new ProductHeaderValue ("ab"), "#1");
			Assert.AreEqual (value, new ProductHeaderValue ("AB"), "#2");
			Assert.AreNotEqual (value, new ProductHeaderValue ("AA"), "#3");

			value = new ProductHeaderValue ("ab", "DD");
			Assert.AreEqual (value, new ProductHeaderValue ("Ab", "DD"), "#4");
			Assert.AreNotEqual (value, new ProductHeaderValue ("AB"), "#5");
			Assert.AreEqual (value, new ProductHeaderValue ("Ab", "dd"), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = ProductHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Name, "#1");
			Assert.IsNull (res.Version, "#2");
			Assert.AreEqual ("c", res.ToString (), "#3");

			res = ProductHeaderValue.Parse (" mm / ppp");
			Assert.AreEqual ("mm", res.Name, "#4");
			Assert.AreEqual ("ppp", res.Version, "#5");
			Assert.AreEqual ("mm/ppp", res.ToString (), "#6");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				ProductHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				ProductHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				ProductHeaderValue.Parse ("a;b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				ProductHeaderValue.Parse ("a/");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new ProductHeaderValue ("s", "p");
			Assert.AreEqual ("s", value.Name, "#1");
			Assert.AreEqual ("p", value.Version, "#2");

			value = new ProductHeaderValue ("s");
			Assert.AreEqual ("s", value.Name, "#3");
			Assert.IsNull (value.Version, "#4");
		}

		[Test]
		public void TryParse ()
		{
			ProductHeaderValue res;
			Assert.IsTrue (ProductHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Name, "#2");
			Assert.IsNull (res.Version, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			ProductHeaderValue res;
			Assert.IsFalse (ProductHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
