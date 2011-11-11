//
// HttpRequestMessageTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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
using System.Net.Http;
using System.Net;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpRequestMessageTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new HttpRequestMessage (null, "");
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Ctor_Default ()
		{
			var m = new HttpRequestMessage ();
			Assert.IsNull (m.Content, "#1");
			Assert.IsNotNull (m.Headers, "#2");
			Assert.AreEqual (HttpMethod.Get, m.Method, "#3");
			Assert.IsNotNull (m.Properties, "#4");
			Assert.IsNull (m.RequestUri, "#5");
			Assert.AreEqual (new Version (1, 1), m.Version, "#6");
		}

		[Test]
		public void Ctor_Uri ()
		{
			var c = new HttpRequestMessage (HttpMethod.Get, new Uri ("http://xamarin.com"));
			Assert.AreEqual ("http://xamarin.com/", c.RequestUri.AbsoluteUri, "#1");

			c = new HttpRequestMessage (HttpMethod.Get, new Uri ("https://xamarin.com"));
			Assert.AreEqual ("https://xamarin.com/", c.RequestUri.AbsoluteUri, "#2");

			c = new HttpRequestMessage (HttpMethod.Get, new Uri ("HTTP://xamarin.com:8080"));
			Assert.AreEqual ("http://xamarin.com:8080/", c.RequestUri.AbsoluteUri, "#3");

			var a = Uri.UriSchemeHttps;
			var b = new Uri ("http://xamarin.com");

			try {
				new HttpRequestMessage (HttpMethod.Get, new Uri ("ftp://xamarin.com"));
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Properties ()
		{
			var c = new HttpRequestMessage ();
			c.Content = null;
			c.Method = HttpMethod.Post;
			c.Properties.Add ("a", "test");
			c.RequestUri = null;
			c.Version = HttpVersion.Version10;

			Assert.IsNull (c.Content, "#1");
			Assert.AreEqual (HttpMethod.Post, c.Method, "#2");
			Assert.AreEqual ("test", c.Properties["a"], "#3");
			Assert.IsNull (c.RequestUri, "#4");
			Assert.AreEqual (HttpVersion.Version10, c.Version, "#5");
		}

		[Test]
		public void Properties_Invalid ()
		{
			var c = new HttpRequestMessage ();
			try {
				c.Method = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				c.RequestUri = new Uri ("ftp://xamarin.com");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				c.Version = null;
				Assert.Fail ("#3");
			} catch (ArgumentNullException) {
			}
		}
	}
}
