//
// HttpMethodTest.cs
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
using System.Net.Http;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class HttpMethodTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new HttpMethod (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new HttpMethod ("");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				new HttpMethod (" ");
				Assert.Fail ("#3");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			var m = new HttpMethod ("get");
			Assert.AreEqual ("get", m.Method, "#1");
		}

		[Test]
		public void Equal ()
		{
			var m = new HttpMethod ("get");
			Assert.AreEqual (m, new HttpMethod ("get"), "#1");
			Assert.AreEqual (m, HttpMethod.Get, "#2");
			Assert.AreNotEqual (m, null, "#3");
		}

		[Test]
		public void StaticProperties ()
		{
			Assert.AreEqual ("DELETE", HttpMethod.Delete.Method, "#1");
			Assert.AreEqual ("GET", HttpMethod.Get.Method, "#2");
			Assert.AreEqual ("HEAD", HttpMethod.Head.Method, "#3");
			Assert.AreEqual ("OPTIONS", HttpMethod.Options.Method, "#4");
			Assert.AreEqual ("POST", HttpMethod.Post.Method, "#5");
			Assert.AreEqual ("PUT", HttpMethod.Put.Method, "#6");
			Assert.AreEqual ("TRACE", HttpMethod.Trace.Method, "#7");
		}
	}
}
