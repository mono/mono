//
// FormUrlEncodedContentTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using NUnit.Framework;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class FormUrlEncodedContentTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new FormUrlEncodedContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			var s = new List<KeyValuePair<string, string>> () {
				new KeyValuePair<string, string> ("key", "44"),
				new KeyValuePair<string, string> ("key 2", "&+/Ž@:=")
			};

			using (var m = new FormUrlEncodedContent (s)) {
				Assert.AreEqual ("application/x-www-form-urlencoded", m.Headers.ContentType.MediaType, "#1");
				Assert.AreEqual (37, m.Headers.ContentLength, "#2");
				Assert.AreEqual ("key=44&key+2=%26%2B%2F%C5%BD%40%3A%3D", m.ReadAsStringAsync ().Result, "#3");
			}

			s = new List<KeyValuePair<string, string>> ();
			using (var m = new FormUrlEncodedContent (s)) {
				Assert.AreEqual ("application/x-www-form-urlencoded", m.Headers.ContentType.MediaType, "#11");
				Assert.AreEqual (0, m.Headers.ContentLength, "#12");
				Assert.AreEqual ("", m.ReadAsStringAsync ().Result, "#13");
			}

			s = new List<KeyValuePair<string, string>> () {
				new KeyValuePair<string, string> ( "key", ""),
				new KeyValuePair<string, string> ( "key+ 2", null)
			};

			using (var m = new FormUrlEncodedContent (s)) {
				Assert.AreEqual ("application/x-www-form-urlencoded", m.Headers.ContentType.MediaType, "#21");
				Assert.AreEqual (14, m.Headers.ContentLength, "#22");
				Assert.AreEqual ("key=&key%2B+2=", m.ReadAsStringAsync ().Result, "#23");
			}
		}
	}
}
