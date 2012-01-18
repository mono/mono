//
// ProductInfoHeaderValueTest.cs
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
	public class ProductInfoHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new ProductInfoHeaderValue (null as string);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new ProductInfoHeaderValue (" ", null);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new ProductInfoHeaderValue (null as ProductHeaderValue);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new ProductInfoHeaderValue ("(ab)");
			Assert.AreEqual (value, new ProductInfoHeaderValue ("(ab)"), "#1");
			Assert.AreNotEqual (value, new ProductInfoHeaderValue ("(AB)"), "#2");
			Assert.AreNotEqual (value, new ProductInfoHeaderValue ("(AA)"), "#3");

			value = new ProductInfoHeaderValue ("ab", "DD");
			Assert.AreEqual (value, new ProductInfoHeaderValue ("Ab", "DD"), "#4");
			Assert.AreNotEqual (value, new ProductInfoHeaderValue ("(AB)"), "#5");
			Assert.AreEqual (value, new ProductInfoHeaderValue ("Ab", "dd"), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = ProductInfoHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Product.Name, "#1");
			Assert.IsNull (res.Product.Version, "#2");
			Assert.IsNull (res.Comment, "#3");
			Assert.AreEqual ("c", res.ToString (), "#4");

			res = ProductInfoHeaderValue.Parse (" b / 6");
			Assert.AreEqual ("b", res.Product.Name, "#11");
			Assert.AreEqual ("6", res.Product.Version, "#12");
			Assert.IsNull (res.Comment, "#13");
			Assert.AreEqual ("b/6", res.ToString (), "#14");

			res = ProductInfoHeaderValue.Parse (" (  cccc )   ");
			Assert.IsNull (res.Product, "#21");
			Assert.AreEqual ("(  cccc )", res.Comment, "#22");
			Assert.AreEqual ("(  cccc )", res.ToString (), "#23");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				ProductInfoHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				ProductInfoHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				ProductInfoHeaderValue.Parse ("a;b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new ProductInfoHeaderValue ("s", "p");
			Assert.AreEqual ("s", value.Product.Name, "#1");
			Assert.AreEqual ("p", value.Product.Version, "#2");
			Assert.IsNull (value.Comment, "#3");

			value = new ProductInfoHeaderValue ("(s)");
			Assert.AreEqual ("(s)", value.Comment, "#4");
			Assert.IsNull (value.Product, "#5");
		}

		[Test]
		public void TryParse ()
		{
			ProductInfoHeaderValue res;
			Assert.IsTrue (ProductInfoHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Product.Name, "#2");
			Assert.IsNull (res.Comment, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			ProductInfoHeaderValue res;
			Assert.IsFalse (ProductInfoHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
