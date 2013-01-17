//
// MediaTypeWithQualityHeaderValueTest.cs
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
	public class MediaTypeWithQualityHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new MediaTypeWithQualityHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new MediaTypeWithQualityHeaderValue ("audio/", 0.1);
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				new MediaTypeWithQualityHeaderValue ("audio/*", 2);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new MediaTypeWithQualityHeaderValue ("audio/x");
			Assert.AreEqual (value, new MediaTypeWithQualityHeaderValue ("audio/x"), "#1");
			Assert.AreEqual (value, new MediaTypeWithQualityHeaderValue ("aUdio/X"), "#2");
			Assert.AreNotEqual (value, new MediaTypeWithQualityHeaderValue ("audio/y"), "#3");

			value = new MediaTypeWithQualityHeaderValue ("audio/x", 0.3);
			Assert.AreEqual (value, new MediaTypeWithQualityHeaderValue ("audio/x", 0.3), "#4");
			Assert.AreNotEqual (value, new MediaTypeWithQualityHeaderValue ("audio/x"), "#5");
			Assert.AreNotEqual (value, new MediaTypeWithQualityHeaderValue ("audio/Y", 0.6), "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = MediaTypeWithQualityHeaderValue.Parse ("audio/ aa");
			Assert.AreEqual ("audio/aa", res.MediaType, "#1");
			Assert.AreEqual (0, res.Parameters.Count, "#1b");
			Assert.AreEqual ("audio/aa", res.ToString (), "#1c");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				MediaTypeWithQualityHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				MediaTypeWithQualityHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				MediaTypeWithQualityHeaderValue.Parse ("audio/");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new MediaTypeWithQualityHeaderValue ("audio/*", 0.3);
			Assert.IsNull (value.CharSet, "#1");
			Assert.AreEqual ("audio/*", value.MediaType, "#2");
			Assert.AreEqual ("q", value.Parameters.First ().Name, "#3");
			Assert.AreEqual (0.3, value.Quality, "#4");

			value.Parameters.Add (new NameValueHeaderValue ("q", "b"));
		}

		[Test]
		public void TryParse ()
		{
			MediaTypeWithQualityHeaderValue res;
			Assert.IsTrue (MediaTypeWithQualityHeaderValue.TryParse ("audio/*", out res), "#1");
			Assert.AreEqual (0, res.Parameters.Count, "#1");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			MediaTypeWithQualityHeaderValue res;
			Assert.IsFalse (MediaTypeWithQualityHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
