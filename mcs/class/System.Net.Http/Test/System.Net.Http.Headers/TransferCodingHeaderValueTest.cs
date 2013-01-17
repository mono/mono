//
// TransferCodingHeaderValueTest.cs
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
	public class TransferCodingHeaderValueTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				var tfhv = new TransferCodingHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				var tfhv = new TransferCodingHeaderValue ("my value");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var tfhv = new TransferCodingHeaderValue ("abc");
			Assert.AreEqual (tfhv, new TransferCodingHeaderValue ("abc"), "#1");
			Assert.AreEqual (tfhv, new TransferCodingHeaderValue ("AbC"), "#2");

			tfhv.Parameters.Add (new NameValueHeaderValue ("p", "v"));

			Assert.AreNotEqual (tfhv, new TransferCodingHeaderValue ("abc"), "#3");

			var tfhv2 = new TransferCodingHeaderValue ("abc");
			Assert.AreNotEqual (tfhv, tfhv2, "#4");

			tfhv2.Parameters.Add (new NameValueHeaderValue ("p", "v"));

			Assert.AreEqual (tfhv, tfhv2, "#5");
		}

		[Test]
		public void Parse ()
		{
			var res = TransferCodingHeaderValue.Parse ("content");
			Assert.AreEqual ("content", res.Value, "#1");
			Assert.AreEqual ("content", res.ToString (), "#1a");

			res = TransferCodingHeaderValue.Parse ("  a ;b;c  ");
			Assert.AreEqual ("a", res.Value, "#2");
			Assert.AreEqual ("a; b; c", res.ToString (), "#2a");
			Assert.AreEqual ("b", res.Parameters.First ().ToString (), "#2b");

			res = TransferCodingHeaderValue.Parse ("  a ; v = m ");
			Assert.AreEqual ("a", res.Value, "#3");
			Assert.AreEqual ("a; v=m", res.ToString (), "#3a");
			Assert.AreEqual ("m", res.Parameters.First ().Value, "#3b");

			res = TransferCodingHeaderValue.Parse ("\ta; v = \"mmm\" ");
			Assert.AreEqual ("a", res.Value, "#4");
			Assert.AreEqual ("a; v=\"mmm\"", res.ToString (), "#4a");
			Assert.AreEqual ("\"mmm\"", res.Parameters.First ().Value, "#4b");

		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				TransferCodingHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				TransferCodingHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				TransferCodingHeaderValue.Parse ("a b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				TransferCodingHeaderValue.Parse ("a;");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}

			try {
				TransferCodingHeaderValue.Parse ("u;v=\"\"\"\"");
				Assert.Fail ("#5");
			} catch (FormatException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			TransferCodingHeaderValue res;
			Assert.IsTrue (TransferCodingHeaderValue.TryParse ("content", out res), "#1");
			Assert.AreEqual ("content", res.Value, "#2");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			TransferCodingHeaderValue res;
			Assert.IsFalse (TransferCodingHeaderValue.TryParse ("a b", out res), "#1");
			Assert.IsNull (res, "#2");
		}

		[Test]
		public void Value ()
		{
			var tfhv = new TransferCodingHeaderValue ("value");
			Assert.AreEqual ("value", tfhv.Value, "#1");
			Assert.IsNotNull (tfhv.Parameters, "#2");
		}
	}
}
