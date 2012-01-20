//
// StringWithQualityHeaderValueTest.cs
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
	public class StringWithQualityHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new StringWithQualityHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new StringWithQualityHeaderValue (" ", 1);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new StringWithQualityHeaderValue ("s", 1.1);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			var a = new StringWithQualityHeaderValue ("s", 0.123456);
			Assert.AreEqual ("s; q=0.123", a.ToString ());
		}

		[Test]
		public void Equals ()
		{
			var value = new StringWithQualityHeaderValue ("ab");
			Assert.AreEqual (value, new StringWithQualityHeaderValue ("ab"), "#1");
			Assert.AreEqual (value, new StringWithQualityHeaderValue ("AB"), "#2");
			Assert.AreNotEqual (value, new StringWithQualityHeaderValue ("AA"), "#3");
			Assert.AreEqual ("ab", value.ToString (), "#33");

			value = new StringWithQualityHeaderValue ("ab", 1);
			Assert.AreEqual (value, new StringWithQualityHeaderValue ("Ab", 1), "#4");
			Assert.AreNotEqual (value, new StringWithQualityHeaderValue ("AB", 0), "#5");
			Assert.AreNotEqual (value, new StringWithQualityHeaderValue ("AA", 1), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = StringWithQualityHeaderValue.Parse ("c");
			Assert.AreEqual ("c", res.Value, "#1");
			Assert.IsNull (res.Quality, "#2");
			Assert.AreEqual ("c", res.ToString (), "#3");

			res = StringWithQualityHeaderValue.Parse (" * ;  q   = 0.2");
			Assert.AreEqual ("*", res.Value, "#11");
			Assert.AreEqual (0.2, res.Quality, "#12");
			Assert.AreEqual ("*; q=0.2", res.ToString (), "#13");

			res = StringWithQualityHeaderValue.Parse ("aa ;Q=0");
			Assert.AreEqual ("aa", res.Value, "#21");
			Assert.AreEqual (0, res.Quality, "#22");
			Assert.AreEqual ("aa; q=0.0", res.ToString (), "#23");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				StringWithQualityHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("a,b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("b;");
				Assert.Fail ("#4");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("b;q=");
				Assert.Fail ("#5");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("b;q=2");
				Assert.Fail ("#6");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("b;q=-0.2");
				Assert.Fail ("#7");
			} catch (FormatException) {
			}

			try {
				StringWithQualityHeaderValue.Parse ("b;X=2");
				Assert.Fail ("#8");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new StringWithQualityHeaderValue ("s", 1);
			Assert.AreEqual ("s", value.Value, "#1");
			Assert.AreEqual (1, value.Quality, "#2");

			value = new StringWithQualityHeaderValue ("ss");
			Assert.AreEqual ("ss", value.Value, "#3");
			Assert.IsNull (value.Quality, "#4");
		}

		[Test]
		public void TryParse ()
		{
			StringWithQualityHeaderValue res;
			Assert.IsTrue (StringWithQualityHeaderValue.TryParse ("a", out res), "#1");
			Assert.AreEqual ("a", res.Value, "#2");
			Assert.IsNull (res.Quality, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			StringWithQualityHeaderValue res;
			Assert.IsFalse (StringWithQualityHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
