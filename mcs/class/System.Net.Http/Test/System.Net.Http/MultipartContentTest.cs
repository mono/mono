//
// MultipartContentTest.cs
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
using System.Text;
using System.Linq;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class MultipartContentTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new MultipartContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new MultipartContent ("v", null);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				new MultipartContent ("st", "[]");
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				new MultipartContent ("st", "1234567890123456789012345678901234567890123456789012345678901234567890X");
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				new MultipartContent ("st", "st ");
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}

			try {
				new MultipartContent ("st", "@");
				Assert.Fail ("#6");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			using (var m = new MultipartContent ("a", "b")) {
				m.Headers.Add ("extra", "value");
				Assert.AreEqual ("multipart/a", m.Headers.ContentType.MediaType, "#1");
				Assert.AreEqual (14, m.Headers.ContentLength, "#2");
				Assert.AreEqual ("--b\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#3");
			}

			using (var m = new MultipartContent ()) {
				Assert.AreEqual ("multipart/mixed", m.Headers.ContentType.MediaType, "#11");
				Assert.AreEqual (84, m.Headers.ContentLength, "#12");
			}

			using (var m = new MultipartContent ("ggg")) {
				Assert.AreEqual ("multipart/ggg", m.Headers.ContentType.MediaType, "#21");
				Assert.AreEqual (84, m.Headers.ContentLength, "#22");
			}
		}

		[Test]
		public void Add ()
		{
			var m = new MultipartContent ("a", "b");

			var other = new MultipartContent ("2", "44");
			other.Headers.Expires = new DateTimeOffset (2020, 11, 30, 19, 55, 22, TimeSpan.Zero);
			m.Add (other);

			Assert.AreEqual ("multipart/a", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (114, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--b\r\nContent-Type: multipart/2; boundary=\"44\"\r\nExpires: Mon, 30 Nov 2020 19:55:22 GMT\r\n\r\n--44\r\n\r\n--44--\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
		}

		[Test]
		public void Add_2 ()
		{
			var m = new MultipartContent ("a", "X");

			var other = new MultipartContent ("2", "2a");
			m.Add (other);
			var other2 = new MultipartContent ("3", "3a");
			other2.Headers.Add ("9", "9n");
			m.Add (other2);

			Assert.AreEqual ("multipart/a", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (148, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--X\r\nContent-Type: multipart/2; boundary=\"2a\"\r\n\r\n--2a\r\n\r\n--2a--\r\n\r\n--X\r\nContent-Type: multipart/3; boundary=\"3a\"\r\n9: 9n\r\n\r\n--3a\r\n\r\n--3a--\r\n\r\n--X--\r\n",
				m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
		}

		[Test]
		public void Add_Resursive ()
		{
			var m = new MultipartContent ("1", "1a");

			var other = new MultipartContent ("2", "2a");
			m.Add (other);

			var other2 = new MultipartContent ("3", "3a");
			other.Add (other2);

			Assert.AreEqual ("multipart/1", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (136, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--1a\r\nContent-Type: multipart/2; boundary=\"2a\"\r\n\r\n--2a\r\nContent-Type: multipart/3; boundary=\"3a\"\r\n\r\n--3a\r\n\r\n--3a--\r\n\r\n--2a--\r\n\r\n--1a--\r\n",
				m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
		}

		[Test]
		public void Add_Invalid ()
		{
			var m = new MultipartContent ("a", "b");
			try {
				m.Add (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}
	}
}
