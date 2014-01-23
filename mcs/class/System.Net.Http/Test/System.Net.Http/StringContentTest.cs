//
// StringContentTest.cs
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
using System.Text;

namespace MonoTests.System.Net.Http
{
	[TestFixture]
	public class StringContentTest
	{
		[Test]
		public void Ctor_Invalid ()
		{
			try {
				new StringContent (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				new StringContent ("", null, "aaa");
				Assert.Fail ("#2");
			} catch (FormatException) {
			}
		}

		[Test]
		public void Ctor ()
		{
			using (var m = new StringContent ("abcd")) {
			}

			var s = new StringContent ("aaa", null, "multipart/*");
			Assert.AreEqual ("Content-Type: multipart/*; charset=utf-8\r\n", s.Headers.ToString ());
#if !MOBILE
			s = new StringContent ("aaa", Encoding.GetEncoding (852), "multipart/*");
			Assert.AreEqual ("Content-Type: multipart/*; charset=ibm852\r\n", s.Headers.ToString ());
#endif
		}

		[Test]
		public void CopyToAsync_Invalid ()
		{
			var sc = new StringContent ("");
			try {
				sc.CopyToAsync (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void CopyToAsync ()
		{
			var sc = new StringContent ("gt");

			var dest = new MemoryStream ();
			var task = sc.CopyToAsync (dest);
			task.Wait ();
			Assert.AreEqual (2, dest.Length, "#1");
		}

		[Test]
		public void LoadIntoBuffer ()
		{
			var sc = new StringContent ("b");
			sc.LoadIntoBufferAsync (400).Wait ();
		}

		[Test]
		public void ReadAsByteArrayAsync ()
		{
			var sc = new StringContent ("h");
			var res = sc.ReadAsByteArrayAsync ().Result;
			Assert.AreEqual (1, res.Length, "#1");
			Assert.AreEqual (104, res[0], "#2");
		}

		[Test]
		public void ReadAsStringAsync ()
		{
			var sc = new StringContent ("abž");
			var res = sc.ReadAsStringAsync ().Result;
			Assert.AreEqual ("abž", res, "#1");
		}
	}
}
