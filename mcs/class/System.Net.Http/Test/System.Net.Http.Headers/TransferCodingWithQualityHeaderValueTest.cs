//
// TransferCodingWithQualityHeaderValueTest.cs
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
	public class TransferCodingWithQualityHeaderValueTest
	{
		[Test]
		public void Ctor ()
		{
			var v = new TransferCodingWithQualityHeaderValue ("value");
			Assert.AreEqual ("value", v.Value, "#1");
			Assert.IsNull (v.Quality);
		}

		[Test]
		public void Equals ()
		{
			var tfhv = new TransferCodingWithQualityHeaderValue ("abc");
			Assert.AreEqual (tfhv, new TransferCodingWithQualityHeaderValue ("abc"), "#1");
			Assert.AreEqual (tfhv, new TransferCodingWithQualityHeaderValue ("AbC"), "#2");
			Assert.AreNotEqual (tfhv, new TransferCodingWithQualityHeaderValue ("ab"), "#3");
			Assert.AreNotEqual (tfhv, new TransferCodingWithQualityHeaderValue ("ab", 1), "#3");

			tfhv = new TransferCodingWithQualityHeaderValue ("abc", 0.3);
			Assert.AreEqual (tfhv, new TransferCodingWithQualityHeaderValue ("abc", 0.3), "#4");
			Assert.AreEqual (tfhv, new TransferCodingWithQualityHeaderValue ("AbC", 0.3), "#5");
			Assert.AreNotEqual (tfhv, new TransferCodingWithQualityHeaderValue ("abc"), "#6");

			var custom_param = new TransferCodingHeaderValue ("abc");
			custom_param.Parameters.Add (new NameValueHeaderValue ("q", "0.3"));
			Assert.AreEqual (tfhv, custom_param, "#7");
		}

		[Test]
		public void Parse ()
		{
			var res = TransferCodingWithQualityHeaderValue.Parse ("1.1");
			Assert.AreEqual (0, res.Parameters.Count, "#1");
			Assert.IsNull (res.Quality, "#1b");
			Assert.AreEqual ("1.1", res.Value, "#1c");
			Assert.AreEqual ("1.1", res.ToString (), "#1d");

			res = TransferCodingWithQualityHeaderValue.Parse ("a ;  b ");
			Assert.AreEqual (1, res.Parameters.Count, "#2");
			Assert.IsNull (res.Quality, "#2b");
			Assert.AreEqual ("a", res.Value, "#2c");
			Assert.AreEqual ("a; b", res.ToString (), "#2d");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				TransferCodingWithQualityHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				TransferCodingWithQualityHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var v = new TransferCodingWithQualityHeaderValue ("value", 0.412);
			Assert.AreEqual ("value", v.Value, "#1");
			Assert.AreEqual (0.412, v.Quality, "#2");
			Assert.AreEqual ("0.412", v.Parameters.First ().Value, "#3");

			v.Parameters.Add (new NameValueHeaderValue ("q", "0.2"));
			Assert.AreEqual (0.412, v.Quality, "#4");

			v = new TransferCodingWithQualityHeaderValue ("value");
			v.Parameters.Add (new NameValueHeaderValue ("q", "0.112"));
			Assert.AreEqual (0.112, v.Quality, "#5");

			v = new TransferCodingWithQualityHeaderValue ("value");
			v.Parameters.Add (new NameValueHeaderValue ("q", "test"));
			Assert.IsNull (v.Quality, "#6");
		}

		[Test]
		public void Properties_Invalid ()
		{
			try {
				new TransferCodingWithQualityHeaderValue ("value", 4);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			var v = new TransferCodingWithQualityHeaderValue ("value");
			try {
				v.Quality = -1;
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
	}
}