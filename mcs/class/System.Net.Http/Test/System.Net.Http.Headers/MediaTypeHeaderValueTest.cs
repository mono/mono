//
// MediaTypeHeaderValueTest.cs
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
	public class MediaTypeHeaderValueTest
	{
		[Test]
		public void Ctor_InvalidArguments ()
		{
			try {
				new MediaTypeHeaderValue (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new MediaTypeHeaderValue (" ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Equals ()
		{
			var value = new MediaTypeHeaderValue ("multipart/*");
			Assert.AreEqual (value, new MediaTypeHeaderValue ("multipart/*"), "#1");
			Assert.AreEqual (value, new MediaTypeHeaderValue ("Multipart/*"), "#2");
			Assert.AreNotEqual (value, new MediaTypeHeaderValue ("multipart/A"), "#3");

			value.CharSet = "chs";
			Assert.AreNotEqual (value, new MediaTypeHeaderValue ("multipart/*"), "#5");

			var custom_param = new MediaTypeHeaderValue ("Multipart/*");
			custom_param.Parameters.Add (new NameValueHeaderValue ("Charset", "chs"));
			Assert.AreEqual (value, custom_param, "#6");
		}

		[Test]
		public void Parse ()
		{
			var res = MediaTypeHeaderValue.Parse ("multipart  / b*  ");
			Assert.AreEqual ("multipart/b*", res.MediaType, "#1");
			Assert.IsNull (res.CharSet, "#1b");
			Assert.AreEqual ("multipart/b*", res.ToString (), "#1c");

			res = MediaTypeHeaderValue.Parse ("mu / m; CHarset=jj'  ");
			Assert.AreEqual ("mu/m", res.MediaType, "#2");
			Assert.AreEqual ("jj'", res.CharSet, "#2b");
			Assert.AreEqual ("mu/m; CHarset=jj'", res.ToString (), "#2c");
		}

		[Test]
		public void Parse_Invalid ()
		{
			try {
				MediaTypeHeaderValue.Parse (null);
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				MediaTypeHeaderValue.Parse ("  ");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}

			try {
				MediaTypeHeaderValue.Parse ("a;b");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var value = new MediaTypeHeaderValue ("multipart/*");
			Assert.AreEqual ("multipart/*", value.MediaType, "#1");
			Assert.IsNull (value.CharSet, "#2");
			Assert.AreEqual (0, value.Parameters.Count, "#3");

			value.CharSet = "chs";
			Assert.AreEqual ("chs", value.CharSet, "#4");

			value = new MediaTypeHeaderValue ("multipart/*");
			value.Parameters.Add (new NameValueHeaderValue ("CHarSEt", "te-va"));
			Assert.AreEqual ("te-va", value.CharSet, "#5");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var value = new MediaTypeHeaderValue ("multipart/*");

			try {
				value.CharSet = ";;";
				Assert.Fail ("#1");
			} catch (FormatException) {
			}

			try {
				value.MediaType = null;
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void TryParse ()
		{
			MediaTypeHeaderValue res;
			Assert.IsTrue (MediaTypeHeaderValue.TryParse ("multipart/*", out res), "#1");
			Assert.AreEqual ("multipart/*", res.MediaType, "#2");
			Assert.IsNull (res.CharSet, "#3");
		}

		[Test]
		public void TryParse_Invalid ()
		{
			MediaTypeHeaderValue res;
			Assert.IsFalse (MediaTypeHeaderValue.TryParse ("", out res), "#1");
			Assert.IsNull (res, "#2");
		}
	}
}
