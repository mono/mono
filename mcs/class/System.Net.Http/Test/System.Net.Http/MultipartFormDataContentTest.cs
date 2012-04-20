//
// MultipartFormDataContentTest.cs
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
using System.Net.Http.Headers;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class MultipartFormDataContentTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new MultipartFormDataContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				new MultipartFormDataContent ("[]");
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				new MultipartFormDataContent ("1234567890123456789012345678901234567890123456789012345678901234567890X");
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				new MultipartFormDataContent ("st ");
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}

			try {
				new MultipartFormDataContent ("@");
				Assert.Fail ("#5");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			using (var m = new MultipartFormDataContent ("b")) {
				m.Headers.Add ("extra", "value");
				Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#1");
				Assert.IsNull (m.Headers.ContentDisposition, "#2");
				Assert.AreEqual (14, m.Headers.ContentLength, "#3");
				Assert.AreEqual ("--b\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#4");
			}

			using (var m = new MultipartFormDataContent ()) {
				Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#11");
				Assert.AreEqual (84, m.Headers.ContentLength, "#12");
			}

			using (var m = new MultipartFormDataContent ("ggg")) {
				Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#21");
				Assert.AreEqual (18, m.Headers.ContentLength, "#22");
			}
		}

		[Test]
		public void Add ()
		{
			var m = new MultipartFormDataContent ("b");

			var other = new MultipartFormDataContent ("44");
			other.Headers.Expires = new DateTimeOffset (2020, 11, 30, 19, 55, 22, TimeSpan.Zero);
			m.Add (other);

			Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (154, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--b\r\nContent-Type: multipart/form-data; boundary=\"44\"\r\nExpires: Mon, 30 Nov 2020 19:55:22 GMT\r\nContent-Disposition: form-data\r\n\r\n--44\r\n\r\n--44--\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
			Assert.IsNull (m.Headers.ContentDisposition, "#5");
			Assert.AreEqual ("form-data", other.Headers.ContentDisposition.ToString (), "#6");
		}

		[Test]
		public void Add_2 ()
		{
			var m = new MultipartFormDataContent ("b");

			var other = new MultipartFormDataContent ("44");
			m.Add (other, "name", "fname");

			Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (165, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--b\r\nContent-Type: multipart/form-data; boundary=\"44\"\r\nContent-Disposition: form-data; name=name; filename=fname; filename*=utf-8''fname\r\n\r\n--44\r\n\r\n--44--\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
			Assert.IsNull (m.Headers.ContentDisposition, "#5");
			Assert.AreEqual ("form-data; name=name; filename=fname; filename*=utf-8''fname", other.Headers.ContentDisposition.ToString (), "#6");
		}

		[Test]
		public void Add_3 ()
		{
			var m = new MultipartFormDataContent ("b");

			var other = new MultipartFormDataContent ("44");
			other.Headers.ContentDisposition = new ContentDispositionHeaderValue ("dt");
			m.Add (other, "name", "fname");

			Assert.AreEqual ("multipart/form-data", m.Headers.ContentType.MediaType, "#1");
			Assert.AreEqual (107, m.Headers.ContentLength, "#2");
			Assert.AreEqual ("--b\r\nContent-Type: multipart/form-data; boundary=\"44\"\r\nContent-Disposition: dt\r\n\r\n--44\r\n\r\n--44--\r\n\r\n--b--\r\n", m.ReadAsStringAsync ().Result, "#3");
			Assert.AreEqual (other, m.First (), "#4");
			Assert.IsNull (m.Headers.ContentDisposition, "#5");
			Assert.AreEqual ("dt", other.Headers.ContentDisposition.ToString (), "#6");
		}

		[Test]
		public void Add_Invalid ()
		{
			var m = new MultipartFormDataContent ("a");
			try {
				m.Add (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				m.Add (new MultipartFormDataContent ("44"), null);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			try {
				m.Add (new MultipartFormDataContent ("44"), "s", null);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			try {
				m.Add (new MultipartFormDataContent ("44"), "s", "   ");
				Assert.Fail ("#4");
			} catch (ArgumentException) {
			}
		}
	}
}
